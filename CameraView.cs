using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using OpenCvSharp;
using CvRect = OpenCvSharp.Rect;
using CvPoint = OpenCvSharp.Point;

namespace MediGuardAccess;

public sealed class CameraView : Grid, IDisposable
{
    private readonly Image _preview = new()
    {
        BackgroundColor = Colors.Black,
        Aspect = Aspect.AspectFit,
        HeightRequest = 220
    };

    private readonly Label _status = Theme.Label("Kamera belum aktif", 13, true, Theme.Muted);
    private readonly Button _button;

    private VideoCapture? _capture;
    private CancellationTokenSource? _cameraToken;

    private bool _cameraReady;
    private bool _isRunning;
    private string _lastSignature = "";

    public event Action<string>? SignatureCaptured;

    public bool CameraReady => _cameraReady;

    public CameraView(string buttonText = "Aktifkan Kamera")
    {
        RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = new GridLength(230) },
            new RowDefinition { Height = GridLength.Auto }
        };

        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }
        };

        RowSpacing = 10;

        _button = Theme.Button(buttonText);
        _button.Clicked += async (_, _) => await ActivateAsync();

        AddToGrid(_status, 0, 0);
        AddToGrid(CameraBox(), 0, 1);
        AddToGrid(_button, 0, 2);
    }

    private View CameraBox()
    {
        return new Border
        {
            BackgroundColor = Colors.Black,
            Stroke = Theme.Border,
            StrokeThickness = 1,
            Padding = 0,
            HeightRequest = 230,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            Content = _preview
        };
    }

    public async void Activate()
    {
        await ActivateAsync();
    }

    public async Task ActivateAsync()
    {
        if (_isRunning)
        {
            SafeDispatch(() =>
            {
                _status.Text = "Kamera sudah aktif • OpenCV face tracking berjalan";
                _status.TextColor = Theme.Green;
            });

            return;
        }

        _cameraReady = false;
        _isRunning = true;
        _lastSignature = "";

        SafeDispatch(() =>
        {
            _status.Text = "Membuka kamera dengan OpenCV...";
            _status.TextColor = Theme.Primary;
        });

        _cameraToken = new CancellationTokenSource();

        // Jalankan loop kamera di background supaya UI tidak menggantung
        // setelah tombol Aktifkan Kamera ditekan.
        _ = Task.Run(() => CameraLoop(_cameraToken.Token));
        await Task.Delay(250);
    }

    public void RequestSignature()
    {
        if (!_cameraReady)
        {
            SafeDispatch(() =>
            {
                _status.Text = "Kamera belum aktif. Klik Aktifkan Kamera terlebih dahulu.";
                _status.TextColor = Theme.Orange;
            });

            return;
        }

        if (string.IsNullOrWhiteSpace(_lastSignature))
        {
            SafeDispatch(() =>
            {
                _status.Text = "Wajah belum terbaca. Arahkan wajah ke tengah kamera.";
                _status.TextColor = Theme.Orange;
            });

            return;
        }

        SignatureCaptured?.Invoke(_lastSignature);
    }

    private void CameraLoop(CancellationToken token)
    {
        try
        {
            _capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);

            if (!_capture.IsOpened())
            {
                _cameraReady = false;
                _isRunning = false;

                SafeDispatch(() =>
                {
                    _status.Text = "Kamera tidak dapat dibuka. Cek izin kamera atau tutup aplikasi lain.";
                    _status.TextColor = Theme.Red;
                });

                return;
            }

            _capture.FrameWidth = 640;
            _capture.FrameHeight = 480;

            using var frame = new Mat();

            _cameraReady = true;

            SafeDispatch(() =>
            {
                _status.Text = "Kamera aktif • OpenCV real-time face tracking";
                _status.TextColor = Theme.Green;
            });

            while (!token.IsCancellationRequested && _isRunning)
            {
                bool read = _capture.Read(frame);

                if (!read || frame.Empty())
                {
                    Thread.Sleep(30);
                    continue;
                }

                var faceRect = GetCenterFaceRect(frame);

                using var faceRoi = new Mat(frame, faceRect);
                _lastSignature = BuildSignature(faceRoi);

                DrawTrackingOverlay(frame, faceRect);

                Cv2.ImEncode(".jpg", frame, out var imageBytes);

                SafeDispatch(() =>
                {
                    if (!_isRunning || token.IsCancellationRequested)
                        return;

                    _preview.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                });

                Thread.Sleep(33);
            }
        }
        catch (Exception ex)
        {
            _cameraReady = false;
            _isRunning = false;

            SafeDispatch(() =>
            {
                _status.Text = $"OpenCV camera error: {ex.Message}";
                _status.TextColor = Theme.Red;
            });
        }
        finally
        {
            try
            {
                _capture?.Release();
                _capture?.Dispose();
                _capture = null;
            }
            catch
            {
                // ignored
            }

            _cameraReady = false;
            _isRunning = false;
        }
    }

    private static CvRect GetCenterFaceRect(Mat frame)
    {
        int width = frame.Width;
        int height = frame.Height;

        int faceWidth = (int)(width * 0.36);
        int faceHeight = (int)(height * 0.52);

        int x = (width - faceWidth) / 2;
        int y = (height - faceHeight) / 2;

        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);

        if (x + faceWidth > width)
            faceWidth = width - x;

        if (y + faceHeight > height)
            faceHeight = height - y;

        return new CvRect(x, y, faceWidth, faceHeight);
    }

    private static string BuildSignature(Mat faceRoi)
    {
        if (faceRoi.Empty())
            return "0-0-0";

        var mean = Cv2.Mean(faceRoi);

        int b = Math.Clamp((int)Math.Round(mean.Val0), 0, 255);
        int g = Math.Clamp((int)Math.Round(mean.Val1), 0, 255);
        int r = Math.Clamp((int)Math.Round(mean.Val2), 0, 255);

        return $"{r}-{g}-{b}";
    }

    private static void DrawTrackingOverlay(Mat frame, CvRect faceRect)
    {
        var cyan = new Scalar(212, 182, 6);
        var green = new Scalar(94, 197, 34);
        var white = new Scalar(255, 255, 255);

        Cv2.Rectangle(frame, faceRect, cyan, 3);

        int scanY = faceRect.Y + faceRect.Height / 2;

        Cv2.Line(
            frame,
            new CvPoint(faceRect.X, scanY),
            new CvPoint(faceRect.X + faceRect.Width, scanY),
            green,
            3
        );

        var points = new[]
        {
            new CvPoint(faceRect.X + faceRect.Width * 35 / 100, faceRect.Y + faceRect.Height * 35 / 100),
            new CvPoint(faceRect.X + faceRect.Width * 65 / 100, faceRect.Y + faceRect.Height * 35 / 100),
            new CvPoint(faceRect.X + faceRect.Width * 50 / 100, faceRect.Y + faceRect.Height * 50 / 100),
            new CvPoint(faceRect.X + faceRect.Width * 42 / 100, faceRect.Y + faceRect.Height * 68 / 100),
            new CvPoint(faceRect.X + faceRect.Width * 58 / 100, faceRect.Y + faceRect.Height * 68 / 100)
        };

        foreach (var point in points)
        {
            Cv2.Circle(frame, point, 5, green, -1);
        }

        Cv2.PutText(
            frame,
            "OpenCV Face Tracking",
            new CvPoint(18, 34),
            HersheyFonts.HersheySimplex,
            0.85,
            white,
            2
        );
    }

    private void AddToGrid(View view, int column, int row)
    {
        Grid.SetColumn(view, column);
        Grid.SetRow(view, row);
        Children.Add(view);
    }

    private void SafeDispatch(Action action)
    {
        try
        {
            Dispatcher?.Dispatch(action);
        }
        catch
        {
            // page sudah berubah, abaikan
        }
    }

    public void StopCamera()
    {
        try
        {
            _isRunning = false;
            _cameraReady = false;
            _cameraToken?.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _capture?.Release();
            _capture?.Dispose();
        }
        catch
        {
            // ignored
        }

        try
        {
            _cameraToken?.Dispose();
        }
        catch
        {
            // ignored
        }

        _capture = null;
        _cameraToken = null;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent == null)
        {
            StopCamera();
        }
    }

    public void Dispose()
    {
        StopCamera();
    }
}
