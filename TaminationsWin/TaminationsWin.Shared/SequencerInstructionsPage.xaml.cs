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
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Web;


namespace TaminationsWin {

  public sealed partial class SequencerInstructionsPage : Page, IUriToStreamResolver {

    public SequencerInstructionsPage() {
      this.InitializeComponent();
      Uri url = webview.BuildLocalStreamUri("assets", "info/sequencer.html");
      webview.NavigateToLocalStreamUri(url, this);
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

    private void Back_Tapped(object sender, TappedRoutedEventArgs e) {
      this.Frame.GoBack();
    }

  }
}
