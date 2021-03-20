using Socona.Fiveocks.HttpProtocol;
using Socona.Fiveocks.SocksProtocol;
using Socona.Fiveocks.TCP;
using Socona.Fiveocks.Tools;
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Socona Fiveocks Server");
            Console.WriteLine("Ver. 1.0");

            var port = 10084;
            IPAddress[] localIps = Dns.GetHostAddresses(Dns.GetHostName());
            StringBuilder ipBuilder = new StringBuilder();
            foreach (var ip in localIps)
            {
                ipBuilder.Append(ip.ToString());
                ipBuilder.Append("; ");
            }
            Console.WriteLine($"Listen Address: {ipBuilder}");
            Console.WriteLine($"Listen Port: {port}");


            Console.WriteLine("Starting Server...");
            var x = new Socks5Server(IPAddress.Any, port);
            x.Start();
            Console.WriteLine("OK");


            //   TestServer("localhost", 10084, "www.baidu.com", 80);

            await Task.Run(async () =>
            {
                while (true)
                {
                    ShowStats(x.Stats);
                    // TestServer("127.0.0.1", 10084, "www.baidu.com", 443);
                    await Task.Delay(60000);
                }
            });
        }


        public static async void ShowStats(NetworkStats stats)
        {
            Console.WriteLine();
            Console.WriteLine($"I @{DateTime.Now:G}  #Clients: {stats.TotalClients}  #OutBytes: {stats.TotalSent}  #InBytes: {stats.TotalReceived}");
        }

        public static async void TestServer(string socks5Server, int sock5ServerPort, string target, int targetPort)
        {
            try
            {

                Socks5OutboundEntry p = new Socks5OutboundEntry(socks5Server, sock5ServerPort, target, targetPort);
                // p.OnConnected += p_OnConnected;
                if (await p.ConnectAsync())
                {
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent();
                    var memory = memoryOwner.Memory;
                    var request = new BinaryHttpRequest().BuildGetRequest("www.baidu.com", 443);
                    int length = request.TryGetBytes(memory);
                    await p.SendAsync(memory.Slice(0, length));


                    var recv = await p.ReceiveAsync(memory);
                    if (recv <= 0)
                    {
                        Console.WriteLine("E ====        CHK NET        ====");
                        return;
                    }
                    using FileStream fileStream = File.OpenWrite($"T{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt");

                    fileStream.Write(memory.Slice(0, recv).Span);

                    while (recv > 0)
                    {
                        recv = await p.ReceiveAsync(memory);
                        fileStream.Write(memory.Slice(0, recv).Span);
                    }
                    fileStream.Close();

                    Console.WriteLine("i ====       TEST OK       ====");
                    p.Dispose();
                }
                else
                {
                    Console.WriteLine("E ====        CHK NET        ====");
                }
                //p.Send(new byte[] {0x11, 0x21});

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {

            }
        }
    }

}
