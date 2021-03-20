using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    class SocksPackageParser
    {
        public static SocksPackageParser Shared { get; } = new SocksPackageParser();
        private SocksPackageParser() { }

        public SocksUser ParseUserLoginRequest(Memory<byte> memory, int length)
        {
            // .------.------------------.-----------.------------------.-----------.
            // | VER  | USERNAME_LENGTH  |  USERNAME |  PASSWORD_LNEGTH | PASSWORD  |
            // :------+------------------+-----------+------------------+-----------:
            // | 0x01 |      1Byte       |  Variable |     1Byte        | Variable  |
            // '------'------------------'-----------'------------------'-----------'

            if (length <= 0 || memory.Span[0] != 0x01)
            {
                return null;
            }
            int numusername = memory.Span[1];
            int numpassword = memory.Span[numusername + 2];

            string username = Encoding.ASCII.GetString(memory.Slice(2, numusername).Span);
            string password = Encoding.ASCII.GetString(memory.Slice(numusername + 3, numpassword).Span);
            return new SocksUser(username, password);
        }

        public SocksStatus ParseHandShakeResponse(Memory<byte> memory, int length)
        {
            if (length > 0 && memory.Span[0] == (byte)SocksVersions.Socks5)
            {
                return (SocksStatus)memory.Span[1];
            }
            return SocksStatus.Unreachable;
        }

        public SocksAuthencation ParseAuthencatingResponse(Memory<byte> memory, int length)
        {
            //+-------+----------+
            //|  VER  |  METHOD  | 
            //+-------+----------+
            //| 1Byte |   1Byte  | 
            //+-------+----------+
            //Ver Socks4=0x01 Sock5=0x05     

            //check for server version.
            if (length > 0 && memory.Span[0] == 0x05)
            {
                return (SocksAuthencation)memory.Span[1];
            }
            return SocksAuthencation.Unsupported;
        }

        public SocksUserLoginResult ParseUserLoginResponse(Memory<byte> memory, int length)
        {
            //.-------.----------.
            //|  VER  |  STATUS  |
            //:-------+----------:
            //| 0x01  |  1Byte   |
            //'-------'--------- '
            // STATUS 0x00 = Succeed 0x01 = Denied

            if (length > 0 && memory.Span[0] == 0x01)
            {
                return (SocksUserLoginResult)memory.Span[1];
            }
            return SocksUserLoginResult.Denied;
        }
    }
}
