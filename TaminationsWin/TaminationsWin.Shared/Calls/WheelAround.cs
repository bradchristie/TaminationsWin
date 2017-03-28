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
  class WheelAround : Action {

    public WheelAround() { name = "Wheel Around"; }

    public override Path performOne(Dancer d, CallContext ctx) {
      var d2 = d.data.partner;
      if (d2 != null) {
        if (!d2.data.active)
          throw new CallError($"Dancer {d} must Wheel Around with partner");
        if ((d.data.beau ^ d2.data.beau) && (d.data.belle ^ d2.data.belle))
          return TamUtils.getMove(d.data.beau ? "Beau Wheel" : "Belle Wheel");
        else
          throw new CallError($"Dancer {d} is not part of a Facing Couple");
      } else
        throw new CallError($"Dancer {d} is not part of a Facing Couple");
    }

  }
}
