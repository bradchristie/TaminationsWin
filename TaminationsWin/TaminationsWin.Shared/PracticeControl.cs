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
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TaminationsWin {
  class PracticeControl
  {

    protected PracticePage page;
    protected ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
    public string link = "";

    public PracticeControl(PracticePage page) {
      this.page = page;
    }

    public virtual void nextAnimation() {
      var calldoc = TamUtils.getXMLAsset("src/calls.xml");
      var selector = LevelData.find(page.Intent()["level"]).selector;
      var calls = calldoc.SelectNodes($"/calls/call[@{selector}]");
      IXmlNode tam = null;
      var rand = new Random();
      while (tam == null) {
        var e = calls[rand.Next((int)calls.Length)];
        //  Remember link for definition
        link = e.attr("link");
        var tamdoc = TamUtils.getXMLAsset(link);
        var tams = tamdoc.SelectNodes("/tamination/tam")
          //  For now, skip any "difficult" animations
          .Where((IXmlNode x) => { return x.attr("difficulty") != "3"; })
          //  Skip any call with parens in the title - it could be a cross-reference
          //  to a concept call from a higher level
          .Where((IXmlNode x) => { return !x.attr("title").Contains("("); });
        if (tams.Count() > 0) {
          tam = tams.ElementAt(rand.Next(tams.Count()));
          var gender = settings.Values["practicegender"]?.ToString() == "Boy"
            ? (int)Gender.BOY : (int)Gender.GIRL;
          page.Animation.setAnimation(tam, gender);
          switch (settings.Values["practicespeed"]?.ToString()) {
            case "Normal": page.Animation.setSpeed("Normal"); break;
            case "Moderate": page.Animation.setSpeed("Moderate"); break;
            default: page.Animation.setSpeed("Slow"); break;
          }
          Callouts.SetTitle(tam.attr("title"));
        }
      }
    }

    //  This is a hook for TutorialActivity, which postpones the start
    //  until the user dismisses the instructions    
    public virtual void animationReady() {
      page.Animation.doPlay();
    }

    //  These are hooks so the tutorial can get the result
    //  Since the tutorial should not show the Definitions button
    //  we will turn it on in these routines
    public virtual void success() {
      page.DefinitionButton.Visibility = Visibility.Visible;
    }

    public virtual void failure() {
      page.DefinitionButton.Visibility = Visibility.Visible;
    }

    public virtual void do_repeat() {
      page.hideExtraStuff();
      page.Animation.doPlay();
    }

    public virtual void do_continue() {
      page.hideExtraStuff();
      nextAnimation();
    }

  }
}
