using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.Globalization;

namespace MediGuardAccess;

public class UserDashboardPage : ContentPage
{
    private readonly Entry _name = Theme.Entry("Nama pengguna");
    private readonly Entry _age = Theme.Entry("Contoh: 20", Keyboard.Numeric);
    private readonly Entry _complaint = Theme.Entry("Contoh: pusing, dada tidak nyaman, jantung berdebar");

    private readonly Entry _systolic = Theme.Entry("Sistolik, contoh: 128", Keyboard.Numeric);
    private readonly Entry _diastolic = Theme.Entry("Diastolik, contoh: 82", Keyboard.Numeric);
    private readonly Entry _recheckSystolic = Theme.Entry("Sistolik ulang", Keyboard.Numeric);
    private readonly Entry _recheckDiastolic = Theme.Entry("Diastolik ulang", Keyboard.Numeric);
    private readonly Entry _bpm = Theme.Entry("Normal 60–100", Keyboard.Numeric);
    private readonly Entry _spo2 = Theme.Entry("Normal 95–100", Keyboard.Numeric);
    private readonly Entry _rr = Theme.Entry("Normal 12–20", Keyboard.Numeric);
    private readonly Entry _temp = Theme.Entry("Contoh: 36.8", Keyboard.Numeric);
    private readonly Entry _duration = Theme.Entry("Durasi keluhan (hari)", Keyboard.Numeric);

    private readonly HorizontalStackLayout _genderChips = new() { Spacing = 10 };
    private readonly Label _genderPreview = Theme.Label("Jenis kelamin: Laki-laki", 12, true, Theme.Primary);
    private string _selectedGender = "Laki-laki";

    private readonly CheckBox c1 = new();
    private readonly CheckBox c2 = new();
    private readonly CheckBox c3 = new();
    private readonly CheckBox c4 = new();
    private readonly CheckBox c5 = new();
    private readonly CheckBox c6 = new();
    private readonly CheckBox c7 = new();
    private readonly CheckBox c8 = new();

    private readonly Label _latestLevel = Theme.Label("Belum ada hasil", 22, true, Theme.Muted);
    private readonly Label _latestSummary = Theme.Label("Isi skrining hipertensi untuk melihat prioritas awal.", 12, false, Theme.Muted);
    private readonly Border _latestBox = new();
    private readonly VerticalStackLayout _history = new() { Spacing = 10 };

    private readonly ChatbotView _chat = new()
    {
        IsVisible = false,
        HorizontalOptions = LayoutOptions.End,
        VerticalOptions = LayoutOptions.End,
        Margin = new Thickness(0, 0, 22, 22),
        WidthRequest = 410,
        HeightRequest = 560
    };

    public UserDashboardPage()
    {
        Title = "Dashboard Pengguna";
        BackgroundColor = Theme.Bg;
        _name.Text = AppData.CurrentUser?.FullName ?? "";
        RenderGenderChips();

        var analyze = Theme.Button("Analisis Hipertensi", Theme.Primary);
        analyze.Clicked += OnProcess;

        var reset = Theme.OutlineButton("Reset", Theme.PrimaryDark);
        reset.Clicked += (_, _) => ResetForm();

        var chatButton = Theme.Button("MediGuard AI", Theme.Cyan);
        chatButton.Clicked += (_, _) => _chat.IsVisible = !_chat.IsVisible;

        var ebookButton = Theme.Button("Library 8 Bab", Theme.Orange);
        ebookButton.Clicked += async (_, _) => await Navigation.PushAsync(new HypertensionEBookPage());

        var logout = Theme.Button("Logout", Theme.Red);
        logout.WidthRequest = 110;
        logout.Clicked += async (_, _) => await Logout();

        var content = new VerticalStackLayout
        {
            Spacing = 18,
            Children =
            {
                Header(logout),
                TopInsights(chatButton, ebookButton),
                MainGrid(analyze, reset),
                Theme.Notice("MediGuardAccess adalah skrining awal risiko hipertensi dan prioritas kardiovaskular. Hasil aplikasi bukan diagnosis final dan tidak menggantikan pemeriksaan tenaga kesehatan.")
            }
        };

        var root = new Grid();
        root.Children.Add(Theme.DesktopScroll(content, 1420));
        root.Children.Add(_chat);
        Content = root;

        LoadHistory();
    }

