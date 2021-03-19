using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.SocksProtocol;
using Socona.Fiveocks.Socks5Client.Events;
using Socona.Fiveocks.TCP;


namespace Socona.Fiveocks.Socks5Client
{
    //public class Socks5Client
    //{
    //    private IPAddress ipAddress;


    //    private Socket socket;
    //    private int Port;
    //    public bool reqPass = false;

    //    private string _username;
    //    private string _password;
    //    private string _destination;
    //    private int _destinationPort;

    //    public Encryption.SocksEncryption Encryption { get; set; }

    //    public event EventHandler<Socks5ClientArgs> SocketConnected;
    //    public event EventHandler<Socks5ClientDataArgs> DataReceived;
    //  //  public event EventHandler<Socks5ClientDataArgs> DataSent;
    //    public event EventHandler<Socks5ClientArgs> SocketDisconnected;


    //    public Socks5Client(string ipOrDomain, int port, string dest, int destport, string username = null, string password = null)
    //    {
    //        //Parse IP?
    //        if (!IPAddress.TryParse(ipOrDomain, out ipAddress))
    //        {
    //            //not connected.
    //            try
    //            {
    //                foreach (IPAddress p in Dns.GetHostAddresses(ipOrDomain))
    //                    if (p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork || p.AddressFamily == AddressFamily.InterNetworkV6)
    //                    {
    //                        DoSocks(p, port, dest, destport, username, password);
    //                        return;
    //                    }
    //            }
    //            catch
    //            {
    //                throw new NullReferenceException();
    //            }
    //        }
    //        DoSocks(ipAddress, port, dest, destport, username, password);
    //    }
    //    public Socks5Client(IPAddress ip, int port, string dest, int destport, string username = null, string password = null)
    //    {
    //        DoSocks(ip, port, dest, destport, username, password);
    //    }

    //    private void DoSocks(IPAddress ip, int port, string dest, int destport, string username = null, string password = null)
    //    {
    //        ipAddress = ip;
    //        Port = port;
    //        //check for username & pw.
    //        if (username != null && password != null)
    //        {
    //            _username = username;
    //            _password = password;
    //            reqPass = true;
    //        }
    //        _destination = dest;
    //        _destinationPort = destport;
    //        this.Encryption = new SocksEncryption();
    //    }

    //    public async Task<bool> ConnectAsync()
    //    {
    //        Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    //        try
    //        {
    //            return await Task.Run<bool>(() =>
    //             {
    //                 Socket.Connect(new IPEndPoint(ipAddress, Port));
    //                 if (Socks.DoSocksAuth(this, _username, _password))
    //                 {
    //                     SockStatus p1 = Socks.SendRequest(Socket, Encryption, _destination, _destinationPort);
    //                     this.SocketConnected?.Invoke(this, new Socks5ClientArgs(this, p1));
    //                     return true;
    //                 }
    //                 else
    //                 {
    //                     this.SocketDisconnected?.Invoke(this, new Socks5ClientArgs(this, SockStatus.Failure));
    //                     return false;
    //                 }
    //             });
    //        }
    //        catch (SocketException ex)
    //        {
    //            this.SocketDisconnected?.Invoke(this, new Socks5ClientArgs(null, SockStatus.Failure));
    //            Console.WriteLine(ex.Message);
    //            return false;
    //        }
    //    }


    //    void ClientDisconnected(object sender, DisconnectEventArgs e)
    //    {
    //        this.SocketDisconnected?.Invoke(this, new Socks5ClientArgs(this, SockStatus.Expired));
    //    }


    //    public async Task<bool> SendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    //    {
    //        try
    //        {
    //            //buffer sending.
    //            var array = memory.ToArray();
    //            byte[] outputdata = Encryption.ProcessOutputData(array, 0, array.Length);
    //            await Socket.SendAsync(memory, SocketFlags.None, cancellationToken);
    //            return true;
    //            //    while (true)
    //            //    {
    //            //       
    //            //        offset += (length - offset > 4096 ? 4096 : length - offset);
    //            //        //craft headers & shit.
    //            //        //send outputdata's length firs.t
    //            //        if (Encryption.GetAuthType() != AuthTypes.Login && Encryption.GetAuthType() != AuthTypes.None)
    //            //        {
    //            //            Socket.Send(BitConverter.GetBytes(outputdata.Length));
    //            //        }
    //            //        Socket.Send(outputdata, outputdata.Length, SocketFlags.None);
    //            //        if (offset >= buffer.Length)
    //            //        {
    //            //            //exit;
    //            //            return true;
    //            //        }
    //            //    }
    //        }
    //        catch (Exception)
    //        {

    //        }
    //        return false;
    //    }
    //    //public bool Send(byte[] buffer, int offset, int length)
    //    //{
    //    //    try
    //    //    {
    //    //        //buffer sending.
    //    //        while (true)
    //    //        {
    //    //            byte[] outputdata = Encryption.ProcessOutputData(buffer, offset, (length - offset > 4096 ? 4096 : length - offset));
    //    //            offset += (length - offset > 4096 ? 4096 : length - offset);
    //    //            //craft headers & shit.
    //    //            //send outputdata's length firs.t
    //    //            if (Encryption.GetAuthType() != AuthTypes.Login && Encryption.GetAuthType() != AuthTypes.None)
    //    //            {
    //    //                Socket.Send(BitConverter.GetBytes(outputdata.Length));
    //    //            }
    //    //            Socket.Send(outputdata, outputdata.Length, SocketFlags.None);
    //    //            if (offset >= buffer.Length)
    //    //            {
    //    //                //exit;
    //    //                return true;
    //    //            }
    //    //        }
    //    //    }
    //    //    catch (Exception)
    //    //    {

