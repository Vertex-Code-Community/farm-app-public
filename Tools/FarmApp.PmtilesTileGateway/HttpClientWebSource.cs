using System.Net.Http.Headers;
using PMTiles;
using PMTiles.Sources;

namespace FarmApp.PmtilesTileGateway;

/// <summary>
/// Same behavior as PMTiles <see cref="WebSource"/>, but uses an injected <see cref="HttpClient"/>
/// so TLS (e.g. custom validation, SNI) can be configured — the stock <c>WebSource</c> uses a default
/// <see cref="HttpClient"/> and can hit <c>RemoteCertificateNameMismatch</c> with some CDNs / redirects.
/// </summary>
internal sealed class HttpClientWebSource : Source, IDisposable
{
    private readonly HttpClient _client;
    private readonly bool _disposeClient;
    private readonly string _url;

    public HttpClientWebSource(string url, HttpClient client, bool disposeClient = true)
    {
        _url = url;
        _client = client;
        _disposeClient = disposeClient;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _disposeClient)
            _client.Dispose();
        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_disposeClient)
            _client.Dispose();
        await base.DisposeAsyncCore();
    }

    /// <remarks>PMTiles.NET 0.1.2 uses sync <see cref="GetTileData"/> + async <see cref="GetTileDataAsync"/>.</remarks>
    protected override Memory<byte> GetTileData(MemoryPosition position) =>
        GetTileDataAsync(position).GetAwaiter().GetResult();

    protected override async Task<Memory<byte>> GetTileDataAsync(MemoryPosition position) =>
        await ReadRangeAsync(position).ConfigureAwait(false);

    private async Task<Memory<byte>> ReadRangeAsync(MemoryPosition position)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _url);
        request.Headers.Range = new RangeHeaderValue(
            (long)position.Offset,
            (long)(position.Offset + position.Length - 1));

        using var response = await _client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get PMTiles data. Status code: {response.StatusCode}");

        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return bytes;
    }
}
