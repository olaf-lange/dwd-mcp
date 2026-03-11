using System.Net;
using DwdMcp.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace DwdMcp.Tests.Resilience;

public class ResilienceTests
{
    [Fact]
    public async Task TransientFailure_TriggersRetry()
    {
        var callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            callCount++;
            if (callCount <= 2)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        });

        var services = new ServiceCollection();
        services.AddHttpClient("test", c => c.BaseAddress = new Uri("https://example.com/"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(10);
                options.Retry.UseJitter = false;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("test");

        var response = await client.GetAsync("test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().BeGreaterThan(1, "retries should have occurred");
    }

    [Fact]
    public async Task NonTransientFailure_IsNotRetried()
    {
        var callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad request")
            });
        });

        var services = new ServiceCollection();
        services.AddHttpClient("test", c => c.BaseAddress = new Uri("https://example.com/"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(10);
                options.Retry.UseJitter = false;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("test");

        var response = await client.GetAsync("test");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        callCount.Should().Be(1, "4xx should not trigger retries");
    }

    [Fact]
    public async Task CircuitBreaker_OpensAfterRepeatedFailures()
    {
        var callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        });

        var services = new ServiceCollection();
        services.AddHttpClient("test", c => c.BaseAddress = new Uri("https://example.com/"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 1;
                options.Retry.Delay = TimeSpan.FromMilliseconds(1);
                options.Retry.UseJitter = false;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.1;
                options.CircuitBreaker.MinimumThroughput = 2;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
            });

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("test");

        // Make several requests to trigger circuit breaker
        for (var i = 0; i < 5; i++)
        {
            try { await client.GetAsync("test"); } catch { }
        }

        // Circuit breaker should now be open — next call should fail fast
        var act = () => client.GetAsync("test");

        try
        {
            await act();
        }
        catch
        {
            // expected
        }

        // We expect the handler to have been called fewer times than total requests * (retries + 1)
        // because circuit breaker should prevent some calls
        callCount.Should().BeLessThan(12, "circuit breaker should prevent some requests from reaching the handler");
    }

    [Fact]
    public async Task Timeout_TriggersWhenRequestTakesTooLong()
    {
        var handler = new MockHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var services = new ServiceCollection();
        services.AddHttpClient("test", c => c.BaseAddress = new Uri("https://example.com/"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 1;
                options.Retry.Delay = TimeSpan.FromMilliseconds(1);
                options.Retry.UseJitter = false;
                options.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(100);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMilliseconds(500);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            });

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("test");

        var act = () => client.GetAsync("test");

        await act.Should().ThrowAsync<Exception>();
    }
}
