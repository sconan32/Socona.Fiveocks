using Socona.ToolBox.Tools;
using System.Threading;

namespace Socona.Fiveocks.Tools
{
    public class NetworkStats
    {
        readonly BandwidthCounter upCounter;
        readonly BandwidthCounter downCounter;
        public NetworkStats()
        {
            upCounter = new BandwidthCounter();
            downCounter = new BandwidthCounter();
        }
        public void AddClient()
        {
            Interlocked.Increment(ref totalClients);
            Interlocked.Increment(ref clientsSinceRun);
        }

        public void DecreaseClient()
        {
            Interlocked.Decrement(ref totalClients);
        }
        public void ResetClients(int count)
        {
            TotalClients = count;
        }

        public void AddBytes(int bytes, ByteType type)
        {
            if (type != ByteType.Sent)
            {
                DownCounter.AddBytes(bytes);
                return;
            }
            UpCounter.AddBytes(bytes);
        }

        public void AddPacket(PacketType pkt)
        {
            if (pkt != PacketType.Sent)
                PacketsReceived++;
            else
                PacketsSent++;
        }

        private int totalClients;
        public int TotalClients
        {
            get { return totalClients; }
            private set { totalClients = value; }
        }

        private int clientsSinceRun;
        public int ClientsSinceRun
        {
            get { return clientsSinceRun; }
            private set { clientsSinceRun = value; }
        }


        public ulong PacketsSent { get; private set; }
        public ulong PacketsReceived { get; private set; }
        //per sec.
        public string BytesReceivedPerSec
        {
            get { return DownCounter.GetPerSecondString(); }
        }
        public string BytesSentPerSec
        {
            get { return UpCounter.GetPerSecondString(); }
        }
        public string TotalSent
        { get { return UpCounter.ToString(); } }

        public string TotalReceived
        { get { return DownCounter.ToString(); } }

        public BandwidthCounter UpCounter => upCounter;

        public BandwidthCounter DownCounter => downCounter;
    }
    public enum PacketType
    {
        Sent,
        Received
    }
    public enum ByteType
    {
        Sent,
        Received
    }
}
