using System.Net.Http.Headers;
using System.Text;

namespace CSharpApp.Application.Products;

public class ProductsService : IProductsService
{
    private readonly HttpClient _httpClient;
    private readonly RestApiSettings _restApiSettings;
    private readonly ILogger<ProductsService> _logger;

    public ProductsService(IOptions<RestApiSettings> restApiSettings,
        ILogger<ProductsService> logger, HttpClient httpClient
        )
    {
        _httpClient = httpClient;
        _restApiSettings = restApiSettings.Value;
        _logger = logger;

    }

    public async Task<IReadOnlyCollection<Product>> GetProducts()
    {
        string? content = null;
        try
        {
            var response = await _httpClient.GetAsync("");
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("Response content could not be read properly"));
        }


        try
        {
            var res = JsonSerializer.Deserialize<IList<Product>>(content);

            return res.AsReadOnly();
        }
        catch(Exception e)
        {
            
            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }
        
    }
    
    public async Task<Product> GetProduct(int id)
    {
        string? content = null;
        try
        {
            var newUri = string.Format("/{0}", id);
            var response = await _httpClient.GetAsync(newUri);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("Response content could not be read properly"));
        }

        try
        {
            var res = JsonSerializer.Deserialize<Product>(content);

            return res;
        }
        catch (Exception e)
        {

            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }

       
    }

    public async Task<Product> CreateProduct(Product? product)
    {

        string? content = null;
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(product));
            var response = await _httpClient.PostAsync("", jsonContent);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("Response content could not be read properly"));
        }

        try
        {
            var res = JsonSerializer.Deserialize<Product>(content);

            return res;
        }
        catch (Exception e)
        {

            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }

    }
}