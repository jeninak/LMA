using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Notes.Models;

namespace Notes.Services
{
    public class SqliteNotesService : INotesService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SqliteNotesService()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Directory.CreateDirectory(dir);
            _dbPath = Path.Combine(dir, "notes.db");
            _connectionString = new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();
            Initialize();
        }

        private void Initialize()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Notes (
                    Filename TEXT PRIMARY KEY,
                    Text     TEXT NOT NULL,
                    DateUtc  TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        public async Task<IEnumerable<Note>> GetAllAsync(int page = 0, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var list = new List<Note>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Filename, Text, DateUtc FROM Notes ORDER BY DateUtc DESC LIMIT @limit OFFSET @offset;";
            cmd.Parameters.AddWithValue("@limit", pageSize);
            cmd.Parameters.AddWithValue("@offset", page * pageSize);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var filename = reader.GetString(0);
                var text = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var dateText = reader.IsDBNull(2) ? DateTime.UtcNow.ToString("o") : reader.GetString(2);
                var note = new Note
                {
                    Filename = filename,
                    Text = text,
                    Date = DateTime.Parse(dateText, null, System.Globalization.DateTimeStyles.AdjustToUniversal).ToUniversalTime()
                };
                list.Add(note);
            }
            return list;
        }

        public async Task<Note?> GetByIdAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filename)) return null;
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Filename, Text, DateUtc FROM Notes WHERE Filename = @f;";
            cmd.Parameters.AddWithValue("@f", filename);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;
            var f = reader.GetString(0);
            var t = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var d = reader.IsDBNull(2) ? DateTime.UtcNow.ToString("o") : reader.GetString(2);
            return new Note
            {
                Filename = f,
                Text = t,
                Date = DateTime.Parse(d, null, System.Globalization.DateTimeStyles.AdjustToUniversal).ToUniversalTime()
            };
        }

        public async Task AddAsync(Note note, CancellationToken cancellationToken = default)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            // Ensure the note has a stable filename/identifier when saved to DB.
            // If the app created the Note with an empty Filename (new note), generate one here.
            if (string.IsNullOrWhiteSpace(note.Filename))
            {
                note.Filename = $"{Guid.NewGuid():N}.note";
            }

            note.Date = note.Date == default ? DateTime.UtcNow : note.Date;

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT OR REPLACE INTO Notes (Filename, Text, DateUtc) VALUES (@f, @t, @d);";
            cmd.Parameters.AddWithValue("@f", note.Filename);
            cmd.Parameters.AddWithValue("@t", note.Text ?? string.Empty);
            cmd.Parameters.AddWithValue("@d", note.Date.ToUniversalTime().ToString("o"));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task UpdateAsync(Note note, CancellationToken cancellationToken = default) => AddAsync(note, cancellationToken);

        public async Task DeleteAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM Notes WHERE Filename = @f;";
            cmd.Parameters.AddWithValue("@f", filename);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}