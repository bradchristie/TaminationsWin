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
      var vdif = computeFormationOffsets(ctx, ctx2);
      var bmax = allp.Max(p => p.beats);
      xmlmap.ForEach((m, i3) => {
        var p = allp[m >> 1];
        if (p.movelist.Count == 0)
          p.add(TamUtils.getMove("Stand"));
        //  Scale active dancers to fit the space they need
        //  Compute difference between current formation and XML formation
        var vd = vdif[i3].Rotate(-ctx.actives[i3].tx.Angle());
        //  Apply formation difference to first movement of XML path
        if (vd.Length()>0.1)
          p.movelist[0].skew(-vd.X,-vd.Y);
        //  Add XML path to dancer
        ctx.actives[i3].path.add(p);
        //  Move dancer to end so any subsequent modifications (e.g. roll)
        //  use the new position
        ctx.actives[i3].animateToEnd();
      });
      ctx.levelBeats();
      ctx.analyze();
    }

    //  Once a mapping of the current formation to an XML call is found,
    //  we need to compute the difference between the two,
    //  and that difference will be added as an offset to the first movement
    private Vector2[] computeFormationOffsets(CallContext ctx1, CallContext ctx2) {
      var dvbest = new Vector2[ctx1.dancers.Count];
      var dtotbest = 0.0;
      //  We don't know how the XML formation needs to be turned to overlap
      //  the current formation.  So try all 4 angles and use the best.
      double[,] bxa = { { 0,0,0 },{ 0,0,0 },{ 0,0,0 },};
      ctx1.actives.ForEach((d1,i) => {
        var v1 = d1.location;
        var v2 = ctx2.dancers[xmlmap[i]].location;
        bxa[0,0] += v1.X * v2.X;
        bxa[0,1] += v1.Y*v2.X;
        bxa[1,0] += v1.X * v2.Y;
        bxa[1,1] += v1.Y * v2.Y;
      });
      var svd = Matrix.SVD(bxa);
      var ut = Matrix.putArray(svd.Item1.transpose());
      var v = Matrix.putArray(svd.Item3);
      var rotmat = ut * v;
      //  Now rotate the formation and compute any remaining
      //  differences in position
      ctx1.actives.ForEach((d2,j) => {
        var v1 = d2.location;
        var v2 = ctx2.dancers[xmlmap[j]].location.concatenate(rotmat);
        dvbest[j] = v1 - v2;
        dtotbest += dvbest[j].Length();
      });
      return dvbest;
    }

  }
}
