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

namespace TaminationsWin
{

  public class Bezier {

    public double x1, y1, ctrlx1, ctrly1, ctrlx2, ctrly2, x2, y2;
    private double ax, bx, cx, ay, by, cy;

    public Bezier(double x1, double y1,
                  double ctrlx1, double ctrly1,
                  double ctrlx2, double ctrly2,
                  double x2, double y2) {
      this.x1 = x1;
      this.y1 = y1;
      this.ctrlx1 = ctrlx1;
      this.ctrly1 = ctrly1;
      this.ctrlx2 = ctrlx2;
      this.ctrly2 = ctrly2;
      this.x2 = x2;
      this.y2 = y2;
      cx = 3 * (ctrlx1 - x1);
      bx = 3 * (ctrlx2 - ctrlx1) - cx;
      ax = x2 - x1 - cx - bx;
      cy = 3 * (ctrly1 - y1);
      by = 3 * (ctrly2 - ctrly1) - cy;
      ay = y2 - y1 - cy - by;
    }

    //  Compute X, Y values for a specific t value
    private double xt(double t) {
      return x1 + t * (cx + t * (bx + t * ax));
    }
    private double yt(double t) {
      return y1 + t * (cy + t * (by + t * ay));
    }
    //  Compute dx, dy values for a specific t value
    private double dxt(double t) {
      return cx + t * (2.0 * bx + t * 3.0 * ax);
    }
    private double dyt(double t) {
      return cy + t * (2.0 * by + t * 3.0 * ay);
    }
    private double angle(double t) {
      return Math.Atan2(dyt(t),dxt(t));
    }

    //  Return the movement along the curve given "t" between 0 and 1
    public Matrix3x2 translate(double t) {
      var x = xt(t);
      var y = yt(t);
      return Matrix.CreateTranslation(x, y);
    }

    public Matrix3x2 rotate(double t) {
      var theta = angle(t);
      return Matrix.CreateRotation(theta);
    }

    //  Return turn direction at end of curve
    public double rolling() {
      //  Check angle at end
      var theta = angle(1.0);
      //  If it's 180 then use angle at halfway point
      if (theta.angleEquals(Math.PI))
        theta = angle(0.5);
      //  If angle is 0 then no turn
      if (theta.angleEquals(0.0))
        return 0.0;
      else
        return theta;
    }

  }

}
