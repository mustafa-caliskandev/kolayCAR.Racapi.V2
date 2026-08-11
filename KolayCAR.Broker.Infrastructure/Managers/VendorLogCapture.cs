using KolayCAR.Broker.Domain.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Infrastructure.Managers
{
    public sealed class VendorLogCaptureScope : IDisposable
    {
        private readonly VendorLogCaptureScope _parent;
        private bool _disposed;

        internal VendorLogCaptureScope(VendorLogCaptureScope parent, int vendorId, string vendorName, string vendorType)
        {
            _parent = parent;
            Log = new VendorAvailabilityLog { VendorId = vendorId, VendorName = vendorName, VendorType = vendorType };
        }

        public VendorAvailabilityLog Log { get; }

        internal void Add(VendorHttpLogEntry entry)
        {
            lock (Log.Entries)
            {
                Log.Entries.Add(entry);
                Log.Success = Log.Entries.Count > 0 && Log.Entries.TrueForAll(x => x.Success);
            }
        }

        public void AddException(Exception exception)
        {
            if (exception != null)
                Add(new VendorHttpLogEntry { Success = false, ExceptionMessage = exception.GetBaseException().Message });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            VendorLogCapture.Restore(_parent);
        }
    }

    public static class VendorLogCapture
    {
        private static readonly AsyncLocal<VendorLogCaptureScope> CurrentScope = new AsyncLocal<VendorLogCaptureScope>();

        public static VendorLogCaptureScope Begin(int vendorId, string vendorName, string vendorType)
        {
            var scope = new VendorLogCaptureScope(CurrentScope.Value, vendorId, vendorName, vendorType);
            CurrentScope.Value = scope;
            return scope;
        }

        internal static void Restore(VendorLogCaptureScope scope) => CurrentScope.Value = scope;
        internal static void Add(VendorHttpLogEntry entry) => CurrentScope.Value?.Add(entry);

        public static void RecordResponse(string responseContent, int? httpStatusCode = 200, string httpMethod = "POST")
        {
            Add(new VendorHttpLogEntry
            {
                Success = httpStatusCode.HasValue && httpStatusCode.Value >= 200 && httpStatusCode.Value <= 299,
                HttpStatusCode = httpStatusCode,
                HttpMethod = httpMethod,
                ResponseContent = responseContent
            });
        }

        public static void RecordException(Exception exception, string responseContent = null, string httpMethod = "POST")
        {
            if (exception == null)
                return;

            Add(new VendorHttpLogEntry
            {
                Success = false,
                HttpMethod = httpMethod,
                ResponseContent = responseContent,
                ExceptionMessage = exception.GetBaseException().Message
            });
        }
    }

    internal sealed class VendorLogHttpMessageHandler : DelegatingHandler
    {
        public VendorLogHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                var responseContent = response.Content == null ? null : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                VendorLogCapture.Add(new VendorHttpLogEntry
                {
                    Success = response.IsSuccessStatusCode,
                    HttpStatusCode = (int)response.StatusCode,
                    HttpMethod = request.Method.Method,
                    ResponseContent = responseContent
                });
                return response;
            }
            catch (Exception ex)
            {
                VendorLogCapture.Add(new VendorHttpLogEntry
                {
                    Success = false,
                    HttpMethod = request.Method.Method,
                    ExceptionMessage = ex.GetBaseException().Message
                });
                throw;
            }
        }
    }
}
