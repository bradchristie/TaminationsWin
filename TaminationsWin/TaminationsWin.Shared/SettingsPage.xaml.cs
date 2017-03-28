/*

    Taminations Square Dance Animations App for Android
    Copyright (C) 2017 Brad Christie

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace TaminationsWin {

  public sealed partial class SettingsPage : Page {

    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

    public SettingsPage() {
      this.InitializeComponent();
      Callouts.SetTitle("Settings");
      switch (settings.Values["speed"]?.ToString()) {
        case "Fast": speedFastRB.IsChecked = true; break;
        case "Slow": speedSlowRB.IsChecked = true; break;
        default: speedNormalRB.IsChecked = true; break;
      }
      
      switch (settings.Values["numbers"]?.ToString()) {
        case "1-8": numbersDancersRB.IsChecked = true; break;
        case "1-4": numbersCouplesRB.IsChecked = true; break;
        default: numbersNoneRB.IsChecked = true; break;
      }
      switch (settings.Values["geometry"]?.ToString()) {
        case "Hexagon": geometryHexagonRB.IsChecked = true; break;
        case "Bigon": geometryBigonRB.IsChecked = true; break;
        default: geometrySquareRB.IsChecked = true; break;
      }
      gridSwitch.IsOn = settings.Values["grid"]?.ToString() == "On";
      loopSwitch.IsOn = settings.Values["loop"]?.ToString() == "On";
      pathsSwitch.IsOn = settings.Values["paths"]?.ToString() == "On";
      phantomsSwitch.IsOn = settings.Values["phantoms"]?.ToString() == "On";
    }

    private void speedSlowRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["speed"] = "Slow";
      Callouts.settingsChanged();
    }

    private void speedNormalRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["speed"] = "Normal";
      Callouts.settingsChanged();
    }

    private void speedFastRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["speed"] = "Fast";
      Callouts.settingsChanged();
    }

    private void gridSwitch_Toggled(object sender, RoutedEventArgs e) {
      settings.Values["grid"] = (sender as ToggleSwitch).IsOn ? "On" : "Off";
      Callouts.settingsChanged();
    }

    private void loopSwitch_Toggled(object sender, RoutedEventArgs e) {
      settings.Values["loop"] = (sender as ToggleSwitch).IsOn ? "On" : "Off";
      Callouts.settingsChanged();
    }

    private void pathsSwitch_Toggled(object sender, RoutedEventArgs e) {
      settings.Values["paths"] = (sender as ToggleSwitch).IsOn ? "On" : "Off";
      Callouts.settingsChanged();
    }

    private void phantomsSwitch_Toggled(object sender, RoutedEventArgs e) {
      settings.Values["phantoms"] = (sender as ToggleSwitch).IsOn ? "On" : "Off";
      Callouts.settingsChanged();
    }

    private void numbersNoneRB_Tapped(object sender, TappedRoutedEventArgs e) {
      settings.Values["numbers"] = "off";
      Callouts.settingsChanged();
    }

    private void numbersDancersRB_Tapped(object sender, TappedRoutedEventArgs e) {
      settings.Values["numbers"] = "1-8";
      Callouts.settingsChanged();
    }

    private void numbersCouplesRB_Tapped(object sender, TappedRoutedEventArgs e) {
      settings.Values["numbers"] = "1-4";
      Callouts.settingsChanged();
    }

    private void geometrySquareRB_Tapped(object sender, TappedRoutedEventArgs e) {
      settings.Values["geometry"] = "Square";
      Callouts.settingsChanged();
    }

    private void geometryHexagonRB_Tapped(object sender, TappedRoutedEventArgs e) {
      settings.Values["geometry"] = "Hexagon";
      Callouts.settingsChanged();
    }

    private void geometryBigonRB_Tapped(object sender, TappedRoutedEventArgs e) {
      settings.Values["geometry"] = "Bigon";
      Callouts.settingsChanged();
    }
  }
}
