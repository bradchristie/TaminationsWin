﻿/*

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
  class FaceIn : QuarterTurns {

    public FaceIn(string calltext) { name = calltext.ToCapCase(); }

    public override string select(CallContext ctx, Dancer d) {
      switch (name) {
        case "Face In": return CallContext.angle(d) < 0 ? "Quarter Right" : "Quarter Left";
        case "Face Out": return CallContext.angle(d) > 0 ? "Quarter Right" : "Quarter Left";
        case "Face Left": return "Quarter Left";
        case "Face Right": return "Quarter Right";
        default: return "Stand";
      }
    }
  }

}
