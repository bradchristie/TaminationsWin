using System;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TaminationsWin {

  public sealed partial class MainPage : Page {

    public MainPage() {
      this.InitializeComponent();
      this.NavigationCacheMode = NavigationCacheMode.Required;
    }

    public static Vector2 ScreenSize() {
      var info = DisplayInformation.GetForCurrentView();
      var width = Window.Current.Bounds.Width * info.RawPixelsPerViewPixel;
      var height = Window.Current.Bounds.Height * info.RawPixelsPerViewPixel;
      var dpi = info.RawDpiY;
      return Vector.Create(width / dpi, height / dpi);
    }

    public static void SetRotation() {
      var info = DisplayInformation.GetForCurrentView();
      var width = Window.Current.Bounds.Width * info.RawPixelsPerViewPixel;
      var height = Window.Current.Bounds.Height * info.RawPixelsPerViewPixel;
      var dpi = info.RawDpiY;
      var screenDiagonal = Math.Sqrt(Math.Pow(width / dpi, 2) + Math.Pow(height / dpi, 2));
      if (screenDiagonal < 6)
        DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
    }

    /// <summary>
    /// Invoked when this page is about to be displayed in a Frame.
    /// </summary>
    /// <param name="e">Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.</param>
    protected override void OnNavigatedTo(NavigationEventArgs e) {
      // TODO: Prepare page for display here.

      // TODO: If your application contains multiple pages, ensure that you are
      // handling the hardware Back button by registering for the
      // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
      // If you are using the NavigationHelper provided by some templates,
      // this event is handled for you.
    }
  }
}
