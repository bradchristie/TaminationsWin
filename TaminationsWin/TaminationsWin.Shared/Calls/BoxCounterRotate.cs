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

namespace TaminationsWin.Calls {
  class BoxCounterRotate : Action {

    public BoxCounterRotate() {
      name = "Box Counter Rotate";
    }

    public override Path performOne(Dancer d, CallContext ctx) {
      var v = d.location;
      var v2 = v;
      var cy4 = 0.0;
      var y4 = 0.0;
      var a1 = d.tx.Angle();
      var a2 = v.Angle();
      //  Determine if this is a rotate left or right
      var angdif = a2.angleDiff(a1);
      if (angdif < 0) {
        //  Left
        v2 = v.Rotate(Math.PI / 2);
        cy4 = 0.45;
        y4 = 1;
      } else {
        //  Right
        v2 = v.Rotate(-Math.PI / 2);
        cy4 = -0.45;
        y4 = -1;
      }
      //  Compute the control points
      var dv = (v2 - v).Rotate(-a1);
      var cv1 = (v2 * 0.5f).Rotate(-a1);
      var cv2 = (v * 0.5f).Rotate(-a1) + dv;
      var m = new Movement(2.0, Hands.NOHANDS,
        cv1.X, cv1.Y, cv2.X, cv2.Y, dv.X, dv.Y, 0.55, 1, cy4, 1, y4);
      return new Path(m);
    }

  }
}
