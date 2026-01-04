namespace Notes;

public partial class App : Application
{
    public App()
    {
        // Ensure XAML resources are loaded so styles are available
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}