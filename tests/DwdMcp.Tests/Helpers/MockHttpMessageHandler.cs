using System.Net;
using System.Text;

namespace DwdMcp.Tests.Helpers;

public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
    private readonly List<HttpRequestMessage> _requests = [];

    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    public MockHttpMessageHandler(HttpStatusCode statusCode, string content, string contentType = "application/json")
        : this((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, contentType)
        }))
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);
        return await _handler(request, cancellationToken);
    }

    public static MockHttpMessageHandler WithJsonFile(string testDataFileName, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = TestDataLoader.Load(testDataFileName);
        return new MockHttpMessageHandler(statusCode, json);
    }
}
