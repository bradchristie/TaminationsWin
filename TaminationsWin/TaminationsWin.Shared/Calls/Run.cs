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
          //  Find dancer to run around
          //  Usually it's the partner
          var d2 = d.data.partner;
          if (d2 == null)
            throw new CallError($"Dancer {d.number} has nobody to Run around.");
          //  But special case of t-bones, could be the dancer on the other side,
          //  check if another dancer is running around this dancer's "partner"
          var d3 = d2.data.partner;
          if (d != d3 && d3 != null && d3.data.active) {
            d2 = ctx.dancerToRight(d);
            if (d2 == d.data.partner)
              d2 = ctx.dancerToLeft(d);
          }
          //  Partner must be inactive
          if (d2==null || d2.data.active)
            throw new CallError($"Dancer {d.number} has nobody to Run around.");
          var m = CallContext.isRight(d)(d2) ? "Run Right" : "Run Left";
          var dist = CallContext.distance(d,d2);
          d.path = TamUtils.getMove(m).scale(1,dist/2);
          //  Also set path for partner
          if (CallContext.isRight(d2)(d))
            m = "Dodge Right";
          else if (CallContext.isLeft(d2)(d))
            m = "Dodge Left";
          else if (CallContext.isInFront(d2)(d))
            m = "Forward 2";
          else if (CallContext.isInBack(d2)(d))
            m = "Back 2";   //  really ???
          else
            m = "Stand";   // should never happen
          d2.path = TamUtils.getMove(m).scale(1,dist/2);
        }
      });      
    }

  }
}
