using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        Title = "MediGuardAccess";
        BackgroundColor = Theme.Bg;

        var loginButton = Theme.Button("Masuk ke Sistem", Theme.Primary);
        var registerButton = Theme.OutlineButton("Buat Akun Baru", Theme.PrimaryDark);

        loginButton.Clicked += async (_, _) => await Navigation.PushAsync(new LoginPage());
        registerButton.Clicked += async (_, _) => await Navigation.PushAsync(new RegisterPage());

        Content = Theme.DesktopScroll(
            new VerticalStackLayout
            {
                Spacing = 20,
                Children =
                {
                    Hero(loginButton, registerButton),
                    FeatureGrid()
                }
            },
            1280
        );
    }

    private View Hero(Button loginButton, Button registerButton)
    {
        var left = new VerticalStackLayout
        {
            Spacing = 18,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Theme.LogoWordmark(68),
                Theme.Badge("MediGuardAccess Premium", Theme.Primary),
                Theme.Label("Hypertension Priority Screening & Secure Health Access", 38, true, Theme.Dark),
                Theme.Label("Platform desktop untuk skrining awal risiko hipertensi, Smart BP Recheck, red flag kardiovaskular, verifikasi wajah, riwayat lokal, dashboard admin, dan AI navigator yang aman.", 15, false, Theme.Muted),
                new HorizontalStackLayout
                {
                    Spacing = 14,
                    Children =
                    {
                        WithWidth(loginButton, 220),
                        WithWidth(registerButton, 220)
                    }
                }
            }
        };

        var right = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    Theme.Badge("Access Flow", Theme.PrimaryDark),
                    Theme.Label("Credential → Face Check → Hypertension Check", 25, true, Theme.Dark),
                    FlowItem("01", "Secure Login", "Akun user/admin terbaca otomatis setelah kredensial benar."),
                    FlowItem("02", "MediFace Gate", "Template wajah dipakai untuk membuka data skrining yang sesuai akun."),
                    FlowItem("03", "Hypertension Workspace", "User mengisi tekanan darah dan red flag; admin memantau level prioritas pengguna."),
                    Theme.Notice("Skrining awal bersifat edukatif dan tidak menggantikan diagnosis tenaga kesehatan.")
                }
            },
            22
        );

        var grid = new Grid
        {
            ColumnSpacing = 28,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.85, GridUnitType.Star) }
            }
        };
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 1);
        grid.Children.Add(left);
        grid.Children.Add(right);

        return Theme.HeroCard(grid, 30);
    }

    private static Button WithWidth(Button button, double width)
    {
        button.WidthRequest = width;
        return button;
    }

    private static View FlowItem(string number, string title, string body)
    {
        return new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            },
            Children =
            {
                NumberBadge(number),
                TextBlock(title, body)
            }
        };
    }

    private static Border NumberBadge(string text)
    {
        return new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = Theme.PrimaryDark,
            Stroke = Theme.PrimaryDark,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = new Label { Text = text, TextColor = Colors.White, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
        };
    }

    private static View TextBlock(string title, string body)
    {
        var stack = new VerticalStackLayout { Spacing = 2 };
        stack.Children.Add(Theme.Label(title, 15, true));
        stack.Children.Add(Theme.Label(body, 12, false, Theme.Muted));
        Grid.SetColumn(stack, 1);
        return stack;
    }

    private static View FeatureGrid()
    {
        var grid = new Grid
        {
            ColumnSpacing = 16,
            RowSpacing = 16,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        AddFeature(grid, 0, "MediFace", "Verifikasi wajah untuk akses riwayat dan dashboard.", Theme.Primary);
        AddFeature(grid, 1, "Smart BP Recheck", "Tekanan darah awal, pengukuran ulang, tanda vital, dan red flag kardiovaskular.", Theme.Cyan);
        AddFeature(grid, 2, "AI Navigator", "Penjelasan hasil, gejala umum, dan arahan awal yang aman.", Color.FromArgb("#7C3AED"));
        AddFeature(grid, 3, "Admin Insight", "Monitoring user, data skrining hipertensi, filter level, dan detail riwayat.", Theme.Orange);
        return grid;
    }

    private static void AddFeature(Grid grid, int column, string title, string body, Color accent)
    {
        var card = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    Theme.Badge(title, accent),
                    Theme.Label(body, 13, false, Theme.Muted)
                }
            },
            18
        );
        Grid.SetColumn(card, column);
        grid.Children.Add(card);
    }
}
