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
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;

namespace TaminationsWin {

  public enum Gender {
    BOY = 1,
    GIRL = 2,
    PHANTOM = 3
  }

  public struct DancerNumbers {
    public const int OFF = 0;
    public const int DANCERS = 1;
    public const int COUPLES = 2;
  }

  public class Dancer : IComparable {

    public string number = " ";
    private string number_couple = " ";
    public int showNumber = DancerNumbers.OFF;
    private Geometry geom;
    protected Color drawColor;
    protected Color fillColor;
    public Gender gender;
    public bool hidden = false;
    public Matrix3x2 starttx;
    public Path path;
    public bool showPath = false;
    public Dancer clonedFrom = null;
    public int hands = (int)Hands.NOHANDS;
    public Matrix3x2 tx;
    public Vector2 loc;
    private CanvasCachedGeometry pathpath = null;
    private double lastbeat;

    //  for computing handholds
    public Dancer leftdancer = null;
    public Dancer rightdancer = null;
    public Dancer leftgrip = null;
    public Dancer rightgrip = null;
    public bool rightHandVisibility = false;
    public bool leftHandVisibility = false;
    public bool rightHandNewVisibility = false;
    public bool leftHandNewVisibility = false;

    public DancerData data = new DancerData();

    /**
     *     Constructor for a new dancer
     * @param number    Number to show when Number display is on
     * @param number_couple  Number to show when Couples Number display is on
     * @param gender    Gender - boy, girl, phantom
     * @param fillcolor    Base color
     * @param mat  Transform for dancer's start position
     * @param geom  Square, Bigon, Hexagon
     * @param moves   List of Movements for dancer's path
     */
    public Dancer(string number, string number_couple, Gender gender,
      Color fillColor, Matrix3x2 mat, Geometry geom, List<Movement> moves) {
      this.fillColor = fillColor;
      this.drawColor = fillColor.darker();
      this.gender = gender;
      this.geom = geom;
      this.number = number;
      this.number_couple = number_couple;
      starttx = geom.startMatrix(mat);  // only changed by sequencer
      path = new Path(moves);  // only changed by sequencer
      // Compute points of path for drawing path
      tx = Matrix3x2.Identity;
      //  TODO pathpath
      // ...
      //  Restore dancer to start position
      animateComputed(-2.0);
    }

    public Dancer(Dancer from) : this(from.number, from.number_couple, from.gender, from.fillColor, from.tx,
      GeometryMaker.makeOne(from.geom.geometry(), 0), new List<Movement>()) {
      clonedFrom = from;
    }

    public Vector2 location {
      get {
        return new Vector2(tx.M31, tx.M32);
      }
    }

    public bool isPhantom {
      get {
        return gender == Gender.PHANTOM;
      }
    }

    /**
     *   Used for hexagon handholds
     * @return  True if dancer is close enough to center to make a center star
     */
    public bool inCenter {
      get {
        return location.Length() < 1.1;
      }
    }

    /**
     * @return  Total number of beats used by dancer's path
     */
    public double beats {
      get {
        return path.beats;
      }
    }

    /**
     *   Move dancer to location along path
     * @param beat where to place dancer
     */
    public virtual void animate(double beat) {
      hands = path.hands(beat);
      tx = path.animate(beat) * starttx;
      tx = tx * geom.pathMatrix(starttx, tx, beat);
      lastbeat = beat; //  needed in case paths are computed
    }

    public void animateToEnd() {
      animate(beats);
    }

    protected virtual void animateComputed(double beat) {
      animate(beat);
    }

    public void rotateStartAngle(double angle) {
      starttx = Matrix.CreateRotation(angle.toRadians()) * starttx;
      tx = starttx;  // structs so this makes a copy
    }

    /**
     *   Draw the entire dancer's path as a translucent colored line
     * @param ds  Canvas to draw to
     */
    public void drawPath(CanvasDrawingSession ds) {
      if (pathpath == null) {
        var savebeat = lastbeat;
        CanvasPathBuilder pathBuilder = new CanvasPathBuilder(ds);
        animateComputed(0.0);
        var loc = location;
        pathBuilder.BeginFigure(loc.X, loc.Y);
        for (double beat = 0.1; beat <= beats; beat += 0.1) {
          animateComputed(beat);
          loc = location;
          pathBuilder.AddLine(loc.X, loc.Y);
        }
        pathBuilder.EndFigure(CanvasFigureLoop.Open);
        var cg = CanvasGeometry.CreatePath(pathBuilder);
        pathpath = CanvasCachedGeometry.CreateStroke(cg, 0.1f);
        animate(savebeat);  //  restore current position
      }
      ds.DrawCachedGeometry(pathpath, Color.FromArgb(80, fillColor.R, fillColor.G, fillColor.B));
    }

    public void draw(CanvasDrawingSession ds) {
      //  Draw the head
      ds.FillCircle(Vector.Create(0.5, 0), 0.33f, drawColor);
      //  Draw the body
      //  Draw the body outline
      var c = showNumber == DancerNumbers.OFF || gender == Gender.PHANTOM ? fillColor : fillColor.veryBright();
      switch (gender) {
        case Gender.BOY:
          ds.FillRectangle(-0.5f, -0.5f, 1, 1, c);
          ds.DrawRectangle(-0.5f, -0.5f, 1, 1, drawColor, 0.1f);
          break;
        case Gender.GIRL:
          ds.FillCircle(Vector.Create(0,0), 0.5f, c);
          ds.DrawCircle(Vector.Create(0, 0), 0.5f, drawColor, 0.1f);
          break;
        default:  // phantom
          ds.FillRoundedRectangle(-0.5f, -0.5f, 1, 1, 0.3f, 0.3f, c);
          ds.DrawRoundedRectangle(-0.5f, -0.5f, 1, 1, 0.3f, 0.3f, drawColor, 0.1f);
          break;
      }
      //  Draw number if on
      if (showNumber != DancerNumbers.OFF) {
        //  The dancer is rotated relative to the display, but of course
        //  the dancer number should not be rotated.
        //  So the number needs to be transformed back
        var angle = Math.Atan2(ds.Transform.M21, ds.Transform.M22);
        var txtext = Matrix.CreateScale(1,-1) * Matrix.CreateRotation(-angle + Math.PI); // * Matrix.CreateScale(1, -1);
        ds.Transform = txtext * ds.Transform;
        var font = new CanvasTextFormat();
        font.FontSize = 0.8f;
        font.VerticalAlignment = CanvasVerticalAlignment.Center;
        font.HorizontalAlignment = CanvasHorizontalAlignment.Center;
        ds.DrawText(showNumber == DancerNumbers.COUPLES ? number_couple : number, 0f, 0f, Colors.Black, font);
      }
    }

    public int CompareTo(object obj) {
      return ((IComparable)number).CompareTo(((Dancer)obj).number);
    }
  }

}
