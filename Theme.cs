using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediGuardAccess;

public static class Theme
{
    public static readonly Color Primary = Color.FromArgb("#0F9F8F");
    public static readonly Color PrimaryDark = Color.FromArgb("#0B3248");
    public static readonly Color Cyan = Color.FromArgb("#38BDF8");
    public static readonly Color Teal = Color.FromArgb("#14B8A6");
    public static readonly Color Green = Color.FromArgb("#16A34A");
    public static readonly Color Orange = Color.FromArgb("#F59E0B");
    public static readonly Color Red = Color.FromArgb("#DC2626");

    public static readonly Color Dark = Color.FromArgb("#102A43");
    public static readonly Color Muted = Color.FromArgb("#627D98");
    public static readonly Color LightMuted = Color.FromArgb("#9FB3C8");

    public static readonly Color Bg = Color.FromArgb("#F7FAFC");
    public static readonly Color Surface = Color.FromArgb("#FFFFFF");
    public static readonly Color SoftBlue = Color.FromArgb("#EAF6FF");
    public static readonly Color SoftCyan = Color.FromArgb("#E6FFFA");
    public static readonly Color SoftGreen = Color.FromArgb("#ECFDF5");
    public static readonly Color SoftOrange = Color.FromArgb("#FFF7E6");
    public static readonly Color SoftRed = Color.FromArgb("#FEF2F2");
    public static readonly Color SoftGray = Color.FromArgb("#F3F7FA");

    public static readonly Color Border = Color.FromArgb("#D9E2EC");
    public static readonly Color BorderLight = Color.FromArgb("#E8EEF5");
    public static readonly Color White = Colors.White;

    public static ResourceDictionary CreateResources() => new()
    {
        ["PrimaryBlue"] = Primary,
        ["PrimaryDark"] = PrimaryDark,
        ["CyanAccent"] = Cyan,
        ["TealAccent"] = Teal,
        ["GreenNormal"] = Green,
        ["OrangeWarning"] = Orange,
        ["RedRisk"] = Red,
        ["DarkText"] = Dark,
        ["SecondaryText"] = Muted,
        ["LightMuted"] = LightMuted,
        ["BackgroundLight"] = Bg,
        ["SoftBlue"] = SoftBlue,
        ["SoftCyan"] = SoftCyan,
        ["SoftGray"] = SoftGray,
        ["BorderSoft"] = Border,
        ["CardWhite"] = White
    };

