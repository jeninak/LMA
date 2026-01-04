using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Notes.ViewModels;

internal class NoteViewModel : ObservableObject, IQueryAttributable
{
    private Models.Note _note;

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

    public NoteViewModel()
    {
        _note = new Models.Note();
        SaveCommand = new AsyncRelayCommand(Save);
        DeleteCommand = new AsyncRelayCommand(Delete);
    }

    public NoteViewModel(Models.Note note)
    {
        _note = note;
        SaveCommand = new AsyncRelayCommand(Save);
        DeleteCommand = new AsyncRelayCommand(Delete);
    }

    private async Task Save()
    {
        _note.Date = DateTime.Now;
        _note.Save();
        await Shell.Current.GoToAsync($"..?saved={_note.Filename}");
    }

    private async Task Delete()
    {
        _note.Delete();
        await Shell.Current.GoToAsync($"..?deleted={_note.Filename}");
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("load", out var loadObj) && loadObj?.ToString() is string filename && filename.Length > 0)
        {
            var loaded = Models.Note.Load(filename);
            if (loaded != null)
            {
                _note = loaded;
                RefreshProperties();
            }
        }
    }

    public void Reload()
    {
        var reloaded = Models.Note.Load(_note.Filename);
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