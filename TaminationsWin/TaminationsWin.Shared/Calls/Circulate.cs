using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaminationsWin.Calls
{
  class Circulate : Action
  {

    public Circulate() {
      name = "Circulate";
    }

    public override void perform(CallContext ctx,int i = 0) {
      //  If just 4 dancers, try Box Circulate
      if (ctx.actives.Count == 4) {
        if (ctx.actives.All(d => d.data.center)) {
          try {
            ctx.applyCalls("box circulate");
          }
          catch (CallError) {
            //  That didn't work, try to find a circulate path for each dancer
            base.perform(ctx);
          }
        }
        else
          base.perform(ctx);
      }
      //  If two-faced lines, do Couples Circulate
      else if (ctx.isTwoFacedLines())
        ctx.applyCalls("couples circulate");
      //  If in waves or lines, then do All 8 Circulate
      else if (ctx.isLines())
        ctx.applyCalls("all 8 circulate");
      //  If in columns, do Column Circulate
      else if (ctx.isColumns())
        ctx.applyCalls("column circulate");
      //  Otherwise ... ???
      else
        throw new CallError("Cannot figure out how to Circulate.");
    }

    public override Path performOne(Dancer d,CallContext ctx) {
      if (d.data.leader) {
        //  Find another active dancer in the same line and move to that spot
        Dancer d2 = ctx.dancerClosest(d,dx => dx.data.active &&
                      (CallContext.isRight(d)(dx) || CallContext.isLeft(d)(dx)));
        if (d2 != null) {
          double dist = CallContext.distance(d,d2);
          return TamUtils.getMove(CallContext.isRight(d)(d2) ? "Run Right" : "Run Left")
        .scale(dist/3,dist/2).changebeats(4.0);
        }
      }
      else if (d.data.trailer) {
        //  Looking at active leader?  Then take its place
        //  TODO maybe allow diagonal circulate?
        Dancer d2 = ctx.dancerInFront(d);
        if (d2 != null && d2.data.active) {
          double dist = CallContext.distance(d,d2);
          return TamUtils.getMove("Forward").scale(dist,1.0).changebeats(4.0);
        }
      }
      throw new CallError("Cannot figure out how to Circulate.");
    }

  }
}
