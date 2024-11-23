using Android.Views;
using Android.Widget;
using System.Runtime.Versioning;
using static Android.Resource;
using Activity = Android.App.Activity;
using Rect = Android.Graphics.Rect;
using View = Android.Views.View;

// ReSharper disable once CheckNamespace
namespace BaroquenMelody.App;

[SupportedOSPlatform("Android30.0")]
public static class WebViewSoftInputPatch
{
    private static Activity Activity => Platform.CurrentActivity!;

    private static View? _mChildOfContent;

    private static FrameLayout.LayoutParams? _frameLayoutParams;

    private static int _usableHeightPrevious;

    public static void Initialize()
    {
        var content = (FrameLayout)(Activity.FindViewById(Id.Content) ?? throw new InvalidOperationException("Content can't be null."));

        _mChildOfContent = content.GetChildAt(0);
        _mChildOfContent!.ViewTreeObserver!.GlobalLayout += (_, _) => PossiblyResizeChildOfContent();
        _frameLayoutParams = (FrameLayout.LayoutParams)_mChildOfContent.LayoutParameters!;
    }

    private static void PossiblyResizeChildOfContent()
    {
        var usableHeightNow = ComputeUsableHeight();

        if (usableHeightNow != _usableHeightPrevious)
        {
            var usableHeightSansKeyboard = _mChildOfContent!.RootView!.Height;
            var heightDifference = usableHeightSansKeyboard - usableHeightNow;

            if (heightDifference < 0)
            {
                usableHeightSansKeyboard = _mChildOfContent.RootView.Width;
                heightDifference = usableHeightSansKeyboard - usableHeightNow;
            }

            _frameLayoutParams!.Height = heightDifference > usableHeightSansKeyboard / 4 ? usableHeightSansKeyboard - heightDifference : usableHeightNow;
        }

        _mChildOfContent!.RequestLayout();
        _usableHeightPrevious = usableHeightNow;
    }

    private static int ComputeUsableHeight()
    {
        var rect = new Rect();

        _mChildOfContent!.GetWindowVisibleDisplayFrame(rect);

        if (IsImmersiveMode())
        {
            return rect.Bottom;
        }

        return rect.Bottom - GetStatusBarHeight();
    }

    private static int GetStatusBarHeight()
    {
        var result = 0;
        var resources = Activity.Resources!;
        var resourceId = resources.GetIdentifier("status_bar_height", "dimen", "android");

        if (resourceId > 0)
        {
            result = resources.GetDimensionPixelSize(resourceId);
        }

        return result;
    }

    private static bool IsImmersiveMode()
    {
        var decorView = Activity!.Window!.DecorView;
        var uiOptions = decorView!.WindowInsetsController!.SystemBarsAppearance;

        return (uiOptions & (int)SystemUiFlags.Immersive) == (int)SystemUiFlags.Immersive;
    }
}
