using System;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TaminationsWin {

  public sealed partial class MainPage : Page {

    public MainPage() {
      this.InitializeComponent();
    }

    public static Vector2 ScreenSize() {
      var info = DisplayInformation.GetForCurrentView();
      var width = Window.Current.Bounds.Width * (int)info.ResolutionScale / 100;
      var height = Window.Current.Bounds.Height * (int)info.ResolutionScale / 100;
      var dpi = info.RawDpiY;
      return Vector.Create(width / dpi, height / dpi);
    }

    public static void SetRotation() {
      var info = DisplayInformation.GetForCurrentView();
      var width = Window.Current.Bounds.Width * (int)info.ResolutionScale / 100;
      var height = Window.Current.Bounds.Height * (int)info.ResolutionScale / 100;
      var dpi = info.RawDpiY;
      var screenDiagonal = Math.Sqrt(Math.Pow(width / dpi, 2) + Math.Pow(height / dpi, 2));
      if (screenDiagonal < 6)
        DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait;
    }

  }

}
