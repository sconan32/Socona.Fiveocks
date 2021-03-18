using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Plugin
{
    class ClientConnectHandlerExample : ClientConnectedHandler
    {

        private static readonly string listUrl = "https://autoproxy-gfwlist.googlecode.com/svn/trunk/gfwlist.txt";

        private  List<string> patternList = null;


        public ClientConnectHandlerExample()
        {

        }
        public override bool OnConnect(Socket socketClient, System.Net.IPEndPoint IP)
        {


            if (IP.Address.ToString() != "127.0.0.1")
                //deny the connection.
                return false;
            return true;
            //With this function you can also Modify the Socket, as it's stored in e.Client.Sock.
        }

        private bool DownloadGfwList()
        {
            try
            {
                var stream = File.OpenWrite("list.txt");
                var request = HttpWebRequest.CreateHttp(listUrl);
                var streamreader = new StreamReader(request.GetResponse().GetResponseStream());
                var data = streamreader.ReadToEnd();
                var rawdata = Encoding.UTF8.GetBytes(data);
                var newrawdata = new FromBase64Transform().TransformFinalBlock(rawdata, 0, rawdata.Length);
                var newdata = Encoding.UTF8.GetString(newrawdata);
                StreamWriter sw = new StreamWriter(stream);
                sw.Write(newdata);
                sw.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;

        }

        private bool ShallUseProxy(string addr)
        {
            if (patternList == null)
            {
                LoadPatternList();
            }
            foreach (var r in patternList)
            {
                Regex regex = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (regex.IsMatch(addr))
                {
                    return true;
                }
            }

            return false;
        }

        private bool LoadPatternList()
        {
            try
            {
                if (!File.Exists("list.txt"))
                {
                    DownloadGfwList();
                }
                var stream = File.OpenRead("list.txt");
                StreamReader sr = new StreamReader(stream);

                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    string regex = null;
                    if (s[0] == '!' || s[0] == '[' || s.StartsWith("@@"))
                    {
                        continue;
                    }
                    else if (s[0] == '/' && s[^1] == '/')
                    {
                        regex = s[1..];
                    }
                    else
                    {
                        s = s.Replace("*", ".+");
                        s = s.Replace("(", @"\(");
                        s = s.Replace(")", @"\)");
                        if (s.StartsWith("||"))
                        {
                            regex = @"^https?:\/\/" + s[2..] + ".*";
                        }
                        else if (s.StartsWith("|"))
                        {
                            regex = @"^" + s[1..] + ".*";
                        }
                        else if (s[^1] == '|')
                        {
                            regex = ".*" + s[0..^1] + "$";
                        }
                        else
                        {
                            regex = ".*" + s + ".*";
                        }
                    }
                    if (regex != null)
                    {
                        patternList.Add(regex);
                    }
                }
                sr.Close();
                return true;
            }
            catch (Exception )
            { }
            return false;
        }
        private bool enabled = false;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public override string Name { get => nameof(ClientConnectedHandler); set => throw new NotImplementedException(); }
    }
}
