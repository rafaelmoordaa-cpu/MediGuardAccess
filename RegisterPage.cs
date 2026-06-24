using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public class RegisterPage : ContentPage
{
    private readonly Entry _name = Theme.Entry("Nama lengkap");
    private readonly Entry _user = Theme.Entry("Username baru");
    private readonly Entry _pass = Theme.Entry("Password minimal 6 karakter");
    private readonly Entry _confirm = Theme.Entry("Ulangi password");
    private readonly Entry _adminCode = Theme.Entry("Kode admin");
    private readonly VerticalStackLayout _adminSection = new() { Spacing = 8, IsVisible = false };

    private readonly HorizontalStackLayout _roleChips = new() { Spacing = 10 };
    private readonly Label _rolePreview = Theme.Label("Akun baru akan dibuat sebagai User.", 12, true, Theme.Primary);
    private readonly Label _face = Theme.Label("Template wajah belum tersimpan.", 12, true, Theme.Orange);
    private readonly CameraView _camera = new("Aktifkan kamera");

    private UserRole _selectedRole = UserRole.User;
    private string? _signature;

    public RegisterPage()
    {
        Title = "Register";
        BackgroundColor = Theme.Bg;
        _pass.IsPassword = true;
        _confirm.IsPassword = true;
        _adminCode.IsPassword = true;

        _adminSection.Children.Add(Theme.Field("Kode Admin", _adminCode));
        _adminSection.Children.Add(Theme.Notice("Kode admin hanya muncul jika memilih tipe akun Admin. Gunakan kode khusus sesuai arahan penguji/presenter."));

        RenderRoleChips();

        _camera.SignatureCaptured += signature =>
        {
            _signature = signature;
            _face.Text = "Template wajah tersimpan untuk verifikasi akses.";
            _face.TextColor = Theme.Green;
        };

        var captureButton = Theme.Button("Ambil Template Wajah", Theme.Orange);
        var registerButton = Theme.Button("Simpan Akun", Theme.Primary);

        captureButton.Clicked += (_, _) => _camera.RequestSignature();
        registerButton.Clicked += OnRegister;

        Content = Theme.DesktopScroll(RegisterLayout(registerButton, captureButton), 1180);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_camera.CameraReady)
        {
            await Task.Delay(250);
            await _camera.ActivateAsync();
        }
    }

    private View RegisterLayout(Button registerButton, Button captureButton)
    {
        var form = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    Theme.LogoWordmark(58),
                    Theme.Label("Buat Akun Baru", 30, true),
                    Theme.Label("Role dipilih hanya saat registrasi. Setelah itu login cukup memakai username dan password.", 13, false, Theme.Muted),
                    Theme.Field("Nama Lengkap", _name),
                    Theme.Field("Username", _user),
                    TwoColumn(Theme.Field("Password", _pass), Theme.Field("Konfirmasi", _confirm)),
                    RolePanel(),
                    _adminSection,
                    registerButton
                }
            },
            26
        );

        var activateButton = Theme.Button("Aktifkan Kamera", Theme.PrimaryDark);
        activateButton.Clicked += async (_, _) => await _camera.ActivateAsync();

        var face = Theme.Card(
            new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    Theme.Badge("MediFace Enrollment", Theme.Primary),
                    Theme.Label("Daftarkan wajah untuk akses aman", 28, true),
                    Theme.Label("Sistem menyimpan template wajah lokal untuk membuka dashboard dan riwayat sesuai akun. Pada demo, kamera dapat memakai mode aman jika perangkat menolak akses.", 13, false, Theme.Muted),
                    new Border
                    {
                        BackgroundColor = Theme.PrimaryDark,
                        Stroke = Theme.Primary,
                        StrokeThickness = 2,
                        Padding = new Thickness(10),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(24) },
                        Content = _camera
                    },
                    new Grid
                    {
                        ColumnSpacing = 12,
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Star }
                        },
                        Children =
                        {
                            Position(activateButton, 0),
                            Position(captureButton, 1)
                        }
                    },
                    new Border
                    {
                        BackgroundColor = Theme.SoftCyan,
                        Stroke = Color.FromArgb("#BFEFEB"),
                        StrokeThickness = 1,
                        Padding = new Thickness(14),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(18) },
                        Content = _face
                    }
                }
            },
            26
        );


        var main = new Grid
        {
            ColumnSpacing = 26,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(0.92, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.08, GridUnitType.Star) }
            }
        };
        Grid.SetColumn(form, 0);
        Grid.SetColumn(face, 1);
        main.Children.Add(form);
        main.Children.Add(face);
        return main;
    }

    private View RolePanel()
    {
        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                Theme.Label("Pilih Tipe Akun", 12, true, Theme.Muted),
                new Border
                {
                    BackgroundColor = Theme.SoftGray,
                    Stroke = Theme.BorderLight,
                    StrokeThickness = 1,
                    Padding = new Thickness(12),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(18) },
                    Content = new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children = { _roleChips, _rolePreview }
                    }
                }
            }
        };
    }

    private void RenderRoleChips()
    {
        _roleChips.Children.Clear();
        var user = Theme.ChipButton("User", _selectedRole == UserRole.User);
        var admin = Theme.ChipButton("Admin", _selectedRole == UserRole.Admin);
        user.WidthRequest = 150;
        admin.WidthRequest = 150;

        user.Clicked += (_, _) =>
        {
            _selectedRole = UserRole.User;
            _rolePreview.Text = "Akun baru akan dibuat sebagai User.";
            _rolePreview.TextColor = Theme.Primary;
            _adminCode.Text = string.Empty;
            _adminSection.IsVisible = false;
            RenderRoleChips();
        };
        admin.Clicked += (_, _) =>
        {
            _selectedRole = UserRole.Admin;
            _rolePreview.Text = "Admin dipilih. Kode khusus admin wajib diisi.";
            _rolePreview.TextColor = Theme.Orange;
            _adminSection.IsVisible = true;
            RenderRoleChips();
        };

        _roleChips.Children.Add(user);
        _roleChips.Children.Add(admin);
    }

    private async void OnRegister(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_user.Text) || string.IsNullOrWhiteSpace(_pass.Text))
        {
            await DisplayAlert("Validasi", "Nama, username, dan password wajib diisi.", "OK");
            return;
        }

        if (_pass.Text!.Length < 6)
        {
            await DisplayAlert("Validasi", "Password minimal 6 karakter.", "OK");
            return;
        }

        if (_pass.Text != _confirm.Text)
        {
            await DisplayAlert("Validasi", "Konfirmasi password tidak sama.", "OK");
            return;
        }

        if (_selectedRole == UserRole.Admin && (_adminCode.Text?.Trim() ?? "") != "MEDIGUARDADMIN2026")
        {
            await DisplayAlert("Kode Admin", "Kode admin tidak sesuai. Gunakan role User atau masukkan kode admin yang benar.", "OK");
            return;
        }

        bool success = AppData.Register(_name.Text!, _user.Text!, _pass.Text!, _selectedRole, _signature, out var message);
        if (success)
        {
            await DisplayAlert("Berhasil", message, "OK");
            await Navigation.PushAsync(new LoginPage());
        }
        else
        {
            await DisplayAlert("Gagal", message, "OK");
        }
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
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 1);
        grid.Children.Add(left);
        grid.Children.Add(right);
        return grid;
    }

    private static T Position<T>(T view, int column) where T : View
    {
        Grid.SetColumn(view, column);
        return view;
    }
}
