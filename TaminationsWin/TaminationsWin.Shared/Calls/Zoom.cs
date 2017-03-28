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
  class Zoom : Action {

    public Zoom() { name = "Zoom"; }

    public override Path performOne(Dancer d, CallContext ctx) {
      if (d.data.leader) {
        var d2 = ctx.dancerInBack(d);
        if (d2 == null || !d2.data.active)
          throw new CallError($"Trailer of {d} must also Zoom");
        var a = CallContext.angle(d);
        var c = a < 0 ? "Run Left" : "Run Right";
        var dist = CallContext.distance(d, d2);
        return TamUtils.getMove(c).changebeats(2).skew(-dist / 2, 0)
          .add(TamUtils.getMove(c).changebeats(2).skew(dist / 2, 0));
      } else if (d.data.trailer) {
        var d2 = ctx.dancerInFront(d);
        if (d2 == null || !d2.data.active)
          throw new CallError($"Leader of {d} must also Zoom");
        var dist = CallContext.distance(d, d2);
        return TamUtils.getMove("Forward").changebeats(4).scale(dist, 1);
      } else
        throw new CallError($"Dancer {d} cannot Zoom");
    }

  }
}
