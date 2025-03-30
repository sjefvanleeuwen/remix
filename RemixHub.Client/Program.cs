using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RemixHub.Client;
using RemixHub.Client.Services;
using RemixHub.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Server API URL - read from configuration
var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5001";

// Register the authorization message handler first
builder.Services.AddScoped<AuthorizationMessageHandler>();

// Configure HttpClient with authorization handler
builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri(apiUrl)
    };
    return httpClient;
});

// Add authentication services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// Add application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IInstrumentTypeService, InstrumentTypeService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

await builder.Build().RunAsync();
