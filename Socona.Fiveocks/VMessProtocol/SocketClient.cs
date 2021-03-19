using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Socona.Fiveocks.TCP
{
    //public class SocketClient : ISocketClient, IDisposable
    //{
    //    private int packetSize = 4096;

    //    public bool Receiving { get; set; }

    //    private bool disposed;

    //    public event EventHandler<DisconnectEventArgs> Disconnecting;

    //    public event EventHandler<DataEventArgs> DataReceived;

    //    public event EventHandler<DataEventArgs> DataSent;


    //    public Socket Socket
    //    {
    //        get;
    //        set;
    //    }

    //    public SocketClient(Socket sock, int PacketSize)
    //    {
    //        this.Socket = sock;
    //        this.packetSize = PacketSize;
    //    }

    //    private bool SocketConnected(Socket s)
    //    {
    //        if (!s.Connected)
    //        {
    //            return false;
    //        }
    //        bool part = s.Poll(10000, SelectMode.SelectError);
    //        bool part2 = s.Available == 0;
    //        return !part || !part2;
    //    }

    //    public int Receive(byte[] data, int offset, int count)
    //    {
    //        int result;
    //        try
    //        {
    //            int received = this.Socket.Receive(data, offset, count, SocketFlags.None);
    //            if (received <= 0)
    //            {
    //                this.Disconnect();
    //                result = -1;
    //            }
    //            else
    //            {
    //                new DataEventArgs(data, received, null);
    //                result = received;
    //            }
    //        }
    //        catch (SocketException ex)
    //        {
    //            Console.WriteLine("E >>>> Error In Receive! DCing! <<<< {0}", ex.Message);
    //            this.Disconnect();
    //            result = -1;
    //        }
    //        return result;
    //    }

    //    //public async Task ReceiveAsyncNew()
    //    //{
    //    //    this.Receiving = true;
    //    //    SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
    //    //    byte[] array = BufferManager.DefaultManager.CheckOut();
    //    //    socketAsyncEventArgs.SetBuffer(array, 0, array.Length);
    //    //    SocketAwaitable awaitable = new SocketAwaitable(socketAsyncEventArgs);
    //    //    while (this.Socket != null && this.Socket.Connected)
    //    //    {
    //    //        try
    //    //        {
    //    //            await this.Socket.ReceiveAsync(awaitable);

    //    //            int bytesTransferred = socketAsyncEventArgs.BytesTransferred;
    //    //            this.DataReceived?.Invoke(this, new DataEventArgs(array, bytesTransferred, null));

    //    //            if (bytesTransferred <= 0)
    //    //            {
    //    //                break;
    //    //            }
    //    //        }
    //    //        catch (SocketException ex)
    //    //        {
    //    //            Console.WriteLine("E >>>>  ReceiveAsyncNew! DCing! <<<< {0}", ex.Message);
    //    //            break;
    //    //        }
    //    //    }
    //    //    this.Receiving = false;
    //    //    socketAsyncEventArgs.Dispose();
    //    //    BufferManager.DefaultManager.CheckIn(array);

    //    //    this.Disconnect();
    //    //}

    //    public void Disconnect()
    //    {
    //        if (!this.disposed)
    //        {
    //            if (this.Socket != null)
    //            {
    //                Disconnecting?.Invoke(this, new DisconnectEventArgs(Socket.LocalEndPoint, Socket.RemoteEndPoint));
    //                this.Socket.Shutdown(SocketShutdown.Both);
    //            }
    //        }
    //    }

    //    private void DataSentCallBack(IAsyncResult res)
    //    {
    //        try
    //        {
    //            int sent = ((Socket)res.AsyncState).EndSend(res);
    //            if (sent < 0)
    //            {
    //                this.Socket.Shutdown(SocketShutdown.Send);
    //                this.Disconnect();
    //            }
    //            else
    //            {
    //                DataEventArgs data = new DataEventArgs(new byte[0], sent, null);
    //                this.DataSent?.Invoke(this, data);
    //            }
    //        }
    //        catch
    //        {
    //            this.Disconnect();
    //        }
    //    }

    //    public bool Send(byte[] buff)
    //    {
    //        return this.Send(buff, 0, buff.Length);
    //    }

    //    public void SendAsync(byte[] buff, int offset, int count)
    //    {
    //        try
    //        {
    //            if (this.Socket != null && this.Socket.Connected)
    //            {
    //                this.Socket.BeginSend(buff, offset, count, SocketFlags.None, new AsyncCallback(this.DataSentCallBack), this.Socket);
    //            }
    //        }
    //        catch
    //        {
    //            Console.WriteLine("E >>>> Error In SendAsync! DCing! <<<<");
    //            this.Disconnect();
    //        }
    //    }

    //    public bool Send(byte[] buff, int offset, int count)
    //    {
    //        bool result;
    //        try
    //        {
    //            if (this.Socket != null && Socket.Connected)
    //            {
    //                if (this.Socket.Send(buff, offset, count, SocketFlags.None) <= 0)
    //                {
    //                    this.Disconnect();
    //                    result = false;
    //                }
    //                else
    //                {
    //                    DataEventArgs data = new DataEventArgs(buff, count, null);
    //                    this.DataSent?.Invoke(this, data);
    //                    result = true;
    //                }
    //            }
    //            else
    //            {
    //                result = false;
    //            }
    //        }
    //        catch (SocketException)
    //        {
    //            Console.WriteLine("E >>>> Error In Send! DCing! <<<<");
    //            this.Disconnect();
    //            result = false;
    //        }
    //        return result;
    //    }

    //    public void Dispose()
    //    {
    //        this.Dispose(true);
    //    }

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (this.disposed)
    //        {
    //            return;
    //        }
    //        if (disposing)
    //        {
    //            this.Socket.Dispose();
    //            this.Socket = null;
    //            this.Disconnecting = null;
    //            this.DataReceived = null;
    //            this.DataSent = null;
    //        }
    //        this.disposed = true;
    //    }

    //    //public async Task<int> ReceiveAsync(byte[] buff, int count)
    //    //{
    //    //    SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
    //    //    socketAsyncEventArgs.SetBuffer(buff, 0, count);
    //    //    SocketAwaitable awaitable = new SocketAwaitable(socketAsyncEventArgs);
    //    //    try
    //    //    {
    //    //        await this.Socket.ReceiveAsync(awaitable);
    //    //        int bytesTransferred = socketAsyncEventArgs.BytesTransferred;
    //    //       // this.DataReceived?.Invoke(this, new DataEventArgs(buff, bytesTransferred, null));
    //    //        return bytesTransferred;
    //    //    }
    //    //    catch (SocketException ex)
    //    //    {
    //    //        Console.WriteLine("E >>>>  ReceiveAsyncNew! DCing! <<<< {0}", ex.Message);
    //    //    }
    //    //    socketAsyncEventArgs.Dispose();
    //    //    return 0;

    //    //}

    //    //public async Task SendAsync(byte[] buff, int count)
    //    //{
    //    //    SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
    //    //    socketAsyncEventArgs.SetBuffer(buff, 0, count);
    //    //    SocketAwaitable awaitable = new SocketAwaitable(socketAsyncEventArgs);
    //    //    try
    //    //    {
    //    //        await this.Socket.SendAsync(awaitable);
    //    //        int bytesTransferred = socketAsyncEventArgs.BytesTransferred;
    //    //        //this.DataReceived?.Invoke(this, new DataEventArgs(buff, bytesTransferred, null));
    //    //    }
    //    //    catch (SocketException ex)
    //    //    {
    //    //        Console.WriteLine("E >>>>  ReceiveAsyncNew! DCing! <<<< {0}", ex.Message);
    //    //    }
    //    //    socketAsyncEventArgs.Dispose();
    //    //}
    //}
}
