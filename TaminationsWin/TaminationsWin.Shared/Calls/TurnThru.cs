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
  class TurnThru : Action {

    public TurnThru() { name = "Turn Thru"; }

    public override Path performOne(Dancer d, CallContext ctx) {
      //  Can only turn thru with another dancer
      //  in front of this dancer
      //  who is also facing this dancer
      var d2 = ctx.dancerInFront(d);
      if (d2 != null) {
        var dist = CallContext.distance(d, d2);
        return TamUtils.getMove("Extend Left").scale(dist / 2, 0.5)
          .add(TamUtils.getMove("Swing Right").scale(0.5, 0.5))
          .add(TamUtils.getMove("Extend Right").scale(dist / 2, 0.5));
      }
      else
        throw new CallError($"Cannot find dancer to Turn Thru with {d}");
    }

  }
}
