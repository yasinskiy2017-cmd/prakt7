using System.Globalization;
using System.Text.Json;
using MovieCatalogApp.Models;

namespace MovieCatalogApp.Services;

public class MovieService
{
    private static readonly Uri DefaultPoster =
        new("https://placehold.co/300x440?text=No+Poster");

    private readonly HttpClient _httpClient;
    private string? _nextPageToken;
    public bool HasMorePages => !string.IsNullOrWhiteSpace(_nextPageToken);

    public MovieService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.imdbapi.dev/");
        _httpClient.Timeout = TimeSpan.FromSeconds(20);
    }

    public async Task<List<Movie>> GetMoviesAsync(bool reset = false)
    {
        if (reset)
        {
            _nextPageToken = null;
        }

        try
        {
            var endpoint = "titles";
            if (!string.IsNullOrWhiteSpace(_nextPageToken))
            {
                endpoint += $"?pageToken={Uri.EscapeDataString(_nextPageToken)}";
            }

            using var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var root = document.RootElement;

            _nextPageToken = root.TryGetProperty("nextPageToken", out var tokenElement)
                && tokenElement.ValueKind == JsonValueKind.String
                    ? tokenElement.GetString()
                    : null;

            if (!root.TryGetProperty("titles", out var titlesElement)
                || titlesElement.ValueKind != JsonValueKind.Array)
            {
                return new List<Movie>();
            }

            var movies = new List<Movie>();
            foreach (var item in titlesElement.EnumerateArray())
            {
                var id = GetString(item, "id");
                var title = GetString(item, "primaryTitle", "Без назви");
                var year = GetYear(item);
                var description = GetString(item, "plot", "Опис відсутній.");
                var rating = GetRating(item);
                var poster = GetPoster(item);

                movies.Add(new Movie
                {
                    Id = id,
                    Title = title,
                    Year = $"Рік випуску: {year}",
                    Poster = poster,
                    Description = description,
                    Rating = $"IMDb: {rating}"
                });
            }

            return movies;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка завантаження каталогу: {exception}");
            return new List<Movie>();
        }
    }

    private static string GetString(JsonElement element, string propertyName, string fallback = "")
    {
        return element.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? fallback
                : fallback;
    }

    private static string GetYear(JsonElement item)
    {
        if (!item.TryGetProperty("startYear", out var yearElement))
        {
            return "Невідомо";
        }

        return yearElement.ValueKind switch
        {
            JsonValueKind.Number when yearElement.TryGetInt32(out var year) =>
                year.ToString(CultureInfo.InvariantCulture),
            JsonValueKind.String when !string.IsNullOrWhiteSpace(yearElement.GetString()) =>
                yearElement.GetString()!,
            _ => "Невідомо"
        };
    }

    private static string GetRating(JsonElement item)
    {
        if (item.TryGetProperty("rating", out var ratingElement)
            && ratingElement.ValueKind == JsonValueKind.Object
            && ratingElement.TryGetProperty("aggregateRating", out var aggregateElement)
            && aggregateElement.ValueKind == JsonValueKind.Number
            && aggregateElement.TryGetDouble(out var rating))
        {
            return $"{rating:0.0}/10";
        }

        return "Немає оцінки";
    }

    private static Uri GetPoster(JsonElement item)
    {
        if (item.TryGetProperty("primaryImage", out var imageElement)
            && imageElement.ValueKind == JsonValueKind.Object)
        {
            var imageUrl = GetString(imageElement, "url");
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var parsedUri)
                && (parsedUri.Scheme == Uri.UriSchemeHttps || parsedUri.Scheme == Uri.UriSchemeHttp))
            {
                return parsedUri;
            }
        }

        return DefaultPoster;
    }
}
