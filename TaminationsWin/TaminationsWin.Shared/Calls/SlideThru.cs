﻿/*

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
  class SlideThru : Action {

    public SlideThru() { name = "Slide Thru"; }

    public override Path performOne(Dancer d, CallContext ctx) {
      var d2 = ctx.dancerFacing(d);
      if (d2 != null) {
        var dist = CallContext.distance(d, d2);
        var m = TamUtils.getMove("Extend Left").scale(dist / 2, 0.5);
        if (d.gender == Gender.BOY)
          m.add(TamUtils.getMove("Lead Right").scale(dist / 2, 0.5));
        else
          m.add(TamUtils.getMove("Quarter Left").skew(dist / 2, -0.5));
        return m;
      } else throw new CallError($"Dancer {d.number} has nobody to Slide Thru with");
    }

  }
}
