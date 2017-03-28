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

namespace TaminationsWin.Calls {
  abstract class QuarterTurns : Action {

    public virtual string select(CallContext ctx, Dancer d) { return null; }

    public override Path performOne(Dancer d, CallContext ctx) {
      var offsetX = 0.0;
      var move = select(ctx, d);
      //  If leader or trailer, make sure to adjust quarter turn
      //  so handhold is possible
      if (move != "Stand") {
        if (d.data.leader) {
          var d2 = ctx.dancerInBack(d);
          var dist = CallContext.distance(d,d2);
          if (dist > 2)
            offsetX = -(dist-2)/2;
        }
        if (d.data.trailer) {
          var d2 = ctx.dancerInFront(d);
          var dist = CallContext.distance(d,d2);
          if (dist > 2)
            offsetX = (dist-2)/2;
        }
      }
      return TamUtils.getMove(move).skew(offsetX, 0.0);
    }

  }
}
