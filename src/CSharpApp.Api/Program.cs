using System.Runtime.CompilerServices;
using Autofac.Core;
using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Logging.ClearProviders().AddSerilog(logger);

builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddTransient<LoggingMessageHandler>();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration();

builder.Services.AddHttpConfiguration();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();
builder.Services.AddMemoryCache();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//app.UseHttpsRedirection();

var versionedEndpointRouteBuilder = app.NewVersionedApi();

#region products
var endpointsGroup = versionedEndpointRouteBuilder.MapGroup("api/v{version:apiVersion}");

endpointsGroup
    .HasApiVersion(1.0);


endpointsGroup.MapGet("getproducts", async (IProductsService productsService) =>
{
    var products = await productsService.GetProducts();
    return products;
})
    .WithName("GetProducts");

endpointsGroup.MapGet("product/{id}", async ([FromRoute] int id, IProductsService productsService) =>
{
    var product = await productsService.GetProduct(id);
    return product;
})
.WithName("GetProductById");

endpointsGroup.MapPost("createproduct", async ([FromBody] Product product, IProductsService productsService) =>
{
    var createdProduct = await productsService.CreateProduct(product);
    return createdProduct;
}
)
.WithName("CreateProduct");
#endregion
#region categories
endpointsGroup.MapGet("getcategories", async (ICategoriesService categoriesService) =>
{
    var categories = await categoriesService.GetCategories();
    return categories;
})
    .WithName("GetCategories");

endpointsGroup.MapGet("category/{id}", async ([FromRoute] int id, ICategoriesService categoriesService) =>
{
    var category = await categoriesService.GetCategory(id);
    return category;
})
.WithName("GetCategoryById");

endpointsGroup.MapPost("createcategory", async ([FromBody] Category category, ICategoriesService categoriesService) =>
{
    var createdCategory = await categoriesService.CreateCategory(category);
    return createdCategory;
}
)
.WithName("CreateCategory");
#endregion

app.Run();