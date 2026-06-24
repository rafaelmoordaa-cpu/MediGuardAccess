using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class AdminDashboardPage : ContentPage
{
    private readonly VerticalStackLayout _users = new() { Spacing = 10 };
    private readonly VerticalStackLayout _records = new() { Spacing = 10 };
    private string _selectedRecordFilter = "Semua";

    public AdminDashboardPage()
    {
        Title = "Dashboard Admin";
        BackgroundColor = Theme.Bg;

        var logoutButton = Theme.Button("Logout", Theme.Red);
        logoutButton.WidthRequest = 110;
        logoutButton.Clicked += async (_, _) => await Logout();

        var pageContent = new VerticalStackLayout
        {
            Spacing = 18,
            Children =
            {
                Header(logoutButton),
                Stats(),
                MainDashboardGrid(),
                Theme.Notice(
                    "Dashboard admin digunakan untuk memantau akun pengguna dan riwayat skrining. Data tersimpan di database lokal aplikasi."
                )
            }
        };

        Content = Theme.DesktopScroll(pageContent, 1380);

        Load();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Load();
    }

    private View Header(Button logoutButton)
    {
        var left = new HorizontalStackLayout
        {
            Spacing = 14,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Theme.LogoIcon(54),

                new VerticalStackLayout
                {
                    Spacing = 2,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        Theme.Label("MediGuardAccess", 26, true, Theme.Primary),
                        Theme.Label("Dashboard Admin", 13, true, Theme.Muted),
                        Theme.Label(
                            "Pantau pengguna, hasil skrining hipertensi, red flag, dan tekanan darah yang perlu perhatian.",
                            12,
                            false,
                            Theme.Muted
                        )
                    }
                }
            }
        };

        var header = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        AddToGrid(header, left, 0, 0);
        AddToGrid(header, Theme.Badge("Admin", Theme.Dark), 1, 0);
        AddToGrid(header, logoutButton, 2, 0);

        return Theme.HeroCard(header, 18);
    }

    private View Stats()
    {
        int totalUsers = AppData.Users.Count;
        int totalUser = AppData.Users.Count(user => user.Role == UserRole.User);
        int totalAdmin = AppData.Users.Count(user => user.Role == UserRole.Admin);
        int totalRecords = AppData.Records.Count;
        int totalNormal = AppData.Records.Count(record => record.Result == "Normal");
        int totalWaspada = AppData.Records.Count(record => record.Result == "Waspada");
        int totalBerisiko = AppData.Records.Count(record => record.Result == "Berisiko");

        var grid = new Grid
        {
            ColumnSpacing = 14,
            RowSpacing = 14,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        AddToGrid(grid, Stat("Total User", totalUsers.ToString(), "Semua akun", Theme.Primary), 0, 0);
        AddToGrid(grid, Stat("User", totalUser.ToString(), "Akun pengguna", Theme.Cyan), 1, 0);
        AddToGrid(grid, Stat("Admin", totalAdmin.ToString(), "Akun admin", Theme.Dark), 2, 0);
        AddToGrid(grid, Stat("Skrining HTN", totalRecords.ToString(), "Total riwayat", Theme.Teal), 3, 0);

        AddToGrid(grid, Stat("Normal", totalNormal.ToString(), "Aman awal", Theme.Green), 0, 1);
        AddToGrid(grid, Stat("Waspada", totalWaspada.ToString(), "Perlu dipantau", Theme.Orange), 1, 1);
        AddToGrid(grid, Stat("Berisiko", totalBerisiko.ToString(), "Perlu tindakan", Theme.Red), 2, 1);
        AddToGrid(grid, Stat("Red Flag", totalBerisiko.ToString(), "Queue prioritas", Theme.Red), 3, 1);

        return grid;
    }

    private static View Stat(string title, string value, string subtitle, Color accent)
    {
        return Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 7,
                Children =
                {
                    Theme.Badge(title, accent),
                    Theme.Label(value, 26, true, accent),
                    Theme.Label(subtitle, 12, false, Theme.Muted)
                }
            },
            16
        );
    }

    private View MainDashboardGrid()
    {
        var grid = new Grid
        {
            ColumnSpacing = 18,
            RowSpacing = 18,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(0.78, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.42, GridUnitType.Star) }
            }
        };

        AddToGrid(grid, UserManagementCard(), 0, 0);
        AddToGrid(grid, ScreeningDataCard(), 1, 0);

        return grid;
    }

    private View UserManagementCard()
    {
        return Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    SectionHeader(
                        "Manajemen Pengguna",
                        "Lihat data pengguna dan ubah status akun aktif/nonaktif."
                    ),
                    new ScrollView
                    {
                        HeightRequest = 600,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                        Content = _users
                    }
                }
            },
            20
        );
    }

    private View ScreeningDataCard()
    {
        return Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    SectionHeader(
                        "Data Skrining Hipertensi",
                        "Riwayat tekanan darah, hasil prioritas, dan red flag pengguna."
                    ),
                    FilterBar(),
                    new ScrollView
                    {
                        HeightRequest = 600,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                        Content = _records
                    }
                }
            },
            20
        );
    }


    private View FilterBar()
    {
        var all = FilterButton("Semua");
        var normal = FilterButton("Normal");
        var waspada = FilterButton("Waspada");
        var berisiko = FilterButton("Berisiko");

        return new HorizontalStackLayout
        {
            Spacing = 8,
            Children =
            {
                all,
                normal,
                waspada,
                berisiko
            }
        };
    }

    private Button FilterButton(string filter)
    {
        bool selected = _selectedRecordFilter == filter;
        var button = Theme.ChipButton(filter, selected);
        button.HeightRequest = 34;
        button.FontSize = 11;

        button.Clicked += (_, _) =>
        {
            _selectedRecordFilter = filter;
            LoadRecords();
        };

        return button;
    }

    private static View SectionHeader(string title, string subtitle)
    {
        return new VerticalStackLayout
        {
            Spacing = 3,
            Children =
            {
                Theme.SectionTitle(title),
                Theme.Label(subtitle, 12, false, Theme.Muted)
            }
        };
    }

    private void Load()
    {
        LoadUsers();
        LoadRecords();
    }

    private void LoadUsers()
    {
        _users.Children.Clear();

        if (AppData.Users.Count == 0)
        {
            _users.Children.Add(
                Theme.CyanCard(
                    Theme.Label("Belum ada pengguna terdaftar.", 12, false, Theme.Muted),
                    12
                )
            );
            return;
        }

        foreach (var user in AppData.Users)
        {
            var actionButton = Theme.Button(
                user.IsActive ? "Nonaktifkan" : "Aktifkan",
                user.IsActive ? Theme.Orange : Theme.Green
            );

            actionButton.WidthRequest = 118;
            actionButton.HeightRequest = 40;

            actionButton.Clicked += (_, _) =>
            {
                user.IsActive = !user.IsActive;
                AppData.Save();
                LoadUsers();
            };

            var statusBadge = user.IsActive
                ? Theme.Badge("Aktif", Theme.Green)
                : Theme.Badge("Nonaktif", Theme.Orange);

            var roleBadge = user.Role == UserRole.Admin
                ? Theme.Badge("Admin", Theme.Dark)
                : Theme.Badge("User", Theme.Primary);

            var avatar = Theme.MiniIcon(
                user.Role == UserRole.Admin ? "A" : "U",
                user.Role == UserRole.Admin ? Theme.Dark : Theme.Primary
            );

            var info = new HorizontalStackLayout
            {
                Spacing = 12,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    avatar,

                    new VerticalStackLayout
                    {
                        Spacing = 5,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            Theme.Label(user.FullName, 14, true, Theme.Dark),
                            Theme.Label($"@{user.Username}", 12, false, Theme.Muted),

                            new HorizontalStackLayout
                            {
                                Spacing = 8,
                                Children =
                                {
                                    roleBadge,
                                    statusBadge
                                }
                            }
                        }
                    }
                }
            };

            var row = new Grid
            {
                ColumnSpacing = 12,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            AddToGrid(row, info, 0, 0);
            AddToGrid(row, actionButton, 1, 0);

            _users.Children.Add(
                new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = Theme.BorderLight,
                    StrokeThickness = 1,
                    Padding = new Thickness(12),
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(16)
                    },
                    Content = row
                }
            );
        }
    }

    private void LoadRecords()
    {
        _records.Children.Clear();

        var recordsQuery = AppData.Records.AsEnumerable();

        if (_selectedRecordFilter != "Semua")
            recordsQuery = recordsQuery.Where(record => record.Result == _selectedRecordFilter);

        var records = recordsQuery
            .Reverse()
            .Take(20)
            .ToList();

        if (records.Count == 0)
        {
            _records.Children.Add(
                Theme.CyanCard(
                    Theme.Label($"Belum ada data skrining untuk filter {_selectedRecordFilter}.", 12, false, Theme.Muted),
                    12
                )
            );
            return;
        }

        foreach (var record in records)
        {
            _records.Children.Add(RecordItem(record));
        }
    }

