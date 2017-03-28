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
using System.Linq;

namespace TaminationsWin.Calls {
  class Trade : Action {

    public Trade() { name = "Trade"; }

    public override Path performOne(Dancer d, CallContext ctx) {
      //  Figure out what dancer we're trading with
      var leftcount = 0;
      var bestleft = d;
      var rightcount = 0;
      var bestright = d;
      ctx.actives.ForEach(d2 => {
        if (d2 != d) {
          if (CallContext.isLeft(d)(d2)) {
            if (leftcount == 0 || CallContext.distance(d, d2) < CallContext.distance(d, bestleft))
              bestleft = d2;
            leftcount++;
          } else if (CallContext.isRight(d)(d2)) {
            if (rightcount == 0 || CallContext.distance(d, d2) < CallContext.distance(d, bestright))
              bestright = d2;
            rightcount++;
          }
        }
      });

      var dtrade = d;
      var samedir = false;
      var call = "";
      //  We trade with the nearest dancer in the direction with
      //  an odd number of dancers
      if (rightcount % 2 == 1 & leftcount % 2 == 0) {
        dtrade = bestright;
        call = "Run Right";
        samedir = CallContext.isLeft(dtrade)(d);
      } else if (rightcount % 2 == 0 && leftcount % 2 == 1) {
        dtrade = bestleft;
        call = "Run Left";
        samedir = CallContext.isRight(dtrade)(d);
      } else
        throw new CallError("Unable to calculate Trade");

      //  Found the dancer to trade with.
      //  Now make room for any dancers in between
      var hands = Hands.NOHANDS;
      var dist = CallContext.distance(d, dtrade);
      var scaleX = 1.0;
      if (ctx.inBetween(d, dtrade).Count() > 0) {
        //  Intervening dancers
        //  Allow enough room to get around them and pass right shoulders
        if (call == "Run Right" && samedir)
          scaleX = 2.0;
      } else {
        //  No intervening dancers
        if (call == "Run Left" && samedir)
          //  Partner trade, flip the belle
          call = "Flip Left";
        else
          scaleX = dist / 2;
        //  Hold hands for miniwave trades
        if (!samedir && dist < 2.1)
          hands = call == "Run Left" ? Hands.LEFTHAND :Hands.RIGHTHAND;
      }
      return TamUtils.getMove(call).changehands(hands).scale(scaleX, dist / 2);
    }

  }
}