    public static Border Card(View content, double padding = 18)
    {
        return new Border
        {
            BackgroundColor = Surface,
            Stroke = BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(padding),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(28) },
            Content = content,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.06f,
                Radius = 18,
                Offset = new Point(0, 8)
            }
        };
    }

    public static Border HeroCard(View content, double padding = 24)
    {
        return new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#E6FFFA"), 0.0f),
                    new GradientStop(Color.FromArgb("#EAF6FF"), 0.55f),
                    new GradientStop(Color.FromArgb("#FFF7E6"), 1.0f)
                }
            },
            Stroke = Color.FromArgb("#D7F3EF"),
            StrokeThickness = 1,
            Padding = new Thickness(padding),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(30) },
            Content = content,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.07f,
                Radius = 20,
                Offset = new Point(0, 8)
            }
        };
    }

    public static Border SoftCard(View content, double padding = 16)
    {
        return new Border
        {
            BackgroundColor = SoftBlue,
            Stroke = Color.FromArgb("#D8ECFF"),
            StrokeThickness = 1,
            Padding = new Thickness(padding),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
            Content = content
        };
    }

    public static Border CyanCard(View content, double padding = 16)
    {
        return new Border
        {
            BackgroundColor = SoftCyan,
            Stroke = Color.FromArgb("#BFEFEB"),
            StrokeThickness = 1,
            Padding = new Thickness(padding),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
            Content = content
        };
    }

    public static Label Label(string text, double size = 14, bool bold = false, Color? color = null)
    {
        return new Label
        {
            Text = text,
            FontSize = size,
            FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
            TextColor = color ?? Dark,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    public static Label SmallLabel(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 12,
            TextColor = Muted,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    public static Label SectionTitle(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Dark,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    public static Button Button(string text, Color? bg = null)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = bg ?? Primary,
            TextColor = Colors.White,
            CornerRadius = 18,
            HeightRequest = 48,
            FontAttributes = FontAttributes.Bold,
            FontSize = 13,
            Padding = new Thickness(18, 0)
        };
    }

    public static Button OutlineButton(string text, Color? color = null)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Colors.Transparent,
            BorderColor = color ?? Primary,
            BorderWidth = 1,
            TextColor = color ?? Primary,
            CornerRadius = 18,
            HeightRequest = 48,
            FontAttributes = FontAttributes.Bold,
            FontSize = 13,
            Padding = new Thickness(18, 0)
        };
    }

    public static Button ChipButton(string text, bool selected)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = selected ? Primary : Colors.White,
            TextColor = selected ? Colors.White : PrimaryDark,
            BorderColor = selected ? Primary : Border,
            BorderWidth = 1,
            CornerRadius = 18,
            HeightRequest = 40,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(14, 0)
        };
    }

    public static Entry Entry(string placeholder, Keyboard? keyboard = null)
    {
        return new Entry
        {
            Placeholder = placeholder,
            BackgroundColor = Colors.Transparent,
            TextColor = Dark,
            PlaceholderColor = LightMuted,
            HeightRequest = 42,
            Keyboard = keyboard ?? Keyboard.Default,
            FontSize = 13,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
    }

    public static Picker Picker(string title, string[] items)
    {
        var picker = new Picker
        {
            Title = title,
            BackgroundColor = Colors.Transparent,
            TextColor = Dark,
            TitleColor = LightMuted,
            FontSize = 13,
            HeightRequest = 42
        };

        foreach (var item in items)
            picker.Items.Add(item);

        return picker;
    }

    public static Border FieldContainer(View input)
    {
        return new Border
        {
            BackgroundColor = White,
            Stroke = Border,
            StrokeThickness = 1,
            Padding = new Thickness(12, 1),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Content = input
        };
    }

    public static View Field(string label, View input)
    {
        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Label(label, 12, true, Muted),
                FieldContainer(input)
            }
        };
    }

    public static Border Badge(string text, Color bg, Color? textColor = null)
    {
        return new Border
        {
            BackgroundColor = bg,
            Stroke = bg,
            StrokeThickness = 1,
            Padding = new Thickness(12, 6),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Content = new Label
            {
                Text = text,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = textColor ?? Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };
    }

    public static Border StatusBadge(string status) => Badge(status, StatusColor(status));

    public static Color StatusColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "normal" => Green,
            "waspada" => Orange,
            "berisiko" => Red,
            _ => Muted
        };
    }

    public static Color StatusSoftColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "normal" => SoftGreen,
            "waspada" => SoftOrange,
            "berisiko" => SoftRed,
            _ => SoftBlue
        };
    }

    public static BoxView Divider()
    {
        return new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = BorderLight,
            Margin = new Thickness(0, 8)
        };
    }

    public static Border Notice(string text)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#F8FCFF"),
            Stroke = BorderLight,
            StrokeThickness = 1,
            Padding = new Thickness(14),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
            Content = Label(text, 12, false, Dark)
        };
    }

    public static Border MiniIcon(string text, Color bg)
    {
        return new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = bg,
            Stroke = bg,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Content = new Label
            {
                Text = text,
                TextColor = Colors.White,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };
    }

    public static Border LogoIcon(double size = 64)
    {
        return new Border
        {
            WidthRequest = size,
            HeightRequest = size,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Primary, 0.0f),
                    new GradientStop(Cyan, 1.0f)
                }
            },
            Stroke = Color.FromArgb("#FFFFFF66"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(size * 0.30) },
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.12f, Radius = 12, Offset = new Point(0, 6) },
            Content = new Grid
            {
                Children =
                {
                    new Label
                    {
                        Text = "MG",
                        FontSize = size * 0.28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    },
                    new Border
                    {
                        WidthRequest = size * 0.30,
                        HeightRequest = size * 0.30,
                        BackgroundColor = Color.FromArgb("#FFFFFF30"),
                        Stroke = Colors.Transparent,
                        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(0, size * 0.10, size * 0.10, 0)
                    }
                }
            }
        };
    }

    public static View LogoWordmark(double iconSize = 50, bool showTagline = true)
    {
        var textStack = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            Children = { Label("MediGuardAccess", 23, true, Dark) }
        };

        if (showTagline)
            textStack.Children.Add(Label("Secure access • triage • AI navigator", 11, false, Muted));

        return new HorizontalStackLayout
        {
            Spacing = 12,
            VerticalOptions = LayoutOptions.Center,
            Children = { LogoIcon(iconSize), textStack }
        };
    }

    public static Border StatCard(string title, string value, string subtitle, Color accent)
    {
        return Card(
            new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    Badge(title, accent),
                    Label(value, 24, true, Dark),
                    Label(subtitle, 12, false, Muted)
                }
            },
            16
        );
    }

    public static Border ResultPanel(string title, string body, string status)
    {
        return new Border
        {
            BackgroundColor = StatusSoftColor(status),
            Stroke = StatusColor(status),
            StrokeThickness = 1,
            Padding = new Thickness(16),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    Label(title, 18, true, StatusColor(status)),
                    Label(body, 13, false, Dark)
                }
            }
        };
    }

    public static ScrollView DesktopScroll(View content, double maxWidth = 1180)
    {
        return new ScrollView
        {
            BackgroundColor = Bg,
            Content = new Grid
            {
                Padding = new Thickness(26),
                Children =
                {
                    new Border
                    {
                        BackgroundColor = Colors.Transparent,
                        Stroke = Colors.Transparent,
                        Padding = 0,
                        MaximumWidthRequest = maxWidth,
                        HorizontalOptions = LayoutOptions.Center,
                        Content = content
                    }
                }
            }
        };
    }
}
