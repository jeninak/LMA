using CommunityToolkit.Mvvm.Input;
using Notes.Models;
using Notes.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;

namespace Notes.ViewModels;

internal class NotesViewModel : IQueryAttributable
{
    private readonly INotesService _notesService;

    public ObservableCollection<ViewModels.NoteViewModel> AllNotes { get; }
    public ICommand NewCommand { get; }
    public ICommand SelectNoteCommand { get; }

    // Parameterless ctor used by XAML or fallback; resolves service via MAUI service provider if available.
    public NotesViewModel() : this(
        (Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(INotesService)) as INotesService)
        ?? new NotesService())
    {
    }

    public NotesViewModel(INotesService notesService)
    {
        _notesService = notesService;
        AllNotes = new ObservableCollection<ViewModels.NoteViewModel>();
        NewCommand = new AsyncRelayCommand(NewNoteAsync);
        SelectNoteCommand = new AsyncRelayCommand<ViewModels.NoteViewModel?>(SelectNoteAsync);

        // load first page
        _ = LoadNotesAsync();
    }

    private async Task LoadNotesAsync(int page = 0, int pageSize = 50)
    {
        var notes = await _notesService.GetAllAsync(page, pageSize);
        AllNotes.Clear();
        foreach (var n in notes)
            AllNotes.Add(new NoteViewModel(n, _notesService));
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
                // load from db and insert
                var loaded = _notesService.GetByIdAsync(savedId).GetAwaiter().GetResult();
                if (loaded != null)
                    AllNotes.Insert(0, new NoteViewModel(loaded, _notesService));
            }
        }
    }
}