using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class ChatbotView : Border
{
    private readonly VerticalStackLayout _messages = new()
    {
        Spacing = 10,
        Padding = new Thickness(14)
    };

    private readonly Entry _input = Theme.Entry("Tulis gejala, hasil tekanan darah, atau pertanyaan...");
    private readonly Button _sendButton = Theme.Button("Kirim", Theme.Primary);
    private readonly Button _closeButton = Theme.OutlineButton("Tutup", Theme.Red);

    private readonly ScrollView _scroll;
    private readonly List<AiChatMessage> _chatHistory = new();

    private bool _isSending;

    public ChatbotView()
    {
        BackgroundColor = Colors.White;
        Stroke = Theme.BorderLight;
        StrokeThickness = 1;
        Padding = 0;
        StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(22)
        };

        Shadow = new Shadow
        {
            Brush = Brush.Black,
            Opacity = 0.10f,
            Radius = 18,
            Offset = new Point(0, 8)
        };

        WidthRequest = 430;
        HeightRequest = 610;

        _input.ReturnType = ReturnType.Send;
        _input.Completed += async (_, _) => await SendMessage();

        _sendButton.WidthRequest = 82;
        _sendButton.HeightRequest = 42;
        _sendButton.Clicked += async (_, _) => await SendMessage();

        _closeButton.HeightRequest = 38;
        _closeButton.WidthRequest = 78;
        _closeButton.Clicked += (_, _) => IsVisible = false;

        _scroll = new ScrollView
        {
            Orientation = ScrollOrientation.Vertical,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            Content = _messages
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        AddToGrid(root, Header(), 0);
        AddToGrid(root, QuickButtons(), 1);
        AddToGrid(root, ChatArea(), 2);
        AddToGrid(root, InputArea(), 3);

        Content = root;

        AddBot(
            "Halo, aku MediGuard AI. Kamu bisa bertanya tentang keluhan, hasil screening, atau istilah kesehatan dengan bahasa yang sederhana."
        );
    }

    private View Header()
    {
        var title = new HorizontalStackLayout
        {
            Spacing = 12,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Theme.LogoIcon(46),

                new VerticalStackLayout
                {
                    Spacing = 2,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "MediGuard AI",
                            FontSize = 19,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White
                        },
                        new Label
                        {
                            Text = "Pendamping skrining • jawaban ramah dan mudah dipahami",
                            FontSize = 11,
                            TextColor = Color.FromArgb("#DFF8E8")
                        }
                    }
                }
            }
        };

        var statusBadge = Theme.Badge("Mode Medis", Theme.Primary, Colors.White);
        statusBadge.VerticalOptions = LayoutOptions.Center;

        var top = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        Grid.SetColumn(title, 0);
        Grid.SetColumn(_closeButton, 1);

        top.Children.Add(title);
        top.Children.Add(_closeButton);

        var subtitle = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#BFEAD0"),
            Padding = new Thickness(12, 8),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(16)
            },
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    new Label
                    {
                        Text = "Tulis keluhan, hasil skrining, atau pertanyaan kesehatan awal. Jawaban dibuat sederhana, mudah dipahami, dan tetap aman sebagai skrining awal, bukan diagnosis final.",
                        FontSize = 11,
                        TextColor = Theme.Dark,
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    statusBadge
                }
            }
        };

        Grid.SetColumn(statusBadge, 1);

        return new Border
        {
            BackgroundColor = Theme.Primary,
            Stroke = Colors.Transparent,
            Padding = new Thickness(16, 14),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(22, 22, 0, 0)
            },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    top,
                    subtitle
                }
            }
        };
    }

    private View QuickButtons()
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#F4FBF7"),
            Stroke = Colors.Transparent,
            Padding = new Thickness(0),
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                Padding = new Thickness(12, 10),
                Content = new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        QuickButton("Jelaskan hasil saya"),
                        QuickButton("Apa itu hipertensi?"),
                        QuickButton("Tekanan darah saya tinggi"),
                        QuickButton("Nyeri dada berat"),
                        QuickButton("Kapan harus ke dokter?")
                    }
                }
            }
        };
    }

    private Button QuickButton(string text)
    {
        var button = Theme.ChipButton(text, false);

        button.HeightRequest = 36;
        button.FontSize = 11;

        button.Clicked += async (_, _) =>
        {
            _input.Text = text;
            await SendMessage();
        };

        return button;
    }

    private View ChatArea()
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#F6FBF8"),
            Stroke = Colors.Transparent,
            Padding = 0,
            Content = _scroll
        };
    }

    private View InputArea()
    {
        var grid = new Grid
        {
            Padding = new Thickness(12),
            ColumnSpacing = 10,
            BackgroundColor = Color.FromArgb("#ECFDF5"),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var inputBox = Theme.FieldContainer(_input);

        Grid.SetColumn(inputBox, 0);
        Grid.SetColumn(_sendButton, 1);

        grid.Children.Add(inputBox);
        grid.Children.Add(_sendButton);

        return grid;
    }

    private async Task SendMessage()
    {
        if (_isSending)
            return;

        string text = _input.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(text))
            return;

        _isSending = true;
        _sendButton.IsEnabled = false;
        _sendButton.Text = "...";

        AddUser(text);
        _input.Text = "";

        _chatHistory.Add(new AiChatMessage("user", text));
        TrimHistory();

        var loading = AddBot("MediGuard AI sedang menyiapkan jawaban...");

        try
        {
            string reply;

            if (ShouldExplainLatestResult(text))
                reply = ExplainLatestResult();
            else
                reply = await GroqService.AskAsync(_chatHistory);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                loading.Text = reply;

                _chatHistory.Add(new AiChatMessage("assistant", reply));
                TrimHistory();

                await ScrollToBottom();
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                string fallback = NlpEngine.Reply(text);

                loading.Text =
                    "Chatbot sedang mengalami gangguan.\n\n" +
                    $"Detail: {ex.Message}\n\n" +
                    fallback;

                _chatHistory.Add(new AiChatMessage("assistant", fallback));
                TrimHistory();

                await ScrollToBottom();
            });
        }
        finally
        {
            _isSending = false;
            _sendButton.IsEnabled = true;
            _sendButton.Text = "Kirim";
        }
    }


    private static bool ShouldExplainLatestResult(string text)
    {
        string s = (text ?? "").Trim().ToLowerInvariant();
        return s.Contains("jelaskan hasil saya") ||
               s.Contains("hasil saya") ||
               s.Contains("kenapa hasil saya") ||
               s.Contains("arti hasil saya");
    }

    private static string ExplainLatestResult()
    {
        var record = AppData.MyRecords().LastOrDefault();

        if (record == null)
            return "Aku belum menemukan hasil skrining terbaru di akunmu. Silakan lakukan skrining dulu, lalu aku bisa bantu menjelaskannya dengan bahasa yang lebih sederhana.";

        string levelText = record.Result switch
        {
            "Normal" => "kondisimu saat ini cenderung aman pada skrining awal",
            "Waspada" => "ada beberapa hal yang perlu kamu pantau lebih dekat",
            _ => "ada beberapa tanda yang perlu lebih diperhatikan"
        };

        return
            $"Hasil terakhirmu masuk kategori {record.Result}. Artinya, {levelText}. " +
            $"Perkiraan tingkat perhatianmu sekitar {record.RiskScore}% dengan skor gejala {record.SymptomScore}. " +
            $"Hal yang paling memengaruhi hasil ini adalah: {FriendlyReason(record.Reason)} " +
            $"Saran awalnya: {record.Recommendation}";
    }

    private static string FriendlyReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return "gabungan data tubuh dan gejala yang kamu isi.";

        return reason
            .Replace("Output Normal karena", "hasil ini dipengaruhi oleh")
            .Replace("Output Waspada karena", "hasil ini dipengaruhi oleh")
            .Replace("Output Berisiko karena", "hasil ini dipengaruhi oleh")
            .Replace("SpO2", "kadar oksigen darah")
            .Replace("BPM", "denyut nadi")
            .Replace("Respiratory Rate", "frekuensi napas")
            .Replace("Suhu", "suhu tubuh")
            .Replace("Skor gejala", "jumlah gejala penting")
            .Replace("Berisiko", "perlu perhatian")
            .Replace("Waspada", "perlu dipantau")
            .Replace("terdapat tanda prioritas/red flag", "ada tanda yang perlu diperhatikan")
            .TrimEnd('.') + ".";
    }

    private void AddUser(string text)
    {
        var bubble = UserBubble(text);

        var row = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            Children =
            {
                bubble
            }
        };

        _messages.Children.Add(row);
        _ = ScrollToBottom();
    }

    private Label AddBot(string text)
    {
        var bubble = BotBubble(text);

        var row = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Start,
            Children =
            {
                bubble
            }
        };

        _messages.Children.Add(row);
        _ = ScrollToBottom();

        return bubble;
    }

    private static Label UserBubble(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = Colors.White,
            BackgroundColor = Theme.PrimaryDark,
            FontSize = 13,
            Padding = new Thickness(15, 11),
            MaximumWidthRequest = 305,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    private static Label BotBubble(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = Theme.Dark,
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            FontSize = 13,
            Padding = new Thickness(15, 11),
            MaximumWidthRequest = 330,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    private void TrimHistory()
    {
        while (_chatHistory.Count > 12)
        {
            _chatHistory.RemoveAt(0);
        }
    }

    private async Task ScrollToBottom()
    {
        try
        {
            await Task.Delay(120);

            double targetY = _messages.Height;

            if (targetY < 0)
                targetY = 0;

            await _scroll.ScrollToAsync(0, targetY, true);
        }
        catch
        {
            // Abaikan kalau UI belum siap.
        }
    }

    private static void AddToGrid(Grid grid, View view, int row)
    {
        Grid.SetRow(view, row);
        grid.Children.Add(view);
    }
}
