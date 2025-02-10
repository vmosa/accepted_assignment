using System.Net.Http.Headers;
using System.Text;

namespace CSharpApp.Application.Categories;

public class CategoriesService : ICategoriesService
{
    private readonly HttpClient _httpClient;
    private readonly RestApiSettings _restApiSettings;
    private readonly ILogger<CategoriesService> _logger;

    public CategoriesService(IOptions<RestApiSettings> restApiSettings, HttpClient httpClient, ILogger<CategoriesService> logger)
    {
        _httpClient = httpClient;
        _restApiSettings = restApiSettings.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Category>> GetCategories()
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
            var res = JsonSerializer.Deserialize<List<Category>>(content);
            return res.AsReadOnly();
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }
    }
    public async Task<Category> CreateCategory(Category? category)
    {

        string? content = null;
        try
        {

            var json = JsonSerializer.Serialize(category);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("", data);
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
            var res = JsonSerializer.Deserialize<Category>(content);
            return res;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }

    }
    public async Task<Category> GetCategory(int id)
    {

        string? content = null;
        try {

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
            var res = JsonSerializer.Deserialize<Category>(content);
            return res;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("OriginalExceptionMessage: {0} \n Unable to deserialize the result: \n {1}", e.Message, content), e.InnerException);
        }
    }
}