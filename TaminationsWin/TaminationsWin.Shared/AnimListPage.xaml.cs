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

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Collections.Generic;

namespace TaminationsWin {

  public enum CellType { Header, Separator, Indented, Plain }

  public class AnimListItem {
    public CellType celltype;
    public string title = "";
    public string name = "";
    public string group = "";
    public int animnumber = -1;
    public int difficulty = -1;
    public bool isSelected = false;
    public bool wasSelected = false;
    public string Text {
      get {
        return name;
      }
      set { name = value; }
    }
    public string TextColor { get {
        return celltype == CellType.Header || celltype == CellType.Separator
          ? "White" : "Black";
    } }
    public string CellColor { get {
        if (isSelected)
          return "Blue";
        if (celltype == CellType.Header || celltype == CellType.Separator)
          return "#FF804080";
        else if (difficulty == 1)
          return "#FFC0FFC0";
        else if (difficulty == 2)
          return "#FFFFFFC0";
        else if (difficulty == 3)
          return "#FFFFC0C0";
        else
          return "White";
    } }
    public string Margin { get {
        return celltype == CellType.Indented ? "30,0,0,0" : "0";
    } }
  }

  public sealed partial class AnimListPage : Page {

    private string link = "";

    public ObservableCollection<AnimListItem> anims = new ObservableCollection<AnimListItem>();
    public AnimListPage() {
      this.InitializeComponent();
      reset();
    }

    public void reset() {
      // Fetch the list of animations and build the table
      var prevtitle = "";
      var prevgroup = "";
      link = this.Intent()["link"];
      Callouts.SetLevel(link);
      XmlDocument tamdoc = TamUtils.getXMLAsset(link);
      var title = tamdoc.SelectSingleNode("/tamination").attr("title");
      Callouts.SetTitle(title);
      var tams = TamUtils.tamList(tamdoc);
      var diffsum = 0;
      var firstanim = -1;
      var i = 0;
      foreach (IXmlNode tam in tams) {
        if (tam.attr("display") == "none")
          continue;
        var tamtitle = tam.attr("title");
        var from = TamUtils.tamXref(tam).attr("from");
        var group = tam.attr("group");
        var diffstr = TamUtils.tamXref(tam).attr("difficulty");
        var diff = diffstr.Length > 0 ? int.Parse(diffstr) : 0;
        diffsum += diff;
        if (group.Length > 0) {
          // Add header for new group as needed
          if (group != prevgroup) {
            if (Regex.Match(group, @"^\s+$").Success) {
              // Blank group, for calls with no common starting phrase
              // Add a green separator unless it's the first group
              if (anims.Count > 0)
                anims.Add(new AnimListItem() {
                  celltype = CellType.Separator
                });
            } else
              // Named group e.g. "As Couples.."
              // Add a header with the group name, which starts
              // each call in the group
              anims.Add(new AnimListItem() {
                celltype = CellType.Header,
                name = group
              });
          }
          from = tamtitle.Replace(group, " ").Trim();
        } else if (tamtitle != prevtitle) {
          // Not a group but a different call
          // Put out a header with this call
          anims.Add(new AnimListItem() {
            celltype = CellType.Header,
            name = tamtitle + " from"
          });
        }
        //  Build list item for this animation
        prevtitle = tamtitle;
        prevgroup = group;
        //  TODO posanim(av.getCount) = i
        //  Remember where the first real animation is in the list
        if (firstanim < 0)
          firstanim = anims.Count;
        //  TODO selectanim and weblink
        //  ...
        // Put out a selectable item
        anims.Add(new AnimListItem() {
          celltype = Regex.Match(group,@"^\s+$").Success ? CellType.Plain : CellType.Indented,
          title = tamtitle,
          name = from,
          group = group.Length > 0 ? group : tamtitle + " from",
          animnumber = i,
          difficulty = diff
        });
        i = i + 1;
      }
      if (diffsum <= 0)
        DifficultyLegend.Visibility = Visibility.Collapsed;
          AnimList.ItemsSource = anims;
      if (firstanim >= 0)
        Callouts.FirstAnimationReady(anims[firstanim]);
    }

    private void AnimList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var item = (AnimListItem)AnimList.SelectedItem;
      if (item.animnumber >= 0) {
        item.isSelected = true;
        Callouts.AnimationSelected(item);
      }
    }

    private void Definition_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
      this.Navigate(typeof(DefinitionPage), this.Intent());
    }

    private void Settings_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
      this.Navigate(typeof(SettingsPage), new Dictionary<string, string>());
    }

  }
}
