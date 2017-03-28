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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TaminationsWin
{
  public sealed partial class PracticePage : Page
  {
    //private XmlDocument calldoc;
    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
    private bool ready = false;
    private PracticeControl control;
    public AnimationView Animation { get { return animationView; } }
    public Button DefinitionButton { get { return definitionButton; } }
    public Button ContinueButton { get { return continueButton; } }
    public TextBlock CongratsView { get { return congratsView; } }

    public PracticePage() {
      this.InitializeComponent();
      control = this.Intent()["level"] == "Tutorial" 
        ? new TutorialControl(this) 
        : new PracticeControl(this);
      Callouts.animationReady = ( ) => {
        hideExtraStuff();
        animationView.setGridVisibility(true);
        ready = true;
        control.animationReady();
      };
      Callouts.progressCallback = (double beat) => {
        scoreView.Text = ((int)animationView.getScore()).ToString();
        countdown.Text = beat < animationView.leadin ? ((int)Math.Floor(beat - animationView.leadin)).ToString() : "";
      };
      Callouts.AnimationFinished = () => {
        resultsPanel.Visibility = Visibility.Visible;
        continueButton.Visibility = Visibility.Visible;
        var score = animationView.getScore();
        var perfect = animationView.movingBeats * 10;
        var result = $"{(int)score} / {(int)perfect}";
        finalScore.Text = result;
        if (score / perfect >= 0.9) {
          control.success();
          congratsView.Text = "Excellent!";
        } else if (score / perfect >= 0.7) {
          control.success();
          congratsView.Text = "Very Good!";
        } else {
          control.failure();
          congratsView.Text = "Poor";
        }
      };
      control.nextAnimation();
    }

    public void hideExtraStuff() {
      resultsPanel.Visibility = Visibility.Collapsed;
    }

    private void repeatButton_Tapped(object sender, TappedRoutedEventArgs e) {
      control.do_repeat();
    }

    private void continueButton_Tapped(object sender, TappedRoutedEventArgs e) {
      control.do_continue();
    }

    private void returnButton_Tapped(object sender, TappedRoutedEventArgs e) {
      Frame.GoBack();
    }

    private void definitionButton_Tapped(object sender, TappedRoutedEventArgs e) {
      Dictionary<string, string> intent = new Dictionary<string, string>();
      intent["link"] = control.link;
      intent["level"] = "";
      this.Navigate(typeof(DefinitionPage), intent);
    }


    private void animationView_PointerMoved(object sender, PointerRoutedEventArgs e) {
      if (ready && animationView.idancer != null)
        animationView.idancer.doTouch(e.GetCurrentPoint(this), TouchAction.MOVED, this);
    }

    private void animationView_PointerReleased(object sender, PointerRoutedEventArgs e) {
      if (ready && animationView.idancer != null)
        animationView.idancer.doTouch(e.GetCurrentPoint(this), TouchAction.ENDED, this);
    }

    private void animationView_PointerPressed(object sender, PointerRoutedEventArgs e) {
      if (ready && animationView.idancer != null)
        animationView.idancer.doTouch(e.GetCurrentPoint(this), TouchAction.BEGAN, this);
    }
  }
}
