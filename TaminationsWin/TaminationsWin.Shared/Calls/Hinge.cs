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
  class Hinge : Action {

    public Hinge() { name = "Hinge"; }

    public override Path performOne(Dancer d, CallContext ctx) {
      //  Find the dancer to hinge with
      Dancer d2 = null;
      var d3 = ctx.dancerToRight(d);
      var d4 = ctx.dancerToLeft(d);
      var d0 = d.data.partner;
      if (d0 != null && d0.data.active)
        d2 = d0;
      else if (d3 != null & d3.data.active)
        d2 = d3;
      else if (d4 != null && d4.data.active)
        d2 = d4;
      if (d2 == null)
        throw new CallError($"Dancer {d.number} cannot Hinge");
      var m = CallContext.isRight(d)(d2) ? "Hinge Right" : "Hinge Left";
      return TamUtils.getMove(m);        
    }

  }
}
