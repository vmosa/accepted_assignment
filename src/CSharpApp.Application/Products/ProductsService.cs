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

        var response = await _httpClient.GetAsync("");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        try
        {
            var res = JsonSerializer.Deserialize<List<Product>>(content);

            return res.AsReadOnly();
        }
        catch(Exception e)
        {
            
            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }
        
    }
    
    public async Task<Product> GetProduct(int id)
    {
                
        var newUri = string.Format("/{0}", id);
        var response = await _httpClient.GetAsync(newUri);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

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

    public async Task<string> CreateProduct(Product? product)
    {
      
        var jsonContent = new StringContent(JsonSerializer.Serialize(product));
        var response = await _httpClient.PostAsync("",jsonContent);
        //response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        //var res = JsonSerializer.Deserialize<Product>(content);

        return content;
    }
}