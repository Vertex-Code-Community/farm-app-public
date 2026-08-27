using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using FarmApp.PmtilesTileGateway.Configuration;
using PMTiles;

namespace FarmApp.PmtilesTileGateway.Services;

public sealed class PmtilesReaderProvider : IPmtilesReaderProvider, IDisposable
{
    private readonly GatewayOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PmtilesReaderProvider> _logger;
    private readonly object _lock = new();

    private PMTilesReader? _reader;
    private string? _initError;
    private HttpClient? _ownedHttp;

    public PmtilesReaderProvider(
        GatewayOptions options,
        IHttpClientFactory httpClientFactory,
        ILogger<PmtilesReaderProvider> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public PMTilesReader? Reader => _reader;
    public string? InitError => _initError;

    public void EnsureInitialized()
    {
        if (_reader is not null) return;

        lock (_lock)
        {
            if (_reader is not null) return;

            _initError = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(_options.PmtilesLocalPath))
                    InitFromFile(_options.PmtilesLocalPath!);
                else
                    InitFromHttp();
            }
            catch (Exception ex)
            {
                _initError = ex.ToString();
                _logger.LogError(ex, "PMTiles init failed.");
            }
        }
    }

    private void InitFromFile(string localPath)
    {
        var full = Path.GetFullPath(localPath);
        if (!File.Exists(full))
        {
            _initError = $"PmtilesLocalPath not found: {full}";
            _logger.LogError("{Message}", _initError);
            return;
        }

        _reader = PMTilesReader.FromFile(full);
        if (_reader is null)
        {
            _initError = "PMTilesReader.FromFile returned null.";
            return;
        }

        _logger.LogInformation("PMTiles reader open from file {Path}", full);
    }

    private void InitFromHttp()
    {
        if (!Uri.TryCreate(_options.PmtilesUrl, UriKind.Absolute, out var pmUri)
            || (pmUri.Scheme != "http" && pmUri.Scheme != "https"))
        {
            _initError = "PmtilesUrl must be a valid http(s) URL, or set PmtilesLocalPath to a .pmtiles on disk.";
            _logger.LogError("{Message}", _initError);
            return;
        }

        LogHttpProbe();

        if (_options.PmtilesInsecureSkipTlsVerify)
        {
            _logger.LogWarning(
                "PmtilesInsecureSkipTlsVerify=true: TLS validation disabled for PMTiles HTTP. Not for production.");
        }

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.All
        };
        if (_options.PmtilesInsecureSkipTlsVerify)
        {
            handler.SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = static (_, _, _, _) => true
            };
        }

        _ownedHttp = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromMinutes(15)
        };
        _ownedHttp.DefaultRequestHeaders.UserAgent.ParseAdd("FarmApp.PmtilesTileGateway/1.0");

        _reader = new PMTilesReader(
            new HttpClientWebSource(_options.PmtilesUrl, _ownedHttp, disposeClient: false));

        _logger.LogInformation(
            "PMTiles reader ready for {Url} (custom HttpClient + range GET)", _options.PmtilesUrl);
    }

    private void LogHttpProbe()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("styles");
            using var msg = new HttpRequestMessage(HttpMethod.Get, _options.PmtilesUrl);
            msg.Headers.Range = new RangeHeaderValue(0, 16_383);
            using var resp = client.Send(msg, HttpCompletionOption.ResponseHeadersRead);
            _logger.LogInformation(
                "Pmtiles HTTP probe {Url} -> {Status} {Reason}, Accept-Ranges: {Ar}",
                _options.PmtilesUrl,
                (int)resp.StatusCode,
                resp.ReasonPhrase,
                string.Join(", ", resp.Headers.AcceptRanges));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Pmtiles HTTP probe failed (init may still succeed) for {Url}",
                _options.PmtilesUrl);
        }
    }

    public void Dispose()
    {
        _ownedHttp?.Dispose();
    }
}
