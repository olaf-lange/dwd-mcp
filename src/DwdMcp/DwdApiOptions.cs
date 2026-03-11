namespace DwdMcp;

public sealed class DwdApiOptions
{
    public const string SectionName = "DwdApi";

    public string StationBaseUrl { get; set; } = "https://app-prod-ws.warnwetter.de/v30";
    public string WarningBaseUrl { get; set; } = "https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16";
    public int TimeoutSeconds { get; set; } = 30;
}
