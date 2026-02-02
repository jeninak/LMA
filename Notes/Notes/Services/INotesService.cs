using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notes.Models;

namespace Notes.Services
{
    public interface INotesService
    {
        Task<IEnumerable<Note>> GetAllAsync(int page = 0, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<Note?> GetByIdAsync(string filename, CancellationToken cancellationToken = default);
        Task AddAsync(Note note, CancellationToken cancellationToken = default);
        Task UpdateAsync(Note note, CancellationToken cancellationToken = default);
        Task DeleteAsync(string filename, CancellationToken cancellationToken = default);
    }
}