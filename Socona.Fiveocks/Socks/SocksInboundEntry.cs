using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Socks
{
    //public class SocksInboundEntry : IDisposable
    //{
    //    public event EventHandler<SocksClientEventArgs> Disconnected;
    //    public event EventHandler<DataEventArgs> UpStreamDataTransfered;
    //    public event EventHandler<DataEventArgs> DownStreamDataTransfered;


    //    public Socket Socket { get; set; }
    //    public int Authenticated { get; private set; }


    //    public SocksInboundEntry(Socket cli)
    //    {
    //        Socket = cli;
    //    }
    //    private SocksRequest req1;
    //    public SocksRequest Destination { get { return req1; } }
    //    public async Task StartAsync(int PacketSize, int Timeout, CancellationToken cancellationToken)
    //    {
    //        SocksEncryption w = null;
    //        if (Socket == null)
    //        {
    //            return;
    //        }

    //        Authenticated = AuthenticateConnection(ref w);
    //        //Request Site Data.
    //        if (Authenticated == 1)
    //        {
    //            w = new SocksEncryption();
    //            w.SetType(AuthTypes.Login);
    //            SocksRequest req = Socks5.RequestTunnel(this, w);
    //            if (req == null) { Socket.Disconnect(false); return; }
    //            req1 = new SocksRequest(req.StreamType, req.Type, null, req.Address, req.Port);
    //            //call on plugins for connect callbacks.
    //            //foreach (ConnectHandler conn in PluginLoader.LoadPlugin(typeof(ConnectHandler)))
    //            //    if (conn.Enabled)
    //            //        if (conn.OnConnect(req1) == false)
    //            //        {
    //            //            req.Error = SockStatus.Failure;
    //            //            Socket.Send(req.GetData(true));

    //            //            Socket.Disconnect(false);
    //            //            return;
    //            //        }
    //            //Send Tunnel Data back.
    //            SocksTunnel x = new SocksTunnel(this, req, req1, PacketSize, Timeout);
    //            x.TunnelDisconnecting += x_TunnelDisposing;
    //            x.UpStreamDataTransfered += X_UpStreamDataTransfered;
    //            x.DownStreamDataTransfered += X_DownStreamDataTransfered;
    //            await x.OpenAsync(cancellationToken);
    //            x.Dispose();
    //        }
    //        else if (Authenticated == 2)
    //        {
    //            SocksRequest req = Socks5.RequestTunnel(this, w);
    //            if (req == null) { Socket.Disconnect(false); return; }
    //            req1 = new SocksRequest(req.StreamType, req.Type, null, req.Address, req.Port);
    //            if (PluginLoader.LoadPlugin(typeof(ConnectHandler)).Cast<ConnectHandler>().Where(conn => conn.Enabled).Any(conn => conn.OnConnect(req1) == false))
    //            {
    //                req.Error = SockStatus.Failure;
    //                Socket.Send(req.GetData(true));
    //                Socket.Disconnect(false);
    //                return;
    //            }
    //            //Send Tunnel Data back.
    //            SocksSpecialTunnel x = new SocksSpecialTunnel(this, w, req, req1, PacketSize, Timeout);
    //            x.TunnelDisposing += x_TunnelDisposing;
    //            x.Start();
    //            x.Dispose();
    //        }
    //    }

    //    private void X_DownStreamDataTransfered(object sender, DataEventArgs e)
    //    {
    //        this.DownStreamDataTransfered?.Invoke(sender, e);
    //    }

    //    private void X_UpStreamDataTransfered(object sender, DataEventArgs e)
    //    {
    //        this.UpStreamDataTransfered?.Invoke(sender, e);
    //    }

    //    void x_TunnelDisposing(object sender, EventArgs e)
    //    {
    //        this.Disconnected?.Invoke(this, new SocksClientEventArgs(this));
    //    }

    //    public int AuthenticateConnection(ref SocksEncryption encryption)
    //    {
    //        List<AuthTypes> authtypes = Socks5.RequestAuth(this);
    //        if (authtypes == null || authtypes.Count <= 0)
    //        {
    //            Socket.Send(new byte[] { 0x00, 0xFF });
    //            Console.WriteLine("E Auth type missing. [None] needed");
    //            Socket.Disconnect(false);
    //            return 0;
    //        }
    //        this.Authenticated = 0;

    //        List<object> lhandlers = PluginLoader.LoadPlugin(typeof(LoginHandler));
    //        bool loginenabled = false;
    //        if (lhandlers.Count > 0)
    //        {
    //            loginenabled = lhandlers.Cast<LoginHandler>().Any(l => l.Enabled);
    //        }
    //        using var headerMemoryOwner = MemoryPool<byte>.Shared.Rent(2);
    //        var headerSpan = headerMemoryOwner.Memory.Span;
    //        headerSpan[0] = (byte)HeaderTypes.Socks5;

    //        //check out different auth types, none will have no authentication, the rest do.
    //        if (loginenabled && (authtypes.Contains(AuthTypes.SocksBoth) || authtypes.Contains(AuthTypes.SocksEncrypt) || authtypes.Contains(AuthTypes.SocksCompress) || authtypes.Contains(AuthTypes.Login)))
    //        {
    //            //this is the preferred method.
    //            encryption = Socks5.RequestSpecialMode(authtypes, Socket);

    //            foreach (LoginHandler lh in lhandlers)
    //            {
    //                if (lh.Enabled)
    //                {
    //                    //request login.
    //                    SocksUser user = Socks5.RequestLogin(this);
    //                    if (user == null)
    //                    {
    //                        Socket.Disconnect(false);
    //                        return 0;
    //                    }
    //                    LoginStatus status = lh.HandleLogin(user) ? LoginStatus.Correct : LoginStatus.Denied;
    //                    headerSpan[1] = (byte)status;
    //                    Socket.Send(headerSpan.Slice(0, 2));
    //                    if (status == LoginStatus.Denied)
    //                    {
    //                        Console.WriteLine("> Login Denied!");
    //                        Socket.Disconnect(false);
    //                        return 0;
    //                    }
    //                    else if (status == LoginStatus.Correct)
    //                    {
    //                        Authenticated = (encryption.GetAuthType() == AuthTypes.Login ? 1 : 2);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //        else if (authtypes.Contains(AuthTypes.None))
    //        {
    //            //no authentication.
    //            if (lhandlers.Count <= 1)
    //            {
    //                //unsupported methods y0
    //                Authenticated = 1;
    //                headerSpan[1] = (byte)HeaderTypes.Zero;
    //                Socket.Send(headerSpan.Slice(0, 2));
    //            }
    //            else
    //            {
    //                //unsupported.
    //                headerSpan[1] = (byte)AuthTypes.Unsupported;
    //                Socket.Send(headerSpan.Slice(0, 2));
    //                Socket.Disconnect(false);
    //                return 0;
    //            }
    //        }
    //        else
    //        {
    //            //unsupported.
    //            headerSpan[1] = (byte)AuthTypes.Unsupported;
    //            Socket.Send(headerSpan.Slice(0, 2));
    //            Socket.Disconnect(false);
    //            return 0;
    //        }
    //        return Authenticated;
    //    }

    //    public List<AuthTypes> RequestAuth(SocksInboundEntry client)
    //    {
    //        var buff = BufferManager.DefaultManager.CheckOut();
    //        int recv = client.Socket.Receive(buff, buff.Length, SocketFlags.None);

    //        if (buff == null || (HeaderTypes)buff[0] != HeaderTypes.Socks5) return new List<AuthTypes>();

    //        int methods = Convert.ToInt32(buff[1]);
    //        List<AuthTypes> types = new List<AuthTypes>();
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
    //        return types;
    //    }

    //    public SocksEncryption RequestSpecialMode(List<AuthTypes> auth, Socket client)
    //    {
    //        //select mode, do key exchange if encryption, or start compression.
    //        if (auth.Contains(AuthTypes.SocksBoth))
    //        {
    //            //tell client that we chose socksboth.
    //            client.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.SocksBoth });
    //            //wait for public key.
    //            SocksEncryption ph = new SocksEncryption();
    //            ph.GenerateKeys();
    //            //wait for public key.
    //            byte[] buffer = new byte[4096];
    //            int keysize = client.Receive(buffer, buffer.Length, SocketFlags.None);
    //            //store key in our encryption class.
    //            ph.SetKey(buffer, 0, keysize);
    //            //send key.
    //            client.Send(ph.GetPublicKey());
    //            //now we give them our key.
    //            client.Send(ph.ShareEncryptionKey());
    //            //send more.
    //            int enckeysize = client.Receive(buffer, buffer.Length, SocketFlags.None);
    //            //decrypt with our public key.
    //            byte[] newkey = new byte[enckeysize];
    //            Buffer.BlockCopy(buffer, 0, newkey, 0, enckeysize);
    //            ph.SetEncKey(ph.key.Decrypt(newkey, false));

    //            ph.SetType(AuthTypes.SocksBoth);
    //            //ready up our client.
    //            return ph;
    //        }
    //        else if (auth.Contains(AuthTypes.SocksEncrypt))
    //        {
    //            //tell client that we chose socksboth.
    //            client.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.SocksEncrypt });
    //            //wait for public key.
    //            SocksEncryption ph = new SocksEncryption();
    //            ph.GenerateKeys();
    //            //wait for public key.
    //            byte[] buffer = new byte[4096];
    //            int keysize = client.Receive(buffer, buffer.Length, SocketFlags.None);
    //            //store key in our encryption class.
    //            ph.SetKey(buffer, 0, keysize);
    //            ph.SetType(AuthTypes.SocksBoth);
    //            //ready up our client.
    //            return ph;
    //        }
    //        else if (auth.Contains(AuthTypes.SocksCompress))
    //        {
    //            //start compression.
    //            client.Send(new byte[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.SocksCompress });
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


    //    public void Dispose()
    //    {
    //        if (Socket != null)
    //        {
    //            Socket.Disconnect(false);
    //        }
    //        Socket = null;
    //        req1 = null;
    //    }
    //}
    public class SocksUser
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public IPEndPoint IP { get; private set; }
        public SocksUser(string un, string pw, IPEndPoint ip)
        {
            Username = un;
            Password = pw;
            IP = ip;
        }
    }
}
