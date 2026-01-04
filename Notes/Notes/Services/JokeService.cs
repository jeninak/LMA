using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Notes.Services;

public class JokeService : IJokeService
{
    private readonly HttpClient _httpClient;

    public JokeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetJokeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Request a single programming joke.
            var url = "https://v2.jokeapi.dev/joke/Programming?blacklistFlags=nsfw,religious,political,racist,sexist,explicit&type=single&amount=1";

            using var resp = await _httpClient.GetAsync(url, cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;

            await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            // If API returned an array of jokes (when amount>1) use first
            if (root.TryGetProperty("jokes", out var jokes) && jokes.ValueKind == JsonValueKind.Array && jokes.GetArrayLength() > 0)
            {
                var first = jokes[0];
                // single-type joke inside array
                if (first.TryGetProperty("joke", out var j1) && j1.ValueKind == JsonValueKind.String)
                    return j1.GetString();

                // two-part joke inside array
                if (first.TryGetProperty("type", out var t1) && t1.GetString() == "twopart")
                {
                    var setup = first.TryGetProperty("setup", out var s1) ? s1.GetString() : null;
                    var delivery = first.TryGetProperty("delivery", out var d1) ? d1.GetString() : null;
                    return string.Join(" ", new[] { setup, delivery }.Where(s => !string.IsNullOrEmpty(s)));
                }
            }

            // If API returned a single joke object:
            if (root.TryGetProperty("joke", out var j) && j.ValueKind == JsonValueKind.String)
            {
                return j.GetString();
            }

            // Handle two-part single-object response
            if (root.TryGetProperty("type", out var t) && t.GetString() == "twopart")
            {
                var setup = root.TryGetProperty("setup", out var s) ? s.GetString() : null;
                var delivery = root.TryGetProperty("delivery", out var d) ? d.GetString() : null;
                return string.Join(" ", new[] { setup, delivery }.Where(s2 => !string.IsNullOrEmpty(s2)));
            }

            return null;
        }
        catch
        {
            // Don't throw here; AboutViewModel will show a friendly fallback.
            return null;
        }
    }
}