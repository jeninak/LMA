using Microsoft.Extensions.Logging;
using Notes.Services;
using Notes.ViewModels;
using System.Net.Http;

namespace Notes;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Provide a single HttpClient instance and register the joke service without requiring AddHttpClient extension.
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<IJokeService, JokeService>();

        // Keep the AboutViewModel registered (used by About page)
        builder.Services.AddSingleton<AboutViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}