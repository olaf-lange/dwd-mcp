using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DwdMcp.Tests.Configuration;

public class ConfigurationTests
{
    [Fact]
    public void DefaultOptions_HaveExpectedValues()
    {
        var options = new DwdApiOptions();

        options.StationBaseUrl.Should().Be("https://app-prod-ws.warnwetter.de/v30");
        options.WarningBaseUrl.Should().Be("https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16");
        options.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void CustomBaseUrls_AreRespectedFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DwdApi:StationBaseUrl"] = "https://custom-station.example.com/v30",
                ["DwdApi:WarningBaseUrl"] = "https://custom-warning.example.com/v16",
                ["DwdApi:TimeoutSeconds"] = "60"
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<DwdApiOptions>(config.GetSection(DwdApiOptions.SectionName));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<DwdApiOptions>>().Value;

        options.StationBaseUrl.Should().Be("https://custom-station.example.com/v30");
        options.WarningBaseUrl.Should().Be("https://custom-warning.example.com/v16");
        options.TimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void MissingConfiguration_UsesDefaults()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.Configure<DwdApiOptions>(config.GetSection(DwdApiOptions.SectionName));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<DwdApiOptions>>().Value;

        options.StationBaseUrl.Should().Be("https://app-prod-ws.warnwetter.de/v30");
        options.WarningBaseUrl.Should().Be("https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16");
        options.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void SectionName_IsDwdApi()
    {
        DwdApiOptions.SectionName.Should().Be("DwdApi");
    }
}
