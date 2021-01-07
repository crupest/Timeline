using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Timeline.Swagger
{
    public static class OpenApiServiceCollectionExtensions
    {
        public static void AddOpenApiDocs(this IServiceCollection services)
        {
            services.AddSwaggerDocument(document =>
            {
                document.DocumentName = "Timeline";
                document.Title = "Timeline REST API Reference";
                document.Version = typeof(Startup).Assembly.GetName().Version?.ToString() ?? "unknown version";
                document.DocumentProcessors.Add(new DocumentDescriptionDocumentProcessor());
                document.DocumentProcessors.Add(
                    new SecurityDefinitionAppender("JWT",
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Create token via `/api/token/create` ."
                    }));
                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
                document.OperationProcessors.Add(new DefaultDescriptionOperationProcessor());
                document.OperationProcessors.Add(new ByteDataRequestOperationProcessor());
            });
        }
    }
}
