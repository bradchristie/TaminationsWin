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
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace TaminationsWin {

  public sealed partial class AnimationPage : Page {

    bool playing = false;
    bool userDrag = true;
    int x1, x2;
    int animnum;
    int animtot;
    IEnumerable<IXmlNode> alltams;
    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

    public AnimationPage() {
      this.InitializeComponent();
      var link = this.Intent()["link"];
      var tamdoc = TamUtils.getXMLAsset(this.Intent()["link"]);
      alltams = TamUtils.tamList(tamdoc).Where(t => t.attr("display") != "none");
      animtot = alltams.Count();
      animnum = int.Parse(this.Intent()["anim"]);
      Callouts.SetLevel(link);
      setAnimation();
      Callouts.AnimationFinished = () => {
        PlayPath.Fill = new SolidColorBrush(Colors.Black);
        PausePath.Fill = new SolidColorBrush(Colors.Transparent);
        playing = false;
      };
      ManipulationMode = ManipulationModes.TranslateX;
      ManipulationStarted += (x,e) => x1 = (int)e.Position.X;
      ManipulationCompleted += (x,e) =>
      {
        x2 = (int)e.Position.X;
        if (x1-x2 > animationView.ActualWidth/2 && animnum+1 < animtot) {
          // swipe left
          animnum++;
          setAnimation();
        } else if (x2-x1 > animationView.ActualWidth/2 && animnum > 0) {
          // swipe right
          animnum--;
          setAnimation();
        }
      };
    }

    private void setAnimation() {
      var tam = alltams.ElementAt(animnum);
      Callouts.SetTitle(tam.attr("title"));
      animationView.setAnimation(tam);
      sliderTicView.setTics(animationView.totalBeats, animationView.parts,isParts:animationView.hasParts);
      Callouts.progressCallback = (double beat) => {
        //  Set slider to the current beat
        userDrag = false;
        slider.Value = beat * 100 / animationView.totalBeats;
        userDrag = true;
        //  Fade out any Taminator text
        //  Win wants an "opacity" value from 1 to 0
        var a = Math.Max((2.0 - beat) / 2.01, 0.0);
        saysPanel.Opacity = a;
      };
      var taminator = tam.SelectSingleNode("taminator");
      if (taminator != null) {
        saysText.Text = taminator.FirstChild.NodeValue.ToString().Trim().ReplaceAll("\\s+"," ");
      }
      animnumText.Text = $"{animnum+1} of {animtot}";
      readSettings();
    }

    private void Play_Tapped(object sender, TappedRoutedEventArgs e) {
      if (playing) {
        animationView.doPause();
        PlayPath.Fill = new SolidColorBrush(Colors.Black);
        PausePath.Fill = new SolidColorBrush(Colors.Transparent);
        playing = false;
      } else {
        animationView.doPlay();
        PlayPath.Fill = new SolidColorBrush(Colors.Transparent);
        PausePath.Fill = new SolidColorBrush(Colors.Black);
        playing = true;
      }
    }

    private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
      if (userDrag) {
        var time = (e.NewValue/100) * animationView.totalBeats;
        animationView.setTime(time);
      }
    }

    //  Read settings and apply them to the animation
    private void readSettings() {
      switch (settings.Values["geometry"]?.ToString()) {
        case "Hexagon": animationView.setHexagon(); break;
        case "Bigon": animationView.setBigon(); break;
        default: animationView.setSquare(); break;
      }
      animationView.setGridVisibility(settings.Values["grid"]?.ToString() == "On");
      animationView.setNumbers(settings.Values["numbers"]?.ToString());
      animationView.setPhantomVisibility(settings.Values["phantoms"]?.ToString() == "On");
      animationView.setPathVisibility(settings.Values["paths"]?.ToString() == "On");
      animationView.setLoop(settings.Values["loop"]?.ToString() == "On");
      animationView.setSpeed(settings.Values["speed"]?.ToString());
      var options = "";
      if (settings.Values["speed"]?.ToString() == "Fast")
        options= "Fast ";
      else if (settings.Values["speed"]?.ToString() == "Slow")
        options = "Slow ";
      if (settings.Values["loop"]?.ToString() == "On")
        options += "Loop";
      optionsText.Text = options;
    }

    private void Start_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doPrevPart();
    }

    private void Back_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doBackup();
    }

    private void Forward_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doForward();
    }

    private void End_Tapped(object sender, TappedRoutedEventArgs e) {
      animationView.doNextPart();
    }

    private void Definition_Tapped(object sender, TappedRoutedEventArgs e) {
      Callouts.AnimationDefinitionAction(this.Intent()["link"]);
    }

    private void Settings_Tapped(object sender, TappedRoutedEventArgs e) {
      Callouts.AnimationSettingsAction();
    }

    private void animationView_Tapped(object sender, TappedRoutedEventArgs e) {
      var p = e.GetPosition(animationView);
      animationView.doTouch(p.X, p.Y);
    }
  }
}
