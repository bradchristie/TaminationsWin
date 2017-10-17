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
  class ZigZag : QuarterTurns {

    public ZigZag(string callname) { name = callname.ToCapCase(); }

    public override string select(CallContext ctx, Dancer d) {
      if (d.data.leader)
        return name.StartsWith("Zig") ? "Quarter Right" : "Quarter Left";
      if (d.data.trailer)
        return name.EndsWith("Zig") ? "Quarter Right" : "Quarter Left";
      return "Stand";
    }

  }
}
