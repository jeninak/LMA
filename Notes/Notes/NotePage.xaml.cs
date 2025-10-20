namespace Notes;

[QueryProperty(nameof(ItemId), nameof(ItemId))]
public partial class NotePage : ContentPage
{
    string _fileName;

    public NotePage()
    {
        InitializeComponent();
    }

    public string ItemId
    {
        set
        {
            _fileName = value;
            if (File.Exists(_fileName))
                editor.Text = File.ReadAllText(_fileName);
        }
    }

    async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_fileName))
        {
            string fileName = Path.Combine(FileSystem.AppDataDirectory, $"{Path.GetRandomFileName()}.notes.txt");
            File.WriteAllText(fileName, editor.Text);
        }
        else
        {
            File.WriteAllText(_fileName, editor.Text);
        }

        await Shell.Current.GoToAsync("..");
    }

    async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (File.Exists(_fileName))
            File.Delete(_fileName);

        await Shell.Current.GoToAsync("..");
    }
}