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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TaminationsWin {

  public sealed partial class PortraitPage : Page
  {

    public PortraitPage()
    {
      this.InitializeComponent();
#if WINDOWS_PHONE_APP
      Windows.Phone.UI.Input.HardwareButtons.BackPressed += OnBackPressed;
#endif
      Callouts.SetTitle = (string title) => {
        Title.Text = title;
      };
      Callouts.LevelButtonAction = (string level) => {
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["level"] = level;
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(CallListPage), intent);
      };
      Callouts.AboutAction = () => {
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(AboutPage), new Dictionary<string, string>());
      };
      Callouts.LevelSettingsAction = () => {
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(SettingsPage), new Dictionary<string, string>());
      };
      Callouts.StartPracticeAction = () => {
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(StartPractice), new Dictionary<string, string>());
      };
      Callouts.CallSelected = (string link) => {
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["link"] = link;
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(AnimListPage), intent);
      };
      Callouts.AnimationSelected = (AnimListItem item) => {
        var page = (Page)PageFrame.Content;
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["anim"] = item.animnumber.ToString();
        intent["title"] = item.title;
        //  TODO calltitle
        intent["name"] = item.group + " " + item.name;
        intent["link"] = page.Intent()["link"];
        intent["level"] = "";  // TODO
        page.Navigate(typeof(AnimationPage), intent);
      };
      Callouts.AnimationSettingsAction = Callouts.LevelSettingsAction;
      Callouts.AnimationDefinitionAction = (string link) => {
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["link"] = link;
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(DefinitionPage), intent);
      };
      Callouts.SetLevel = (string levelstr) => {
        //  accept link or level
        Level.Content = LevelData.find(levelstr.Split('/')[0]).name;
        Level.Visibility = Visibility.Visible;
      };
      Callouts.SequencerAction = () => {
        var page = (Page)PageFrame.Content;
        page.Navigate(typeof(SequencerPage), new Dictionary<string, string>());
      };
      PageFrame.Navigate(typeof(LevelPage));
    }

    private void Back_Tapped(object sender, TappedRoutedEventArgs e) {
      PageFrame.GoBack();
    }    

    private void Level_Tapped(object sender, TappedRoutedEventArgs e) {
      var depth = PageFrame.BackStackDepth;
      for (int i=1; i < depth; i++)
        PageFrame.GoBack();
    }


    private void PageFrame_Navigated(object sender, NavigationEventArgs e) {
      Back.Visibility = PageFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PageFrame_Navigating(object sender, NavigatingCancelEventArgs e) {
      Level.Visibility = Visibility.Collapsed;
    }

#if WINDOWS_PHONE_APP
    private void OnBackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
    {
      if (PageFrame.CanGoBack)
      {
        e.Handled = true;
        PageFrame.GoBack();
      }
    }
#endif

  }
}
