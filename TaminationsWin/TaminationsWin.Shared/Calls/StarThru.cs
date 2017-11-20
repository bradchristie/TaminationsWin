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
  class StarThru : Action {

    public StarThru() { name = "Star Thru"; }

    public override void perform(CallContext ctx, int i = 0) {
      //  Check that facing dancers are opposite genders
      ctx.actives.ForEach(d =>
      {
        var d2 = ctx.dancerInFront(d);
        if (d2 == null || !d2.data.active || ctx.dancerInFront(d2) != d)
          throw new CallError($"Dancer {d.number} has nobody to Star Thru with");
        if (d2.gender == d.gender)
          throw new CallError($"Dancer {d.number} cannot Star Thru with another dancer of the same gender");
      });
      //  All ok
      ctx.applyCalls("Slide Thru");
    }

  }
}
