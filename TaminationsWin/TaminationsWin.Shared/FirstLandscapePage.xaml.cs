/*

    Taminations Square Dance Animations App for Android
    Copyright (C) 2016 Brad Christie

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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TaminationsWin {

  public sealed partial class FirstLandscapePage : Page {

    public FirstLandscapePage() {
      this.InitializeComponent();
      Callouts.SetTitle = (string title) => {
        if (!title.Contains("Taminations"))
          title = "Taminations - " + title;
        Title.Text = title;
      };
      Callouts.LevelButtonAction = (string level) => {
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["level"] = level;
        FrameUtilities.Navigate(RightFrame,typeof(CallListPage), intent);
      };
      Callouts.AboutAction = () => {
        FrameUtilities.Navigate(RightFrame, typeof(AboutPage), new Dictionary<string, string>());
      };
      Callouts.LevelSettingsAction = () => {
        FrameUtilities.Navigate(RightFrame, typeof(SettingsPage), new Dictionary<string, string>());
      };
      Callouts.CallSelected = (string link) => {
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["link"] = link;
        this.Navigate(typeof(SecondLandscapePage), intent);
      };
      Back.Visibility = Visibility.Collapsed;
      LeftFrame.Navigate(typeof(LevelPage));
      if (PageUtilities.intents.ContainsKey(typeof(CallListPage)))
        RightFrame.Navigate(typeof(CallListPage));
      else
        RightFrame.Navigate(typeof(AboutPage));
    }

    private void Back_Tapped(object sender, TappedRoutedEventArgs e) {
    }

    private void PageFrame_Navigated(object sender, NavigationEventArgs e) {
    }

    private void LeftFrame_Navigated(object sender, NavigationEventArgs e) {

    }

    private void RightFrame_Navigated(object sender, NavigationEventArgs e) {

    }
  }
}
