using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DesktopCollage;

/// <summary>HTTPS client pinned to the local companion certificate.</summary>
internal static class CompanionHttpClient
{
    public static HttpClient Create()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCollage", "companion", "transport.pfx");
        if (!File.Exists(path))
            throw new FileNotFoundException("Start the DesktopCollage companion service before pairing.", path);
        using var certificate = X509CertificateLoader.LoadPkcs12FromFile(path, null,
            X509KeyStorageFlags.EphemeralKeySet);
        var expected = SHA256.HashData(certificate.RawData);
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, actual, _, errors) =>
            {
                if (actual is null) return false;
                var fingerprint = SHA256.HashData(actual.RawData);
                return CryptographicOperations.FixedTimeEquals(expected, fingerprint);
            }
        };
        return new HttpClient(handler) { BaseAddress = new Uri("https://127.0.0.1:47832") };
    }
}
