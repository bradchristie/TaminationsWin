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

  /**
   *   Parent class of all classes that select a group of dancers
   *   such as boys, leaders, centers, belles
   */
  abstract class FilterActives : CodedCall {

    //  Child classes need to define one of these isActive methods
    //  according to which dancers should be selected
    virtual public bool isActive(Dancer d) { return true; }
    virtual public bool isActive(Dancer d, CallContext ctx) { return isActive(d); }

    /**
     *   Set the active dancers based on the predicate
     *   defined by the child class
     */
    public override void preProcess(CallContext ctx, int i) {
      ctx.dancers.Where(d => d.data.active).ForEach(
        d => { d.data.active = isActive(d, ctx); });
    }

  }

}
