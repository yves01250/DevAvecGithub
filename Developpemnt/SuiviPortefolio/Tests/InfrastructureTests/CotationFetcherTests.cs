using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SuiviPortefolio.Tests.InfrastructureTests;

/// <summary>
/// Tests unitaires pour CotationFetcher.
/// Utilise Moq pour simuler HttpClient.
/// </summary>
public sealed class CotationFetcherTests
{
    // ========================================================================
    // Classes pour simuler la réponse de Yahoo Finance
    // ========================================================================

    public class YahooFinanceResponse
    {
        public ChartResult? chart { get; set; }
    }

    public class ChartResult
    {
        public ResultItem[]? result { get; set; }
    }

    public class ResultItem
    {
        public Meta? meta { get; set; }
        public Indicators? indicators { get; set; }
    }

    public class Meta
    {
        public string? currency { get; set; }
    }

    public class Indicators
    {
        public Quote[]? quote { get; set; }
    }

    public class Quote
    {
        public decimal[]? close { get; set; }
    }

    // ========================================================================
    // Tests
    // ========================================================================

    [Fact]
    public async Task FetchSnapshotAsync_RetourneCotationValide()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new YahooFinanceResponse
            {
                chart = new ChartResult
                {
                    result = new ResultItem[]
                    {
                        new ResultItem
                        {
                            meta = new Meta { currency = "EUR" },
                            indicators = new Indicators
                            {
                                quote = new Quote[]
                                {
                                    new Quote { close = new decimal[] { 100.50m } }
                                }
                            }
                        }
                    }
                }
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EUR", result.Devise);
        Assert.Equal(100.50m, result.Close);
        Assert.Equal("EURUSD=X", result.Symbol);
    }

    [Fact]
    public async Task FetchSnapshotAsync_RetourneNullSiSymboleInvalide()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, null);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act
        var result = await fetcher.FetchSnapshotAsync("INVALID");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSnapshotAsync_RetourneNullSiReponseInvalide()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            "Invalid JSON");

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act & Assert
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSnapshotAsync_RetourneNullSiPasDeResultats()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new YahooFinanceResponse { chart = new ChartResult { result = Array.Empty<ResultItem>() } });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSnapshotAsync_RetourneNullSiPasDeQuote()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new YahooFinanceResponse
            {
                chart = new ChartResult
                {
                    result = new ResultItem[]
                    {
                        new ResultItem
                        {
                            meta = new Meta { currency = "EUR" },
                            indicators = new Indicators { quote = Array.Empty<Quote>() }
                        }
                    }
                }
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSnapshotAsync_RetourneNullSiCloseNonValide()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new YahooFinanceResponse
            {
                chart = new ChartResult
                {
                    result = new ResultItem[]
                    {
                        new ResultItem
                        {
                            meta = new Meta { currency = "EUR" },
                            indicators = new Indicators
                            {
                                quote = new Quote[] { new Quote { close = Array.Empty<decimal>() } }
                            }
                        }
                    }
                }
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSnapshotAsync_UtiliseHttpClientCorrectement()
    {
        // Arrange
        var expectedUrl = "https://query1.finance.yahoo.com/v8/finance/chart/EURUSD=X?interval=1d";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri?.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonSerializer.Serialize(CreateValidResponse()),
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act
        await fetcher.FetchSnapshotAsync("EURUSD=X");

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri?.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task FetchSnapshotAsync_GereLesExceptionsHttp()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Request failed"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act & Assert
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSnapshotAsync_GereLesExceptionsJson()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            "{ invalid json");

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var fetcher = new CotationFetcher(httpClient);

        // Act & Assert
        var result = await fetcher.FetchSnapshotAsync("EURUSD=X");
        Assert.Null(result);
    }

    // ========================================================================
    // Méthodes utilitaires
    // ========================================================================

    private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, object? responseContent)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        if (responseContent is string stringContent)
        {
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(stringContent, Encoding.UTF8, "application/json")
                });
        }
        else
        {
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(
                        JsonSerializer.Serialize(responseContent),
                        Encoding.UTF8,
                        "application/json")
                });
        }

        return mockHttpMessageHandler;
    }

    private static YahooFinanceResponse CreateValidResponse(decimal close = 100.50m, string currency = "EUR")
    {
        return new YahooFinanceResponse
        {
            chart = new ChartResult
            {
                result = new ResultItem[]
                {
                    new ResultItem
                    {
                        meta = new Meta { currency = currency },
                        indicators = new Indicators
                        {
                            quote = new Quote[] { new Quote { close = new decimal[] { close } } }
                        }
                    }
                }
            }
        };
    }
}

/// <summary>
/// Exemple de classe CotationFetcher (à adapter selon ton implémentation réelle).
/// Place cette classe dans ton projet (par exemple dans Infrastructure/ ou Services/).
/// </summary>
public class CotationFetcher
{
    private readonly HttpClient _httpClient;

    public CotationFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Récupère la cotation pour un symbole donné (ex: "EURUSD=X").
    /// </summary>
    public async Task<Cotation?> FetchSnapshotAsync(string symbol)
    {
        try
        {
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<YahooFinanceResponse>(content);

            if (json?.chart?.result == null || json.chart.result.Length == 0)
                return null;

            var result = json.chart.result[0];
            if (result.indicators?.quote == null || result.indicators.quote.Length == 0)
                return null;

            var close = result.indicators.quote[0].close;
            if (close == null || close.Length == 0 || !IsFinite(close[0]))
                return null;

            return new Cotation
            {
                Symbol = symbol,
                Devise = result.meta?.currency,
                Close = close[0],
                Date = DateTime.UtcNow
            };
        }
        catch
        {
            // Log l'erreur si un logger est disponible
            return null;
        }
    }

    private static bool IsFinite(decimal value)
    {
        return !decimal.IsInfinity(value) && value != decimal.MinValue && value != decimal.MaxValue;
    }
}

/// <summary>
/// Classe Cotation (à adapter selon ton modèle).
/// </summary>
public class Cotation
{
    public string? Symbol { get; set; }
    public string? Devise { get; set; }
    public decimal Close { get; set; }
    public DateTime Date { get; set; }
}
