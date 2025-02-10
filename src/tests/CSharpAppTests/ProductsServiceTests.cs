
using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace CSharpAppTests
{
    public class ProductsServiceTests
    {
        private readonly Mock<IOptions<RestApiSettings>> mockRestApiSettings;
        private Mock<HttpClient> mockHttpClient;
        private readonly Mock<ILogger<ProductsService>> mockLogger;
        public ProductsServiceTests()
        {
            mockRestApiSettings = new Mock<IOptions<RestApiSettings>>();
            mockRestApiSettings.Setup(x => x.Value).Returns(new RestApiSettings
            {
                BaseUrl = "http://localhost:5000/api/products",
                Products = "products",
                Categories = "categories",
                Auth = "auth",
                Username = "admin",
                Password = "admin"
            });
            mockLogger = new Mock<ILogger<ProductsService>>();
            


        }
        [Fact]
        public async void GetProducts_ReturnsProducts()
        {
            // Arrange
            var results = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1"},
                new Product { Id = 2, Title = "Product 2" }
            };
            var resultsJson = JsonSerializer.Serialize(results);

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(resultsJson)
                });
            mockHttpClient = new Mock<HttpClient>(handler.Object);


            mockHttpClient.Object.BaseAddress = new Uri("http://localhost:5000/api/products");

            var mockProductsService = new ProductsService(mockRestApiSettings.Object, mockLogger.Object, mockHttpClient.Object);
            var products = await mockProductsService.GetProducts();


            // Assert
            
            Assert.Equal(1, products.First().Id);
            Assert.Equal("Product 1", products.First().Title);

        }
        [Fact]
        public async void GetProduct_ReturnsProduct()
        {
            // Arrange
            var results = new Product { Id = 1, Title = "Product 1" };
               
            var resultsJson = JsonSerializer.Serialize(results);
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(resultsJson)
                });
            mockHttpClient = new Mock<HttpClient>(handler.Object);
            mockHttpClient.Object.BaseAddress = new Uri("http://localhost:5000/api/products");

            var mockProductsService = new ProductsService(mockRestApiSettings.Object, mockLogger.Object, mockHttpClient.Object);
            var product = await mockProductsService.GetProduct(1);
            // Assert
            Assert.Equal(1, product.Id);
            Assert.Equal("Product 1", product.Title);
        }
        [Fact]
        public async void GetProducts_ThrowsException()
        {

            // Arrange
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"id\":1,\"title\":\"Product 1\", \"price\": 999999999999999999999999999999},{\"id\":2,\"title\":\"Product 2\"}]")
                });

            mockHttpClient = new Mock<HttpClient>(handler.Object);
            mockHttpClient.Object.BaseAddress = new Uri("http://localhost:5000/api/products");
            var mockProductsService = new ProductsService(mockRestApiSettings.Object, mockLogger.Object, mockHttpClient.Object);
            // Act
            var exception = await Assert.ThrowsAnyAsync<Exception>(() => mockProductsService.GetProducts());
            // Assert
            Assert.Contains("OriginalExceptionMessage:", exception.Message);
        }
    }
}