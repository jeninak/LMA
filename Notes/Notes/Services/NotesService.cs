using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notes.Models;

namespace Notes.Services
{
    public class NotesService : INotesService
    {
        public Task<IEnumerable<Note>> GetAllAsync(int page = 0, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var all = Note.LoadAll().ToList();
            var paged = all.Skip(page * pageSize).Take(pageSize).ToList();
            return Task.FromResult<IEnumerable<Note>>(paged);
        }

        public Task<Note?> GetByIdAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filename)) return Task.FromResult<Note?>(null);
            try
            {
                var note = Note.Load(filename);
                return Task.FromResult<Note?>(note);
            }
            catch (FileNotFoundException)
            {
                return Task.FromResult<Note?>(null);
            }
        }

        public Task AddAsync(Note note, CancellationToken cancellationToken = default)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            note.Save();
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            note.Save();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            try
            {
                var note = Note.Load(filename);
                note.Delete();
            }
            catch (FileNotFoundException)
            {
                // already gone — swallow or rethrow depending on desired behavior
            }
            return Task.CompletedTask;
        }
    }
}