using System;

namespace Socona.Fiveocks.SocksProtocol
{

    public enum SocksAuthencation
    {
        None = 0x00,            //NO AUTHENTICATION REQUIRED 不需要认证       
        GSSAPI = 0x01,          //GSSAPI, 类似SSH的认证协议   (不支持）
        Login = 0x02,           //USERNAME/PASSWORD 用户名密码认证
                                //0x03-0x7F  IANA ASSIGNED 协会保留方法
                                //0x80-0xFE  自定义方法
        SocksCompress = 0x88,   //
        SocksEncrypt = 0x90,
        SocksBoth = 0xFE,
        Unsupported = 0xFF,     // NO ACCEPTABLE METHODS 没有可接受的方法

    }

    public enum SocksVersions
    {
        Socks5 = 0x05,
        Socks4 = 0x04,           //Unsupported
        Socks = 0x01,
        Zero = 0x00
    }

    public enum SocksCommand
    {
        TcpStream = 0x01,
        TcpBind = 0x02,
        UDPPort = 0x03,
                            //Extended
        TorResolve = 0xF0,
        TorResolvePtr = 0xF1
    }

    public enum SocksUserLoginResult
    {
        Succeed = 0x00,
        Denied = 0x01,
    }

    public enum SocksAddressType
    {
        IP = 0x01,
        Domain = 0x03,
        IPv6 = 0x04
    }

    public enum SocksStatus
    {
        Granted = 0x00,
        Failure = 0x01,
        NotAllowed = 0x02,
        Unreachable = 0x03,
        HostUnreachable = 0x04,
        Refused = 0x05,
        Expired = 0x06,
        NotSupported = 0x07,
        AddressNotSupported = 0x08,
        LoginRequired = 0x90
    }
}
