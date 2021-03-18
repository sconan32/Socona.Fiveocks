using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Socks
{
    //public static class Socks5
    //{
    //    public static List<AuthTypes> RequestAuth(this SocksInboundEntry client)
    //    {
    //        byte[] buff = BufferManager.DefaultManager.CheckOut();
    //        int recv = client.Socket.Receive(buff, buff.Length, SocketFlags.None);

    //        HeaderTypes headerType = (HeaderTypes)buff[0];
    //        if (headerType != HeaderTypes.Socks5)
    //        {
    //            BufferManager.DefaultManager.CheckIn(buff);
    //            return null;
    //        }


    //        int methods = Convert.ToInt32(buff[1]);
    //        List<AuthTypes> types = new List<AuthTypes>(5);
    //        for (int i = 2; i < methods + 2; i++)
    //        {
    //            switch ((AuthTypes)buff[i])
    //            {
    //                case AuthTypes.Login:
    //                    types.Add(AuthTypes.Login);
    //                    break;
    //                case AuthTypes.None:
    //                    types.Add(AuthTypes.None);
    //                    break;
    //                case AuthTypes.SocksBoth:
    //                    types.Add(AuthTypes.SocksBoth);
    //                    break;
    //                case AuthTypes.SocksEncrypt:
    //                    types.Add(AuthTypes.SocksEncrypt);
    //                    break;
    //                case AuthTypes.SocksCompress:
    //                    types.Add(AuthTypes.SocksCompress);
    //                    break;
    //            }
    //        }
    //        BufferManager.DefaultManager.CheckIn(buff);
    //        return types;
    //    }

    //    public static SocksEncryption RequestSpecialMode(List<AuthTypes> auth, Socket socketClient)
    //    {
    //        //select mode, do key exchange if encryption, or start compression.
    //        if (auth.Contains(AuthTypes.SocksBoth))
    //        {
    //            //tell Client that we chose socksboth.
    //            socketClient.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.SocksBoth });
    //            //wait for public key.
    //            SocksEncryption ph = new SocksEncryption();
    //            ph.GenerateKeys();
    //            //wait for public key.
    //            byte[] buffer = new byte[4096];
    //            int keysize = socketClient.Receive(buffer, buffer.Length, SocketFlags.None);
    //            //store key in our encryption class.
    //            ph.SetKey(buffer, 0, keysize);
    //            //send key.
    //            socketClient.Send(ph.GetPublicKey());
    //            //now we give them our key.
    //            socketClient.Send(ph.ShareEncryptionKey());
    //            //send more.
    //            int enckeysize = socketClient.Receive(buffer, buffer.Length, SocketFlags.None);
    //            //decrypt with our public key.
    //            byte[] newkey = new byte[enckeysize];
    //            Buffer.BlockCopy(buffer, 0, newkey, 0, enckeysize);
    //            ph.SetEncKey(ph.key.Decrypt(newkey, false));

    //            ph.SetType(AuthTypes.SocksBoth);
    //            //ready up our Client.
    //            return ph;
    //        }
    //        else if (auth.Contains(AuthTypes.SocksEncrypt))
    //        {
    //            //tell Client that we chose socksboth.
    //            socketClient.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.SocksEncrypt });
    //            //wait for public key.
    //            SocksEncryption ph = new SocksEncryption();
    //            ph.GenerateKeys();
    //            //wait for public key.
    //            byte[] buffer = new byte[4096];
    //            int keysize = socketClient.Receive(buffer, buffer.Length, SocketFlags.None);
    //            //store key in our encryption class.
    //            ph.SetKey(buffer, 0, keysize);
    //            ph.SetType(AuthTypes.SocksBoth);
    //            //ready up our Client.
    //            return ph;
    //        }
    //        else if (auth.Contains(AuthTypes.SocksCompress))
    //        {
    //            //start compression.
    //            socketClient.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.SocksCompress });
    //            SocksEncryption ph = new SocksEncryption();
    //            ph.SetType(AuthTypes.SocksCompress);
    //            //ready
    //        }
    //        else if (auth.Contains(AuthTypes.Login))
    //        {
    //            SocksEncryption ph = new SocksEncryption();
    //            ph.SetType(AuthTypes.Login);
    //            return ph;
    //        }

    //        return null;
    //    }

    //    public static SocksUser RequestLogin(this SocksInboundEntry client)
    //    {
    //        //request authentication.
    //        client.Socket.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.Login });
    //        byte[] buff = BufferManager.DefaultManager.CheckOut();

    //        int recv = client.Socket.Receive(buff, buff.Length, SocketFlags.None);

    //        if (buff == null || buff[0] != 0x01) return null;

    //        int numusername = Convert.ToInt32(buff[1]);
    //        int numpassword = Convert.ToInt32(buff[(numusername + 2)]);
    //        string username = Encoding.ASCII.GetString(buff, 2, numusername);
    //        string password = Encoding.ASCII.GetString(buff, numusername + 3, numpassword);
    //        BufferManager.DefaultManager.CheckIn(buff);
    //        return new SocksUser(username, password, (IPEndPoint)client.Socket.RemoteEndPoint);
    //    }

    //    public static SocksRequest RequestTunnel(this SocksInboundEntry client, SocksEncryption ph)
    //    {
    //        byte[] data = BufferManager.DefaultManager.CheckOut();
    //        int recv = client.Socket.Receive(data, data.Length, SocketFlags.None);
    //        byte[] buff = ph.ProcessInputData(data, 0, recv);
    //        BufferManager.DefaultManager.CheckIn(data);

    //        if (buff == null || (HeaderTypes)buff[0] != HeaderTypes.Socks5) return null;
    //        switch ((StreamTypes)buff[1])
    //        {
    //            case StreamTypes.Stream:
    //                {
    //                    int fwd = 4;
    //                    string address = "";
    //                    switch ((AddressType)buff[3])
    //                    {
    //                        case AddressType.IP:
    //                            {
    //                                for (int i = 4; i < 8; i++)
    //                                {
    //                                    //grab IP.
    //                                    address += Convert.ToInt32(buff[i]).ToString() + (i != 7 ? "." : "");
    //                                }
    //                                fwd += 4;
    //                            }
    //                            break;
    //                        case AddressType.Domain:
    //                            {
    //                                int domainlen = Convert.ToInt32(buff[4]);
    //                                address += Encoding.ASCII.GetString(buff, 5, domainlen);
    //                                fwd += domainlen + 1;
    //                            }
    //                            break;
    //                        case AddressType.IPv6:
    //                            //can't handle IPV6 traffic just yet.
    //                            return null;
    //                    }
    //                    byte[] po = new byte[2];
    //                    Array.Copy(buff, fwd, po, 0, 2);
    //                    Int16 x = BitConverter.ToInt16(po, 0);
    //                    int port = Convert.ToInt32(IPAddress.NetworkToHostOrder(x));
    //                    port = (port < 1 ? port + 65536 : port);
    //                    return new SocksRequest(StreamTypes.Stream, (AddressType)buff[3],null, address, port);
    //                }
    //            default:
    //                //not supported.
    //                return null;

    //        }
    //    }


    //}

    public class SocksRequest
    {
        private List<IPAddress> _ipAddresses;
        public AddressType Type { get; set; }
        public StreamTypes StreamType { get; private set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public SockStatus Error { get; set; }
        public SocksRequest(StreamTypes type, AddressType addrtype, IPAddress ipAddress, string address, int port)
        {
            Type = addrtype;
            StreamType = type;
            Address = address;
            Port = port;
            _ipAddresses = new List<IPAddress>();
            if (ipAddress != null)
            {
                _ipAddresses.Add(ipAddress);
            }
        }
        public List<IPAddress> IPAddresses => _ipAddresses;


        public int MakeSocks5Request(Memory<byte> memory, bool isNetToHost)
        {
            memory.Span[0] = 0x05;
            memory.Span[1] = (byte)Error;
            memory.Span[2] = 0x00;
            memory.Span[3] = (byte)Type;

            int headerIdx = 4;
            if (Type == AddressType.Domain)
            {
                //+-------+-------+-------+-------+----------+----------+
                //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                //+-------+-------+-------+-------+----------+----------+
                //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
                //+-------+-------+-------+-------+----------+----------+

                int bytesNeeded = Encoding.ASCII.GetByteCount(Address);
                memory.Span[headerIdx++] = (byte)bytesNeeded;
                if (MemoryMarshal.TryGetArray(memory.Slice(headerIdx, bytesNeeded), out ArraySegment<byte> arraySeg))
                {
                    Encoding.ASCII.GetBytes(Address, 0, Address.Length, arraySeg.Array, arraySeg.Offset);
                    headerIdx += bytesNeeded;
                }
            }
            else if (IPAddress.TryParse(Address, out IPAddress ipaddr))
            {
                if (Type == AddressType.IP && ipaddr.AddressFamily == AddressFamily.InterNetwork)
                {
                    //+-------+-------+-------+-------+----------+----------+
                    //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                    //+-------+-------+-------+-------+----------+----------+
                    //| 1Byte | 1Byte |  0x00 | 1Byte |  4Bytes  |  2Bytes  |
                    //+-------+-------+-------+-------+----------+----------+
                    ipaddr.TryWriteBytes(memory.Slice(headerIdx, 4).Span, out int length);
                }
                else if (Type == AddressType.IPv6 && ipaddr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    //+-------+-------+-------+-------+----------+----------+
                    //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                    //+-------+-------+-------+-------+----------+----------+
                    //| 1Byte | 1Byte |  0x00 | 1Byte | 16Bytes  |  2Bytes  |
                    //+-------+-------+-------+-------+----------+----------+

                    ipaddr.TryWriteBytes(memory.Slice(headerIdx, 16).Span, out int length);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }

            short port = isNetToHost ? (short)IPAddress.NetworkToHostOrder((short)Port) : (short)IPAddress.HostToNetworkOrder((short)Port);
            if (BitConverter.TryWriteBytes(memory.Span.Slice(headerIdx, 2), port))
            {
                return headerIdx + 2;
            }
            return -1;

        }
        public byte[] GetData(bool networkToHostOrder)
        {
            byte[] data;
            var port = networkToHostOrder ? IPAddress.NetworkToHostOrder(Port) : IPAddress.HostToNetworkOrder(Convert.ToInt16(Port));

            if (Type == AddressType.IP)
            {
                data = new byte[10];
                string[] content = IPAddresses[0].ToString().Split('.');
                for (int i = 4; i < content.Length + 4; i++)
                    data[i] = Convert.ToByte(Convert.ToInt32(content[i - 4]));
                Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, 8, 2);
            }
            else if (Type == AddressType.Domain)
            {
                data = new byte[Address.Length + 7];
                data[4] = Convert.ToByte(Address.Length);
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(Address), 0, data, 5, Address.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, data.Length - 2, 2);
            }
            else return null;
            data[0] = 0x05;
            data[1] = (byte)Error;
            data[2] = 0x00;
            data[3] = (byte)Type;
            return data;
        }
    }

    public enum AuthTypes
    {
        None = 0x00,            //NO AUTHENTICATION REQUIRED 不需要认证       
        GSSAPI = 0x01,            //GSSAPI, 类似SSH的认证协议   (不支持）
        Login = 0x02,           //USERNAME/PASSWORD 用户名密码认证
                                //0x03-0x7F  IANA ASSIGNED 协会保留方法
                                //0x80-0xFE  自定义方法
        SocksCompress = 0x88,   //
        SocksEncrypt = 0x90,
        SocksBoth = 0xFE,
        Unsupported = 0xFF,     // NO ACCEPTABLE METHODS 没有可接受的方法

    }

    public enum HeaderTypes
    {
        Socks5 = 0x05,
        Zero = 0x00
    }

    public enum StreamTypes
    {
        Stream = 0x01,
        Bind = 0x02,
        UDP = 0x03
    }

    public enum AddressType
    {
        IP = 0x01,
        Domain = 0x03,
        IPv6 = 0x04
    }

    public enum SockStatus
    {
        Granted = 0x00,
        Failure = 0x01,
        NotAllowed = 0x02,
        Unreachable = 0x03,
        HostUnreachable = 0x04,
        Refused = 0x05,
        Expired = 0x06,
        NotSupported = 0x07,
        AddressNotSupported = 0x08,
        LoginRequired = 0x90
    }
}
