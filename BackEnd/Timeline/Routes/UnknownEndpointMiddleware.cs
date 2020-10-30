using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Mime;
using System.Text.Json;
using Timeline.Models.Http;

namespace Timeline.Routes
{
    public static class UnknownEndpointMiddleware
    {
        public static void Attach(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.GetEndpoint() != null)
                {
                    await next();
                    return;
                }

                if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = MediaTypeNames.Application.Json;

                    var body = JsonSerializer.SerializeToUtf8Bytes(ErrorResponse.Common.UnknownEndpoint(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    context.Response.ContentLength = body.Length;
                    await context.Response.Body.WriteAsync(body);
                    await context.Response.CompleteAsync();
                    return;
                }

                await next();
            });
        }
    }
}
