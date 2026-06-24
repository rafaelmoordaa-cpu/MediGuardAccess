using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class LoginPage : ContentPage
{
    private readonly Entry _user = Theme.Entry("username");
    private readonly Entry _pass = Theme.Entry("password");

    public LoginPage()
    {
        Title = "Login";
        BackgroundColor = Theme.Bg;
        _pass.IsPassword = true;

        var loginButton = Theme.Button("Masuk & Verifikasi Wajah", Theme.Primary);
        var registerButton = Theme.OutlineButton("Buat Akun Baru", Theme.PrimaryDark);

        loginButton.Clicked += OnLogin;
        registerButton.Clicked += async (_, _) => await Navigation.PushAsync(new RegisterPage());

        Content = Theme.DesktopScroll(LoginLayout(loginButton, registerButton), 1160);
    }

    private View LoginLayout(Button loginButton, Button registerButton)
    {
        loginButton.HeightRequest = 52;
        registerButton.HeightRequest = 52;

        var form = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    Theme.LogoWordmark(64),
                    Theme.Label("Masuk ke MediGuardAccess", 32, true),
                    Theme.Label("Login cukup memakai username dan password. Role user/admin akan dibaca otomatis, lalu akses tetap melewati verifikasi wajah.", 14, false, Theme.Muted),
                    Theme.Field("Username", _user),
                    Theme.Field("Password", _pass),
                    loginButton,
                    registerButton
                }
            },
            26
        );

        var info = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    Theme.Badge("Secure Role Routing", Theme.Primary),
                    Theme.Label("Credential → Face Verification → Dashboard", 27, true),
                    RouteCard("User Dashboard", "Form skrining, hasil, riwayat, MediGuard AI, dan rekomendasi awal.", Theme.SoftCyan),
                    RouteCard("Admin Dashboard", "Monitoring pengguna, data skrining, filter status, dan detail riwayat.", Theme.SoftBlue),
                    RouteCard("Privacy Note", "Informasi akun presentasi berada di halaman registrasi/demo, bukan di login utama.", Theme.SoftOrange)
                }
            },
            26
        );

        var grid = new Grid
        {
            ColumnSpacing = 26,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(0.88, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.12, GridUnitType.Star) }
            }
        };
        Grid.SetColumn(form, 0);
        Grid.SetColumn(info, 1);
        grid.Children.Add(form);
        grid.Children.Add(info);
        return grid;
    }

    private static Border RouteCard(string title, string body, Color bg)
    {
        return new Border
        {
            BackgroundColor = bg,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(16),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(20) },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    Theme.Label(title, 18, true),
                    Theme.Label(body, 13, false, Theme.Muted)
                }
            }
        };
    }

    private async void OnLogin(object? sender, EventArgs e)
    {
        string username = _user.Text?.Trim() ?? "";
        string password = _pass.Text ?? "";

        bool success = AppData.Login(username, password, out string message);
        if (!success)
        {
            await DisplayAlert("Login gagal", message, "OK");
            return;
        }

        await Navigation.PushAsync(new FaceVerificationPage());
    }
}
