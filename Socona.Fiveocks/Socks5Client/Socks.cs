using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Socks;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Socks5Client
{
    public static class Socks
    {
        public static AuthTypes Greet(this Socket socketClient)
        {
            socketClient.Send(new byte[] { 0x05, Convert.ToByte(5), (byte)AuthTypes.None, (byte)AuthTypes.Login, (byte)AuthTypes.SocksCompress, (byte)AuthTypes.SocksEncrypt, (byte)AuthTypes.SocksBoth });
            byte[] buffer = new byte[512];
            int received = socketClient.Receive(buffer, buffer.Length, SocketFlags.None);
            if(received > 0)
            {
                //check for server version.
                if (buffer[0] == 0x05)
                {
                    return (AuthTypes)buffer[1];
                }
            }
            return 0;
        }

        public static int SendLogin(this Socket cli, string Username, string Password)
        {
            byte[] x = new byte[Username.Length + Password.Length + 3];
            int total = 0;
            x[total++] = 0x01;
            x[total++] = Convert.ToByte(Username.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(Username), 0, x, 2, Username.Length);
            total += Username.Length;
            x[total++] = Convert.ToByte(Password.Length); 
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(Password), 0, x, total, Password.Length);
            //send request.
            cli.Send(x);
            byte[] buffer = new byte[512];
            cli.Receive(buffer, buffer.Length, SocketFlags.None);
            if (buffer[1] == 0x00)
            {
                return 1;
            }
            else if (buffer[1] == 0xFF)
            {
                return 0;
            }
            return 0;
        }

        public static SockStatus SendRequest(this Socket cli, SocksEncryption enc, string ipOrDomain, int port)
        {
            AddressType type;            
            if (!IPAddress.TryParse(ipOrDomain, out IPAddress ipAddress))
                //it's a domain. :D (hopefully).
                type = AddressType.Domain;
            else
                type = AddressType.IP;
            SocksRequest sr = new SocksRequest(StreamTypes.Stream, type,null,ipOrDomain, port);
            //send data.
            byte[] p = sr.GetData(false);
            p[1] = 0x01;
            //process data.
            cli.Send(enc.ProcessOutputData(p, 0, p.Length),SocketFlags.None);
            byte[] buffer = new byte[512];// BufferManager.DefaultManager.CheckOut();
            //process input data.
            int recv = cli.Receive(buffer,buffer.Length, SocketFlags.None);
            if(recv == -1)
            {
                return SockStatus.Failure;
            }
            byte[] buff = enc.ProcessInputData(buffer, 0, recv);
          //  BufferManager.DefaultManager.CheckIn(buffer);
            return (SockStatus)buff[1];
        }

        public static bool DoSocksAuth(this Socks5Client p, string Username, string Password)
        {
            AuthTypes auth = Socks.Greet(p.Socket);
            if (auth == AuthTypes.Unsupported)
            {
                p.Socket.Close();
                return false;
            }
            if (auth != AuthTypes.None)
            {
                p.Encryption = new Encryption.SocksEncryption();
                switch (auth)
                {
                    case AuthTypes.Login:
                        //logged in.
                        p.Encryption.SetType(AuthTypes.Login);
                        //just reqeust login?

                        break;
                    case AuthTypes.SocksBoth:
                        //socksboth.
                        p.Encryption.SetType(AuthTypes.SocksBoth);
                        p.Encryption.GenerateKeys();
                        //send public key.
                        p.Socket.Send(p.Encryption.GetPublicKey());
                        //now receive key.

                        byte[] buffer = new byte[4096];
                        int keysize = p.Socket.Receive(buffer, buffer.Length, SocketFlags.None);
                        p.Encryption.SetKey(buffer, 0, keysize);
                        //let them know we got it
                        //now receive our encryption key.
                        int enckeysize = p.Socket.Receive(buffer,buffer.Length, SocketFlags.None);
                        //decrypt with our public key.
                        byte[] newkey = new byte[enckeysize];
                        Buffer.BlockCopy(buffer, 0, newkey, 0, enckeysize);
                        p.Encryption.SetEncKey(p.Encryption.key.Decrypt(newkey, false));
                        //now we share our encryption key.
                        p.Socket.Send(p.Encryption.ShareEncryptionKey());

                        break;
                    case AuthTypes.SocksEncrypt:
                        p.Encryption.SetType(AuthTypes.SocksEncrypt);
                        p.Encryption.GenerateKeys();
                        //send public key.
                        p.Socket.Send(p.Encryption.GetPublicKey());
                        //now receive key.

                        buffer = new byte[4096];
                        keysize = p.Socket.Receive(buffer, buffer.Length,   SocketFlags.None);
                        p.Encryption.SetKey(buffer, 0, keysize);
                        //let them know we got it
                        p.Socket.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)HeaderTypes.Zero });
                        //now receive our encryption key.
                        enckeysize = p.Socket.Receive(buffer, buffer.Length, SocketFlags.None);
                        //decrypt with our public key.
                        newkey = new byte[enckeysize];
                        Buffer.BlockCopy(buffer, 0, newkey, 0, enckeysize);
                        p.Encryption.SetEncKey(p.Encryption.key.Decrypt(newkey, false));
                        //wait for server to confirm we got it.
                        p.Socket.Receive(buffer, buffer.Length, SocketFlags.None);
                        //now we share our encryption key.

                        p.Socket.Send(p.Encryption.ShareEncryptionKey());

                        //socksencrypt.
                        break;
                    case AuthTypes.SocksCompress:
                        p.Encryption.SetType(AuthTypes.SocksCompress);
                        //sockscompress.
                        break;
                    default:
                        p.Socket.Close();
                        return false;
                }
                if (p.Encryption.GetAuthType() != AuthTypes.Login)
                {
                    //now receive login params.
                    byte[] buff = new byte[1024];
                    int recv = p.Socket.Receive(buff, buff.Length, SocketFlags.None);
                    //check for 
                    if (recv > 0)
                    {
                        //check if socks5 version is 5
                        if (buff[0] == 0x05)
                        {
                            //good.
                            if (buff[1] == (byte)AuthTypes.Login)
                            {
                                if (Username == null || Password == null) { p.Socket.Close(); return false; }
                                int ret = Socks.SendLogin(p.Socket, Username, Password);
                                if (ret != 1)
                                {
                                    p.Socket.Close();
                                    return false;
                                }
                            }
                            else
                            {
                                //idk? close for now.
                                p.Socket.Close();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        p.Socket.Close();
                        return false;
                    }
                }
                else
                {
                    if (Username == null || Password == null) { p.Socket.Close(); return false; }
                    int ret = Socks.SendLogin(p.Socket, Username, Password);
                    if (ret != 1)
                    {
                        p.Socket.Close();
                        return false;
                    }
                }
            }
            return true;
        }
    }
}