private static Label WrapLabel(string text, double size, Color color)
{
    return new Label
    {
        Text = text,
        FontSize = size,
        TextColor = color,
        LineBreakMode = LineBreakMode.WordWrap,
        HorizontalOptions = LayoutOptions.Fill
    };
}

private static string FriendlyReason(string reason)
{
    if (string.IsNullOrWhiteSpace(reason))
        return "hasil dibaca dari gabungan data tubuh dan gejala pengguna.";

    return reason
        .Replace("Output Normal karena", "hasil aman karena")
        .Replace("Output Waspada karena", "hasil perlu dipantau karena")
        .Replace("Output Berisiko karena", "hasil perlu perhatian karena")
        .Replace("tekanan darah", "tekanan darah")
        .Replace("SpO2", "kadar oksigen darah")
        .Replace("BPM", "denyut nadi")
        .Replace("Respiratory Rate", "frekuensi napas")
        .Replace("Suhu", "suhu tubuh")
        .Replace("Skor gejala", "jumlah gejala penting")
        .Replace("terdapat tanda prioritas/red flag", "ada tanda yang perlu diperhatikan");
}

    private static View RecordItem(ScreeningRecord record)
    {
        var top = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var identity = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Theme.Label(record.Name, 14, true, Theme.Dark),
                Theme.Label($"{record.Date:dd/MM/yyyy HH:mm}", 12, false, Theme.Muted)
            }
        };

        AddToGrid(top, identity, 0, 0);
        AddToGrid(top, Theme.StatusBadge(record.Result), 1, 0);

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(14),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(16)
            },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    top,

                    new Border
                    {
                        BackgroundColor = Theme.StatusSoftColor(record.Result),
                        Stroke = Theme.StatusColor(record.Result),
                        StrokeThickness = 1,
                        Padding = new Thickness(10),
                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = new CornerRadius(14)
                        },
                        Content = WrapLabel(
                            $"TD {record.Systolic}/{record.Diastolic} mmHg • Nadi {record.Bpm} • SpO2 {record.SpO2}% • Skor {record.RiskScore}%",
                            12,
                            Theme.Dark
                        )
                    },

                    WrapLabel(
                        $"Alasan: {FriendlyReason(record.Reason)}",
                        12,
                        Theme.Dark
                    ),

                    WrapLabel(
                        $"Saran awal: {record.Recommendation}",
                        12,
                        Theme.Muted
                    )
                }
            }
        };
    }

    private static void AddToGrid(Grid grid, View view, int column, int row)
    {
        Grid.SetColumn(view, column);
        Grid.SetRow(view, row);
        grid.Children.Add(view);
    }

    private async Task Logout()
    {
        bool confirm = await DisplayAlert("Keluar", "Apakah kamu ingin keluar dari MediGuardAccess?", "Ya", "Batal");

        if (confirm)
        {
            AppData.Logout();
            await Navigation.PopToRootAsync();
        }
    }
}
