using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace RemixHub.Client.Auth
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;

        public JwtAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsStringAsync("authToken");
                
                Console.WriteLine($"Retrieved token from storage, length: {token?.Length ?? 0}");
                
                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("No auth token found in local storage");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Validate token format - JWT must have 3 segments separated by dots
                if (!token.Contains('.') || token.Count(c => c == '.') != 2)
                {
                    Console.WriteLine($"Invalid JWT token format: missing segments. Token: {token}");
                    await _localStorage.RemoveItemAsync("authToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                try
                {
                    var claims = ParseClaimsFromJwt(token);
                    
                    // Log the claims for debugging
                    foreach (var claim in claims)
                    {
                        Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                    }
                    
                    var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                    
                    if (expClaim != null)
                    {
                        var exp = long.Parse(expClaim.Value);
                        var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                        
                        if (expDate <= DateTime.UtcNow)
                        {
                            Console.WriteLine("Auth token has expired");
                            await _localStorage.RemoveItemAsync("authToken");
                            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                        }
                        
                        Console.WriteLine($"Token valid until: {expDate}");
                    }
                    
                    Console.WriteLine("Valid auth token found - user is authenticated");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing JWT token: {ex.Message}");
                    await _localStorage.RemoveItemAsync("authToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAuthenticationStateAsync: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyUserLogout();
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            
            try
            {
                // Check for valid JWT format
                var segments = jwt.Split('.');
                if (segments.Length != 3)
                {
                    Console.WriteLine("Invalid JWT format: incorrect number of segments");
                    return claims;
                }
                
                var payload = segments[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs != null)
                {
                    keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

                    if (roles != null)
                    {
                        if (roles.ToString().Trim().StartsWith("["))
                        {
                            var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                            if (parsedRoles != null)
                            {
                                foreach (var parsedRole in parsedRoles)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                                }
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                        }

                        keyValuePairs.Remove(ClaimTypes.Role);
                    }

                    claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JWT claims: {ex.Message}");
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
