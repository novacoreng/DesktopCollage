using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DesktopCollage.Companion;

/// <summary>Creates a persistent self-signed transport identity for future pinned TLS clients.</summary>
internal static class TransportIdentity
{
    public static X509Certificate2 LoadOrCreate()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCollage", "companion");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "transport.pfx");
        if (File.Exists(path)) return X509CertificateLoader.LoadPkcs12FromFile(path, null,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

        using var key = RSA.Create(3072);
        var request = new CertificateRequest("CN=DesktopCollage Companion", key,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName("localhost");
        san.AddIpAddress(System.Net.IPAddress.Loopback);
        request.CertificateExtensions.Add(san.Build());
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddYears(2));
        var bytes = certificate.Export(X509ContentType.Pfx);
        // CreateNew prevents silently replacing a concurrent identity.
        using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            output.Write(bytes);
        }
        return X509CertificateLoader.LoadPkcs12(bytes, null,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
    }

    public static string Fingerprint(X509Certificate2 certificate) =>
        Convert.ToHexString(SHA256.HashData(certificate.RawData));
}
