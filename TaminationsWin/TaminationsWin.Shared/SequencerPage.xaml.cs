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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace TaminationsWin {

  public sealed partial class SequencerPage : Page
  {
    private bool playing = false;
    private bool userDrag = true;
    private bool isLandscape;
    private string formation = "Static Square";
    private List<double> callBeats = new List<double>();
    public ObservableCollection<CallListItem> calllistdata = new ObservableCollection<CallListItem>();

    public SequencerPage()
    {
      this.InitializeComponent();
      var screenSize = MainPage.ScreenSize();
      isLandscape = screenSize.Length() > 6 && screenSize.X > screenSize.Y;
      if (isLandscape) {
        HorizontalGrid.ColumnDefinitions[1].Width = new GridLength(2, GridUnitType.Star);
        Grid.SetColumn(VerticalGrid, 1);
        var v0 = new Grid();
        var rd0 = new RowDefinition();
        rd0.Height = new GridLength(4, GridUnitType.Star);
        var rd1 = new RowDefinition();
        v0.RowDefinitions.Add(rd0);
        v0.RowDefinitions.Add(rd1);
        Grid.SetRow(NextCall, 1);
        VerticalGrid.Children.Remove(NextCall);
        v0.Children.Add(NextCall);
        HorizontalGrid.Children.Add(v0);
        VerticalGrid.Children.Remove(CallList);
        v0.Children.Add(CallList);
        Grid.SetRow(CallList, 0);
        VerticalGrid.RowDefinitions[5].Height = new GridLength(0);
        VerticalGrid.RowDefinitions[6].Height = new GridLength(0);
      }
      Callouts.AnimationFinished = stopPlay;
      CallList.ItemsSource = calllistdata;
      Callouts.progressCallback = (double beat) => {
        //  Set slider to the current beat
        userDrag = false;
        slider.Value = beat * 100 / animationView.totalBeats;
        userDrag = true;
      };
      DataTransferManager.GetForCurrentView().DataRequested += DataRequested;
      startSequence();
    }

    private void Back_Tapped(object sender, TappedRoutedEventArgs e) {
      this.Frame.GoBack();
    }

    private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
      if (userDrag) {
        var time = (e.NewValue / 100) * animationView.totalBeats;
        animationView.setTime(time);
      }
    }

    private void startSequence() {
      animationView.setAnimation(TamUtils.getFormation(formation));
    }

    private void setFormation(string f) {
      formation = f;
      calllistdata.Clear();
      callBeats.Clear();
      startSequence();
      sliderTicView.setTics(animationView.totalBeats, "", isCalls: true);
    }

    private void Backup_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doBackup();
    }

    private void Start_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doPrevPart();
    }

    private void Play_Tapped(object sender, TappedRoutedEventArgs e) {
      if (playing)
        stopPlay();
      else
        startPlay();
    }

    private void Forward_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doForward();
    }

    private void End_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doNextPart();
    }

    private void CallList_SelectionChanged(object sender, SelectionChangedEventArgs e) {

    }

    private async void Formation_Tapped(object sender, TappedRoutedEventArgs e) {
      MessageDialog dialog = new MessageDialog("Select Starting Formation");
      dialog.Commands.Add(new UICommand("Facing Couples"));
      dialog.Commands.Add(new UICommand("Squared Set"));
      setFormation((await dialog.ShowAsync()).Label);
    }

    private void Instructions_Tapped(object sender, TappedRoutedEventArgs e) {
      Dictionary<string, string> intent = new Dictionary<string, string>();
      this.Navigate(typeof(SequencerInstructionsPage), intent);
    }

    private void Share_Tapped(object sender, TappedRoutedEventArgs e) {
      DataTransferManager.ShowShareUI();
    }

    void DataRequested(DataTransferManager sender, DataRequestedEventArgs args) {
      var text = calllistdata.Aggregate("", (str, d) => str + d.Title + "\n").Trim();
      args.Request.Data.SetText(text);
      args.Request.Data.Properties.Title = "Taminations";
    }

    private void startPlay() {
      animationView.doPlay();
      PlayPath.Fill = new SolidColorBrush(Colors.Transparent);
      PausePath.Fill = new SolidColorBrush(Colors.Black);
      playing = true;
    }
    private void stopPlay() {
      animationView.doPause();
      PlayPath.Fill = new SolidColorBrush(Colors.Black);
      PausePath.Fill = new SolidColorBrush(Colors.Transparent);
      playing = false;
    }

    private async void NextCall_KeyUp(object sender, KeyRoutedEventArgs e) {
      if (e.Key == Windows.System.VirtualKey.Enter) {
        if (!isLandscape) {
          NextCall.IsEnabled = false;
          NextCall.IsTabStop = false;
          NextCall.IsEnabled = true;
          NextCall.IsTabStop = true;
        }
        var calltext = NextCall.Text;
        var avdancers = animationView.dancers;
        var cctx = new CallContext(avdancers);
        NextCall.Text = "";
        try {
          var prevbeats = animationView.movingBeats;
          cctx.interpretCall(calltext);
          cctx.performCall();
          for (var i = 0; i < avdancers.Length; i++)
            avdancers[i].path.add(cctx.dancers[i].path);
          if (animationView.beat > animationView.movingBeats)
            animationView.beat = animationView.movingBeats;  // ??
          animationView.recalculate();
          var newbeats = animationView.movingBeats;
          if (newbeats > prevbeats) {
            //  set parts and tics
            var partstr = callBeats.Count() == 0 ? "" :
              callBeats.Select(b => b.ToString()).Aggregate((a,b) => a+";"+b);
            sliderTicView.setTics(animationView.totalBeats, partstr, isCalls:true);
            startPlay();
            //  Call worked, add it to the list
            calllistdata.Add(new CallListItem() {
              Title = calltext,
              Level = "b1",
              Link = ""
            });
            callBeats.Add(newbeats - prevbeats);
          }
        }
        catch (CallError err) {
          MessageDialog dialog = new MessageDialog(err.Message);
          await dialog.ShowAsync();
        }
      }
    }
  }
}
