using System.Net.Http.Json;

class Program
{
    static async Task Main()
    {
        using var httpClient = new HttpClient();

        string url = "https://v2.jokeapi.dev/joke/Programming?blacklistFlags=nsfw,religious,political,racist,sexist,explicit&type=single&amount=3";

        // Fetches json data from the api and makes it into an object
        var response = await httpClient.GetFromJsonAsync<JokeApiResponse>(url);

        Console.WriteLine("Jokes:\n");

        if (response?.Jokes != null)
        {
            int i = 1;
            foreach (var j in response.Jokes)
            {
                Console.WriteLine($"{i++}. {j.Joke}\n");
            }
        }
        else
        {
            Console.WriteLine("No jokes found");
        }
    }
}

class JokeApiResponse
{
    public List<JokeItem>? Jokes { get; set; }
}

class JokeItem
{
    public string Joke { get; set; } = "";
}