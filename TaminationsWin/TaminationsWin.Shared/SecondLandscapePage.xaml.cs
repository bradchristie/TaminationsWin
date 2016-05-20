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
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace TaminationsWin {

  public sealed partial class SecondLandscapePage : Page {

    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
    private bool firstTitle = true;

    public SecondLandscapePage() {
      this.InitializeComponent();
      Callouts.SetTitle = (string title) => {
        if (firstTitle)
          Title.Text = title;
        firstTitle = false;
        var def = DefinitionFrame.Content as DefinitionPage;
        if (def != null)
          def.setTitle(title);
      };
      Callouts.AnimationSelected = (AnimListItem item) => {
        Dictionary<string, string> intent = new Dictionary<string, string>();
        intent["anim"] = item.animnumber.ToString();
        intent["title"] = item.title;
        //  TODO calltitle
        intent["name"] = item.group + " " + item.name;
        intent["link"] = this.Intent()["link"];
        intent["level"] = "";  // TODO
        FrameUtilities.Navigate(AnimationFrame, typeof(AnimationPage), intent);
      };
      Callouts.FirstAnimationReady = Callouts.AnimationSelected;
      Callouts.AnimationDefinitionAction = (string link) => {
        FrameUtilities.Navigate(DefinitionFrame, typeof(DefinitionPage), this.Intent());
      };
      Callouts.AnimationSettingsAction = () => {
        FrameUtilities.Navigate(DefinitionFrame, typeof(SettingsPage), this.Intent());
      };
      Callouts.settingsChanged = () => {
        var animation = (AnimationView)((FrameworkElement)AnimationFrame.Content).FindName("animationView");
        switch (settings.Values["geometry"]?.ToString()) {
          case "Hexagon": animation.setHexagon(); break;
          case "Bigon": animation.setBigon(); break;
          default: animation.setSquare(); break;
        }
        animation.setGridVisibility(settings.Values["grid"]?.ToString() == "On");
        animation.setLoop(settings.Values["loop"]?.ToString() == "On");
        animation.setPathVisibility(settings.Values["paths"]?.ToString() == "On");
        animation.setPhantomVisibility(settings.Values["phantoms"]?.ToString() == "On");
        animation.setSpeed(settings.Values["speed"]?.ToString());
        animation.setNumbers(settings.Values["numbers"]?.ToString());
        //  TODO show Loop and Speed options
      };
      FrameUtilities.Navigate(AnimListFrame, typeof(AnimListPage), this.Intent());
      var buttonpanel = (Grid)((FrameworkElement)AnimListFrame.Content).FindName("ButtonPanel");
      buttonpanel.Visibility = Visibility.Collapsed;
      FrameUtilities.Navigate(DefinitionFrame, typeof(DefinitionPage), this.Intent());
      Level.Content = LevelData.find(this.Intent()["link"].Split('/')[0]).name;
    }

    private void Back_Tapped(object sender, TappedRoutedEventArgs e) {
      this.Frame.GoBack();
    }

    private void Level_Tapped(object sender, TappedRoutedEventArgs e) {
      //  Hack the button level into the intent for the call list page
      PageUtilities.intents[typeof(CallListPage)]["level"] = Level.Content.ToString();
      this.Frame.GoBack();
    }

  }
}
