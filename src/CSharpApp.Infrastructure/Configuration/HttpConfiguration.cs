using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IAccessTokenStorage, AccessTokenStorage>();
        services.AddHttpClient<IProductsService, ProductsService>()
        .ConfigureHttpClient((sp, client) =>
        {
            var restApiSettings = sp.GetRequiredService<IOptions<RestApiSettings>>().Value;
            var productsUri = string.Format("{0}{1}", restApiSettings.BaseUrl!, restApiSettings.Products!);
            client.BaseAddress = new Uri(productsUri);
        })
        .AddHttpMessageHandler<AuthorizationMessageHandler>()
        .AddHttpMessageHandler<LoggingMessageHandler>();
        services.AddHttpClient<ICategoriesService, CategoriesService>()
        .ConfigureHttpClient((sp, client) =>
        {
            var restApiSettings = sp.GetRequiredService<IOptions<RestApiSettings>>().Value;

            var categoriesUri = string.Format("{0}{1}", restApiSettings.BaseUrl!, restApiSettings.Categories!.StartsWith("/") ? restApiSettings.Categories!.Substring(1) : restApiSettings.Categories!);
            client.BaseAddress = new Uri(categoriesUri);
        })
        .AddHttpMessageHandler<AuthorizationMessageHandler>()
        .AddHttpMessageHandler<LoggingMessageHandler>();

        return services;
    }
}

public interface IAccessTokenStorage
{
    void Add(Uri uri, string scheme, string token);
    string Get(Uri uri, string scheme);
}

public class AccessTokenStorage : IAccessTokenStorage
{
    IList<Tuple<Uri, string, string>> _tokens;
    public AccessTokenStorage()
    {
        _tokens = new List<Tuple<Uri, string, string>>();
    }
    public void Add(Uri uri, string scheme, string token)
    {
        _tokens.Add(new Tuple<Uri, string, string>(uri, scheme, token));
    }

    public string Get(Uri uri, string scheme)
    {
        return _tokens.FirstOrDefault(t => t.Item1 == uri && t.Item2 == scheme)?.Item3??string.Empty;
    }
}


public class AuthorizationMessageHandler: DelegatingHandler
{
    HttpClient _authHttpClient;
    private readonly RestApiSettings _restApiSettings;
    private readonly IAccessTokenStorage _accessTokenStorage;
    public AuthorizationMessageHandler(IOptions<RestApiSettings> restApiSettings, IAccessTokenStorage accessTokenStorage)
    {
        _restApiSettings = restApiSettings.Value;
        _authHttpClient = new HttpClient() { BaseAddress = new Uri(restApiSettings.Value.BaseUrl!) };
        _accessTokenStorage = accessTokenStorage;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = GetToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return base.SendAsync(request, cancellationToken);
    }

    public string GetToken()
    {
        if(_accessTokenStorage == null || string.IsNullOrEmpty(_accessTokenStorage.Get(_authHttpClient.BaseAddress!, "Bearer")))
        {
            var authUri = string.Format("{0}{1}", _restApiSettings.BaseUrl!, _restApiSettings.Auth!.StartsWith("/") ? _restApiSettings.Auth!.Substring(1) : _restApiSettings.Auth!);
            var response = _authHttpClient
            .PostAsync(authUri, new StringContent(
            JsonSerializer.Serialize(
            new { password = _restApiSettings.Password, email = _restApiSettings.Username }
            ),
            Encoding.UTF8,
            "application/json"))
                .Result;

            var token = response.Content.ReadAsStringAsync().Result;
            var bearerToken = JsonSerializer.Deserialize<BearerTokenHelper>(token);
            var accessToken = bearerToken?.AccessToken??string.Empty;
            _accessTokenStorage!.Add(_authHttpClient.BaseAddress!, "Bearer", accessToken);
            return accessToken;
        }
        return _accessTokenStorage.Get(_authHttpClient.BaseAddress!, "Bearer");
    }

}

public sealed class BearerTokenHelper
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}


public class  LoggingMessageHandler: DelegatingHandler
{
    public readonly ILogger<LoggingMessageHandler> _logger;
    public LoggingMessageHandler(ILogger<LoggingMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request: {0}", request);
        _logger.LogInformation("Request Headers: {0}", request.Headers);
        
        return base.SendAsync(request, cancellationToken);
    }


}