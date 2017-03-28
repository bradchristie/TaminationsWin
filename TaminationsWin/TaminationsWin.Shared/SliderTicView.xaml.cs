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

using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace TaminationsWin {
  public sealed partial class SliderTicView : UserControl {

    private double beats = 0;
    private bool isParts = false;
    private bool isCalls = false;
    private double[] parts = null;

    public SliderTicView() {
      this.InitializeComponent();
    }

    //  This is called by the system to redraw the view
    private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
      var ds = args.DrawingSession;
      ds.FillRectangle(new Rect(0.0, 0.0, ActualWidth, ActualHeight), Color.FromArgb(255, 0, 128, 0));
      var myLeft = 10.0;
      var myWidth = ActualWidth - 20;
      if (beats > 0) {
        //  Draw tic marks
        for (int loc=1; loc< beats; loc++) {
          var x = myLeft + myWidth * loc / beats;
          ds.DrawLine((float)x, 0f, (float)x, (float)ActualHeight / 4, Colors.White);
        }
        //  Draw tic labels
        if (beats > 4) {
          var y = ActualHeight * 3 / 8;
          var x1 = myLeft + myWidth * 2 / beats;
          var format = new CanvasTextFormat() {
            FontFamily = "Arial",
            FontSize = 20,
            HorizontalAlignment = CanvasHorizontalAlignment.Center
          };
          ds.DrawText("Start", (float)x1, (float)y, Colors.White, format);
          var x2 = myLeft + myWidth * (beats - 2) / beats;
          ds.DrawText("End", (float)x2, (float)y, Colors.White, format);
          if (parts != null) {
            var denom = $"{(parts.Length + 1)}";
            for (int i=0; i<parts.Length; i++) {
              var x = myLeft + myWidth * (2 + parts[i]) / beats;
              var text = isParts && i == 0 ? "Part 2"
                : isParts || isCalls ? $"{i + 2}"
                : $"{i + 1}/{denom}";
              ds.DrawText(text, (float)x, (float)y, Colors.White, format);
            }
          }
        }
      }
    }

    public void setTics(double beats, string partstr, bool isParts=false, bool isCalls=false) {
      this.beats = beats;
      this.isParts = isParts;
      this.isCalls = isCalls;
      if (partstr.Length > 0) {
        var t = partstr.Split(';');
        parts = new double[t.Length];
        var s = 0.0;
        for (int i=0; i<t.Length; i++) {
          var p = float.Parse(t[i]);
          parts[i] = p + s;
          s += p;
        }
      }
      canvas.Invalidate();
    }

  }
}
