using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.HttpProtocol
{
    public class BinaryHttpRequest
    {
        static readonly string startLinePattern = "{0} / HTTP/1.1\r\n";
        static readonly string hostPattern = "Host: {0}:{1}\r\n";
        static readonly string acceptsPattern = "Accept: {0}\r\n";

        static readonly string userAgentPattern = "User-Agent: {0}\r\n";

        static readonly string connection = "Connection: keep-alive\r\n";
        static readonly string secure = "Upgrade-Insecure-Requests: 1\r\n";
        static readonly string acceptEncoding = "Accept-Encoding: gzip, deflate, br\r\n";
        static readonly string acceptLanguage = "Accept-Language: en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7\r\n";

        static readonly string defaultAccept = "text/html,application/xhtml+xml,application/xml";
        static readonly string defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4435.0 Safari/537.36 Edg/91.0.825.0";

        List<string> _headerLines = new List<string>();
        string _startLine;

        string _request;
        public int TryGetBytes(Memory<byte> memory)
        {
            int length = Encoding.ASCII.GetByteCount(_request);
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment))
            {
                Encoding.ASCII.GetBytes(_request, 0, _request.Length, segment.Array, segment.Offset);
                return length;
            }
            return 0;
        }

        public BinaryHttpRequest Method(HttpMethod httpMethod)
        {
            _startLine = string.Format(startLinePattern, httpMethod.Method);
            return this;
        }

        public BinaryHttpRequest Host(string ipOrDomain, int port)
        {
            _headerLines.Add(string.Format(hostPattern, ipOrDomain, port));
            return this;
        }
        public BinaryHttpRequest Accepts(string accept) // Set the "Accept" header.
        {

            _headerLines.Add(string.Format(acceptsPattern, accept));
            return this;
        }

        public BinaryHttpRequest UserAgent(string userAgent)
        {
            _headerLines.Add(string.Format(userAgentPattern, userAgent));
            return this;
        }

        public BinaryHttpRequest BuildGetRequest(string ipOrDomain, int port)
        {
            return this.Method(HttpMethod.Get)
                .Host(ipOrDomain, port)
                .Accepts(defaultAccept)
                .UserAgent(defaultUserAgent)
                .BuildRequest();
        }

        public BinaryHttpRequest BuildRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_startLine);
            foreach (var header in _headerLines)
            {
                sb.Append(header);
            }
          
            sb.Append(acceptEncoding);
            sb.Append(acceptLanguage);

            sb.Append(connection);
            sb.Append(secure);
            sb.Append("\r\n");
            _request = sb.ToString();
            return this;
        }
    }
}
