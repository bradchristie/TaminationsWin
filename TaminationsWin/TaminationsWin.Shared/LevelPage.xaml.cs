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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace TaminationsWin {

  public sealed partial class LevelPage : Page
  {
    public LevelPage()
    {
      this.InitializeComponent();
      Callouts.SetTitle("Taminations");
    }

    private void About_Tapped(object sender, TappedRoutedEventArgs e)
    {
      Callouts.AboutAction();
    }

    private void Settings_Tapped(object sender, TappedRoutedEventArgs e) {
      Callouts.LevelSettingsAction();
    }

    private void Level_Tapped(object sender, TappedRoutedEventArgs e)
    {
      var grid = (Grid)sender;
      Callouts.LevelButtonAction(grid.Name);
      e.Handled = true;
    }

  }

}
