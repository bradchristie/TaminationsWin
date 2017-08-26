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
using System.Numerics;
using Windows.Data.Xml.Dom;

namespace TaminationsWin {

  public struct Hands {
    public static int NOHANDS = 0;
    public static int LEFTHAND = 1;
    public static int RIGHTHAND = 2;
    public static int BOTHHANDS = 3;
    public static int GRIPLEFT = 5;
    public static int GRIPRIGHT = 6;
    public static int GRIPBOTH = 7;
    public static int ANYGRIP = 4;
  }

  public class Movement {

    public double beats,fullbeats;
    public int hands;
    public Bezier btranslate, brotate;

    public static int getHands(string s) {
      switch (s) {
        case "none": return Hands.NOHANDS;
        case "nohands": return Hands.NOHANDS;
        case "left": return Hands.LEFTHAND;
        case "right": return Hands.RIGHTHAND;
        case "both": return Hands.BOTHHANDS;
        case "anygrip": return Hands.ANYGRIP;
        case "gripleft": return Hands.GRIPLEFT;
        case "gripright": return Hands.GRIPRIGHT;
        case "gripboth": return Hands.GRIPBOTH;
        default: return Hands.NOHANDS;
      }
    }

    public Movement(double fullbeats, int hands,
                    double cx1, double cy1, double cx2, double cy2, double x2, double y2,
                    double cx3, double cx4, double cy4, double x4, double y4, double beats=0) {
      this.beats = beats > 0 ? beats : fullbeats;
      this.hands = hands;
      this.fullbeats = fullbeats;
      btranslate = new Bezier(0, 0, cx1, cy1, cx2, cy2, x2, y2);
      brotate = new Bezier(0, 0, cx3, 0, cx4, cy4, x4, y4);
    }

    public Movement(IXmlNode elem) :
        this(elem.attr("beats").toDouble(),
             getHands(elem.attr("hands")),
             elem.attr("cx1").toDouble(),
             elem.attr("cy1").toDouble(),
             elem.attr("cx2").toDouble(),
             elem.attr("cy2").toDouble(),
             elem.attr("x2").toDouble(),
             elem.attr("y2").toDouble(),
             elem.attr(elem.hasAttr("cx3") ? "cx3" : "cx1").toDouble(),
             elem.attr(elem.hasAttr("cx4") ? "cx4" : "cx2" ).toDouble(),
             elem.attr(elem.hasAttr("cy4") ? "cy4" : "cy2" ).toDouble(),
             elem.attr(elem.hasAttr("x4") ? "x4" : "x2" ).toDouble(),
             elem.attr(elem.hasAttr("y4") ? "y4" : "y2" ).toDouble(),
             elem.attr("beats").toDouble())
      { }
      
    

    /**
     * Return a matrix for the translation part of this movement at time t
     * @param t  Time in beats
     * @return   Matrix for using with canvas
     */
    public Matrix3x2 translate(double t) {
      var tt = Math.Min(Math.Max(0, t), fullbeats);
      return btranslate.translate(tt / fullbeats);
    }
    public Matrix3x2 translate() { return translate(beats); }

    /**
     * Return a matrix for the rotation part of this movement at time t
     * @param t  Time in beats
     * @return   Matrix for using with canvas
     */
    public Matrix3x2 rotate(double t) {
      var tt = Math.Min(Math.Max(0, t), fullbeats);
      return brotate.rotate(tt / fullbeats);
    }
    public Matrix3x2 rotate() { return rotate(beats); }

    /**
     * Return a new movement by changing the beats
     */
    public Movement time(double b) {
      return new Movement(b, hands, btranslate.ctrlx1, btranslate.ctrly1, btranslate.ctrlx2, btranslate.ctrly2,
                          btranslate.x2, btranslate.y2,
                          brotate.ctrlx1, brotate.ctrlx2, brotate.ctrly2, brotate.x2, brotate.y2, b);

    }

    /**
     * Return a new movement by changing the hands
     */
    public Movement useHands(int h) {
      return new Movement(fullbeats, h, btranslate.ctrlx1, btranslate.ctrly1, btranslate.ctrlx2, btranslate.ctrly2,
                          btranslate.x2, btranslate.y2,
                          brotate.ctrlx1, brotate.ctrlx2, brotate.ctrly2, brotate.x2, brotate.y2, beats);
    }

    private int switchHands() {
      if (hands == Hands.RIGHTHAND)
        return Hands.LEFTHAND;
      else if (hands == Hands.LEFTHAND)
        return Hands.RIGHTHAND;
      else if (hands == Hands.GRIPRIGHT)
        return Hands.GRIPLEFT;
      else if (hands == Hands.GRIPLEFT)
        return Hands.GRIPRIGHT;
      else
        return hands;
    }

    /**
     * Return a new Movement scaled by x and y factors.
     * If y is negative hands are also switched.
     */
    public Movement scale(double x, double y) {
      return new Movement(fullbeats, y < 0 ? switchHands() : hands, 
                          btranslate.ctrlx1*x, btranslate.ctrly1*y, btranslate.ctrlx2*x, btranslate.ctrly2*y,
                          btranslate.x2*x, btranslate.y2*y,
                          brotate.ctrlx1*x, brotate.ctrlx2*x, brotate.ctrly2*y, brotate.x2*x, brotate.y2*y, beats);

    }

    /**
     * Return a new Movement with the end point shifted by x and y
     */
    public Movement skew(double x, double y) {
      return beats < fullbeats ? skewClip(x,y) : skewFull(x,y);
    }

    public Movement skewFull(double x, double y) {
      return new Movement(fullbeats, hands, btranslate.ctrlx1, btranslate.ctrly1, 
                          btranslate.ctrlx2+x, btranslate.ctrly2+y, btranslate.x2+x, btranslate.y2+y,
                          brotate.ctrlx1, brotate.ctrlx2, brotate.ctrly2, brotate.x2, brotate.y2, beats);
    }

    /**
     *   Skew a movement that has been clipped, adjusting so the amount of
     *   skew is appplied to the clip point
     */
    public Movement skewClip(double x, double y) {
      var vdelta = Vector.Create(x,y);
      var vfinal = translate().Location() + vdelta;
      var m = this;
      var maxiter = 100;
      do {
        // Shift the end point by the current difference
        m = m.skewFull(vdelta.X,vdelta.Y);
        // See how that affects the clip point
        var loc = m.translate().Location();
        vdelta = vfinal - loc;
        maxiter -= 1;
      } while (vdelta.Length() > 0.001 && maxiter > 0);
      //  If timed out, return original rather than something that
      //  might put the dancers in outer space
      return maxiter > 0 ? m : this;
}

public Movement reflect() {
      return scale(1, -1);
    }

    public Movement clip(double b) {
      return new Movement(fullbeats, hands, btranslate.ctrlx1, btranslate.ctrly1, btranslate.ctrlx2, btranslate.ctrly2,
                          btranslate.x2, btranslate.y2,
                          brotate.ctrlx1, brotate.ctrlx2, brotate.ctrly2, brotate.x2, brotate.y2, b);
    }

  }

}
