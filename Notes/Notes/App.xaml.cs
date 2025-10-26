namespace Notes
{
    public partial class App : Application
    {
        public App()
        {
            // Remove or comment out this line if the partial method is auto-generated elsewhere
            // InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}