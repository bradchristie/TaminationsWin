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

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web;

namespace TaminationsWin {

  public sealed partial class DefinitionPage : Page, IUriToStreamResolver {

    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
    private string title = "";
    private bool loaded = false;

    public DefinitionPage() {
      this.InitializeComponent();
      var link = this.Intent()["link"].ReplaceAll(@"\..*", "") + ".html";
      Callouts.SetLevel(link);
      Uri url = webview.BuildLocalStreamUri("assets", link.ReplaceAll("ms/","ms0/"));
      webview.NavigationCompleted += navigationCompleted;
      webview.NavigateToLocalStreamUri(url, this);
    //  if (!Regex.Match(link, "^(b1|b2|ms0)").Success)
    //    AbbrevFullPanel.Visibility = Visibility.Collapsed;
      Callouts.AnimationPart = async (int part) => {
        await webview.InvokeScriptAsync("eval", new string[] { $"setPart({part})" });
      };
    }

    public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri) {
      if (uri == null) {
        throw new Exception();
      }
      string path = uri.AbsolutePath;

      // Because of the signature of the this method, it can't use await, so we 
      // call into a seperate helper method that can use the C# await pattern.
      return GetContent(path).AsAsyncOperation();
    }

    private async Task<IInputStream> GetContent(string path) {
      // We use a package folder as the source, but the same principle should apply
      // when supplying content from other locations
      try {
        Uri localUri = new Uri("ms-appx:///assets" + path);
        StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(localUri);
        IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
        return stream;
      }
      catch (Exception) { throw new Exception("Invalid path"); }
    }

    public async void setTitle(string title) {
      this.title = title.ReplaceAll("\\s+", "");
      if (loaded)
        await webview.InvokeScriptAsync("eval", new string[] { $"var currentcall = '{this.title}'" });
    }

    private async void navigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {
      //  Inject javascript to highlight current part
      var jsfunction =
          "   function setPart(part)   {" +
          "      var nodes = document.getElementsByTagName('span'); " +
          "      for (var i=0; i<nodes.length; i++) { " +
          "        var elem = nodes.item(i); " +
          "        var classstr = ' '+elem.className+' '; " +
          "        classstr = classstr.replace('definition-highlight',''); " +
          "        var teststr = ' '+classstr+' '+elem.id+' '; " +
          "        if (teststr.indexOf(' '+currentcall+part+' ') > 0 || " +
          "            teststr.indexOf('Part'+part+' ') > 0) " +
          "          classstr += 'definition-highlight'; " +
          "        classstr = classstr.replace(/^\\s+|\\s+$/g,''); " +
          "        elem.className = classstr; " +
          "      } " +
          "   }  ";
      await webview.InvokeScriptAsync("eval", new string[] { jsfunction });
      var isabbrev = settings.Values["isabbrev"]?.ToString() == "On";
      if (isabbrev)
        Abbrev.IsChecked = true;
      else
        Full.IsChecked = true;
      //  Function to show either full or abbrev
      //  We need to wait until the page loading is finished
      //  before injecting this
      var jsfunction2 =
          "    function setAbbrev(isAbbrev) {" +
          "      var nodes = document.getElementsByTagName('*');" +
          "      for (var i=0; i<nodes.length; i++) {" +
          "        var elem = nodes.item(i);" +
          "        if (elem.className.indexOf('abbrev') >= 0)" +
          "          elem.style.display = isAbbrev ? '' : 'none';" +
          "        if (elem.className.indexOf('full') >= 0)" +
          "          elem.style.display = isAbbrev ? 'none' : '';" +
          "      }" +
          "    } " +
          ((isabbrev) ? "setAbbrev(true)" : "setAbbrev(false)");
      await webview.InvokeScriptAsync("eval", new string[] { jsfunction2 });
      loaded = true;
      setTitle(title);
    }

    private async void Abbrev_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
      settings.Values["isabbrev"] = "On";
      await webview.InvokeScriptAsync("eval", new string[] { "setAbbrev(true)" });
    }

    private async void Full_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
      settings.Values["isabbrev"] = "Off";
      await webview.InvokeScriptAsync("eval", new string[] { "setAbbrev(false)" });
    }
  }
}
