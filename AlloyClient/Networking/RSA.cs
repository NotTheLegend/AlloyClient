using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace RealmClient.Networking;

public static class Rsa {
    private const string PublicKey = """
                                     -----BEGIN PUBLIC KEY-----
                                     MFswDQYJKoZIhvcNAQEBBQADSgAwRwJAeyjMOLhcK4o2AnFRhn8vPteUy5Fux/cX
                                     N/J+wT/zYIEUINo02frn+Kyxx0RIXJ3CvaHkwmueVL8ytfqo8Ol/OwIDAQAB
                                     -----END PUBLIC KEY-----
                                     """;

    public static string EncryptPublic(string clearText) {
        var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);
        var encryptEngine = new Pkcs1Encoding(new RsaEngine());

        using (var reader = new StringReader(PublicKey)) {
            var keyParameter = (AsymmetricKeyParameter)new PemReader(reader).ReadObject();
            encryptEngine.Init(true, keyParameter);
        }

        var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
        return encrypted;
    }
}