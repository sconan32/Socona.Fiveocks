using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Encryption
{
    public struct EncryptionMethod
    {
        public string Method { get; set; }

        private EncryptionMethod(string method)
        {
            Method = method;
        }

        public static EncryptionMethod Rc4Md5 { get; } = new EncryptionMethod("rc4-md5");
        public static EncryptionMethod Aes128Gcm { get; } = new EncryptionMethod("aes-128-gcm");
        public static EncryptionMethod Aes192Gcm { get; } = new EncryptionMethod("aes-192-gcm");
        public static EncryptionMethod Aes256Gcm { get; } = new EncryptionMethod("aes-256-gcm");
        public static EncryptionMethod Aes128Cfb { get; } = new EncryptionMethod("aes-128-cfb");
        public static EncryptionMethod Aes192Cfb { get; } = new EncryptionMethod("aes-192-cfb");
        public static EncryptionMethod Aes256Cfb { get; } = new EncryptionMethod("aes-256-cfb");
        public static EncryptionMethod Aes128Ctr { get; } = new EncryptionMethod("aes-128-ctr");
        public static EncryptionMethod Aes192Ctr { get; } = new EncryptionMethod("aes-192-ctr");
        public static EncryptionMethod Aes256Ctr { get; } = new EncryptionMethod("aes-256-ctr");
        public static EncryptionMethod Camellia128Cfb { get; } = new EncryptionMethod("camellia-128-cfb");
        public static EncryptionMethod Camellia192Cfb { get; } = new EncryptionMethod("camellia-192-cfb");
        public static EncryptionMethod Camellia256Cfb { get; } = new EncryptionMethod("camellia-256-cfb");
        public static EncryptionMethod BfCfb { get; } = new EncryptionMethod("bf-cfb");
        public static EncryptionMethod Chacha20IetfPoly1305 { get; } = new EncryptionMethod("chacha20-ietf-poly1305");
        public static EncryptionMethod XChacha20IetfPoly1305 { get; } = new EncryptionMethod("xchacha20-ietf-poly1305");
        public static EncryptionMethod Salsa20 { get; } = new EncryptionMethod("salsa20");
        public static EncryptionMethod Chacha20 { get; } = new EncryptionMethod("chacha20");
        public static EncryptionMethod Chacha20Ietf { get; } = new EncryptionMethod("chacha20-ietf");
    }
}
