using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class ScreeningResultPage : ContentPage
{
    private readonly ScreeningRecord _record;

    public ScreeningResultPage(ScreeningRecord record)
    {
        _record = record;
        Title = "Hasil Skrining Hipertensi";
        BackgroundColor = Theme.Bg;

        Content = Theme.DesktopScroll(new VerticalStackLayout
        {
            Spacing = 18,
            Children =
            {
                Header(),
                ResultHero(),
                DetailGrid(),
                SmartBpCard(),
                RedFlagCard(),
                ReasonAndAiCard(),
                ReferenceCard(),
                RecommendationCard(),
                ActionButtons()
            }
        }, 1360);
    }

    private View Header()
    {
        var grid = new Grid
        {
            ColumnSpacing = 16,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        Add(grid, Theme.LogoIcon(58), 0, 0);
        Add(grid, new VerticalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Theme.Label("Hasil Skrining Hipertensi", 26, true, Theme.PrimaryDark),
                Theme.Label("Ringkasan tekanan darah, red flag kardiovaskular, Smart BP Recheck, dan rekomendasi awal.", 13, false, Theme.Muted)
            }
        }, 1, 0);
        Add(grid, Theme.StatusBadge(_record.Result), 2, 0);
        return Theme.HeroCard(grid, 18);
    }

    private View ResultHero()
    {
        string body =
            $"Nama: {_record.Name}\n" +
            $"Keluhan utama: {_record.ChiefComplaint}\n" +
            $"Waktu skrining: {_record.Date:dd/MM/yyyy HH:mm}\n" +
            $"Level prioritas: {_record.Result}\n" +
            $"Perkiraan tingkat perhatian: {_record.RiskScore}%\n\n" +
            "Catatan: hasil ini adalah skrining awal risiko hipertensi/kardiovaskular, bukan diagnosis final.";

        return Theme.ResultPanel(_record.Result, body, _record.Result);
    }

    private View DetailGrid()
    {
        var grid = new Grid
        {
            ColumnSpacing = 14,
            RowSpacing = 14,
            ColumnDefinitions =
            {
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

        int finalSys = _record.RecheckSystolic > 0 ? _record.RecheckSystolic : _record.Systolic;
        int finalDia = _record.RecheckDiastolic > 0 ? _record.RecheckDiastolic : _record.Diastolic;

        Add(grid, InfoCard("Tekanan Darah", $"{finalSys}/{finalDia}", Friendly(FuzzyEngine.BpReference(finalSys, finalDia)), FuzzyEngine.LevelBp(finalSys, finalDia)), 0, 0);
        Add(grid, InfoCard("Denyut Nadi", $"{_record.Bpm}", Friendly(FuzzyEngine.BpmReference(_record.Bpm)), FuzzyEngine.LevelBpm(_record.Bpm)), 1, 0);
        Add(grid, InfoCard("SpO2", $"{_record.SpO2}%", Friendly(FuzzyEngine.SpO2Reference(_record.SpO2)), FuzzyEngine.LevelSpO2(_record.SpO2)), 2, 0);
        Add(grid, InfoCard("Frekuensi Napas", $"{_record.Rr}", Friendly(FuzzyEngine.RrReference(_record.Rr)), FuzzyEngine.LevelRr(_record.Rr)), 0, 1);
        Add(grid, InfoCard("Suhu Tubuh", $"{_record.Temperature:0.0}°C", Friendly(FuzzyEngine.TempReference(_record.Temperature)), FuzzyEngine.LevelTemp(_record.Temperature)), 1, 1);
        Add(grid, InfoCard("Skor Red Flag", $"{_record.SymptomScore}", Friendly(FuzzyEngine.SymptomReference(_record.SymptomScore)), FuzzyEngine.LevelSymptoms(_record.SymptomScore)), 2, 1);

        return grid;
    }

    private static View InfoCard(string title, string value, string subtitle, int level)
    {
        Color accent = level switch { 0 => Theme.Green, 1 => Theme.Orange, _ => Theme.Red };
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                Theme.Badge(title, accent),
                Theme.Label(value, 24, true, Theme.Dark),
                Theme.Label(subtitle, 11.5, false, Theme.Muted)
            }
        }, 16);
    }

    private View SmartBpCard()
    {
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Theme.SectionTitle("Smart BP Recheck & Trend Reminder"),
                Theme.Label("Fitur ini membandingkan pengukuran awal dan ulang setelah jeda. Tujuannya membantu user tidak mengambil kesimpulan dari satu kali pengukuran saja.", 12.5, false, Theme.Muted),
                new Border
                {
                    BackgroundColor = Theme.SoftOrange,
                    Stroke = Theme.Orange,
                    StrokeThickness = 1,
                    Padding = new Thickness(14),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
                    Content = Theme.Label(_record.BpTrendNote, 13, false, Theme.Dark)
                }
            }
        }, 18);
    }

    private View RedFlagCard()
    {
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                Theme.SectionTitle("Red flag dan faktor risiko yang dipilih"),
                Theme.Label("Bagian ini menunjukkan indikator yang ikut menaikkan level prioritas kardiovaskular.", 12.5, false, Theme.Muted),
                new Border
                {
                    BackgroundColor = Theme.SoftBlue,
                    Stroke = Theme.BorderLight,
                    StrokeThickness = 1,
                    Padding = new Thickness(12),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
                    Content = Theme.Label(_record.SymptomDetails, 12.5, false, Theme.Dark)
                }
            }
        }, 18);
    }

    private View ReasonAndAiCard()
    {
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Theme.SectionTitle("Alasan hasil dan simulasi AI"),
                Theme.Label(FriendlyReason(_record.Reason), 13, false, Theme.Dark),
                new Border
                {
                    BackgroundColor = Theme.SoftCyan,
                    Stroke = Theme.BorderLight,
                    StrokeThickness = 1,
                    Padding = new Thickness(12),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
                    Content = Theme.Label(_record.NnSummary, 12.5, false, Theme.Dark)
                }
            }
        }, 18);
    }

    private View ReferenceCard()
    {
        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Theme.SectionTitle("Patokan pembacaan awal"),
                ReferenceLine("Tekanan darah", "<130/<85 relatif aman • 130–139/85–89 perlu dipantau • ≥140/90 perlu perhatian • ≥160/100 berisiko lebih tinggi"),
                ReferenceLine("Red flag", "nyeri dada berat, nyeri menjalar, sesak berat, keringat dingin, pingsan, atau lemah mendadak harus diprioritaskan"),
                ReferenceLine("Smart BP Recheck", "jika hasil awal tinggi, istirahat sekitar 5 menit lalu lakukan pengukuran ulang dan catat hasilnya"),
                ReferenceLine("Batas sistem", "MediGuardAccess membantu skrining awal, bukan diagnosis hipertensi final")
            }
        }, 18);
    }

    private static View ReferenceLine(string title, string text)
    {
        return new Border
        {
            BackgroundColor = Theme.SoftBlue,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(12, 10),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = new VerticalStackLayout
            {
                Spacing = 3,
                Children = { Theme.Label(title, 12.5, true, Theme.Dark), Theme.Label(text, 11.5, false, Theme.Muted) }
            }
        };
    }

    private View RecommendationCard()
    {
        return new Border
        {
            BackgroundColor = Theme.StatusSoftColor(_record.Result),
            Stroke = Theme.StatusColor(_record.Result),
            StrokeThickness = 1,
            Padding = new Thickness(18),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    Theme.Label("Rekomendasi Awal", 18, true, Theme.StatusColor(_record.Result)),
                    Theme.Label(_record.Recommendation, 14, false, Theme.Dark),
                    Theme.Label(DoctorNote(), 12.5, false, Theme.Muted)
                }
            }
        };
    }

    private string DoctorNote()
    {
        int finalSys = _record.RecheckSystolic > 0 ? _record.RecheckSystolic : _record.Systolic;
        int finalDia = _record.RecheckDiastolic > 0 ? _record.RecheckDiastolic : _record.Diastolic;
        return $"Doctor note: {_record.Name}, {_record.Age} tahun, keluhan utama {_record.ChiefComplaint}, tekanan darah {finalSys}/{finalDia} mmHg, nadi {_record.Bpm}, SpO2 {_record.SpO2}%, level {_record.Result}.";
    }

    private static string Friendly(string text) => text.Replace("Waspada", "perlu dipantau").Replace("Berisiko", "perlu perhatian").Replace("Normal", "aman").Replace("acuan:", "patokan:");

    private static string FriendlyReason(string text) => string.IsNullOrWhiteSpace(text)
        ? "Hasil dibaca dari tekanan darah, tanda vital, dan red flag."
        : text.Replace("Output Normal karena", "Hasil aman karena")
              .Replace("Output Waspada karena", "Hasil perlu dipantau karena")
              .Replace("Output Berisiko karena", "Hasil perlu perhatian karena")
              .Replace("SpO2", "kadar oksigen darah");

    private View ActionButtons()
    {
        var back = Theme.Button("Kembali ke Dashboard", Theme.Primary);
        back.Clicked += async (_, _) => await Navigation.PopAsync();
        var again = Theme.OutlineButton("Isi Skrining Lagi", Theme.PrimaryDark);
        again.Clicked += async (_, _) => await Navigation.PopAsync();
        return new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            Children = { back, again }
        };
    }

    private static void Add(Grid grid, View view, int col, int row)
    {
        Grid.SetColumn(view, col);
        Grid.SetRow(view, row);
        grid.Children.Add(view);
    }
}
