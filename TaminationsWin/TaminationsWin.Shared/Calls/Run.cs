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
  class Run : Action {

    public Run() { name = "Run"; }

    public override void perform(CallContext ctx, int i = 0) {
      //  We need to look at all the dancers, not just actives
      //  because partners of the runners need to dodge
      ctx.dancers.ForEach(d => {
        if (d.data.active) {
          //  Partner must be inactive
          var d2 = d.data.partner;
          if (d2 != null) {
            if (!d2.data.active)
              d.path.add(TamUtils.getMove(d.data.beau ? "Run Right" : "Run Left"));
            else
              throw new CallError("Dancer and partner cannot both Run");
          } else
            throw new CallError($"Dancer {d.number} has nobody to Run around");
        }
        else { // inactive dancer
          var d2 = d.data.partner;
          if (d2 != null && d2.data.active)
            d.path.add(TamUtils.getMove(d.data.beau ? "Dodge Right" : "Dodge Left"));
        }
      });      
    }

  }
}
