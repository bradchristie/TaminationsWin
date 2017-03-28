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
using Windows.UI.Xaml.Controls;
using Windows.Data.Xml.Dom;

namespace TaminationsWin {

  public class CallListItem
  {
    public string Title { get; set; }
    public string Link { get; set; }
    public string Level { get; set; }
    public string Color {
      get {
        return LevelData.find(Level).color.ToString();
      }
    }
  }

  public sealed partial class CallListPage : Page
  {
    XmlNodeList calls;

    public ObservableCollection<CallListItem> calllistdata = new ObservableCollection<CallListItem>();
    public CallListPage() {
      this.InitializeComponent();
      string level = this.Intent()["level"];
      reset(level);
    }

    public void reset(string level) {
      LevelData d = LevelData.find(level);
      var isIndex = d.dir == "all";
      XmlDocument calldoc = TamUtils.getXMLAsset(isIndex ? "src\\callindex.xml" : "src\\calls.xml");
      Callouts.SetTitle(d.name);
      calls = calldoc.SelectNodes(isIndex ? "/calls/call" : $"/calls/call[@{d.selector}]");
      searchCallList("");
    }

    private void searchCallList(string query) {
      calllistdata.Clear();
      var qq = TamUtils.callnameQuery(query);
      foreach (IXmlNode call in calls) {
        var title = (string)call.Attributes.GetNamedItem("title").NodeValue;
        if (Regex.Match(title.ToLower(), qq).Success) {
          var sublevel = (string)call.Attributes.GetNamedItem("sublevel").NodeValue;
          calllistdata.Add(new CallListItem() {
            Title = (string)call.Attributes.GetNamedItem("title").NodeValue,
            Level = (string)call.Attributes.GetNamedItem("sublevel").NodeValue,
            Link = (string)call.Attributes.GetNamedItem("link").NodeValue
          });
        }
      }
      CallList.ItemsSource = calllistdata;      
    }

    private void CallList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var item = (CallListItem)CallList.SelectedItem;
      Callouts.CallSelected(item.Link);
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
      searchCallList(SearchTextBox.Text);
    }

    private void SearchTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
      if (e.Key == Windows.System.VirtualKey.Enter)
        CallList.Focus(Windows.UI.Xaml.FocusState.Programmatic);
    }
  }
}
