using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Notes.Models
{
    public class Note
    {
        public string Filename { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public Note()
        {
            Filename = string.Empty;
            Text = string.Empty;
            Date = DateTime.UtcNow;
        }

        public void Save()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Notes");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Filename);
            File.WriteAllText(path, JsonSerializer.Serialize(this));
        }

        public void Delete()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Notes");
            var path = Path.Combine(dir, Filename);
            if (File.Exists(path)) File.Delete(path);
        }

        public static Note Load(string filename)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Notes");
            var path = Path.Combine(dir, filename);
            if (!File.Exists(path)) throw new FileNotFoundException($"Note file not found: {filename}");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Note>(json) ?? throw new InvalidDataException("Failed to deserialize note.");
        }

        public static IEnumerable<Note> LoadAll()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Notes");
            if (!Directory.Exists(dir)) yield break;
            var files = Directory.GetFiles(dir);
            foreach (var f in files)
            {
                var json = File.ReadAllText(f);
                var note = JsonSerializer.Deserialize<Note>(json);
                if (note != null) yield return note;
            }
        }
    }
}