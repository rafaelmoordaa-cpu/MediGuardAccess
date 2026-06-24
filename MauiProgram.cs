using Microsoft.Extensions.Logging;

#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
#endif

namespace MediGuardAccess;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if WINDOWS
        WebViewHandler.Mapper.AppendToMapping("MediGuardAccessCameraPermission", (handler, view) =>
        {
            if (handler.PlatformView is WebView2 webView)
            {
                webView.CoreWebView2Initialized += (_, _) =>
                {
                    if (webView.CoreWebView2 == null) return;
                    webView.CoreWebView2.PermissionRequested += (_, args) =>
                    {
                        if (args.PermissionKind == CoreWebView2PermissionKind.Camera ||
                            args.PermissionKind == CoreWebView2PermissionKind.Microphone)
                        {
                            args.State = CoreWebView2PermissionState.Allow;
                        }
                    };
                };
            }
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
