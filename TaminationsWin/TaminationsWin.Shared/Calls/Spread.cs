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
using System.Linq;

namespace TaminationsWin.Calls {
  class Spread : Action {

    public Spread() { name = "and Spread"; }

    /*
     * 1. If only some of the dancers are directed to Spread (e.g., from a
     * static square, Heads Star Thru & Spread), they slide apart sideways to
     * become ends, as the inactive dancers step forward between them.
     *
     * 2. If the (Anything) call finishes in lines or waves (e.g., Follow Your Neighbor),
     * the centers anticipate the Spread action by sliding apart sideways to
     * become the new ends, while the original ends anticipate the Spread action
     * by moving into the nearest center position.
     *
     * 3. If the (Anything) call finishes in tandem couples
     *  (e.g., Wheel & Deal from a line of four), the lead dancers slide apart sideways,
     *  while the trailing dancers step forward between them.
     */

    public override void performCall(CallContext ctx, int i) {
      //  Is this spread from waves, tandem, actives?
      Action spreader = null;
      if (ctx.actives.Count == ctx.dancers.Count/2) {
        if (new CallContext(ctx.actives).isLine())
          spreader = new Case2();  //  Case 2: Active dancers in line or wave spread among themselves
        else
          spreader = new Case1();  //  Case 1: Active dancers spread and let in the others
      }
      else if (ctx.isLines())
        spreader = new Case2();  //  Case 2
      else if (ctx.dancers.TrueForAll(d => ctx.isInTandem(d)))
        spreader = new Case3();  // case 3
      if (spreader != null)
        spreader.perform(ctx);
      else
        throw new CallError("Can not figure out how to Spread");
    }

  }

  class Case1 : Action {

    public override void perform(CallContext ctx,int i) {
      ctx.levelBeats();
      ctx.dancers.ForEach(d => {
        if (d.data.active) {
          //  Active dancers spread apart
          String m;
          if (ctx.dancersToRight(d).Count() == 0)
            m = "Dodge Right";
          else if (ctx.dancersToLeft(d).Count() == 0)
            m = "Dodge Left";
          else
            throw new CallError("Can not figure out how to Spread");
          d.path.add(TamUtils.getMove(m).changebeats(2.0));
        }
        else {
          //  Inactive dancers move forward
          Dancer d2 = ctx.dancerInFront(d);
          if (d2 != null) {
            double dist = CallContext.distance(d,d2);
            d.path.add(TamUtils.getMove("Forward").scale(dist,1.0).changebeats(2.0));
          }
        }
      });
    }

  }

  class Case2 : Action {

    public override Path performOne(Dancer d,CallContext ctx) {
      //  This is for waves only
      //  Compute offset for spread
      var v = Vector.Create(0,d.data.belle ? 2 : d.data.beau ? -2 : 0);
      //  Pop off the last movement and shift it by that offset
      var m = d.path.pop();
      var tx = m.rotate();
      var v2 = v.concatenate(tx);
      d.path.add(m.skew(v2.X,v2.Y).useHands(Hands.NOHANDS));
      //  Return dummy path
      return new Path();
    }

  }

  class Case3 : Case1 {

    public override void perform(CallContext ctx, int i) {
      //  Mark the leaders as active
      ctx.dancers.ForEach(d => d.data.active = d.data.leader);
      //  And forward to Case1, actives spread
      base.perform(ctx,i);
    }

  }

}