    private View Header(Button logout)
    {
        var left = new HorizontalStackLayout
        {
            Spacing = 15,
            Children =
            {
                Theme.LogoIcon(58),
                new VerticalStackLayout
                {
                    Spacing = 3,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        Theme.Label("MediGuardAccess", 26, true, Theme.PrimaryDark),
                        Theme.Label("Hypertension Priority Screening", 14, true, Theme.Primary),
                        Theme.Label("Pantau tekanan darah, red flag kardiovaskular, dan rekomendasi awal secara aman.", 12, false, Theme.Muted)
                    }
                }
            }
        };

        var badges = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            Children =
            {
                Theme.Badge("User", Theme.Primary),
                Theme.Badge("Smart BP Recheck", Theme.Orange),
                logout
            }
        };

        var grid = new Grid
        {
            ColumnSpacing = 18,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        AddToGrid(grid, left, 0, 0);
        AddToGrid(grid, badges, 1, 0);
        return Theme.HeroCard(grid, 18);
    }

    private View TopInsights(Button chatButton, Button ebookButton)
    {
        _latestBox.BackgroundColor = Theme.SoftBlue;
        _latestBox.Stroke = Theme.BorderLight;
        _latestBox.StrokeThickness = 1;
        _latestBox.Padding = new Thickness(16);
        _latestBox.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) };
        _latestBox.Content = new VerticalStackLayout
        {
            Spacing = 5,
            Children =
            {
                Theme.Label("Hasil terakhir", 12, true, Theme.Muted),
                _latestLevel,
                _latestSummary
            }
        };

        chatButton.HorizontalOptions = LayoutOptions.Fill;
        ebookButton.HorizontalOptions = LayoutOptions.Fill;

        var grid = new Grid
        {
            ColumnSpacing = 14,
            RowSpacing = 14,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.05, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.85, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.85, GridUnitType.Star) }
            }
        };

        AddToGrid(grid, _latestBox, 0, 0);
        AddToGrid(grid, MetricTile("Tekanan Darah", "<140/90", "pantau bila lebih tinggi", Theme.Primary), 1, 0);
        AddToGrid(grid, MetricTile("Red Flag", "Safety Gate", "dada berat, sesak, pingsan", Theme.Red), 2, 0);
        AddToGrid(grid, Theme.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Theme.Label("AI Navigator", 14, true, Theme.PrimaryDark),
                Theme.Label("Tanya gejala umum atau hasil skrining.", 12, false, Theme.Muted),
                chatButton
            }
        }, 16), 3, 0);

        AddToGrid(grid, Theme.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Theme.Label("E-Book Library", 14, true, Theme.PrimaryDark),
                Theme.Label("Buka 8 bab interaktif: pengertian, ukur tekanan darah, recheck, red flag, dan pencegahan.", 12, false, Theme.Muted),
                ebookButton
            }
        }, 16), 4, 0);

        return grid;
    }

    private static View MetricTile(string title, string value, string desc, Color accent)
    {
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 7,
            Children =
            {
                Theme.Badge(title, accent),
                Theme.Label(value, 22, true, Theme.Dark),
                Theme.Label(desc, 12, false, Theme.Muted)
            }
        }, 16);
    }

    private View MainGrid(Button analyze, Button reset)
    {
        var grid = new Grid
        {
            ColumnSpacing = 18,
            RowSpacing = 18,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.38, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.82, GridUnitType.Star) }
            }
        };

        AddToGrid(grid, FormCard(analyze, reset), 0, 0);
        AddToGrid(grid, SidePanel(), 1, 0);
        return grid;
    }

    private View FormCard(Button analyze, Button reset)
    {
        var identity = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionHeader("1. Profil & Keluhan", "Isi data dasar dan keluhan utama sebelum membaca risiko hipertensi."),
                TwoColumn(Theme.Field("Nama", _name), Theme.Field("Usia", _age)),
                GenderField(),
                Theme.Field("Keluhan Utama", _complaint),
                Theme.Field("Durasi Keluhan", _duration)
            }
        };

        var bp = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionHeader("2. Smart BP Check", "Masukkan tekanan darah awal. Jika tinggi, istirahat 5 menit lalu isi pengukuran ulang."),
                TwoColumn(Theme.Field("Sistolik Awal (mmHg)", _systolic), Theme.Field("Diastolik Awal (mmHg)", _diastolic)),
                TwoColumn(Theme.Field("Sistolik Ulang", _recheckSystolic), Theme.Field("Diastolik Ulang", _recheckDiastolic)),
                Theme.Notice("Fitur Smart BP Recheck membantu membandingkan tekanan darah awal dan ulang agar hasil tidak hanya bergantung pada satu kali pengukuran.")
            }
        };

        var vital = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionHeader("3. Vital Pendukung", "Parameter ini membantu membaca prioritas kardiovaskular secara lebih aman."),
                TwoColumn(Theme.Field("Denyut Nadi / BPM", _bpm), Theme.Field("SpO2 (%)", _spo2)),
                TwoColumn(Theme.Field("Frekuensi Napas", _rr), Theme.Field("Suhu Tubuh (°C)", _temp))
            }
        };

        var indicators = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionHeader("4. Red Flag & Faktor Risiko", "Pilih tanda yang sesuai. Nyeri dada, sesak, pingsan, dan tekanan darah sangat tinggi akan memprioritaskan hasil."),
                IndicatorGrid(),
                ButtonRow(analyze, reset)
            }
        };

        var grid = new Grid
        {
            ColumnSpacing = 18,
            RowSpacing = 16,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        AddToGrid(grid, identity, 0, 0);
        AddToGrid(grid, bp, 1, 0);
        AddToGrid(grid, vital, 0, 1);
        AddToGrid(grid, indicators, 1, 1);
        return Theme.Card(grid, 20);
    }

    private View SidePanel()
    {
        return new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                Theme.Card(new VerticalStackLayout
                {
                    Spacing = 9,
                    Children =
                    {
                        Theme.SectionTitle("Alur Skrining"),
                        Theme.Label("Ikuti alur dari pengisian tanda vital, cek ulang tekanan darah, pemeriksaan tanda bahaya, lalu hasil prioritas dan catatan konsultasi.", 12, false, Theme.Muted),
                        FlowStep("01", "Isi tanda vital", "Tekanan darah, nadi, SpO₂, napas, suhu, dan durasi keluhan.", Theme.SoftCyan, Theme.Primary),
                        FlowStep("02", "Cek ulang tekanan darah", "Masukkan hasil recheck agar sistem tidak menilai dari satu angka saja.", Theme.SoftBlue, Theme.Cyan),
                        FlowStep("03", "Tinjau red flag", "Nyeri dada berat, sesak berat, pingsan, atau lemah mendadak.", Theme.SoftRed, Theme.Red),
                        FlowStep("04", "Simpan hasil & doctor note", "Hasil prioritas tersimpan dan ringkasan konsultasi otomatis dibuat.", Theme.SoftOrange, Theme.Orange),
                        FlowStep("05", "Baca Library 8 Bab", "Pelajari panduan hipertensi interaktif sesuai kebutuhan user.", Theme.SoftGreen, Theme.Green)
                    }
                }, 18),
                HistoryCard()
            }
        };
    }


    private static View FlowStep(string number, string title, string desc, Color bg, Color accent)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        AddToGrid(grid, new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = accent,
            Stroke = accent,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Content = new Label
            {
                Text = number,
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        }, 0, 0);

        AddToGrid(grid, new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Theme.Label(title, 13, true, Theme.PrimaryDark),
                Theme.Label(desc, 11, false, Theme.Muted)
            }
        }, 1, 0);

        return new Border
        {
            BackgroundColor = bg,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(12),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
            Content = grid
        };
    }


    private View IndicatorGrid()
    {
        var grid = new Grid
        {
            ColumnSpacing = 8,
            RowSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        AddToGrid(grid, CheckRow(c1, "Riwayat hipertensi"), 0, 0);
        AddToGrid(grid, CheckRow(c2, "Diabetes / jantung"), 1, 0);
        AddToGrid(grid, CheckRow(c3, "Nyeri dada berat"), 0, 1);
        AddToGrid(grid, CheckRow(c4, "Nyeri menjalar"), 1, 1);
        AddToGrid(grid, CheckRow(c5, "Sesak napas berat"), 0, 2);
        AddToGrid(grid, CheckRow(c6, "Keringat dingin"), 1, 2);
        AddToGrid(grid, CheckRow(c7, "Pingsan / lemah"), 0, 3);
        AddToGrid(grid, CheckRow(c8, "Merokok / stres"), 1, 3);

        return grid;
    }

    private View HistoryCard()
    {
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionHeader("Riwayat & Trend", "Hasil terakhir disimpan agar tekanan darah dan level prioritas dapat dipantau."),
                _history
            }
        }, 18);
    }

    private View GenderField()
    {
        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                Theme.Label("Jenis Kelamin", 12, true, Theme.Muted),
                new Border
                {
                    BackgroundColor = Theme.SoftGray,
                    Stroke = Theme.BorderLight,
                    StrokeThickness = 1,
                    Padding = new Thickness(12),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
                    Content = new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children = { _genderChips, _genderPreview }
                    }
                }
            }
        };
    }

    private void RenderGenderChips()
    {
        _genderChips.Children.Clear();
        var male = Theme.ChipButton("Laki-laki", _selectedGender == "Laki-laki");
        var female = Theme.ChipButton("Perempuan", _selectedGender == "Perempuan");

        male.Clicked += (_, _) => { _selectedGender = "Laki-laki"; _genderPreview.Text = "Jenis kelamin: Laki-laki"; RenderGenderChips(); };
        female.Clicked += (_, _) => { _selectedGender = "Perempuan"; _genderPreview.Text = "Jenis kelamin: Perempuan"; RenderGenderChips(); };
        _genderChips.Children.Add(male);
        _genderChips.Children.Add(female);
    }

    private static View CheckRow(CheckBox checkBox, string text)
    {
        checkBox.Color = Theme.Primary;
        checkBox.WidthRequest = 26;
        checkBox.HorizontalOptions = LayoutOptions.Center;
        checkBox.VerticalOptions = LayoutOptions.Center;

        var label = Theme.Label(text, 11.5, true, Theme.Dark);
        label.LineBreakMode = LineBreakMode.WordWrap;
        label.MaxLines = 2;
        label.VerticalTextAlignment = TextAlignment.Center;

        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(32) },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };
        AddToGrid(grid, checkBox, 0, 0);
        AddToGrid(grid, label, 1, 0);

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(10, 8),
            MinimumHeightRequest = 52,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = grid
        };
    }

    private static View ButtonRow(Button analyze, Button reset)
    {
        analyze.HorizontalOptions = LayoutOptions.Fill;
        reset.HorizontalOptions = LayoutOptions.Fill;
        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) }
            }
        };
        AddToGrid(grid, analyze, 0, 0);
        AddToGrid(grid, reset, 1, 0);
        return grid;
    }

    private async void OnProcess(object? sender, EventArgs e)
    {
        var result = FuzzyEngine.Analyze(
            Int(_systolic),
            Int(_diastolic),
            Int(_recheckSystolic),
            Int(_recheckDiastolic),
            Int(_bpm),
            Int(_rr),
            Int(_spo2),
            Dbl(_temp),
            Int(_duration),
            c1.IsChecked,
            c2.IsChecked,
            c3.IsChecked,
            c4.IsChecked,
            c5.IsChecked,
            c6.IsChecked,
            c7.IsChecked,
            c8.IsChecked
        );

        var record = new ScreeningRecord
        {
            Username = AppData.CurrentUser?.Username ?? "",
            Name = string.IsNullOrWhiteSpace(_name.Text) ? "User" : _name.Text,
            Age = Int(_age),
            Gender = _selectedGender,
            ChiefComplaint = string.IsNullOrWhiteSpace(_complaint.Text) ? "Tidak diisi" : _complaint.Text,
            Systolic = Int(_systolic),
            Diastolic = Int(_diastolic),
            RecheckSystolic = Int(_recheckSystolic),
            RecheckDiastolic = Int(_recheckDiastolic),
            Bpm = Int(_bpm),
            SpO2 = Int(_spo2),
            Rr = Int(_rr),
            Temperature = Dbl(_temp),
            CoughDays = Int(_duration),
            SymptomScore = result.SymptomScore,
            RiskScore = result.RiskScore,
            Result = result.Result,
            Recommendation = result.Recommendation,
            SymptomDetails = result.SymptomDetails,
            Reason = result.Reason,
            BpTrendNote = result.BpTrendNote
        };
        record.NnSummary = NeuralNetworkEngine.Simulate(record);

        AppData.AddRecord(record);
        LoadHistory();

        await DisplayAlert("Tersimpan", "Hasil skrining hipertensi dan prioritas kardiovaskular berhasil disimpan.", "OK");
        await Navigation.PushAsync(new ScreeningResultPage(record));
    }

    private void LoadHistory()
    {
        _history.Children.Clear();
        var records = AppData.MyRecords().AsEnumerable().Reverse().Take(5).ToList();

        if (records.Count == 0)
        {
            UpdateLatest("Belum ada hasil", "Isi skrining hipertensi untuk melihat prioritas awal.");
            _history.Children.Add(Theme.CyanCard(Theme.Label("Belum ada riwayat skrining.", 12, false, Theme.Muted), 12));
            return;
        }

        var latest = records.First();
        UpdateLatest(latest.Result, $"TD {latest.Systolic}/{latest.Diastolic} mmHg • Skor {latest.RiskScore}% • {latest.BpTrendNote}");

        foreach (var record in records)
            _history.Children.Add(HistoryItem(record));
    }

    private void UpdateLatest(string level, string summary)
    {
        _latestLevel.Text = level;
        _latestLevel.TextColor = Theme.StatusColor(level);
        _latestSummary.Text = summary;
        _latestBox.BackgroundColor = Theme.StatusSoftColor(level);
        _latestBox.Stroke = Theme.StatusColor(level);
    }

    private View HistoryItem(ScreeningRecord record)
    {
        var open = Theme.OutlineButton("Detail", Theme.Primary);
        open.HeightRequest = 34;
        open.WidthRequest = 90;
        open.FontSize = 11;
        open.Clicked += async (_, _) => await Navigation.PushAsync(new ScreeningResultPage(record));

        var top = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        AddToGrid(top, new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Theme.Label(record.Name, 13, true, Theme.Dark),
                Theme.Label($"{record.Date:dd/MM/yyyy HH:mm}", 11, false, Theme.Muted)
            }
        }, 0, 0);
        AddToGrid(top, open, 1, 0);

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(12),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    top,
                    Theme.StatusBadge(record.Result),
                    Theme.Label($"TD {record.Systolic}/{record.Diastolic} • Nadi {record.Bpm} • SpO2 {record.SpO2}%", 12, false, Theme.Dark),
                    Theme.Label(record.BpTrendNote, 11, false, Theme.Muted)
                }
            }
        };
    }

    private void ResetForm()
    {
        _name.Text = AppData.CurrentUser?.FullName ?? "";
        _age.Text = _complaint.Text = _systolic.Text = _diastolic.Text = _recheckSystolic.Text = _recheckDiastolic.Text = "";
        _bpm.Text = _spo2.Text = _rr.Text = _temp.Text = _duration.Text = "";
        c1.IsChecked = c2.IsChecked = c3.IsChecked = c4.IsChecked = c5.IsChecked = c6.IsChecked = c7.IsChecked = c8.IsChecked = false;
        _selectedGender = "Laki-laki";
        _genderPreview.Text = "Jenis kelamin: Laki-laki";
        RenderGenderChips();
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

    private static View TwoColumn(View left, View right)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };
        AddToGrid(grid, left, 0, 0);
        AddToGrid(grid, right, 1, 0);
        return grid;
    }

    private static int Int(Entry entry) => int.TryParse(entry.Text, out var value) ? value : 0;

    private static double Dbl(Entry entry)
    {
        return double.TryParse((entry.Text ?? "").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private async Task Logout()
    {
        bool confirm = await DisplayAlert("Keluar", "Apakah kamu ingin keluar dari MediGuardAccess?", "Ya", "Batal");
        if (!confirm) return;
        AppData.Logout();
        await Navigation.PopToRootAsync();
    }

    private static void AddToGrid(Grid grid, View view, int column, int row)
    {
        Grid.SetColumn(view, column);
        Grid.SetRow(view, row);
        grid.Children.Add(view);
    }
}
