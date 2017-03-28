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

namespace TaminationsWin.Calls
{

  abstract class Action : CodedCall {

    //  Wrapper method for performing one call
    public override void performCall(CallContext ctx, int i=0) {
      perform(ctx, i);
      ctx.dancers.ForEach(d => {
        d.path.recalculate();
        d.animateToEnd();
      });
      ctx.levelBeats();
    }

    //  Default method to perform one call
    //  Pass the call on to each active dancer
    //  Then append the returned paths to each dancer
    public virtual void perform(CallContext ctx, int i=0) {
      //  Get all the paths with performOne calls
      ctx.actives.ForEach(d => d.path.add(performOne(d, ctx)));
    }

    //  Default method for one dancer to perform one call
    //  Returns an empty path (the dancer just stands there)
    public virtual Path performOne(Dancer d, CallContext ctx) {
      return new Path();
    }

  }

}
