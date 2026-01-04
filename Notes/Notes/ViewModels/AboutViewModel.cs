using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Notes.Services;
using System.Net.Http;

namespace Notes.ViewModels;

internal class AboutViewModel : INotifyPropertyChanged
{
    private readonly IJokeService _jokeService;

    // Default constructor required by XAML when the viewmodel is created without DI.
    // It provides a simple fallback IJokeService instance so compiled bindings/XAML can instantiate this VM.
    public AboutViewModel() : this(new JokeService(new HttpClient()))
    {
    }

    public AboutViewModel(IJokeService jokeService)
    {
        _jokeService = jokeService;
        ShowMoreInfoCommand = new AsyncRelayCommand(ShowMoreInfo);
        RefreshJokeCommand = new AsyncRelayCommand(LoadJokeAsync);
        Title = AppInfo.Name;
        Version = AppInfo.VersionString;
        Message = "This app is written in XAML and C# with .NET MAUI.";

        // load once on creation
        _ = LoadJokeAsync();
    }

    public string Title { get; }
    public string Version { get; }
    public string Message { get; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    private string? _jokeOfTheDay;
    public string? JokeOfTheDay
    {
        get => _jokeOfTheDay;
        private set => SetProperty(ref _jokeOfTheDay, value);
    }

    public ICommand ShowMoreInfoCommand { get; }
    public IAsyncRelayCommand RefreshJokeCommand { get; }

    private async Task LoadJokeAsync()
    {
        try
        {
            IsLoading = true;
            var joke = await _jokeService.GetJokeAsync();
            JokeOfTheDay = !string.IsNullOrWhiteSpace(joke) ? joke : "Couldn't load a joke right now.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ShowMoreInfo() =>
        await Launcher.Default.OpenAsync("https://youtu.be/xvFZjo5PgG0?si=wE4RoDV7hqYecamH");

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
    #endregion
}