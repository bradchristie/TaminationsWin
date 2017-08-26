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
using System.Numerics;
using System.Text;
using Windows.Data.Xml.Dom;

namespace TaminationsWin.Calls
{
  class XMLCall : Call
  {
    public IXmlNode xelem;
    private int[] xmlmap;
    private CallContext ctx2;

    public XMLCall(IXmlNode xelem, int[] xmlmap, CallContext ctx2) {
      this.xelem = xelem;
      this.xmlmap = xmlmap;
      this.ctx2 = ctx2;
      name = xelem.attr("title");
    }

    override public void performCall(CallContext ctx, int notused) {
      var allp = TamUtils.getPaths(xelem);
      //  If moving just some of the dancers,
      //  see if we can keep them in the same shape
      if (ctx.actives.Count < ctx.dancers.Count) {
        //  No animations have been done on ctx2,
        //  so dancers are still at the start points
        var ctx3 = new CallContext(ctx2);
        //  So ctx3 is a copy of the start point
        var bounds1 = ctx3.bounds();
        //  Now add the paths
        ctx3.dancers.ForEach((d, i) => d.path.add(allp[i >> 1]));
        //  And move it to the end point
        ctx3.levelBeats();
        ctx3.analyze();
      }
      var vdif = ctx.computeFormationOffsets(ctx2,xmlmap);
      xmlmap.ForEach((m, i3) => {
        var p = new Path(allp[m >> 1]);
        //  Scale active dancers to fit the space they need
        //  Compute difference between current formation and XML formation
        var vd = vdif[i3].Rotate(-ctx.actives[i3].tx.Angle());
        //  Apply formation difference to first movement of XML path
        if (vd.Length() > 0.1) {
          if (p.movelist.Count == 0)
            p.add(TamUtils.getMove("Stand"));
          p.skewFirst(-vd.X,-vd.Y);
        }
        //  Add XML path to dancer
        ctx.actives[i3].path.add(p);
        //  Move dancer to end so any subsequent modifications (e.g. roll)
        //  use the new position
        ctx.actives[i3].animateToEnd();
      });
      ctx.levelBeats();
      ctx.analyze();
    }

  }
}
