using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using SuiviPortefolio.Transactions.TransModel;

namespace SuiviPortefolio.Data;

public class CotationFetcher
{
    private static readonly HttpClient _httpClient;

    static CotationFetcher()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/javascript, */*; q=0.01");
    }

    public async Task<IReadOnlyList<Cotation>> SearchAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom à rechercher est obligatoire.", nameof(name));

        var url = $"https://query1.finance.yahoo.com/v1/finance/search?q={Uri.EscapeDataString(name.Trim())}&quotesCount=10&newsCount=0";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        if (!document.RootElement.TryGetProperty("quotes", out var quotes)
            || quotes.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Format de réponse inattendu pour la recherche Yahoo.");

        var results = new List<Cotation>();
        foreach (var quote in quotes.EnumerateArray())
        {
            if (!quote.TryGetProperty("symbol", out var symbolElement)
                || symbolElement.ValueKind != JsonValueKind.String)
                continue;

            var symbol = symbolElement.GetString();
            if (string.IsNullOrWhiteSpace(symbol))
                continue;

            results.Add(new Cotation
            {
                Symbol = symbol,
                Name = GetString(quote, "longname") ?? GetString(quote, "shortname") ?? symbol,
                Instrument = GetString(quote, "quoteType") ?? string.Empty
            });
        }

        return results;
    }

    // Récupère le snapshot (shortName + dernier close + date) d'un ticker, sans notion de période.
    public async Task<Cotation?> FetchSnapshotAsync(string symbol)
    {
        // range=1d : on minimise la charge, le bloc meta contient shortName + regularMarketPrice/Time
        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}?interval=1d&range=1d";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("chart", out var chart))
            throw new InvalidOperationException($"Format de réponse inattendu pour {symbol}");

        if (chart.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
            throw new InvalidOperationException($"Erreur Yahoo pour {symbol} : {error}");

        var result = chart.GetProperty("result")[0];
        var meta = result.GetProperty("meta");


        var shortName = meta.TryGetProperty("shortName", out var sn) && sn.ValueKind == JsonValueKind.String
            ? sn.GetString() : symbol;
        var instrumentType = meta.TryGetProperty("instrumentType", out var tn) && tn.ValueKind == JsonValueKind.String
            ? tn.GetString()
            : symbol;
        var fullExchangeName = meta.TryGetProperty("fullExchangeName", out var fen) && fen.ValueKind == JsonValueKind.String
            ? fen.GetString()
            : symbol;
        var currency = meta.TryGetProperty("currency", out var curr) && curr.ValueKind == JsonValueKind.String
                ? curr.GetString()
                : symbol;

        // Dernier cours de clôture : priorité au dernier point de la série (close de séance),
        // repli sur meta.regularMarketPrice si la série est vide.
        var (lastClose, lastDate) = ExtractLastClose(result, symbol);


        return new Cotation
        {
            Symbol = symbol,
            Name = shortName,
            Instrument = instrumentType,
            Marche = fullExchangeName,
            Devise = currency,
            Close = lastClose,
            Date = lastDate,
            CreatedAt = DateTime.Now
        };
    }

    private static (double close, DateTime date) ExtractLastClose(JsonElement result, string symbol)
    {
        // 1) Dernier point exploitable de timestamp[] / close[] (close de séance stricte)
        if (result.TryGetProperty("timestamp", out var tsElem) && tsElem.ValueKind == JsonValueKind.Array
            && result.TryGetProperty("indicators", out var indicators)
            && indicators.TryGetProperty("quote", out var quoteArray) && quoteArray.GetArrayLength() > 0
            && quoteArray[0].TryGetProperty("close", out var closeElem) && closeElem.ValueKind == JsonValueKind.Array)
        {
            var timestamps = tsElem.EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Number && x.TryGetInt64(out var ts) ? ts : 0L)
                .ToList();

            var closes = closeElem.EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Number && x.TryGetDouble(out var c) ? (double?)c : null)
                .ToList();

            var count = Math.Min(timestamps.Count, closes.Count);
            for (var i = count - 1; i >= 0; i--)
            {
                if (timestamps[i] > 0 && closes[i] is double c)
                {
                    return (c, DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).DateTime);
                }
            }
        }

        // 2) Repli sur meta.regularMarketPrice / regularMarketTime
        var meta = result.GetProperty("meta");
        var price = meta.TryGetProperty("regularMarketPrice", out var rmp) && rmp.ValueKind == JsonValueKind.Number
            ? rmp.GetDouble()
            : 0.0;

        var time = meta.TryGetProperty("regularMarketTime", out var rmt) && rmt.ValueKind == JsonValueKind.Number
            ? rmt.GetInt64()
            : 0L;

        var date = time > 0 ? DateTimeOffset.FromUnixTimeSeconds(time).DateTime : DateTime.MinValue;
        return (price, date);
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
}
