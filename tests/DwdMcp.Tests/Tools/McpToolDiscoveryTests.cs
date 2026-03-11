using System.ComponentModel;
using System.Reflection;
using DwdMcp.Tools;
using FluentAssertions;
using ModelContextProtocol.Server;

namespace DwdMcp.Tests.Tools;

public class McpToolDiscoveryTests
{
    private static readonly string[] ExpectedToolNames =
    [
        "GetStationOverview",
        "GetCrowdWeatherReports",
        "GetNowcastWarnings",
        "GetNowcastWarningsEnglish",
        "GetMunicipalityWarnings",
        "GetMunicipalityWarningsEnglish",
        "GetCoastalWarnings",
        "GetCoastalWarningsEnglish",
        "GetSeaWarningText",
        "GetAlpineWeatherForecast",
        "GetAvalancheWarnings"
    ];

    private static IEnumerable<(string Name, MethodInfo Method)> GetAllToolMethods()
    {
        var toolTypes = new[] { typeof(StationTools), typeof(WarningTools), typeof(ForecastTools) };

        foreach (var type in toolTypes)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = method.GetCustomAttribute<McpServerToolAttribute>();
                if (attr != null)
                    yield return (attr.Name ?? method.Name, method);
            }
        }
    }

    [Fact]
    public void AllExpectedTools_AreDiscoverable()
    {
        var discoveredNames = GetAllToolMethods().Select(t => t.Name).ToList();

        discoveredNames.Should().BeEquivalentTo(ExpectedToolNames);
    }

    [Theory]
    [MemberData(nameof(AllToolMethods))]
    public void EachTool_HasDescription(string toolName, MethodInfo method)
    {
        var description = method.GetCustomAttribute<DescriptionAttribute>()
            ?? method.DeclaringType?.GetMethod(method.Name)?.GetCustomAttribute<DescriptionAttribute>();

        description.Should().NotBeNull($"Tool '{toolName}' should have a [Description] attribute");
        description!.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetStationOverview_StationIdsParameter_HasDescription()
    {
        var method = typeof(StationTools).GetMethod(nameof(StationTools.GetStationOverview))!;
        var param = method.GetParameters().First(p => p.Name == "stationIds");
        var desc = param.GetCustomAttribute<DescriptionAttribute>();

        desc.Should().NotBeNull();
        desc!.Description.Should().NotBeNullOrWhiteSpace();
    }

    public static IEnumerable<object[]> AllToolMethods()
    {
        foreach (var (name, method) in GetAllToolMethods())
            yield return [name, method];
    }
}
