using Microsoft.Extensions.DependencyInjection;
using Notes.ViewModels;

namespace Notes.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();

        // Resolve ViewModel from MAUI's IMauiContext Services if available (works with DI),
        // otherwise fall back to the parameterless constructor.
        var services = Application.Current?.Handler?.MauiContext?.Services;
        BindingContext = services?.GetService<AboutViewModel>() ?? new AboutViewModel();
    }
}