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

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace TaminationsWin {

  public struct Speed {
    public static double SLOW = 1500;
    public static double MODERATE = 1000;
    public static double NORMAL = 500;
    public static double FAST = 200;
  }

  public sealed partial class AnimationView : UserControl {

    private bool looping = false;
    private double speed = Speed.NORMAL;
    private DateTime lastTime;
    private IXmlNode tam = null;
    private int interactiveDancer = -1;
    public double leadin = 2;
    public double leadout = 2;
    private bool isRunning = false;
    private double beats = 0.0;
    public double beat = 0.0;
    private double prevbeat = 0.0;
    public Dancer[] dancers;
    public InteractiveDancer idancer;
    private double iscore;
    private int geometry = GeometryType.SQUARE;
    private bool showPhantoms = false;
    public double totalBeats { get { return leadin + beats; } }
    public double movingBeats { get { return beats - leadout; } }
    public string parts = "";
    private double[] partbeats;
    private int currentPart = 0;
    public bool hasParts = false;
    public bool showGrid = false;
    public bool showPaths = false;

    public AnimationView() {
      this.InitializeComponent();
    }

    /**
     *   Starts the animation
     */
    public void doPlay() {
      lastTime = DateTime.Now;
      if (beat > beats)
        beat = -leadin;
      isRunning = true;
      iscore = 0.0;
      canvas.Invalidate();
    }

    /**
     * Pauses the dancers update & animation.
     */
    public void doPause() {
      isRunning = false;
    }

    /**
     *  Rewinds to the start of the animation, even if it is running
     */
    public void doRewind() {
      beat = -leadin;
      canvas.Invalidate();
    }

    /**
     *   Moves to the end of the animation
     */
    public void doEnd() {
      beat = beats;
      canvas.Invalidate();
    }

    /**
     *   Moves the animation back a little
     */
    public void doBackup() {
      beat = Math.Max(beat - 0.1, -leadin);
      canvas.Invalidate();
    }

    /**
     *   Moves the animation forward a little
     */
    public void doForward() {
      beat = Math.Min(beat + 0.1, beats);
      canvas.Invalidate();
    }

    /**
     *   Build an array of floats out of the parts of the animation
     */
    private double[] partsValues() {
      if (parts.Length == 0)
        return new double[] { -2.0, 0.0, beats - 2.0, beats };
      else {
        var b = 0.0;
        var t = parts.Split(';');
        var retval = new List<double>();
        retval.Add(-leadin);
        retval.Add(0);
        for (int i=0; i<t.Length; i++) {
          b += double.Parse(t[i]);
          retval.Add(b);
        }
        retval.Add(beats - 2);
        retval.Add(beats);
        return retval.ToArray();
      }
    }

    /**
     *   Moves the animation to the next part
     */
    public void doNextPart() {
      if (beat < beats) {
        beat = partsValues().Where(b => b > beat).First();
        canvas.Invalidate();
      }
    }

    /**
     *   Moves the animation to the previous part
     */
    public void doPrevPart() {
      if (beat > -leadin) {
        beat = partsValues().Reverse().Where(b => b < beat).First();
        canvas.Invalidate();
      }
    }

    /**
     *   Set the visibility of the grid
     */
    public void setGridVisibility(bool show) {
      showGrid = show;
      canvas.Invalidate();
    }

    /**
     *   Set the visibility of phantom dancers
     */
    public void setPhantomVisibility(bool show) {
      showPhantoms = show;
      if (dancers != null) {
        foreach (Dancer d in dancers) {
          d.hidden = d.isPhantom && !show;
        }
        canvas.Invalidate();
      }
    }

    /**
     *  Turn on drawing of dancer paths
     */
    public void setPathVisibility(bool show) {
      showPaths = show;
      canvas.Invalidate();
    }

    /**
     *   Set animation looping
     */
    public void setLoop(bool loopit) {
      looping = loopit;
      canvas.Invalidate();
    }

    /**
     *   Set display of dancer numbers
     */
    public void setNumbers(int numberem) {
      var n = interactiveDancer >= 0 ? DancerNumbers.OFF : numberem;
      foreach (Dancer d in dancers) {
        d.showNumber = n;
      }
      canvas.Invalidate();
    }
    public void setNumbers(string numberstr) {
      switch (numberstr) {
        case "1-8": setNumbers(DancerNumbers.DANCERS); break;
        case "1-4": setNumbers(DancerNumbers.COUPLES); break;
        default: setNumbers(DancerNumbers.OFF); break;
      }
    }

    /**
     *   Set speed of animation
     */
    public void setSpeed(string myspeed) {
      switch (myspeed) {
        case "Slow": speed = Speed.SLOW; break;
        case "Moderate": speed = Speed.MODERATE; break;
        case "Fast": speed = Speed.FAST; break;
        default: speed = Speed.NORMAL; break;
      }
      canvas.Invalidate();
    }

    /**  Set hexagon geometry  */
    public void setHexagon() {
      geometry = GeometryType.HEXAGON;
      resetAnimation();
    }

    /**  Set bigon geometry  */
    public void setBigon() {
      geometry = GeometryType.BIGON;
      resetAnimation();
    }

    /**  Set square geometry  */
    public void setSquare() {
      geometry = GeometryType.SQUARE;
      resetAnimation();
    }

    public void setGeometry(int g) {
      if (geometry != g) {
        geometry = g;
        resetAnimation();
      }
    }

    /**
     *   Set time of animation as offset from start including leadin
     */
    public void setTime(double b) {
      beat = b - leadin;
      canvas.Invalidate();
    }

    //  Touching a dancer shows and hides its path
    public void doTouch(double x, double y) {
      //  Convert x and y to dance floor coords
      var range = Math.Min(ActualWidth, ActualHeight);
      var s = range / 13.0;
      var px = -(y - ActualHeight / 2) / s;
      var py = -(x - ActualWidth / 2) / s;
      var pv = Vector.Create(px, py);
      //  Compare with dancer locations
      var bestdist = 0.5;
      Dancer bestd = null;
      foreach (var d in dancers) {
        if (!d.hidden) {
          var dp = d.location;
          var distsq = (pv - dp).LengthSquared();
          if (distsq < bestdist) {
            bestd = d;
            bestdist = distsq;
          }
        }
      }
      if (bestd != null) {
        bestd.showPath = !bestd.showPath;
        canvas.Invalidate();
      }
    }

    public bool isInteractiveDancerOnTrack() {
      //  Get where the dancer should be
      var computex = idancer.computeMatrix(beat);
      //  Get computed and actual location vectors
      var ivu = idancer.tx.Location();
      var ivc = computex.Location();

      //  Check dancer's facing direction
      var au = idancer.tx.Angle();
      var ac = computex.Angle();
      if (Math.Abs(Vector.angleDiff(au, ac)) > Math.PI / 4)
        return false;
      //  Check relationship with the other dancers
      foreach (var d in dancers) {
        if (d != idancer) {
          var dv = d.tx.Location();
          //  Compare angle to computed vs actual
          var d2ivu = dv.vectorTo(ivu);
          var d2ivc = dv.vectorTo(ivc);
          var a = d2ivu.angleDiff(d2ivc);
          if (Math.Abs(a) > Math.PI / 4)
            return false;
        }
      }

      return true;
    }

    //  This is called by the system to redraw the animation
    private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
      if (tam != null) {

        //  Update the animation time
        var now = DateTime.Now;
        var diff = now.Subtract(lastTime).Milliseconds;
        if (isRunning)
          beat = beat + diff / speed;
        lastTime = now;

        //  Move the dancers
        updateDancers();
        //  Draw the dancers
        doDraw(args.DrawingSession);

        //  Remember time of this update, and handle loop and end
        prevbeat = beat;
        if (beat >= beats) {
          if (looping && isRunning) {
            prevbeat = -leadin;
            beat = -leadin;
          }
          else if (isRunning) {
            isRunning = false;
            Callouts.AnimationFinished();
          }
        }
        Callouts.progressCallback(beat+leadin);
        //  Continually epeat by telling the system to re-draw
        if (isRunning)
          canvas.Invalidate();
      }
    }

    /**
     * Updates dancers positions based on the passage of realtime.
     */
    private void updateDancers() {
      //  Move dancers
      //  For big jumps, move incrementally -
      //  this helps hexagon and bigon compute the right location
      var delta = beat - prevbeat;
      var incs = (int)Math.Ceiling(Math.Abs(delta));
      for (int j=1; j <= incs; j++) {
        foreach (Dancer d in dancers)
          d.animate(prevbeat + j * delta / incs);
      }
      //  Find the current part, and send a message if it's changed
      var thispart = 0;
      if (beat >= 0 && beat <= beats) {
        for (var i=0; i<partbeats.Length; i++) {
          if (partbeats[i] < beat)
            thispart = i;
        }
      }
      if (thispart != currentPart) {
        currentPart = thispart;
        Callouts.AnimationPart(currentPart);
      }      
      //  Compute handholds
      var hhlist = new List<Handhold>();
      foreach (Dancer d0 in dancers) {
        d0.rightdancer = null;
        d0.leftdancer = null;
        d0.rightHandVisibility = false;
        d0.leftHandVisibility = false;
      }
      for (int i1=0; i1<dancers.Count()-1; i1++) {
        var d1 = dancers[i1];
        if (!d1.isPhantom || showPhantoms) {
          for (int i2 = i1 + 1; i2 < dancers.Count(); i2++) {
            var d2 = dancers[i2];
            if (!d2.isPhantom || showPhantoms) {
              var hh = Handhold.Create(d1, d2, geometry);
              if (hh != null)
                hhlist.Add(hh);
            }
          }
        }
      }
      //  Sort the array to put best scores first
      var hharr = hhlist.ToArray();
      Array.Sort(hharr);
      //  Apply the handholds in order from best to worst
      //  so that if a dancer has a choice it gets the best handhold
      foreach (Handhold hh in hharr) {
        //  Check that the hands aren't already used
        var incenter = geometry == GeometryType.HEXAGON && hh.inCenter;
        if (incenter ||
          (hh.hold1 == Hands.RIGHTHAND && hh.dancer1.rightdancer == null ||
            hh.hold1 == Hands.LEFTHAND && hh.dancer1.leftdancer == null) &&
            (hh.hold2 == Hands.RIGHTHAND && hh.dancer2.rightdancer == null ||
              hh.hold2 == Hands.LEFTHAND && hh.dancer2.leftdancer == null)) {
          //      	Make the handhold visible
          //  Scale should be 1 if distance is 2
          //  float scale = hh.distance/2f;
          if (hh.hold1 == Hands.RIGHTHAND || hh.hold1 == Hands.GRIPRIGHT) {
            hh.dancer1.rightHandVisibility = true;
            hh.dancer1.rightHandNewVisibility = true;
          }
          if (hh.hold1 == Hands.LEFTHAND || hh.hold1 == Hands.GRIPLEFT) {
            hh.dancer1.leftHandVisibility = true;
            hh.dancer1.leftHandNewVisibility = true;
          }
          if (hh.hold2 == Hands.RIGHTHAND || hh.hold2 == Hands.GRIPRIGHT) {
            hh.dancer2.rightHandVisibility = true;
            hh.dancer2.rightHandNewVisibility = true;
          }
          if (hh.hold2 == Hands.LEFTHAND || hh.hold2 == Hands.GRIPLEFT) {
            hh.dancer2.leftHandVisibility = true;
            hh.dancer2.leftHandNewVisibility = true;
          }

          if (!incenter) {
            if (hh.hold1 == Hands.RIGHTHAND) {
              hh.dancer1.rightdancer = hh.dancer2;
              if ((hh.dancer1.hands & Hands.GRIPRIGHT) == Hands.GRIPRIGHT)
                hh.dancer1.rightgrip = hh.dancer2;
            } else {
              hh.dancer1.leftdancer = hh.dancer2;
            if ((hh.dancer1.hands & Hands.GRIPLEFT) == Hands.GRIPLEFT)
                hh.dancer1.leftgrip = hh.dancer2;
          }
            if (hh.hold2 == Hands.RIGHTHAND) {
              hh.dancer2.rightdancer = hh.dancer1;
              if ((hh.dancer2.hands & Hands.GRIPRIGHT) == Hands.GRIPRIGHT)
                hh.dancer2.rightgrip = hh.dancer1;
            } else {
              hh.dancer2.leftdancer = hh.dancer1;
            if ((hh.dancer2.hands & Hands.GRIPLEFT) == Hands.GRIPLEFT)
                hh.dancer2.leftgrip = hh.dancer1;
            }
          }
        }
      }
      //  Clear handholds no longer visible
      foreach (Dancer d in dancers) {
        if (d.leftHandVisibility && !d.leftHandNewVisibility)
          d.leftHandVisibility = false;
        if (d.rightHandVisibility && !d.rightHandNewVisibility)
          d.rightHandVisibility = false;
      }

      //  Update interactive dancer score
      if (idancer != null && beat > 0.0 && beat < beats-leadout) {
        idancer.onTrack = isInteractiveDancerOnTrack();
        if (idancer.onTrack)
          iscore += (beat - Math.Max(prevbeat, 0)) * 10;
      }
    }

    public double getScore() {
      return iscore;
    }

    private void doDraw(CanvasDrawingSession ds) {
      ds.FillRectangle(new Rect(0.0, 0.0, ActualWidth, ActualHeight), Color.FromArgb(255, 255, 240, 224));
      //  Note loop and dancer speed
      //  ...
      //  For interactive leadin, show countdown
      //  ...
      //  Scale coordinate system to dancer's size
      var range = Math.Min(ActualWidth, ActualHeight);
      ds.Transform = Matrix3x2.CreateTranslation(new Vector2((float)ActualWidth/2, (float)ActualHeight/2));
      var s = range / 13;
      //  Flip and rotate
      ds.Transform = Matrix.CreateScale(s,-s) * ds.Transform;
      ds.Transform = Matrix.CreateRotation(Math.PI / 2) * ds.Transform;
      //  Draw grid if on
      if (showGrid)
        GeometryMaker.makeOne(geometry).drawGrid(s,ds);
      //  Always show bigon center mark
      if (geometry == GeometryType.BIGON) {
        ds.DrawLine(0, -0.5f, 0, 0.5f, Colors.Black, 1 / (float)s);
        ds.DrawLine(-0.5f, 0, 0.5f, 0, Colors.Black, 1 / (float)s);
      }
      //  Draw paths if requested
      foreach (Dancer d in dancers) {
        if (!d.hidden && (showPaths || d.showPath))
          d.drawPath(ds);
      }

      //  Draw handholds
      foreach (Dancer d in dancers) {
        var loc = d.location;
        if (d.rightHandVisibility) {
          if (d.rightdancer == null) {  // hexagon center

          }
          else if (d.rightdancer.CompareTo(d) < 0) {
            var loc2 = d.rightdancer.location;
            ds.DrawLine(loc, loc2, Colors.Orange,0.05f);
            ds.FillCircle((loc + loc2) / 2, 0.125f, Colors.Orange);
          }
        }
        if (d.leftHandVisibility) {
          if (d.leftdancer == null) {   // hexagon center
          }
          else if (d.leftdancer.CompareTo(d) < 0) {
            var loc2 = d.leftdancer.location;
            ds.DrawLine(loc, loc2, Colors.Orange, 0.05f);
            ds.FillCircle((loc + loc2) / 2, 0.125f, Colors.Orange);
          }
        }
      }
      
      //  Draw dancers
      foreach (Dancer d in dancers) {
        if (!d.hidden) {
          var txsave = ds.Transform;
          ds.Transform = d.tx * ds.Transform;
          d.draw(ds);
          ds.Transform = txsave;
        }
      }
    }

    /**
     *   This is called to generate or re-generate the dancers and their
     *   animations based on the call, geometry, and other settings.
     * @param xtam     XML element containing the call
     * @param intdan  Dancer controlled by the user, or -1 if not used
     */
    public IXmlNode setAnimation(IXmlNode xtam, int intdan = -1) {
      tam = TamUtils.tamXref(xtam);
      interactiveDancer = intdan;
      resetAnimation();
      return tam;
    }

    public void resetAnimation() {
      if (tam != null) {
        leadin = interactiveDancer < 0 ? 2 : 3;
        leadout = interactiveDancer < 0 ? 2 : 1;
        // if (isRunnning)
        //  doneCallback();
        isRunning = false;
        beats = 0.0;

        var tlist = tam.SelectNodes("formation");
        var formation = tlist.Length > 0
          ? tlist.First()  //  formation defined in animation
          : tam.hasAttr("formation")
          ? TamUtils.getFormation(tam.attr("formation"))  // formation reference to formations.xml
          : tam;  //  formation passed in for sequencer
        var flist = formation.SelectNodes("dancer");
        dancers = new Dancer[flist.Length * (int)geometry];

        //  Except for the phantoms, these are the standard colors
        //  used for teaching callers
        var dancerColor = geometry == GeometryType.HEXAGON ?
          new Color[] { Colors.Red, Colors.ForestGreen, Colors.Magenta,
                        Colors.Blue, Colors.Yellow, Colors.Cyan,
                        Colors.LightGray, Colors.LightGray, Colors.LightGray, Colors.LightGray }
          : 
          new Color[] { Colors.Red, ColorUtilities.ColorFromHex(0xff00c000), Colors.Blue, Colors.Yellow,
                        Colors.LightGray, Colors.LightGray, Colors.LightGray, Colors.LightGray };
        //  Get numbers for dancers and couples
        //  This fetches any custom numbers that might be defined in
        //  the animation to match a Callerlab or Ceder Chest illustration
        var paths = tam.SelectNodes("path");
        var numbers = geometry == GeometryType.HEXAGON ?
          new string[] { "A","E","I",
                      "B","F","J",
                      "C","G","K",
                      "D","H","L",
                      "u","v","w","x","y","z" }
        : geometry == GeometryType.BIGON || paths.Length == 0 ?
          new string[] { "1", "2", "3", "4", "5", "6", "7", "8" }
        : TamUtils.getNumbers(tam);
        var couples = geometry == GeometryType.HEXAGON ?
          new string[] { "1", "3", "5", "1", "3", "5",
                      "2", "4", "6", "2", "4", "6",
                      "7", "8", "7", "8", "7", "8" }
          : geometry == GeometryType.BIGON ?
          new string[] { "1", "2", "3", "4", "5", "6", "7", "8" }
          : paths.Length == 0 ?
          new string[] { "1", "3", "1", "3", "2", "4", "2", "4" }
          : TamUtils.getCouples(tam);
        var geoms = GeometryMaker.makeAll(geometry);

        //  Select a random dancer of the correct gender for the interactive dancer
        var icount = -1;
        var im = Matrix3x2.Identity;
        if (interactiveDancer > 0) {
          var rand = new Random();
          var selector = interactiveDancer == (int)Gender.BOY
            ? "dancer[@gender='boy']" : "dancer[@gender='girl']";
          var glist = formation.SelectNodes(selector);
          icount = rand.Next(glist.Count);
          //  If the animations starts with "Heads" or "Sides"
          //  then select the first dancer.
          //  Otherwise the formation could rotate 90 degrees
          //  which would be confusing
          var title = tam.attr("title");
          if (title.Contains("Heads") || title.Contains("Sides")) {
            icount = 0;
          }
          //  Find the angle the interactive dancer faces at start
          //  We want to rotate the formation so that direction is up
          var iangle = double.Parse(glist.Item((uint)icount).attr("angle"));
          im = Matrix.CreateRotation(-iangle.toRadians()) * im;
          icount = icount * geoms.Count() + 1;
        }

        //  Create the dancers and set their starting positions
        int dnum = 0;
        for (var i=0; i<flist.Length; i++) {
          var fd = flist.ElementAt(i);
          var x = double.Parse(fd.attr("x"));
          var y = double.Parse(fd.attr("y"));
          var angle = double.Parse(fd.attr("angle"));
          var gender = fd.attr("gender");
          var g = gender == "boy" ? Gender.BOY : gender == "girl" ? Gender.GIRL : Gender.PHANTOM;
          var movelist = paths.Length > i ? TamUtils.translatePath(paths.ElementAt(i)) 
                                          : new List<Movement>();
          //  Each dancer listed in the formation corresponds to
          //  one, two, or three real dancers depending on the geometry
          foreach (Geometry geom in geoms) {
            var m = Matrix3x2.Identity * Matrix.CreateRotation(angle.toRadians()) * Matrix.CreateTranslation(x, y) * im;
            var nstr = g == Gender.PHANTOM ? " " : numbers[dnum];
            var cstr = g == Gender.PHANTOM ? " " : couples[dnum];
            var c = g == Gender.PHANTOM ? Colors.LightGray : dancerColor[int.Parse(cstr) - 1];
            //  add one dancer
            //icount -= 1;
            if ((int)g == interactiveDancer && --icount == 0) {
              idancer = new InteractiveDancer(nstr, cstr, g, c, m, geom.clone(), movelist);
              dancers[dnum] = idancer;
            } else {
              dancers[dnum] = new Dancer(nstr, cstr, g, c, m, geom.clone(), movelist);
              dancers[dnum].hidden = g == Gender.PHANTOM && !showPhantoms;
            }
            beats = Math.Max(dancers[dnum].beats+leadout, beats);
            dnum++;
          }
        }  // All dancers added

        //  Initialize other stuff
        parts = tam.attr("parts") + tam.attr("fractions");
        hasParts = tam.attr("parts").Length > 0;
        isRunning = false;
        beat = -leadin;
        prevbeat = -leadin;
        partbeats = partsValues();
        //  force a redraw
        canvas.Invalidate();
        //  ready callback
        Callouts.animationReady();

      }
    }

    public void recalculate() {
      foreach (Dancer d in dancers)
        beats = Math.Max(beats, d.beats + leadout);
    }

  }
}
