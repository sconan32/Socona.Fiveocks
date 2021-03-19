using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.TCP
{
    class HttpProxyClient
    {
        public event EventHandler<DisconnectEventArgs> Disconnecting;

        public event EventHandler<DataEventArgs> DataReceived = delegate { };
        public event EventHandler<DataEventArgs> DataSent = delegate { };

        // public Socket Sock { get; set; }
        // private byte[] buffer;
       // private int packetSize = 4096;
        public bool Receiving { get; set; }
        private Uri uri;
        private Uri proxyUri;

        public HttpProxyClient(SocksRequest request, Uri proxyAddr)
        {
            //start the data exchange.
            //  Sock = sock;
            //  ClientDisconnecting = delegate { };
            //  buffer = BufferManager.DefaultManager.CheckOut();
            // packetSize = PacketSize;

            uri = new Uri(request.Address + request.Port.ToString());

            proxyUri = proxyAddr;
        }

        public Socket Socket
        {
            get { throw new NotSupportedException(); }
        }

        public int Receive(byte[] data, int offset, int count)
        {
            throw new NotSupportedException();
        }



        public async Task ReceiveAsyncNew()
        {
            Receiving = true;
            // Reusable SocketAsyncEventArgs and awaitable wrapper 

            try
            {
                // Do processing, continually receiving from the socket 
                WebRequest hwr = WebRequest.CreateHttp(uri);
                hwr.Proxy = new WebProxy(proxyUri);
                using WebResponse response = await hwr.GetResponseAsync();
                var stream = response.GetResponseStream();
                while (true)
                {

                    var buffer = BufferManager.DefaultManager.CheckOut();
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                        if (bytesRead <= 0)
                            break;

                        DataEventArgs data = new DataEventArgs(buffer, bytesRead);
                        this.DataReceived(this, data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        BufferManager.DefaultManager.CheckIn(buffer);
                        stream.Close();
                    }
                }

            }
            catch (SocketException )
            {

            }
            finally
            {
                Receiving = false;

                Disconnect();
            }

        }
        public void Disconnect()
        {
            if (!this.disposed)
            {
                Disconnecting(this, null);
                this.Dispose();
            }
        }


        public bool Send(byte[] buff)
        {
            throw new NotSupportedException();
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public bool Send(byte[] buff, int offset, int count)
        {

            throw new NotSupportedException();
        }
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
                // Sock = null;
                //  BufferManager.DefaultManager.CheckIn(buffer);
                //  buffer = null;
                Disconnecting = null;
                DataReceived = null;
                DataSent = null;
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
        }

        public Task<int> ReceiveAsync(byte[] buff, int count)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(byte[] buff, int count)
        {
            throw new NotImplementedException();
        }
    }
}
