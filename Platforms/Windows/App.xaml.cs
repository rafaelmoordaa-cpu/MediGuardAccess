using Microsoft.UI.Xaml;

namespace MediGuardAccess.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp()
    {
        return global::MediGuardAccess.MauiProgram.CreateMauiApp();
    }
}
