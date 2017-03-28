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
using System.Collections.Generic;
using System.Linq;

namespace TaminationsWin.Calls {
  class CrossRun : Action {

    public CrossRun() {
      name = "Cross Run";
    }

    public override void perform(CallContext ctx, int i = 0) {
      //  Centers and ends cannot both cross run
      if (ctx.dancers.Any(d => d.data.active && d.data.center) &&
          ctx.dancers.Any(d => d.data.active && d.data.end))
        throw new CallError("Centers and ends cannot both Cross Run");
      //  We need to look at all the dancers, not just actives
      //  because partners of the runners need to dodge
      ctx.dancers.ForEach(d => {
        if (d.data.active) {
          //  Must be in a 4-dancer wave or line
          if (!d.data.center && !d.data.end)
            throw new CallError("General line required for Cross Run");
          //  Partner must be inactive
          var d2 = d.data.partner;
          if (d2 != null) {
            if (d2.data.active)
              throw new CallError("Dancer and partner cannot both Cross Run");
            //  Center beaus and end belles ruin left
            var isright = d.data.beau ^ d.data.center;
            //  TODO check for runners crossing paths
            var m = isright ? "Run Right" : "Run Left";
            d.path = TamUtils.getMove(m).scale(1, 2);
          } else // Runner has no partner
            throw new CallError("Nobody to Cross Run around");
        } else {
          //  Not an active dancer
          //  If partner is active then this dancer needs to dodge
          var d2 = d.data.partner;
          if (d2 != null && d2.data.active) {
            d.path = TamUtils.getMove(d.data.beau ? "Dodge Right" : "Dodge Left");
          }
        }
      });
    }

  }
}
