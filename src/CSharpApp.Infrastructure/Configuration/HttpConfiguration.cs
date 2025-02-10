using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        //var baseUrl = this.Configuration.GetSection("RestApiSettings")?.GetValue(typeof(string), "BaseUrl") as string;
        
        services.AddHttpClient<IProductsService, ProductsService>()
        .ConfigureHttpClient((sp, client) =>
        {
            var restApiSettings = sp.GetRequiredService<IOptions<RestApiSettings>>().Value;
            var productsUri = string.Format("{0}{1}", restApiSettings.BaseUrl!, restApiSettings.Products!);
            client.BaseAddress = new Uri(productsUri);
        });
        services.AddHttpClient<ICategoriesService, CategoriesService>()
        .ConfigureHttpClient((sp, client) =>
        {
            var restApiSettings = sp.GetRequiredService<IOptions<RestApiSettings>>().Value;

            var categoriesUri = string.Format("{0}{1}", restApiSettings.BaseUrl!, restApiSettings.Categories!.StartsWith("/")? restApiSettings.Categories!.Substring(1): restApiSettings.Categories!);
            client.BaseAddress = new Uri(categoriesUri);
        });
        
        return services;
    }
}