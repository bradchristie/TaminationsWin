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
  class Half : Action {

    double prevbeats = 0.0;
    double halfbeats = 0.0;
    Call call;
    
    public Half() {
      name = "Half";
      call = this;
    }

    public override void perform(CallContext ctx, int i = 0) {
      //  Steal the next call off the stack
      call = ctx.callstack[i + 1];
      XMLCall xcall = call as XMLCall;
      //  For XML calls there should be an explicit number of parts
      if (xcall != null) {
        //  Figure out how many beats are in half the call
        var parts = xcall.xelem.attr("parts");
        var partnums = parts.Split(';');
        halfbeats = partnums.Take((partnums.Count() + 1) / 2).Sum(x => Double.Parse(x));
      }
      prevbeats = ctx.maxBeats();
    }

    //  Call is performed between these two methods

    public override void postProcess(CallContext ctx, int i = 0) {
      //  Coded calls so far do not have explicit parts
      //  so just divide them in two
      CodedCall ccall = call as CodedCall;
      if (ccall != null) {
        halfbeats = (ctx.maxBeats() - prevbeats) / 2;
      }

      //  Chop off the excess half
      ctx.dancers.ForEach(d => {
        Movement mo = null;
        while (d.path.beats > prevbeats + halfbeats)
          mo = d.path.pop();
        if (mo != null && d.path.beats < prevbeats + halfbeats)
          d.path.add(mo.clip(prevbeats + halfbeats - d.path.beats));

      });

    }

  }
}
