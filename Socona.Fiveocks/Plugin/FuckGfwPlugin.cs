using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.SocksProtocol;
using Socona.Fiveocks.TCP;
using Socona.ToolBox.Text;

namespace Socona.Fiveocks.Plugin
{
    public class FuckGfwPlugin
    {
        private static string listUrl = "https://autoproxy-gfwlist.googlecode.com/svn/trunk/gfwlist.txt";

        private List<string> patternList = null;
        private PatternCollection patterns;

        private bool isEnabled = true;


        private string proxyUriStr = "127.0.0.1:1089";
        private string _proxyAddress = "127.0.0.1";
        private int _proxyPort = 1089;
        private string _proxyUserName = null;
        private string _proxyPassword = null;
        private bool DownloadGfwList()
        {
            try
            {
                var stream = File.OpenWrite("gfwlist.txt");
                var request = HttpWebRequest.CreateHttp(listUrl);
                using var streamreader = new StreamReader(request.GetResponse().GetResponseStream());
                var data = streamreader.ReadToEnd();
                var rawdata = Encoding.UTF8.GetBytes(data);
                var newrawdata = new FromBase64Transform().TransformFinalBlock(rawdata, 0, rawdata.Length);
                var newdata = Encoding.UTF8.GetString(newrawdata);
                using StreamWriter sw = new StreamWriter(stream);
                sw.Write(newdata);
                sw.Close();
                return true;
            }
            catch (Exception )
            {

            }
            return false;

        }

        public bool ShallUseProxy(SocksRequest request)
        {
            var addr = $"{request.Address}$";
            if (patterns == null)
            {
                patternList = new List<string>(100);
                LoadPatternList();
                patterns = new PatternCollection(patternList);
            }

            if (patterns.IsMatchAny(addr))
            {
                return true;
            }


            return false;
        }

        private bool LoadPatternList()
        {
            try
            {
                if (!File.Exists("gfwlist.txt"))
                {
                    DownloadGfwList();
                }
                using var stream = File.OpenRead("gfwlist.txt");
                using StreamReader sr = new StreamReader(stream);

                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    string regex = null;
                    if (string.IsNullOrWhiteSpace(s) || s[0] == '!' || s[0] == '[' || s.StartsWith("@@"))
                    {
                        continue;
                    }
                    else if (s[0] == '/' && s[^1] == '/')
                    {
                        regex = s[1..];
                    }
                    else
                    {
                        //s = s.Replace("*", ".+");
                        //s = s.Replace("(", @"\(");
                        //s = s.Replace(")", @"\)");
                        if (s.StartsWith("||"))
                        {
                            // regex = @"^https?:\/\/" + s[2..] + ".*";
                            regex = $"{s[2..]}$";
                        }
                        else if (s.StartsWith("|"))
                        {
                            //regex = @"^" + s[1..] + ".*";
                            regex = $"^{s[8..]}$";
                        }
                        else if (s[^1] == '|')
                        {
                            //regex = ".*" + s[0..] + "$";
                            regex = $"{s}$";
                        }
                        else
                        {
                            //regex = ".*" + s + ".*";
                            regex = $"{s}$";
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

        public bool Enabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public string Name { get => nameof(FuckGfwPlugin); set => throw new NotImplementedException(); }




    }
}
