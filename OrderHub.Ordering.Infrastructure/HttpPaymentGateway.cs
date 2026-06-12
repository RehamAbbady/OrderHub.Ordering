using OrderHub.Ordering.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Ordering.Infrastructure
{
    public sealed class HttpPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _http;

        public HttpPaymentGateway(HttpClient http) => _http = http;

        public async Task<PaymentResult> CreateIntentAsync(decimal amount, string parentEmail, CancellationToken ct)
        {
            var payload = new { amount, email = parentEmail };
            using var response = await _http.PostAsJsonAsync("intents", payload, ct);

            if (!response.IsSuccessStatusCode)
                return new PaymentResult(false, null);

            var intent = await response.Content.ReadFromJsonAsync<PaymentIntentResponse>(cancellationToken: ct);
            return new PaymentResult(true, intent?.Id);
        }

        private sealed record PaymentIntentResponse(string Id);
    }

}
