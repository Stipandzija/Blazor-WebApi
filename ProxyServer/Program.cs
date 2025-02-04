using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowProxy", policy =>
    {
        policy.WithOrigins("https://localhost:7051")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowProxy");

app.Use(async (context, next) =>
{
    try
    {
        var targetServer = "https://localhost:7061";
        var targetUri = new Uri(targetServer + context.Request.Path + context.Request.QueryString);

        using var client = new HttpClient();
        var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

        foreach (var header in context.Request.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        if (context.Request.ContentLength > 0)
        {
            requestMessage.Content = new StreamContent(context.Request.Body);
        }

        var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

        context.Response.StatusCode = (int)response.StatusCode;

        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        await response.Content.CopyToAsync(context.Response.Body);

        await next(context);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        Console.WriteLine($"Error forwarding request: {ex.Message}");
    }
});


app.Run();