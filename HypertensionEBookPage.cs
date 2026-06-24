using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class HypertensionEBookPage : ContentPage
{
    private readonly List<ChapterItem> _chapters;
    private readonly List<Button> _chapterButtons = new();
    private readonly VerticalStackLayout _detailHost = new() { Spacing = 18 };
    private readonly Label _chapterNumber = Theme.Label("BAB 1", 12, true, Theme.Primary);
    private readonly Label _chapterTitle = Theme.Label("", 28, true, Theme.PrimaryDark);
    private readonly Label _chapterSummary = Theme.Label("", 14, false, Theme.Muted);
    private readonly VerticalStackLayout _pointsList = new() { Spacing = 10 };
    private readonly VerticalStackLayout _tipsList = new() { Spacing = 10 };
    private readonly VerticalStackLayout _refsList = new() { Spacing = 8 };
    private readonly Label _takeawayTitle = Theme.Label("Takeaway penting", 16, true, Theme.PrimaryDark);
    private readonly Label _takeawayBody = Theme.Label("", 13, false, Theme.Dark);
    private int _selectedIndex;

    public HypertensionEBookPage()
    {
        Title = "Library Hipertensi";
        BackgroundColor = Theme.Bg;

        _chapters = BuildChapters();

        var layoutGrid = new Grid
        {
            ColumnSpacing = 18,
            RowSpacing = 18,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(0.82, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.58, GridUnitType.Star) }
            }
        };

        AddToGrid(layoutGrid, LeftPanel(), 0, 0);
        AddToGrid(layoutGrid, RightPanel(), 1, 0);

        Content = Theme.DesktopScroll(new VerticalStackLayout
        {
            Spacing = 18,
            Children =
            {
                Header(),
                layoutGrid,
                Theme.Notice("Library ini bersifat edukatif. Isi bab membantu user memahami hipertensi, hasil skrining, dan langkah aman berikutnya. Bukan diagnosis final dan tidak menggantikan pemeriksaan tenaga kesehatan.")
            }
        }, 1420);

        ShowChapter(0);
    }

    private View Header()
    {
        var left = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                Theme.Badge("Learning Library", Theme.Primary),
                Theme.Label("Panduan Interaktif Hipertensi", 30, true, Theme.PrimaryDark),
                Theme.Label("Buka per bab, pelajari inti materi secara ringkas, lalu kembali ke dashboard untuk menghubungkan edukasi dengan hasil skrining user.", 14, false, Theme.Muted)
            }
        };

        var info = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        AddToGrid(info, MetricCard("8", "Bab interaktif", Theme.SoftCyan), 0, 0);
        AddToGrid(info, MetricCard("2021–2026", "Rujukan ringkas", Theme.SoftBlue), 1, 0);
        AddToGrid(info, MetricCard("Smart BP", "Fitur pendukung", Theme.SoftOrange), 2, 0);

        var grid = new Grid
        {
            ColumnSpacing = 18,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };
        AddToGrid(grid, left, 0, 0);
        AddToGrid(grid, info, 1, 0);
        return Theme.HeroCard(grid, 22);
    }

    private View LeftPanel()
    {
        var chapterStack = new VerticalStackLayout { Spacing = 12 };
        for (var i = 0; i < _chapters.Count; i++)
        {
            var index = i;
            var chapter = _chapters[i];
            var button = CreateChapterButton(chapter, index);
            button.Clicked += (_, _) => ShowChapter(index);
            _chapterButtons.Add(button);
            chapterStack.Children.Add(button);
        }

        return Theme.Card(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                Theme.SectionTitle("Daftar Bab"),
                Theme.Label("Klik salah satu bab untuk membuka isi materi. Tiap bab ditampilkan pada layout kanan agar rapi, presisi, dan enak dibaca dalam mode landscape.", 13, false, Theme.Muted),
                chapterStack
            }
        }, 20);
    }

    private View RightPanel()
    {
        var summaryCard = Theme.Card(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                _chapterNumber,
                _chapterTitle,
                _chapterSummary
            }
        }, 20);

        var contentGrid = new Grid
        {
            ColumnSpacing = 14,
            RowSpacing = 14,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1.12, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.88, GridUnitType.Star) }
            }
        };

        AddToGrid(contentGrid, Theme.Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                Theme.SectionTitle("Inti materi"),
                _pointsList
            }
        }, 20), 0, 0);

        AddToGrid(contentGrid, new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                Theme.CyanCard(new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        Theme.SectionTitle("Praktik yang bisa dilakukan"),
                        _tipsList
                    }
                }, 18),
                Theme.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        _takeawayTitle,
                        _takeawayBody
                    }
                }, 18),
                Theme.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        Theme.SectionTitle("Rujukan inti"),
                        _refsList
                    }
                }, 18)
            }
        }, 1, 0);

        _detailHost.Children.Add(summaryCard);
        _detailHost.Children.Add(contentGrid);
        return _detailHost;
    }

    private Button CreateChapterButton(ChapterItem chapter, int index)
    {
        return new Button
        {
            Text = $"Bab {index + 1} • {chapter.Title}",
            BackgroundColor = Colors.White,
            TextColor = Theme.PrimaryDark,
            BorderColor = Theme.BorderLight,
            BorderWidth = 1,
            CornerRadius = 18,
            HeightRequest = 52,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(16, 0)
        };
    }

    private void ShowChapter(int index)
    {
        _selectedIndex = index;
        var chapter = _chapters[index];

        _chapterNumber.Text = $"BAB {index + 1}";
        _chapterTitle.Text = chapter.Title;
        _chapterSummary.Text = chapter.Summary;
        _takeawayBody.Text = chapter.Takeaway;

        _pointsList.Children.Clear();
        foreach (var point in chapter.Points)
            _pointsList.Children.Add(BulletCard(point, Theme.Primary));

        _tipsList.Children.Clear();
        foreach (var tip in chapter.Tips)
            _tipsList.Children.Add(BulletCard(tip, Theme.Orange));

        _refsList.Children.Clear();
        foreach (var item in chapter.References)
            _refsList.Children.Add(Theme.Label("• " + item, 12, false, Theme.Muted));

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        for (var i = 0; i < _chapterButtons.Count; i++)
        {
            var selected = i == _selectedIndex;
            _chapterButtons[i].BackgroundColor = selected ? Theme.PrimaryDark : Colors.White;
            _chapterButtons[i].TextColor = selected ? Colors.White : Theme.PrimaryDark;
            _chapterButtons[i].BorderColor = selected ? Theme.PrimaryDark : Theme.BorderLight;
        }
    }

    private static View MetricCard(string value, string label, Color bg)
    {
        return new Border
        {
            BackgroundColor = bg,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(14),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Content = new VerticalStackLayout
            {
                Spacing = 3,
                Children =
                {
                    Theme.Label(value, 20, true, Theme.PrimaryDark),
                    Theme.Label(label, 11, true, Theme.Muted)
                }
            }
        };
    }

    private static View BulletCard(string text, Color accent)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(12),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Content = new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Border
                    {
                        WidthRequest = 12,
                        HeightRequest = 12,
                        BackgroundColor = accent,
                        Stroke = accent,
                        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(0, 5, 0, 0)
                    },
                    new Label
                    {
                        Text = text,
                        FontSize = 12.5,
                        TextColor = Theme.Dark,
                        LineBreakMode = LineBreakMode.WordWrap,
                        HorizontalOptions = LayoutOptions.Fill,
                        HorizontalTextAlignment = TextAlignment.Start
                    }
                }
            }
        };
    }

    private static List<ChapterItem> BuildChapters() =>
    [
        new ChapterItem
        {
            Title = "Mengenal hipertensi",
            Summary = "Bab ini menjelaskan apa itu hipertensi, mengapa sering disebut silent killer, dan alasan kondisi ini perlu dipantau sejak dini.",
            Takeaway = "Hipertensi tidak selalu menimbulkan gejala, sehingga pemeriksaan tekanan darah tetap penting walaupun tubuh terasa baik-baik saja.",
            Points =
            [
                "Hipertensi adalah kondisi tekanan darah menetap tinggi; secara umum konteks klinis sering menggunakan batas 140/90 mmHg atau lebih pada pengukuran yang valid.",
                "Tekanan darah tinggi yang tidak terkontrol berhubungan dengan peningkatan risiko stroke, penyakit jantung, gagal ginjal, dan komplikasi pembuluh darah.",
                "Banyak orang tidak sadar memiliki hipertensi karena keluhan bisa ringan, tidak khas, atau bahkan tidak ada sama sekali.",
                "Karena itu, skrining awal seperti pada MediGuardAccess difokuskan untuk membantu deteksi prioritas, bukan memberi diagnosis pasti."
            ],
            Tips =
            [
                "Kenali riwayat keluarga, kebiasaan garam tinggi, obesitas, kurang aktivitas, merokok, dan stres sebagai faktor yang sering berperan.",
                "Bila sering pusing, mudah lelah, atau tekanan darah pernah tinggi, lakukan pemeriksaan ulang dengan teknik yang benar.",
                "Simpan hasil pengukuran agar pola tekanan darah lebih mudah terlihat dari waktu ke waktu."
            ],
            References = ["WHO Hypertension Fact Sheet", "2024 ESC Guidelines", "Kemenkes RI Hipertensi"]
        },
        new ChapterItem
        {
            Title = "Cara ukur tekanan darah yang benar",
            Summary = "Bab ini membantu user memahami bahwa angka tekanan darah baru bermakna bila pengukurannya dilakukan dengan posisi dan situasi yang tepat.",
            Takeaway = "Teknik ukur yang salah dapat membuat tekanan darah tampak lebih tinggi atau lebih rendah dari kondisi sebenarnya.",
            Points =
            [
                "Istirahat duduk tenang sekitar 5 menit sebelum pengukuran agar tubuh tidak berada pada fase stres sesaat.",
                "Hindari kopi, teh berkafein tinggi, merokok, atau aktivitas fisik berat sekitar 30 menit sebelum ukur bila memungkinkan.",
                "Duduk dengan punggung bersandar, kaki menapak lantai, dan lengan sejajar dengan tinggi jantung.",
                "Gunakan manset yang sesuai ukuran lengan dan lakukan pengukuran lebih dari satu kali bila hasil awal tinggi."
            ],
            Tips =
            [
                "Catat tanggal, jam, dan kondisi saat pengukuran, misalnya sesudah olahraga, kurang tidur, atau sedang cemas.",
                "Bandingkan hasil rumah dan hasil fasilitas kesehatan bila ada perbedaan besar.",
                "Jangan menilai satu angka saja; lihat tren dari beberapa pengukuran."
            ],
            References = ["ESH 2023 Guidelines", "AHA/ACC BP Measurement Advice", "Kemenkes RI"]
        },
        new ChapterItem
        {
            Title = "Membaca angka tekanan darah",
            Summary = "Bab ini menjelaskan arti sistolik, diastolik, serta bagaimana user memahami hasil pengukuran tanpa panik berlebihan.",
            Takeaway = "Sistolik menunjukkan tekanan saat jantung memompa, sedangkan diastolik menunjukkan tekanan saat jantung berelaksasi.",
            Points =
            [
                "Angka sistolik adalah angka atas; angka diastolik adalah angka bawah pada hasil tekanan darah.",
                "Semakin tinggi hasil yang konsisten, semakin besar kebutuhan untuk evaluasi lanjutan dan pengendalian faktor risiko.",
                "Hasil yang mendekati batas tinggi tetap perlu diperhatikan, terutama bila ada faktor risiko lain seperti diabetes, merokok, atau obesitas.",
                "Pada MediGuardAccess, angka vital dipakai bersama gejala red flag untuk menentukan prioritas Normal, Waspada, atau Berisiko."
            ],
            Tips =
            [
                "Jangan langsung menyimpulkan hipertensi hanya dari satu kali ukur bila sedang sakit, nyeri, atau cemas.",
                "Gunakan istilah 'perlu recheck' untuk hasil yang masih meragukan.",
                "Bawa catatan hasil ke dokter bila pola tekanan darah berulang kali tinggi."
            ],
            References = ["2024 ESC Guidelines", "ISH Lifestyle Paper 2023", "WHO"]
        },
        new ChapterItem
        {
            Title = "Indikasi awal dan faktor risiko",
            Summary = "Bab ini merangkum keluhan awal yang kadang muncul pada hipertensi serta faktor risiko yang mempermudah terjadinya tekanan darah tinggi.",
            Takeaway = "Keluhan awal bisa tidak spesifik; fokus utama adalah mengenali kombinasi tekanan darah, gejala, dan faktor risiko.",
            Points =
            [
                "Beberapa orang mengeluh sakit kepala, tengkuk berat, berdebar, atau mudah lelah, tetapi gejala ini tidak selalu berarti hipertensi dan perlu konteks angka tekanan darah.",
                "Faktor risiko yang sering muncul meliputi usia, riwayat keluarga, berat badan berlebih, konsumsi garam tinggi, kurang gerak, rokok, dan tidur buruk.",
                "Stres kronis, konsumsi alkohol berlebih, dan penyakit metabolik juga dapat memperburuk kontrol tekanan darah.",
                "Karena itulah skrining tidak cukup menanyakan satu gejala saja; perlu pembacaan kondisi tubuh secara lebih menyeluruh."
            ],
            Tips =
            [
                "Isi riwayat gaya hidup dengan jujur agar rekomendasi lebih relevan.",
                "Kenali pola kapan keluhan muncul: saat aktivitas, saat emosional, atau saat tekanan darah sedang tinggi.",
                "Bila memiliki faktor risiko banyak, lakukan pengukuran lebih rutin walau belum ada keluhan."
            ],
            References = ["WHO", "Kemenkes RI", "Hypertension risk factor literature 2021–2026"]
        },
        new ChapterItem
        {
            Title = "Smart BP Recheck",
            Summary = "Bab ini membahas pentingnya pengukuran ulang agar keputusan tidak bergantung pada satu pembacaan tekanan darah semata.",
            Takeaway = "Recheck membantu membedakan kenaikan sementara dari pola tekanan darah yang memang perlu perhatian lebih lanjut.",
            Points =
            [
                "Jika hasil awal tinggi, user dianjurkan istirahat beberapa menit, tenangkan diri, lalu lakukan pengukuran ulang.",
                "Perbedaan besar antara hasil awal dan hasil ulang dapat memberi sinyal bahwa faktor situasional memengaruhi angka pertama.",
                "Bila hasil awal dan ulang tetap tinggi, prioritas evaluasi meningkat dan user sebaiknya mempertimbangkan konsultasi klinis.",
                "Fitur Smart BP Recheck pada project ini menjadi nilai tambah agar skrining lebih logis dan tidak terlalu linear."
            ],
            Tips =
            [
                "Gunakan alat yang sama bila memungkinkan agar pembacaan lebih konsisten.",
                "Jangan mengukur saat baru selesai menaiki tangga atau terburu-buru.",
                "Bandingkan dua hasil dan lihat apakah selisihnya kecil, sedang, atau besar."
            ],
            References = ["Home BP monitoring guidance", "ESH 2023", "ESC 2024"]
        },
        new ChapterItem
        {
            Title = "Pencegahan dan kontrol sehari-hari",
            Summary = "Bab ini berisi strategi pencegahan hipertensi serta pengendalian sederhana yang realistis dilakukan user sehari-hari.",
            Takeaway = "Pencegahan terbaik bukan hanya obat, tetapi kombinasi cek rutin, pola makan lebih sehat, gerak tubuh, tidur cukup, dan pengelolaan stres.",
            Points =
            [
                "Kurangi konsumsi garam berlebihan, makanan ultra-proses, dan minuman tinggi gula sebagai bagian dari kontrol gaya hidup.",
                "Aktivitas fisik teratur membantu memperbaiki kebugaran jantung dan kontrol tekanan darah.",
                "Tidur cukup dan pengelolaan stres berperan penting karena tubuh yang terus tertekan cenderung mempertahankan respons simpatis tinggi.",
                "Konsep nasional seperti CERDIK dan PATUH dapat dipakai sebagai panduan perilaku sehari-hari."
            ],
            Tips =
            [
                "Mulai dari target kecil: jalan kaki rutin, kurangi makanan asin, dan catat tekanan darah mingguan.",
                "Bila sudah diberi obat oleh dokter, konsumsi teratur dan jangan menghentikan tanpa arahan medis.",
                "Libatkan keluarga agar perubahan gaya hidup lebih mudah dijalankan."
            ],
            References = ["ISH Lifestyle 2023", "CERDIK & PATUH Kemenkes", "WHO lifestyle advice"]
        },
        new ChapterItem
        {
            Title = "Red flag dan kapan ke fasilitas kesehatan",
            Summary = "Bab ini membantu user membedakan kondisi yang masih bisa dipantau dari kondisi yang memerlukan evaluasi medis lebih cepat atau segera.",
            Takeaway = "Nyeri dada berat, sesak berat, pingsan, kelemahan mendadak, gangguan bicara, atau tekanan darah sangat tinggi dengan gejala berat tidak boleh ditunda.",
            Points =
            [
                "Red flag kardiovaskular dapat menandakan komplikasi serius seperti krisis hipertensi, gangguan jantung, atau gangguan neurologis.",
                "Keluhan seperti nyeri dada menekan, sesak berat, sakit kepala hebat tiba-tiba, pandangan kabur berat, atau kelumpuhan mendadak perlu penanganan cepat.",
                "Pada aplikasi, bila red flag dicentang atau kombinasi tanda vital sangat buruk muncul, sistem menaikkan prioritas menjadi Berisiko.",
                "Langkah paling aman adalah segera mencari bantuan medis, bukan menunda sambil hanya mengandalkan aplikasi."
            ],
            Tips =
            [
                "Segera ke IGD atau hubungi bantuan bila muncul gejala bahaya mendadak.",
                "Bawa catatan tekanan darah, daftar obat, dan riwayat keluhan saat berkonsultasi.",
                "Jangan menyetir sendiri bila merasa lemah berat atau hampir pingsan."
            ],
            References = ["Emergency hypertension red flag guidance", "ESC 2024", "National referral advice"]
        },
        new ChapterItem
        {
            Title = "Riwayat, doctor note, dan tindak lanjut",
            Summary = "Bab terakhir menghubungkan edukasi dengan fitur project: riwayat hasil, doctor note, dan langkah tindak lanjut yang bisa dilakukan user.",
            Takeaway = "Skrining akan lebih bermanfaat bila hasilnya dicatat, dipelajari, dan dibawa sebagai bahan konsultasi, bukan hanya dibaca sekali lalu ditinggalkan.",
            Points =
            [
                "Riwayat hasil membantu user melihat pola: apakah tekanan darah cenderung stabil, meningkat, atau sering disertai red flag.",
                "Doctor note berfungsi sebagai ringkasan konsultasi awal agar user lebih mudah menjelaskan keluhan saat bertemu tenaga kesehatan.",
                "AI navigator dapat membantu menjelaskan hasil dengan bahasa formal atau nonformal, tetapi tetap harus aman dan tidak melampaui batas edukasi medis.",
                "Pengelolaan jangka panjang membutuhkan kedisiplinan monitoring, gaya hidup sehat, dan evaluasi profesional bila hasil tetap buruk."
            ],
            Tips =
            [
                "Gunakan riwayat hasil untuk membandingkan perbaikan setelah perubahan gaya hidup.",
                "Cetak atau catat ringkasan doctor note sebelum kontrol.",
                "Buat target sederhana: cek berkala, ikuti saran dokter, dan evaluasi ulang faktor risiko."
            ],
            References = ["Digital self-management concepts", "Patient education literature", "Kemenkes + WHO"]
        }
    ];

    private sealed class ChapterItem
    {
        public string Title { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public string Takeaway { get; init; } = string.Empty;
        public List<string> Points { get; init; } = new();
        public List<string> Tips { get; init; } = new();
        public List<string> References { get; init; } = new();
    }

    private static void AddToGrid(Grid grid, View view, int col, int row)
    {
        Grid.SetColumn(view, col);
        Grid.SetRow(view, row);
        grid.Children.Add(view);
    }
}
