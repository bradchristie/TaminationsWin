using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TaminationsWin {
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
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
