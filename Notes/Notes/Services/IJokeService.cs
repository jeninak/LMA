using System.Threading;

namespace Notes.Services;

public interface IJokeService
{
    Task<string?> GetJokeAsync(CancellationToken cancellationToken = default);
}