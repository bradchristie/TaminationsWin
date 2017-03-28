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

using System.Collections.Generic;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TaminationsWin {
  public sealed partial class StartPractice : Page
  {
    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

    public StartPractice() {
      this.InitializeComponent();
      DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
      switch (settings.Values["practicegender"]?.ToString()) {
        case "Girl": genderGirlRB.IsChecked = true; break;
        default: genderBoyRB.IsChecked = true; break;
      }
      switch (settings.Values["primarycontroller"]?.ToString()) {
        case "Left": fingerLeftRB.IsChecked = true; break;
        default: fingerRightRB.IsChecked = true; break;
      }
      switch (settings.Values["practicespeed"]?.ToString()) {
        case "Normal": speedNormalRB.IsChecked = true; break;
        case "Moderate": speedModerateRB.IsChecked = true; break;
        default: speedSlowRB.IsChecked = true; break;
      }
    }

    private void Tutorial_Tapped(object sender, TappedRoutedEventArgs e) {
      Dictionary<string, string> intent = new Dictionary<string, string>();
      intent["level"] = "Tutorial";
      this.Navigate(typeof(PracticePage), intent);
    }

    private void Level_Tapped(object sender, TappedRoutedEventArgs e) {
      var grid = (Grid)sender;
      Dictionary<string, string> intent = new Dictionary<string, string>();
      intent["level"] = grid.Name;
      this.Navigate(typeof(PracticePage), intent);
    }

    private void genderBoyRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["practicegender"] = "Boy";
    }

    private void genderGirlRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["practicegender"] = "Girl";
    }

    private void fingerRightRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["primarycontroller"] = "Right";
    }

    private void fingerLeftRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["primarycontroller"] = "Left";
    }

    private void speedSlowRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["practicespeed"] = "Slow";
    }

    private void speedModerateRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["practicespeed"] = "Moderate";

    }

    private void speedNormalRB_Checked(object sender, RoutedEventArgs e) {
      settings.Values["practicespeed"] = "Normal";
    }
  }

}
