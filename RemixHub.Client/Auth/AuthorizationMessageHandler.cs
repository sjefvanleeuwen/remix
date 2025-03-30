using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace RemixHub.Client.Auth
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthorizationMessageHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get token from local storage
            var token = await _localStorage.GetItemAsStringAsync("authToken");

            // Validate token format
            if (!string.IsNullOrEmpty(token) && token.Contains('.') && token.Count(c => c == '.') == 2)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"Added valid auth token to request: {request.RequestUri}");
            }
            else if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"Invalid token format detected for request: {request.RequestUri}");
                await _localStorage.RemoveItemAsync("authToken");
            }
            else
            {
                Console.WriteLine($"No auth token available for request: {request.RequestUri}");
            }

            try
            {
                // Continue with the request
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in request: {ex.Message}");
                throw;
            }
        }
    }
}
