using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class FaceVerificationPage : ContentPage
{
    private readonly CameraView _camera = new("Aktifkan Kamera")
    {
        WidthRequest = 620,
        HeightRequest = 360,
        HorizontalOptions = LayoutOptions.Center
    };

    private readonly Label _status = Theme.Label(
        "Klik Aktifkan Kamera untuk memulai.",
        13,
        true,
        Theme.Muted
    );

    private readonly Label _matchInfo = Theme.Label(
        "Face Match: -",
        24,
        true,
        Theme.Muted
    );

    private readonly ProgressBar _bar = new()
    {
        Progress = 0,
        ProgressColor = Theme.Primary,
        HeightRequest = 8
    };

    private readonly Button _scanButton = Theme.Button("Mulai Scan Wajah", Theme.Primary);

    private bool _isProcessing;
    private bool _hasNavigated;

    public FaceVerificationPage()
    {
        Title = "Face Verification";
        BackgroundColor = Theme.Bg;

        _camera.SignatureCaptured += OnSignatureCaptured;
        _scanButton.Clicked += OnScanClicked;

        _scanButton.WidthRequest = 190;
        _scanButton.HorizontalOptions = LayoutOptions.Center;

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            Children =
            {
                Header(),
                CenterVerificationCard(),
                Footer()
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_camera.CameraReady)
        {
            await Task.Delay(300);
            await _camera.ActivateAsync();
        }
    }

    private static View Header()
    {
        var header = new Grid
        {
            Padding = new Thickness(34, 22),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var brand = Theme.LogoWordmark(46);
        var badge = Theme.Badge("Face Verification", Theme.Primary);

        Grid.SetColumn(brand, 0);
        Grid.SetColumn(badge, 1);

        header.Children.Add(brand);
        header.Children.Add(badge);

        Grid.SetRow(header, 0);
        return header;
    }

    private View CenterVerificationCard()
    {
        var cameraFrame = new Border
        {
            BackgroundColor = Colors.Black,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(10),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            HorizontalOptions = LayoutOptions.Center,
            Content = _camera
        };

        var actionRow = new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                _scanButton
            }
        };

        var resultBox = new Border
        {
            BackgroundColor = Theme.SoftBlue,
            Stroke = Theme.BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(16),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(18)
            },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    _matchInfo,
                    _bar,
                    _status
                }
            }
        };

        var card = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 6,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            Theme.Label(
                                "Verifikasi Wajah Real-Time",
                                27,
                                true,
                                Theme.Primary
                            ),

                            new Label
                            {
                                Text = "Arahkan wajah ke tengah kamera untuk memverifikasi akun sebelum masuk dashboard.",
                                FontSize = 13,
                                TextColor = Theme.Muted,
                                HorizontalTextAlignment = TextAlignment.Center,
                                MaximumWidthRequest = 620,
                                LineBreakMode = LineBreakMode.WordWrap
                            }
                        }
                    },

                    cameraFrame,

                    actionRow,

                    resultBox,

                    Theme.Notice(
                        "Pastikan wajah terlihat jelas, posisi wajah berada di tengah frame, dan pencahayaan cukup."
                    )
                }
            },
            24
        );

        card.MaximumWidthRequest = 760;
        card.HorizontalOptions = LayoutOptions.Center;
        card.VerticalOptions = LayoutOptions.Center;

        var grid = new Grid
        {
            Padding = new Thickness(24),
            Children =
            {
                card
            }
        };

        Grid.SetRow(grid, 1);
        return grid;
    }

    private static View Footer()
    {
        var footer = new Label
        {
            Text = "MediGuardAccess • Medical face verification gateway",
            FontSize = 11,
            TextColor = Theme.LightMuted,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 18)
        };

        Grid.SetRow(footer, 2);
        return footer;
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        if (_isProcessing || _hasNavigated)
            return;

        if (!_camera.CameraReady)
        {
            _status.Text = "Kamera sedang diaktifkan otomatis...";
            _status.TextColor = Theme.Primary;
            await _camera.ActivateAsync();
            await Task.Delay(300);
        }

        if (!_camera.CameraReady)
        {
            _status.Text = "Kamera belum aktif. Cek izin kamera Windows atau tutup aplikasi kamera lain.";
            _status.TextColor = Theme.Orange;
            return;
        }

        _status.Text = "Memindai wajah secara real-time...";
        _status.TextColor = Theme.Primary;

        _camera.RequestSignature();
    }

    private async void OnSignatureCaptured(string liveSignature)
    {
        if (_isProcessing || _hasNavigated)
            return;

        _isProcessing = true;

        try
        {
            var user = AppData.CurrentUser;

            if (user == null)
            {
                await DisplayAlert("Session berakhir", "Silakan login ulang.", "OK");
                await Navigation.PopToRootAsync();
                return;
            }

            int score = FaceScanService.MatchPercent(user.FaceSignature, liveSignature);
            string category = FaceScanService.AccessCategory(score);
            string decision = FaceScanService.AccessDecision(score);

            _bar.Progress = score / 100.0;
            _matchInfo.Text = $"Face Match: {score}%";
            _status.Text = $"Kategori: {category} • {decision}";

            if (score >= 80)
            {
                _bar.ProgressColor = Theme.Green;
                _matchInfo.TextColor = Theme.Green;
                _status.TextColor = Theme.Green;

                _hasNavigated = true;
                _scanButton.IsEnabled = false;
                _status.Text = "Akses diterima. Membuka dashboard...";

                await Task.Delay(700);

                Page nextPage = user.Role == UserRole.Admin
                    ? new AdminDashboardPage()
                    : new UserDashboardPage();

                _camera.StopCamera();

                await Navigation.PushAsync(nextPage);
            }
            else if (score >= 60)
            {
                _bar.ProgressColor = Theme.Orange;
                _matchInfo.TextColor = Theme.Orange;
                _status.TextColor = Theme.Orange;

                await DisplayAlert(
                    "Scan ulang",
                    "Kecocokan wajah berada pada kategori sedang. Silakan scan ulang dengan posisi wajah lebih jelas dan pencahayaan lebih baik.",
                    "OK"
                );
            }
            else
            {
                _bar.ProgressColor = Theme.Red;
                _matchInfo.TextColor = Theme.Red;
                _status.TextColor = Theme.Red;

                await DisplayAlert(
                    "Akses ditolak",
                    "Kecocokan wajah rendah. Akses ditolak.",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            _hasNavigated = false;

            _status.Text = "Terjadi error saat membuka dashboard.";
            _status.TextColor = Theme.Red;

            await DisplayAlert(
                "Error Navigasi",
                $"Aplikasi berhasil scan wajah, tetapi gagal membuka dashboard.\n\nDetail:\n{ex.Message}",
                "OK"
            );
        }
        finally
        {
            _isProcessing = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _camera.StopCamera();
    }
}
