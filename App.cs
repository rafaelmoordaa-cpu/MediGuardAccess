namespace MediGuardAccess;

public class App : Application
{
    public App()
    {
        UserAppTheme = AppTheme.Light;
        Resources = Theme.CreateResources();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(
            new NavigationPage(new WelcomePage())
            {
                BarBackgroundColor = Theme.Primary,
                BarTextColor = Colors.White
            }
        );
    }
}
