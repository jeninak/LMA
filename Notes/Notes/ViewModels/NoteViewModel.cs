using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using Notes.Services;
using Notes.Models;

namespace Notes.ViewModels;

internal class NoteViewModel : ObservableObject, IQueryAttributable
{
    private Models.Note _note;
    private readonly INotesService _notesService;

    public string Text
    {
        get => _note.Text;
        set
        {
            if (_note.Text != value)
            {
                _note.Text = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime Date => _note.Date;

    public string Identifier => _note.Filename;

    public ICommand SaveCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    // Parameterless ctor for XAML; resolves service, fallback creates local NotesService.
    public NoteViewModel() : this(new Models.Note(),
        (Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(INotesService)) as INotesService) ?? new NotesService())
    {
    }

    public NoteViewModel(Models.Note note) : this(note,
        (Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(INotesService)) as INotesService) ?? new NotesService())
    {
    }

    public NoteViewModel(Models.Note note, INotesService notesService)
    {
        _note = note ?? throw new ArgumentNullException(nameof(note));
        _notesService = notesService ?? throw new ArgumentNullException(nameof(notesService));
        SaveCommand = new AsyncRelayCommand(Save);
        DeleteCommand = new AsyncRelayCommand(Delete);
    }

    private async Task Save()
    {
        _note.Date = DateTime.Now;

        var existing = await _notesService.GetByIdAsync(_note.Filename);
        if (existing == null)
            await _notesService.AddAsync(_note);
        else
            await _notesService.UpdateAsync(_note);

        // Navigate back and notify list to refresh
        await Shell.Current.GoToAsync($"..?saved={_note.Filename}");
    }

    private async Task Delete()
    {
        await _notesService.DeleteAsync(_note.Filename);
        await Shell.Current.GoToAsync($"..?deleted={_note.Filename}");
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("load", out var loadObj) && loadObj?.ToString() is string filename && filename.Length > 0)
        {
            // synchronous load (ApplyQueryAttributes can't be async) - keep null-safe handling
            var loaded = _notesService.GetByIdAsync(filename).GetAwaiter().GetResult();
            if (loaded != null)
            {
                _note = loaded;
                RefreshProperties();
            }
        }
    }

    public void Reload()
    {
        var reloaded = _notesService.GetByIdAsync(_note.Filename).GetAwaiter().GetResult();
        if (reloaded != null)
        {
            _note = reloaded;
            RefreshProperties();
        }
    }

    private void RefreshProperties()
    {
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(Date));
    }
}