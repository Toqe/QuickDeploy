using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace QuickDeploy.Common.CertificateGeneration
{
    public class CertificateGenerator
    {
        // https://svrooij.nl/2018/04/generate-x509certificate2-in-csharp/
        public X509Certificate2 GenerateCertificate(string subject)
        {
            var random = new SecureRandom();
            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            certificateGenerator.SetIssuerDN(new X509Name($"C=DE, O=QuickDeploy, CN={subject}"));
            certificateGenerator.SetSubjectDN(new X509Name($"C=DE, O=QuickDeploy, CN={subject}"));
            certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
            certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));

            const int strength = 2048;
            var keyGenerationParameters = new KeyGenerationParameters(random, strength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            var issuerKeyPair = subjectKeyPair;
            const string signatureAlgorithm = "SHA256WithRSA";
            var signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private);
            var bouncyCert = certificateGenerator.Generate(signatureFactory);

            // Lets convert it to X509Certificate2
            X509Certificate2 certificate;

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.SetKeyEntry($"{subject}_key", new AsymmetricKeyEntry(subjectKeyPair.Private), new[] { new X509CertificateEntry(bouncyCert) });
            string exportpw = Guid.NewGuid().ToString("x");

            using (var ms = new System.IO.MemoryStream())
            {
                store.Save(ms, exportpw.ToCharArray(), random);
                certificate = new X509Certificate2(ms.ToArray(), exportpw, X509KeyStorageFlags.Exportable);
            }

            return certificate;
        }

        public SerializedCertificate SerializeToPfx(X509Certificate2 certificate, string password)
        {
            byte[] privateExport = certificate.Export(X509ContentType.Pfx, password);
            byte[] exportedWithoutPrivateKey = certificate.Export(X509ContentType.Cert, password);
            certificate = new X509Certificate2(exportedWithoutPrivateKey, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            byte[] publicExport = certificate.Export(X509ContentType.Pfx, password);
            return new SerializedCertificate {Private = privateExport, Public = publicExport};
        }

        public class SerializedCertificate
        {
            public byte[] Private { get; set; }

            public byte[] Public { get; set; }
        }
    }
}
