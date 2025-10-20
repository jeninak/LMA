namespace Notes;

public partial class NotesPage : ContentPage
{
    public NotesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadNotes();
    }

    void LoadNotes()
    {
        var notes = Directory
            .EnumerateFiles(FileSystem.AppDataDirectory, "*.notes.txt")
            .Select(filename => new Models.Note
            {
                Filename = filename,
                Text = File.ReadAllText(filename),
                Date = File.GetLastWriteTime(filename)
            })
            .OrderByDescending(note => note.Date);

        notesCollection.ItemsSource = notes;
    }

    async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NotePage));
    }

    async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Models.Note note)
        {
            await Shell.Current.GoToAsync($"{nameof(NotePage)}?ItemId={note.Filename}");
        }
    }
}