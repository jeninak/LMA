using CommunityToolkit.Mvvm.Input;
using Notes.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;

namespace Notes.ViewModels;

internal class NotesViewModel : IQueryAttributable
{
    public ObservableCollection<ViewModels.NoteViewModel> AllNotes { get; }
    public ICommand NewCommand { get; }
    public ICommand SelectNoteCommand { get; }

    public NotesViewModel()
    {
        // Guard against LoadAll returning null
        var notes = Models.Note.LoadAll() ?? Enumerable.Empty<Models.Note>();
        AllNotes = new ObservableCollection<ViewModels.NoteViewModel>(notes.Select(n => new NoteViewModel(n)));
        NewCommand = new AsyncRelayCommand(NewNoteAsync);
        SelectNoteCommand = new AsyncRelayCommand<ViewModels.NoteViewModel?>(SelectNoteAsync);
    }

    private async Task NewNoteAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.NotePage));
    }

    // Accept nullable parameter to match AsyncRelayCommand delegate nullability
    private async Task SelectNoteAsync(ViewModels.NoteViewModel? note)
    {
        if (note != null)
            await Shell.Current.GoToAsync($"{nameof(Views.NotePage)}?load={note.Identifier}");
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("deleted", out var deletedObj) && deletedObj?.ToString() is string deletedId && deletedId.Length > 0)
        {
            NoteViewModel? matchedNote = AllNotes.Where((n) => n.Identifier == deletedId).FirstOrDefault();

            // If note exists, delete it
            if (matchedNote != null)
                AllNotes.Remove(matchedNote);
        }
        else if (query.TryGetValue("saved", out var savedObj) && savedObj?.ToString() is string savedId && savedId.Length > 0)
        {
            NoteViewModel? matchedNote = AllNotes.Where((n) => n.Identifier == savedId).FirstOrDefault();

            // If note is found, update it
            if (matchedNote != null)
            {
                matchedNote.Reload();
                AllNotes.Move(AllNotes.IndexOf(matchedNote), 0);
            }
            // If note isn't found, it's new; add it.
            else
            {
                var loaded = Models.Note.Load(savedId);
                if (loaded != null)
                    AllNotes.Insert(0, new NoteViewModel(loaded));
            }
        }
    }
}