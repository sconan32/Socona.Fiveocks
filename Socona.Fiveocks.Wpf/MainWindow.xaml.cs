using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.SocksProtocol;
using Socona.Fiveocks.SocksServer;
using Socona.Fiveocks.TCP;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Timer = System.Timers.Timer;
using System.Buffers;
using Socona.Fiveocks.HttpProtocol;

namespace Socona.Fiveocks
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool testing = false;
        private int port;
        private Socks5Server x;
        private Timer timer;
        private TextWriter normalOutput;



        private string _username = "socona";
        private string _password = "socona32";


        private long timetickcnt;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {



            port = 10084;
            IPAddress[] localIps = Dns.GetHostAddresses(Dns.GetHostName());
            StringBuilder ipBuilder = new StringBuilder();
            foreach (var ip in localIps)
            {
                ipBuilder.Append(ip.ToString());
                ipBuilder.Append("; ");
            }
            txtIpAddr.Text = ipBuilder.ToString();
            txtPort.Text = port.ToString();


            Task.Run(() =>
            {
                DoLogLine("Starting Server...");
                x = new Socks5Server(IPAddress.Any, port);
                x.Start();
                DoLogLine("OK");


                TestServer("localhost", 10084, "www.baidu.com", 80);
                timer = new Timer(500);
                timer.Elapsed += timer_Elapsed;
                timer.Start();
            });

        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timetickcnt++;
            Task.Run(() =>
            {
                if (timetickcnt % 60 == 0)
                {
                    // TestServer();
                }
                if (timetickcnt % 1200 == 0)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        txtLog.Clear();
                    });
                }
                if (timetickcnt % 2 == 0)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        txtAvailableBuffer.Text = BufferManager.DefaultManager.AvailableBuffers.ToString();
                        if (x != null)
                        {
                            txtDownSpeed.Text = x.Stats.BytesReceivedPerSec;
                            txtUpSpeed.Text = x.Stats.BytesSentPerSec;

                            txtSumRecv.Text = x.Stats.TotalReceived;

                            txtSumSend.Text = x.Stats.TotalSent;
                            txtClients.Text = x.Stats.TotalClients.ToString();
                            TimeSpan ts = new TimeSpan(0, 0, (int)(timetickcnt / 2));
                            txtOnTime.Text = ts.ToString();
                        }
                    });
                }


            });
        }

        async void TestServer(string socks5Server, int sock5ServerPort, string target, int targetPort)
        {

            if (testing == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtStatus.Text = "TEST";
                    txtStatus.Background = new SolidColorBrush(Color.FromRgb(237, 236, 24));
                });
                try
                {
                    testing = true;
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


        private void DoLogLine(string text)
        {
            DoLog($"{text}{Environment.NewLine}");
        }
        private void DoLog(string text)
        {

            var logContent = $"{DateTime.Now:s} - {text}";
            Debug.Write(logContent);
            this.Dispatcher.Invoke(() =>
            {
                txtLog.Text += logContent;
                txtLog.ScrollToEnd();
            });
        }




        //void p_OnConnected(object sender, Socks5ClientArgs e)
        //{
        //    if (e.Status == SockStatus.Granted)
        //    {
        //        e.Client.DataReceived += Client_OnDataReceived;
        //       // e.Client.SocketDisconnected += Client_OnDisconnected;
        //        // M = Encoding.ASCII.GetBytes("Start Sending Data:\n");
        //        //e.Client.Send(m, 0, m.Length);
        //        //e.Client.ReceiveAsync(new SocketAwaitable(new SocketAsyncEventArgs()));
        //    }
        //    else
        //    {
        //        DoLog(string.Format("Failed to connect: {0}.", e.Status.ToString()));
        //    }
        //}

        //void Client_OnDisconnected(object sender, Socks5ClientArgs e)
        //{
        //    //disconnected.
        //    DoLog("Disconnected");
        //}

        //void Client_OnDataReceived(object sender, Socks5ClientDataArgs e)
        //{
        //    DoLog(string.Format("Received {0} bytes from server.", e.Count));
        //    // e.Client.Send(e.Buffer, 0, e.Count);
        //    //e.Client.ReceiveAsync();
        //}

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (normalOutput != null)
            {
                Console.SetOut(normalOutput);
            }
            if (timer != null)
            {
                timer.Stop();
            }
            Task.Run(() =>
            {
                if (x != null)
                {
                    x.Stop();
                }

            });


        }

        private void txtLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtLog.Text.Count(t => t == '\n') > 400)
            {

            }
        }

        private void btnResetSrv_Click(object sender, RoutedEventArgs e)
        {

            var thread = new Thread(() =>
            {
                if (x != null)
                {

                    DoLog("SERVER STOPPING!");
                    try
                    {
                        x.Stop();
                    }
                    catch (Exception ex)
                    {
                        DoLog(ex.Message);
                    }
                    x = null;

                    DoLog("\tSERVER STOPPED!");
                }
                DoLog("SERVER RESTARTING!");
                x = new Socks5Server(IPAddress.Any, port);
                x.Start();
                PluginLoader.ChangePluginStatus(false, typeof(DataHandlerExample));
                    //enable plugin.
                    foreach (object pl in PluginLoader.GetPlugins)
                {
                        //if (pl.GetType() == typeof(LoginHandlerExample))
                        //{
                        //    //enable it.
                        //    PluginLoader.ChangePluginStatus(true, pl.GetType());
                        //    Console.WriteLine("Enabled {0}.", pl.GetType().ToString());
                        //}
                    }

                DoLog("\tSERVER RESTARTED!");
                TestServer("localhost", 10084, "www.baidu.com", 80);
            });
            thread.Start();

        }

        private void btnStopSrv_Click(object sender, RoutedEventArgs e)
        {
            if (x != null)
            {
                var thread = new Thread(() =>
                {
                    DoLog("SERVER STOPPING!");
                    x.Stop();
                    x = null;
                    DoLog("\tSERVER STOPPED!");
                });
                thread.Start();

            }
        }

        private void btnTestForwarding_Click(object sender, RoutedEventArgs e)
        {
            TestServer("127.0.0.1", 1089, "www.youtube.com", 443);
        }
    }
}
