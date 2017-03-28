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
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace TaminationsWin {
  class TutorialControl : PracticeControl {

    static int tutnum = 0;
    string[] tutdata = {
            "Use your %primary% Finger on the %primary% side of the screen."
          + "  Do not put your finger on the dancer."
          + "  Slide your finger forward to move the dancer forward."
          + "  Try to keep pace with the adjacent dancer.",

          "Use your %primary% Finger to follow the turning path."
          + "  Try to keep pace with the adjacent dancer.",

          "Normally your dancer faces the direction you are moving.  "
          + "  But you can use your %secondary% Finger to hold or change the facing direction."
          + "  Press and hold your %secondary% finger on the %secondary% side"
          + " of the screen.  This will keep your dancer facing forward."
          + "  Then use your %primary% finger on the %primary% side"
          + " of the screen to slide your dancer horizontally.",

          "Use your %secondary% finger to turn in place."
          + "  To U-Turn Left, make a 'C' movement with your %secondary% finger."
    };


    public TutorialControl(PracticePage page) : base(page) {
    }

    public override void nextAnimation() {
      if (tutnum >= tutdata.Length)
        tutnum = 0;
      var tamdoc = TamUtils.getXMLAsset("src/tutorial.xml");
      var gender = settings.Values["gender"]?.ToString() == "Girl" ? Gender.GIRL : Gender.BOY;
      var offset = gender == Gender.BOY ? 0 : 1;
      var tamlist = tamdoc.SelectNodes("/tamination/tam");
      var tam = tamlist[tutnum * 2 + offset];
      Callouts.SetTitle(tam.attr("title"));
      page.Animation.setAnimation(tam,(int)gender);
      switch (settings.Values["practicespeed"]?.ToString()) {
        case "Normal": page.Animation.setSpeed("Normal"); break;
        case "Moderate": page.Animation.setSpeed("Moderate"); break;
        default: page.Animation.setSpeed("Slow"); break;
      }
      showInstructions();
    }

    private async void showInstructions() {
      var primaryIsLeft = settings.Values["primarycontrol"]?.ToString() == "Left";
      var instructions = tutdata[tutnum].ReplaceAll("%primary%", primaryIsLeft ? "Left" : "Right")
                                        .ReplaceAll("%secondary%", primaryIsLeft ? "Right" : "Left");
      var title = "Tutorial " + (tutnum + 1) + " of " + tutdata.Length;
      var tif = new MessageDialog(instructions,title);
      await tif.ShowAsync();
      page.Animation.doPlay();
    }

    //  This overrides the parent method which starts the animation
    //  Do not start the animation until the instructions are dismissed
    public override void animationReady() { }

    public override void do_repeat() {
      page.hideExtraStuff();
      showInstructions();
    }

    public override void do_continue() {
      tutnum += 1;
      page.hideExtraStuff();
      nextAnimation();
    }

    public override async void success() {
      if (tutnum + 1 >= tutdata.Length) {
        page.CongratsView.Text = "Tutorial Complete";
        page.ContinueButton.Visibility = Visibility.Collapsed;
        var message = "Congratulations!  You have successfully completed the tutorial." +
                      "  Now select the level you would like to practice.";
        var tif = new MessageDialog(message, "Tutorial Complete");
        await tif.ShowAsync();
        tutnum = 0;
        page.Frame.GoBack();
      }
    }

    public override void failure() {
      page.ContinueButton.Visibility = Visibility.Collapsed;
    }

  }
}
