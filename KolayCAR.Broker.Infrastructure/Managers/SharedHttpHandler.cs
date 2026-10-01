using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;

namespace KolayCAR.Broker.Infrastructure.Managers
{
    /// <summary>
    /// Tedarikçi çağrıları için process genelinde tek bağlantı havuzu.
    /// Her istekte yeni handler yaratmak TIME_WAIT birikmesine ve ephemeral port tükenmesine yol açıyordu.
    /// AppSettings:UseHttpPooling false yapılırsa eski (handler-per-instance) davranışa dönülür.
    /// </summary>
    public static class SharedHttpHandler
    {
        private static bool _isLocked;

        internal static bool PoolingEnabled { get; private set; }

        public static void Initialize(bool useHttpPooling)
        {
            if (_isLocked)
                return;

            PoolingEnabled = useHttpPooling;
            _isLocked = true;
        }

        private static readonly SocketsHttpHandler SocketsHandler = new()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 64,
            // Handler paylaşıldığı için otomatik çerez kavanozu da paylaşılırdı; tedarikçiler arası oturum sızmasını önler.
            // Yolcu360/Sixt2 gibi sağlayıcılar Cookie header'ını zaten elle yönetiyor.
            UseCookies = false,
            SslOptions = new SslClientAuthenticationOptions
            {
                // TODO: Sertifika doğrulaması bypass ediliyor (mevcut davranış korundu). Host bazlı allow-list ile değiştirilmeli.
                RemoteCertificateValidationCallback = static (_, _, _, _) => true
            }
        };

        /// <summary>
        /// HttpClient örneği başına ayrı kalır çünkü çağrı noktaları DefaultRequestHeaders'ı değiştiriyor;
        /// bağlantı havuzunu tutan handler ise paylaşılır.
        /// </summary>
        internal static HttpClient CreateRestManagerClient(Uri baseUri)
            => PoolingEnabled
                ? CreatePooledClient(baseUri)
                : CreateLegacyClient(bypassCertificate: true, SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13, baseUri);

        internal static HttpClient CreateHttpManagerClient(Uri baseUri)
            => PoolingEnabled
                ? CreatePooledClient(baseUri)
                : CreateLegacyClient(bypassCertificate: false, SecurityProtocolType.Tls12, baseUri);

        /// <summary>
        /// RestSharp çağrıları loglamayı kendisi yaptığı için log handler'ı olmadan paylaşılır.
        /// </summary>
        internal static readonly HttpClient RestSharpClient = new(SocketsHandler, disposeHandler: false);

        private static HttpClient CreatePooledClient(Uri baseUri)
            => new(new VendorLogHttpMessageHandler(SocketsHandler, baseUri), disposeHandler: false);

        private static HttpClient CreateLegacyClient(bool bypassCertificate, SecurityProtocolType securityProtocol, Uri baseUri)
        {
            ServicePointManager.SecurityProtocol = securityProtocol;

            var handler = new HttpClientHandler();
            if (bypassCertificate)
                handler.ServerCertificateCustomValidationCallback = static (_, _, _, _) => true;

            return new HttpClient(new VendorLogHttpMessageHandler(handler, baseUri));
        }
    }
}
