using System.Net;
using AspNetCoreAuthKit.ApiKey.Models;
using FluentAssertions;
using AspNetCoreAuthKit.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCoreAuthKit.Tests.ApiKey
{
    public class ApiKeyAuthHandlerTests
    {
        private HttpClient CreateClient(Action<ApiKeyOptions> configure)
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(web =>
                {
                    web.UseTestServer();
                    web.ConfigureServices(services =>
                    {
                        services.AddAuthKit(opt => opt.UseApiKey(configure));
                    });
                    web.Configure(app =>
                    {
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.Run(async ctx =>
                        {
                            if (ctx.User.Identity?.IsAuthenticated == true)
                                await ctx.Response.WriteAsync("OK");
                            else
                                ctx.Response.StatusCode = 401;
                        });
                    });
                });

            var host = builder.Start();
            return host.GetTestClient();
        }

        [Fact]
        public async Task Request_WithValidApiKey_ShouldAuthenticate()
        {
            var client = CreateClient(opt =>
            {
                opt.ValidKeys = ["valid-key-123"];
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-Api-Key", "valid-key-123");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Request_WithInvalidApiKey_ShouldReturn401()
        {
            var client = CreateClient(opt =>
            {
                opt.ValidKeys = ["valid-key-123"];
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-Api-Key", "wrong-key");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Request_WithMissingApiKey_ShouldReturn401()
        {
            var client = CreateClient(opt =>
            {
                opt.ValidKeys = ["valid-key-123"];
            });

            var response = await client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Request_WithValidQueryParam_ShouldAuthenticate()
        {
            var client = CreateClient(opt =>
            {
                opt.ValidKeys = ["valid-key-123"];
                opt.QueryParamName = "api_key";
            });

            var response = await client.GetAsync("/?api_key=valid-key-123");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Request_WithCustomValidator_ShouldAuthenticate()
        {
            var client = CreateClient(opt =>
            {
                opt.ValidateKeyAsync = key => Task.FromResult(key == "dynamic-key");
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-Api-Key", "dynamic-key");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
