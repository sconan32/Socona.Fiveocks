using Socona.Fiveocks.SocksServer;
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
            Console.WriteLine("Hello World!");

            var port = 10084;
            IPAddress[] localIps = Dns.GetHostAddresses(Dns.GetHostName());
            StringBuilder ipBuilder = new StringBuilder();
            foreach (var ip in localIps)
            {
                ipBuilder.Append(ip.ToString());
                ipBuilder.Append("; ");
            }
            Console.WriteLine(ipBuilder.ToString());
            Console.WriteLine(port.ToString());


            Console.WriteLine("Starting Server...");
            var x = new Socks5Server(IPAddress.Any, port);
            x.Start();
            Console.WriteLine("OK");


            //   TestServer("localhost", 10084, "www.baidu.com", 80);

            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(5000);
                    TestServer("210.30.97.227", 10089, "www.baidu.com", 443);
                }
            });
        }


        public static async void TestServer(string socks5Server, int sock5ServerPort, string target, int targetPort)
        {
            try
            {

                Socks5Client.Socks5Client p = new Socks5Client.Socks5Client(socks5Server, sock5ServerPort, target, targetPort);
                // p.OnConnected += p_OnConnected;
                if ( await p.ConnectAsync())
                {
                     
                    using var memoryOwner = MemoryPool<byte>.Shared.Rent();
                    var memory = memoryOwner.Memory;
                    var recv = await p.ReceiveAsync(memory);
                    using FileStream fileStream = File.OpenWrite($"T{DateTime.Now:s}.txt");
                    using StreamWriter sw = new StreamWriter(fileStream);
                    sw.Write(memory.Slice(0, recv));

                    while(recv>0)
                    {
                        recv = await p.ReceiveAsync(memory);
                        sw.Write(memory.Slice(0, recv));
                    }
                    sw.Close();

                    Console.WriteLine("i ====       TEST OK       ====");                  
                    p.Disconnect();
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
