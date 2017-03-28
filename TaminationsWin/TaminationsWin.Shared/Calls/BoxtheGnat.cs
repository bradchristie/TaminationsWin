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

namespace TaminationsWin.Calls{
  class BoxtheGnat : Action {

    public BoxtheGnat() {
      name = "Box the Gnat";
    }

    public override Path performOne(Dancer d, CallContext ctx) {
      var d2 = ctx.dancerFacing(d);
      if (d2 != null) {
        var dist = CallContext.distance(d, d2);
        var cy1 = d.gender == Gender.BOY ? 1 : 0.1;
        var y4 = d.gender == Gender.BOY ? -2 : 2;
        var hands = d.gender == Gender.BOY ? Hands.GRIPLEFT : Hands.GRIPRIGHT;
        var m = new Movement(4.0, hands,
          1, cy1, dist / 2, cy1, dist / 2 + 1, 0, 1.3, 1.3, y4, 0, y4);
        return new Path(m);
      }
      throw new CallError($"Cannot find dancer to turn with {d.number}");
    }

  }
}
