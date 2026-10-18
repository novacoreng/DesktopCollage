using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class CompanionTls
{
    internal static HttpClient Create()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCollage", "companion", "transport.pfx");
        using var cert = X509CertificateLoader.LoadPkcs12FromFile(path, null,
            X509KeyStorageFlags.EphemeralKeySet);
        var expected = SHA256.HashData(cert.RawData);
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, actual, _, _) =>
                actual is not null && CryptographicOperations.FixedTimeEquals(
                    expected, SHA256.HashData(actual.RawData))
        };
        return new HttpClient(handler) { BaseAddress = new Uri("https://127.0.0.1:47832") };
    }
}