    //    //    }
    //    //    return false;

    //    //}

    //    //public bool Send(byte[] buffer, int count)
    //    //{
    //    //    return Send(buffer, count);
    //    //}

    //    public async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    //    {

    //        using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent();
    //        var memory = owner.Memory;
    //        int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
    //        if (recv <= 0)
    //        {
    //            return recv;
    //        }
    //        if (Encryption.GetAuthType() != AuthTypes.Login && Encryption.GetAuthType() != AuthTypes.None)
    //        {
    //            if (recv != 4)
    //            {
    //                throw new Exception($"Recv Size 4 excepted, {recv} get");
    //            }
    //            var torecv = BitConverter.ToInt32(memory.Slice(0, 4).Span);
    //            try
    //            {
    //                recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
    //            }
    //            catch (SocketException ex)
    //            {
    //                Console.WriteLine($"Socket Error:{ex.Message}");
    //            }
    //            if (recv == torecv)
    //            {
    //                //yey
    //                //process packet.
    //                byte[] output = Encryption.ProcessInputData(buffer.ToArray(), 0, recv);
    //                //receive full packet.
    //                new Memory<byte>(output).CopyTo(buffer);
    //                //Buffer.BlockCopy(output, 0, buff, 0, output.Length);
    //                return recv;
    //            }
    //            return 0;
    //        }
    //        else
    //        {
    //            byte[] output = Encryption.ProcessInputData(buffer.ToArray(), 0, recv);
    //            new Memory<byte>(output).CopyTo(buffer);
    //            //Buffer.BlockCopy(output, 0, buff, 0, output.Length);
    //            return recv;
    //        }
    //    }
        //public int Receive(byte[] buff, int count)
        //{
        //    //this should be packet header.
        //    byte[] buff1;
        //    //if we're using special encryptions, get size first.
        //    int recv = 0;

        //    int torecv = 0;
        //    if (Encryption.GetAuthType() != AuthTypes.Login && Encryption.GetAuthType() != AuthTypes.None)
        //    {
        //        byte[] buffer = new byte[sizeof(int)];
        //        recv = Socket.Receive(buffer, buffer.Length, SocketFlags.None);
        //        //get total number of bytes.
        //        torecv = BitConverter.ToInt32(buffer, 0);
        //        buff1 = new byte[torecv];
        //    }
        //    else
        //    {
        //        buff1 = new byte[4096];
        //    }
        //    recv = Socket.Receive(buff1, buf, SocketFlags.None);
        //    if (recv <= 0)
        //    {
        //        throw new Exception();
        //    }
        //    if (Encryption.GetAuthType() != AuthTypes.Login && Encryption.GetAuthType() != AuthTypes.None)
        //    {
        //        if (recv == torecv)
        //        {
        //            //yey
        //            //process packet.
        //            byte[] output = Encryption.ProcessInputData(buff1, 0, recv);
        //            //receive full packet.
        //            Buffer.BlockCopy(output, 0, buff, 0, output.Length);
        //            return recv;
        //        }
        //        else
        //        {
        //            throw new Exception();
        //        }
        //    }
        //    else
        //    {
        //        byte[] output = Encryption.ProcessInputData(buff1, 0, recv);
        //        Buffer.BlockCopy(output, 0, buff, 0, output.Length);
        //        return recv;
        //    }

        //}





//        void Client_onDataReceived(object sender, DataEventArgs e)
//        {
//            //this should be packet header.
//            try
//            {
//                if (Encryption.GetAuthType() != AuthTypes.Login && Encryption.GetAuthType() != AuthTypes.None)
//                {
//                    //get total number of bytes.
//                    int torecv = BitConverter.ToInt32(e.Buffer, e.Offset);

//                    byte[] newbuff = new byte[torecv];
//                    int recv = Socket.Receive(newbuff, newbuff.Length, SocketFlags.None);

//                    if (recv == torecv)
//                    {
//                        //yey
//                        //process packet.
//                        byte[] output = Encryption.ProcessInputData(newbuff, 0, recv);
//                        //receive full packet.
//                        e.Buffer = output;
//                        e.Offset = 0;
//                        e.Count = output.Length;
//                        this.DataReceived(this, new Socks5ClientDataArgs(this, e.Buffer, e.Count, e.Offset));
//                    }
//                    else
//                    {
//                        throw new Exception();
//                    }
//                }
//                else
//                {
//                    this.DataReceived(this, new Socks5ClientDataArgs(this, e.Buffer, e.Count, e.Offset));
//                }
//            }
//            catch
//            {
//                //disconnect.
//                //  Disconnect();
//                throw;
//            }
//        }

//        public bool Connect()
//        {
//            try
//            {
//                Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

//                Socket.Connect(new IPEndPoint(ipAddress, Port));
//                //try the greeting.
//                //Client.onDataReceived += Client_onDataReceived;
//                if (Socks.DoSocksAuth(this, _username, _password))
//                    if (Socks.SendRequest(Socket, Encryption, _destination, _destinationPort) == SockStatus.Granted)
//                        return true;
//                return false;
//            }
//            catch
//            {
//                return false;
//            }
//        }




//        public bool Connected
//        {
//            get { return Socket.Connected; }
//        }

//        public Socket Socket { get => socket; set => socket = value; }

//        //send.
//        public void Disconnect()
//        {
//            Socket.Disconnect(false);
//        }

//    }
}
