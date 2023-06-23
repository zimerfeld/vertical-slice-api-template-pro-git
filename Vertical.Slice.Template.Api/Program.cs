using System.Reflection;
using Vertical.Slice.Template;
using Vertical.Slice.Template.Shared;
using Vertical.Slice.Template.Shared.Core.Extensions.ServiceCollectionsExtensions;
using Vertical.Slice.Template.Shared.Swagger;
using Vertical.Slice.Template.Shared.Web.Minimal.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Host.UseDefaultServiceProvider(
    (context, options) =>
    {
        var isDevMode =
            context.HostingEnvironment.IsDevelopment()
            || context.HostingEnvironment.IsEnvironment("test")
            || context.HostingEnvironment.IsStaging();

        options.ValidateScopes = isDevMode;

        // validate dependencies on the startup immediately instead of waiting for using the service
        options.ValidateOnBuild = isDevMode;
    }
);

builder.AddCatalogsServices();

var app = builder.Build();

app.Logger.LogInformation("Starting Api at { DT}", DateTime.UtcNow.ToLongTimeString());

if (app.Environment.IsDevelopment() && app.Environment.IsEnvironment("test"))
{
    app.Services.ValidateDependencies(
        builder.Services,
        typeof(CatalogsMetadata).Assembly,
        Assembly.GetExecutingAssembly()
    );
}

await app.UseCatalogs();

app.MapCatalogsEndpoints();

app.MapModulesEndpoints();

// #if EnableSwagger
if (app.Environment.IsDevelopment())
{
    // should register as last middleware for discovering all endpoints and its versions correctly
    app.UseCustomSwagger();
}

// #endif
await app.RunAsync();
