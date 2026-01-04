using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Notes.Services;

internal class JokeApiResponse
{
    public List<JokeItem>? Jokes { get; set; }
}

internal class JokeItem
{
    public string Joke { get; set; } = "";
}

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
            var url = "https://v2.jokeapi.dev/joke/Programming?blacklistFlags=nsfw,religious,political,racist,sexist,explicit&type=single&amount=1";
            var response = await _httpClient.GetFromJsonAsync<JokeApiResponse>(url, cancellationToken);
            return response?.Jokes?.FirstOrDefault()?.Joke;
        }
        catch
        {
            return null;
        }
    }
}