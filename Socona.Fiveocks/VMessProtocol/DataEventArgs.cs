using System;
using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.TCP
{
    public class DataEventArgs : EventArgs
    {
     //   public Client Client { get; set; }
        public byte[] Buffer { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
        public SocksRequest Request { get; set; }
        public DataEventArgs(byte[] buffer, int count, SocksRequest req = null)
        {
            //Client = Client;
            Buffer = buffer;
            Count = count;
            Offset = 0;
            Request = req;
        }
    }
}
