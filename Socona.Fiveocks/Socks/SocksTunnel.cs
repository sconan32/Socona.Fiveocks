using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Sockets.SocketTaskExtensions;

namespace Socona.Fiveocks.Socks
{
    //internal class SocksTunnel : IDisposable
    //{
    //    public SocksRequest Req;

    //    public SocksRequest ModifiedReq;

    //    public SocksInboundEntry Client;

    //    public Socket RemoteSocket;

    //    private List<DataHandler> Plugins = new List<DataHandler>();

    //    private int Timeout = 10000;

    //    private int PacketSize = 4096;

    //    private bool isDisposing;

    //    public event EventHandler<DataEventArgs> UpStreamDataTransfered;
    //    public event EventHandler<DataEventArgs> DownStreamDataTransfered;
    //    public event EventHandler TunnelDisconnecting;

    //    public SocksTunnel(SocksInboundEntry p, SocksRequest req, SocksRequest req1, int packetSize, int timeout)
    //    {
    //        this.Client = p;
    //        this.Req = req;
    //        this.ModifiedReq = req1;
    //        this.PacketSize = packetSize;
    //        this.Timeout = timeout;
    //    }

    //    public async Task OpenAsync(CancellationToken cancellationToken)
    //    {
    //        if (this.ModifiedReq.Address == null)
    //        {
    //            this.Client.Socket.Close();
    //            return;
    //        }
    //        Console.WriteLine("+ {0}:{1}({2})", this.ModifiedReq.Address, this.ModifiedReq.Port, this.ModifiedReq.IPAddress ?? IPAddress.Any);
    //        using (List<object>.Enumerator enumerator = PluginLoader.LoadPlugin(typeof(ConnectSocketOverrideHandler)).GetEnumerator())
    //        {
    //            while (enumerator.MoveNext())
    //            {
    //                ConnectSocketOverrideHandler conn = (ConnectSocketOverrideHandler)enumerator.Current;
    //                if (conn.Enabled)
    //                {
    //                    if (await conn.OnConnectOverrideAsync(Client ,this.ModifiedReq,cancellationToken))
    //                    {
    //                        return;
    //                    }
    //                    //if (pm != null)
    //                    //{
    //                    //    try
    //                    //    {
    //                    //        Console.WriteLine("Proxy: {0}:{1}", this.ModifiedReq.Address, this.ModifiedReq.Port);
    //                    //        this.RemoteSocket = pm;
    //                    //        byte[] shit = this.Req.GetData(true);
    //                    //        shit[1] = 0;
    //                    //        this.Client.SocketClient.Send(shit);
    //                    //        this.ConnectHandler(null);
    //                    //        return;
    //                    //    }
    //                    //    catch (Exception)
    //                    //    {
    //                    //    }
    //                    //}
    //                }
    //            }
    //        }
            
    //        this.RemoteSocket = new Socket( SocketType.Stream, ProtocolType.Tcp);           
    //        await TransferAsync(cancellationToken);
    //        TunnelDisconnecting?.Invoke(this, EventArgs.Empty);
    //    }

      
      
    //    private async Task TransferAsync(CancellationToken cancellationToken)
    //    {

    //        //using (List<object>.Enumerator enumerator = PluginLoader.LoadPlugin(typeof(DataHandler)).GetEnumerator())
    //        //{
    //        //    while (enumerator.MoveNext())
    //        //    {
    //        //        DataHandler data = (DataHandler)enumerator.Current;
    //        //        this.Plugins.Push(data);
    //        //    }
    //        //}

    //        var localSocket = this.Client.Socket;
    //        var remoteSocket = this.RemoteSocket;

    //        await remoteSocket.ConnectAsync(new IPEndPoint(this.ModifiedReq.IPAddress, this.ModifiedReq.Port), cancellationToken);
    //        byte[] request = this.Req.GetData(true);
    //        request[1] = 0;
    //        localSocket.Send(request, SocketFlags.None);

    //        using var remoteMemoryOwner = MemoryPool<byte>.Shared.Rent();
    //        using var localMemoryOwner = MemoryPool<byte>.Shared.Rent();
    //        var remoteMemory = remoteMemoryOwner.Memory;
    //        var localMemory = localMemoryOwner.Memory;
           

    //        try
    //        {
    //            var downloadingTask = Task.Run(async () =>
    //            {

    //                var remoteCount = await remoteSocket.ReceiveAsync(remoteMemory, SocketFlags.None, cancellationToken);
    //                while (remoteCount > 0)
    //                {
    //                    DownStreamDataTransfered?.Invoke(null, new DataEventArgs(null, remoteCount));
    //                    await localSocket.SendAsync(remoteMemory.Slice(0, remoteCount), SocketFlags.None, cancellationToken);                        
    //                    remoteCount = await remoteSocket.ReceiveAsync(remoteMemory, SocketFlags.None, cancellationToken);
    //                }

    //            }, cancellationToken);

    //            var uploadingTask = Task.Run(async () =>
    //            {
    //                var localCount = await localSocket.ReceiveAsync(localMemory, SocketFlags.None, cancellationToken);
    //                while (localCount > 0)
    //                {
    //                    UpStreamDataTransfered?.Invoke(null, new DataEventArgs(null, localCount));
    //                    await remoteSocket.SendAsync(localMemory.Slice(0, localCount), SocketFlags.None, cancellationToken);
    //                    localCount = await localSocket.ReceiveAsync(localMemory, SocketFlags.None, cancellationToken);
    //                }
    //            }, cancellationToken);
    //            await Task.WhenAll(uploadingTask, downloadingTask);
    //            remoteSocket.Close();
    //            localSocket.Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //            Console.WriteLine(ex.StackTrace);
    //        }        
    //    }

     

    //    public void Dispose()
    //    {
    //        if (this.isDisposing)
    //        {
    //            return;
    //        }
    //        this.isDisposing = true;
    //        this.RemoteSocket?.Close();
    //        this.RemoteSocket = null;
    //        this.Client = null;
    //        this.ModifiedReq = null;
    //        this.Req = null;
    //    }
    //}
}
