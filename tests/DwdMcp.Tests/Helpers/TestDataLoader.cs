namespace DwdMcp.Tests.Helpers;

public static class TestDataLoader
{
    public static string Load(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        return File.ReadAllText(path);
    }
}
