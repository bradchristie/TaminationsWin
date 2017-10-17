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
using System.Text.RegularExpressions;
using TaminationsWin.Calls;
using Windows.Data.Xml.Dom;
using Windows.UI;

namespace TaminationsWin
{

  class CallContext {

    //  Angle of d2 as viewed from d1
    //  If angle is 0 then d2 is in front of d1
    //  Angle returned is in the range -pi to pi
    static public double angle(Dancer d1,Dancer d2) {
      return d2.location.concatenate(d1.tx.Inverse()).Angle();
    }

    //  Distance between two dancers
    static public double distance(Dancer d1,Dancer d2) {
      return (d1.location - d2.location).Length();
    }

    //  Angle of dancer to the origin
    static public double angle(Dancer d) {
      return new Vector2().preConcatenate(d.tx.Inverse()).Angle();
    }

    //  Distance of one dancer to the origin
    static public double distance(Dancer d) {
      return d.location.Length();
    }

    //  Other geometric interrogatives
    static public bool isFacingIn(Dancer d) {
      var a = Math.Abs(angle(d));
      return !a.isApprox(Math.PI / 2) && a < Math.PI / 2;
    }

    static public bool isFacingOut(Dancer d) {
      var a = Math.Abs(angle(d));
      return !a.isApprox(Math.PI / 2) && a > Math.PI / 2;
    }

    //  Test if dancer d2 is directly in front, back. left, right of dancer d1
    static public Func<Dancer,Func<Dancer,bool>> isInFront = d1 => d2 => {
      return d1 != d2 && angle(d1,d2).angleEquals(0);
    };
    static public Func<Dancer,Func<Dancer,bool>> isInBack = d1 => d2 => {
      return d1 != d2 && angle(d1,d2).angleEquals(Math.PI);
    };
    static public Func<Dancer,Func<Dancer,bool>> isLeft = d1 => d2 => {
      return d1 != d2 && angle(d1,d2).angleEquals(Math.PI / 2);
    };
    static public Func<Dancer,Func<Dancer,bool>> isRight = d1 => d2 => {
      return d1 != d2 && angle(d1,d2).angleEquals(3 * Math.PI / 2);
    };


    public string callname = "";
    public List<Call> callstack = new List<Call>();
    public List<Dancer> dancers;
    public List<Dancer> actives { get {
        return dancers.Where(d => d.data.active).ToList();
      } }
    public bool isVertical = false;
    private Gender genderMap(string g) {
      return g == "phantom" ? Gender.PHANTOM : g == "girl" ? Gender.GIRL : Gender.BOY;
    }


    //  For cases where creating a new context from a source,
    //  get the dancers from the source and clone them.
    //  The new context contains the dancers in their current location
    //  and no paths.
    public CallContext(CallContext source) {
      dancers = source.dancers.Select(d => new Dancer(d)).ToList();
    }
    public CallContext(List<Dancer> source) {
      dancers = source.Select(d => new Dancer(d)).ToList();
    }
    public CallContext(Dancer[] source) {
      dancers = source.Select(d => {
        d.animateToEnd();
        return new Dancer(d);
      }).ToList();
    }

    //  Create a context from a formation defined in XML
    public CallContext(IXmlNode f) {
      dancers = new List<Dancer>();
      var fds = f.SelectNodes("dancer");
      for (uint i = 0; i<fds.Count; i++) {
        var fd = fds.Item(i);
        //  Assume square geometry
        var m = Matrix.CreateRotation(fd.attr("angle").toDouble() * Math.PI / 180);
        m = m * Matrix.CreateTranslation(fd.attr("x").toDouble(),fd.attr("y").toDouble());
        dancers.Add(new Dancer($"{i * 2 + 1}",$"{i + 1}",
          genderMap(fd.attr("gender")),Colors.White,m,
          GeometryMaker.makeOne(GeometryType.SQUARE,0),new List<Movement>()));
        dancers.Add(new Dancer($"{i * 2 + 2}",$"{i + 1}",
          genderMap(fd.attr("gender")),Colors.White,m,
          GeometryMaker.makeOne(GeometryType.SQUARE,1),new List<Movement>()));
      }
    }

    /**
    * Append the result of processing this CallContext to it source.
    * The CallContext must have been previously cloned from the source.
    */
    CallContext appendToSource() {
      foreach (var d in dancers) {
        d.clonedFrom.path.add(d.path);
        d.clonedFrom.animateToEnd();
      }
      return this;
    }

    public CallContext applyCall(string calltext) {
      interpretCall(calltext);
      performCall();
      appendToSource();
      return this;
    }

    public void applyCalls(params string[] calltext) {
      foreach (var callstr in calltext)
        new CallContext(this).applyCall(callstr);
    }

    /**
     * This is the main loop for interpreting a call
     * @param calltxt  One complete call, lower case, words separated by single spaces
     */
    public CallContext interpretCall(string calltxt) {
      var calltext = calltxt;
      CallError err = new CallNotFoundError(calltxt);
      //  Clear out any previous paths from incomplete parsing
      foreach (var d in dancers) {
        d.path = new Path();
      }
      callname = "";
      //  If a partial interpretation is found (like 'boys' of 'boys run')
      //  it gets popped off the front and this loop interprets the rest
      while (calltext.Length > 0) {
        //  Try chopping off each word from the end of the call until
        //  we find something we know
        if (!calltext.chopped().Any((callname) => {
          var success = false;
          //  First try to find an exact match in Taminations
          try {
            success = matchXMLcall(callname);
          }
          catch (CallError err2) {
            err = err2;
          }
          //  Then look for a code match
          try {
            success = success || matchCodedCall(callname);
          }
          catch (CallError err3) {
            err = err3;
          }
          //  Finally try a fuzzier match in Taminations
          try {
            success = success || matchXMLcall(callname,true);
          }
          catch (CallError err4) {
            err = err4;
          }
          if (success) {
            //  Remove the words we matched, break out of and
            //  the chopped loop, and continue if any words left
            calltext = calltext.Replace(callname,"").Trim();
          }
          return success;
        }))
          //  Every combination from callwords.chopped failed
          throw err;
      }
      return this;
    }

    bool matchXMLcall(string calltext, bool fuzzy=false) {
      var found = false;
      var matches = false;
      var ctx0 = this;
      var ctx = this;

      //  If there are precursors, run them first so the result
      //  will be used to match formations
      //  Needed for calls like "Explode And ..."
      if (callstack.Count > 0) {
        ctx = new CallContext(this);
        ctx.callstack = callstack;
        ctx.performCall();
      }

      //  If actives != dancers, create another call context with just the actives
      if (ctx.dancers.Count != ctx.actives.Count)
        ctx = new CallContext(ctx.actives);
      //  Try to find a match in the xml animations
      var callquery = "^" + TamUtils.callnameQuery(calltext) + "$";
      var callfiles = TamUtils.calllistdata.Where(x => Regex.Match(x.text,callquery).Success);
      //  Found xml file with call, now look through each animation
      //  First read and extract all the animations to a list
      var tams = callfiles.SelectMany(d => TamUtils.getXMLAsset(d.link).SelectNodes("/tamination/tam")).ToList();
      found = tams.Count > 0;
      //  Now find the animations that match the name and formation
      tams.Where(tam => Regex.Match(tam.attr("title").ToLower().ReplaceAll("\\W",""),callquery).Success)
        .Any(tam => {
          var f = tam.hasAttr("formation")
                    ? TamUtils.getFormation(tam.attr("formation"))
                    : tam.SelectNodes("formation").First();
          var ctx2 = new CallContext(f);
          var sexy = tam.hasAttr("gender-specific");
          //  Try to match the formation to the current dancer positions
          var mm = matchFormations(ctx,ctx2, sexy, fuzzy);
          if (mm != null) {
            matches = true;
            // add XMLCall object to the call stack
            ctx0.callstack.Add(new XMLCall(tam,mm,ctx2));
            ctx0.callname = callname + tam.attr("title") + " ";
          }
          return matches;
        });
      if (found && !matches)
        //  Found the call but formations did not match
        throw new FormationNotFoundError(calltext);
      return matches;
    }

    //  Once a mapping of the current formation to an XML call is found,
    //  we need to compute the difference between the two,
    //  and that difference will be added as an offset to the first movement
    public Vector2[] computeFormationOffsets(CallContext ctx2,int[] mapping) {
      var dvbest = new Vector2[dancers.Count];
      var dtotbest = 0.0;
      //  We don't know how the XML formation needs to be turned to overlap
      //  the current formation.  So try all 4 angles and use the best.
      double[,] bxa = { { 0,0,0 },{ 0,0,0 },{ 0,0,0 },};
      actives.ForEach((d1,i) => {
        var v1 = d1.location;
        var v2 = ctx2.dancers[mapping[i]].location;
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
      actives.ForEach((d2,j) => {
        var v1 = d2.location;
        var v2 = ctx2.dancers[mapping[j]].location.concatenate(rotmat);
        dvbest[j] = v1 - v2;
        dtotbest += dvbest[j].Length();
      });
      return dvbest;
    }

    /*
     * Algorithm to match formations
     * Match dancers relative to each other, rather than compare absolute positions
     * Returns integer values for axis and quadrant directions
     *           0
     *         7 | 1
     *       6 --+-- 2
     *         5 | 3
     *           4
     * 2 cases
     *   1.  Dancers facing same or opposite directions
     *       - If dancers are lined up 0, 90, 180, 270 angles must match
     *       - Other angles match by quadrant
     *   2.  Dancers facing other relative directions (commonly 90 degrees)
     *       - Dancers must match quadrant or adj boundary
     *
     *
     *
     */
    private int angleBin(double a) {
      if (a.angleEquals(0))
        return 0;
      else if (a.angleEquals(Math.PI / 2))
        return 2;
      else if (a.angleEquals(Math.PI))
        return 4;
      else if (a.angleEquals(-Math.PI / 2))
        return 6;
      else if (a > 0 && a < Math.PI / 2)
        return 1;
      else if (a > Math.PI / 2 && a < Math.PI)
        return 3;
      else if (a < 0 && a > -Math.PI / 2)
        return 7;
      else if (a < -Math.PI / 2 && a > -Math.PI)
        return 5;
      else
        return -1;  // should not happen
    }

    int dancerRelation(Dancer d1,Dancer d2) {
      //  TODO fuzzy cases
      return angleBin(angle(d1,d2));
    }

    int[] matchFormations(CallContext ctx1,CallContext ctx2,
      bool sexy=false, bool fuzzy=false, bool rotate=false) {
      if (ctx1.dancers.Count != ctx2.dancers.Count)
        return null;
      //  Find mapping using DFS
      var mapping = new int[ctx1.dancers.Count];
      var rotated = new bool[ctx1.dancers.Count];
      for (int i = 0; i < ctx1.dancers.Count; i++)
        mapping[i] = -1;
      var mapindex = 0;
      while (mapindex >= 0 && mapindex < ctx1.dancers.Count) {
        var nextmapping = mapping[mapindex] + 1;
        var found = false;
        while (!found && nextmapping < ctx2.dancers.Count) {
          mapping[mapindex] = nextmapping;
          mapping[mapindex + 1] = nextmapping ^ 1;
          if (testMapping(ctx1,ctx2,mapping,mapindex,sexy,fuzzy))
            found = true;
          else
            nextmapping++;
        }
        if (nextmapping >= ctx2.dancers.Count) {
          //  No more mappings for this dancer
          mapping[mapindex] = -1;
          mapping[mapindex + 1] = -1;
          //  If fuzzy, try rotating this dancer
          if (rotate && !rotated[mapindex]) {
            ctx1.dancers[mapindex].rotateStartAngle(180.0);
            ctx1.dancers[mapindex+1].rotateStartAngle(180.0);
            rotated[mapindex] = true;
          } else {
            rotated[mapindex] = false;
            mapindex -= 2;
          }
        } else {
          //  Mapping found
          mapindex += 2;
        }
      }
      return mapindex < 0 ? null : mapping;
    }

    bool testMapping(CallContext ctx1,CallContext ctx2,int[] mapping,int i,
      bool sexy=false,bool fuzzy=false) {
      if (sexy && (ctx1.dancers[i].gender != ctx2.dancers[mapping[i]].gender))
        return false;
      return Enumerable.Range(0,ctx1.dancers.Count).All(j => {
        if (mapping[j] < 0 || i == j)
          return true;
        var relq1 = dancerRelation(ctx1.dancers[i],ctx1.dancers[j]);
        var relt1 = dancerRelation(ctx2.dancers[mapping[i]],ctx2.dancers[mapping[j]]);
        var relq2 = dancerRelation(ctx1.dancers[j],ctx1.dancers[i]);
        var relt2 = dancerRelation(ctx2.dancers[mapping[j]],ctx2.dancers[mapping[i]]);
        //  If dancers are side-by-side, make sure handholding matches by checking distance
        if (!fuzzy && (relq1 == 2 || relq1 == 6)) {
          var d1 = distance(ctx1.dancers[i],ctx1.dancers[j]);
          var d2 = distance(ctx2.dancers[mapping[i]],ctx2.dancers[mapping[j]]);
          return relq1 == relt1 && relq2 == relt2 && (d1 < 2.1) == (d2 < 2.1);
        }
        else if (fuzzy) {
          var reldif1 = (relt1-relq1).Abs();
          var reldif2 = (relt2-relq2).Abs();
          return (reldif1==0 || reldif1==1 || reldif1==7) &&
            (reldif2==0 || reldif2==1 || reldif2==7);
        }
        else
          return relq1 == relt1 && relq2 == relt2;
      });
    }

    bool matchCodedCall(string calltext) {
      var call = CodedCall.getCodedCall(calltext);
      if (call != null) {
        callstack.Add(call);
        callname = callname + call.name + " ";
        return true;
      }
      return false;
    }

    public void performCall() {
      //  Perform calls by popping them off the stack until the stack is empty.
      //  This doesn't run an animation, rather it takes the stack of calls
      //  and builds the dancer movements.
      analyze();
      //  Concepts and modifications primarily use the preProcess and
      //  postProcess methods
      callstack.ForEach((Call c,int i) => c.preProcess(this,i));
      //  Core calls primarly use the performCall method
      callstack.ForEach((Call c,int i) => c.performCall(this,i));
      callstack.ForEach((Call c,int i) => c.postProcess(this,i));
    }

    //  Re-center dancers
    public void center() {
      var xave = dancers.Sum(d => d.location.X) / dancers.Count;
      var yave = dancers.Sum(d => d.location.Y) / dancers.Count;
      dancers.ForEach(d => {
        d.starttx = d.starttx * Matrix.CreateTranslation(xave,yave);
      });
    }

    //  See if the current dancer positions resemble a standard formation
    //  and, if so, snap to the standard
    private String[] standardFormations = new String[] {
      "Normal Lines Compact",
      "Normal Lines",
      "Double Pass Thru",
      "Quarter Tag",
      "Tidal Line RH",
      "Diamonds RH Girl Points",
      "Diamonds RH PTP Girl Points",
      "Hourglass RH BP",
      "Galaxy RH GP",
      "Butterfly RH",
      "O RH",
      "Sausage RH",
      "T-Bone URRD",
      "T-Bone RUUL",
      "Static Square"
    };
    struct BestMapping {
      public String name;
      public int[] mapping;
      public Vector2[] offsets;
      public double totOffset;
    };
    public void matchStandardFormation() {
      //  Make sure newly added animations are finished
      dancers.ForEach (d => { d.path.recalculate(); d.animateToEnd(); } );
      //  Work on a copy with all dancers active, mapping only uses active dancers
      var ctx1 = new CallContext(this);
      ctx1.dancers.ForEach(d => d.data.active = true );
      BestMapping bestMapping = new BestMapping();
      bestMapping.totOffset = -1.0;

      standardFormations.ForEach(f => {
        CallContext ctx2 = new CallContext(TamUtils.getFormation(f));
        //  See if this formation matches
        var mapping = matchFormations(ctx1,ctx2,sexy: false,fuzzy: true);
        if (mapping != null) {
          //  If it does, get the offsets
          var offsets = ctx1.computeFormationOffsets(ctx2,mapping);
          var totOffset = offsets.Aggregate(0.0,(s,v) => s+v.Length());
          //  Favor formations closer to the top of the list
          if (bestMapping.totOffset < 0 || totOffset+0.1 < bestMapping.totOffset) {
            bestMapping.name = f;  // only used for debugging
            bestMapping.mapping = mapping;
            bestMapping.offsets = offsets;
            bestMapping.totOffset = totOffset;
          }
        }
      });
      if (bestMapping.totOffset >= 0) {
        for (var i=0; i<dancers.Count; i++) {
          var d = dancers[i];
          if (bestMapping.offsets[i].Length() > 0.1) {
            //  Get the last movement
            var m = d.path.pop();
            //  Transform the offset to the dancer's angle
            d.animateToEnd();
            var vd = bestMapping.offsets[i].Rotate(-d.tx.Angle());
            //  Apply it
            d.path.add(m.skew(-vd.X,-vd.Y));
            d.animateToEnd();
          }
        }
      }
    }

    //  Return max number of beats among all the dancers
    public double maxBeats() {
      return dancers.Max(d => d.path.beats);
    }

    //  Return all dancers, ordered by distance, that satisfies a conditional
    public IEnumerable<Dancer> dancersInOrder(Dancer d, Func<Dancer,bool> f) {
      return dancers.Where(f).OrderBy(d2 => distance(d, d2));
    }

    //  Return closest dancer that satisfies a given conditional
    public Dancer dancerClosest(Dancer d, Func<Dancer, bool> f) {
      return dancersInOrder(d, f).FirstOrDefault();
    }

    //  Return dancer directly in front of given dancer
    public Dancer dancerInFront(Dancer d) {
      return dancerClosest(d, isInFront(d));
    }

    //  Return dancer directly in back of given dancer
    public Dancer dancerInBack(Dancer d) {
      return dancerClosest(d, isInBack(d));
    }

    //  Return dancer directly to the right of given dancer
    public Dancer dancerToRight(Dancer d) {
      return dancerClosest(d, isRight(d));
    }

    //  Return dancer directly to the left of given dancer
    public Dancer dancerToLeft(Dancer d) {
      return dancerClosest(d, isLeft(d));
    }

    //  Return dancer that is facing the front of this dancer
    public Dancer dancerFacing(Dancer d) {
      var d2 = dancerInFront(d);
      if (d2 != null) {
        var d3 = dancerInFront(d2);
        if (d3 == d)
          return d2;
      }
      return null;
    }

    //  Return dancers that are in between two other dancers
    public IEnumerable<Dancer> inBetween(Dancer d1, Dancer d2) {
      return dancers.Where(d =>
      d != d1 && d != d2 && (distance(d, d1) + distance(d2)).isApprox(distance(d1, d2))
      );
    }

    //  Return all the dancers to the right, in order
    public IEnumerable<Dancer> dancersToRight(Dancer d) {
      return dancersInOrder(d, isRight(d));
    }

    //  Return all the dancers to the left, in order
    public IEnumerable<Dancer> dancersToLeft(Dancer d) {
      return dancersInOrder(d, isLeft(d));
    }

    //  Return all the dancers in front, in order
    public IEnumerable<Dancer> dancersInFront(Dancer d) {
      return dancersInOrder(d,isInFront(d));
    }

    //  Return all the dancers in back, in order
    public IEnumerable<Dancer> dancersInBack(Dancer d) {
      return dancersInOrder(d,isInBack(d));
    }

    //  Return true if this dancer is in a wave or mini-wave
    public bool isInWave(Dancer d) {
      return d.data.partner != null &&
        angle(d, d.data.partner) == angle(d.data.partner, d);
    }

    //  Return true if this dancer is part of a couple facing same direction
    public bool isInCouple(Dancer d) {
      var d2 = d.data.partner;
      return d2 != null && d.tx.Angle().angleEquals(d2.tx.Angle());
    }

    //  Return true if this is 4 dancers in a box
    public bool isBox() {
      //  Must have 4 dancers
      return dancers.Count == 4 &&
        //  Each dancer must have a partner
        //  and must be either a leader or a trailer
        dancers.All(d => d.data.partner != null && (d.data.leader || d.data.trailer));
    }

    //  Return true if this is 4 dancers in a line
    public bool isLine() {
      //  Must have 4 dancers
      return dancers.Count == 4 &&
        //  Each dancer must have right or left shoulder to origin
        dancers.All(d => Math.Abs(angle(d)).isApprox(Math.PI/2)) &&
        //  All dancers must either be on the y axis
        (dancers.All(d => ((double)d.location.X).isApprox(0.0)) ||
          //  or on the x axis
          dancers.All(d => ((double)d.location.Y).isApprox(0.0)));
    }

    //  Return true if 8 dancers are in 2 general lines of 4 dancers each
    public bool isLines() {
      return dancers.All(d => dancersToRight(d).Count() + dancersToLeft(d).Count() == 3);
    }

    //  Return true if 8 dancers are in 2 general columns of 4 dancers each
    public bool isColumns() {
      return dancers.All(d => dancersInFront(d).Count() + dancersInBack(d).Count() == 3);
    }

    //  Level off the number of beats for each dancer
    public void levelBeats() {
      //  get the longest number of beats
      var maxb = maxBeats();
      //  add that number as needed by using the "Stand" move
      dancers.ForEach(d => {
        var b = maxb - d.path.beats;
        if (b > 0)
          d.path.add(TamUtils.getMove("Stand").changebeats(b));
      });
    }

    //  Find the range of the dancers current position
    //  For now we assume the dancers are centered
    //  and return a vector to the max 1st quadrant rectangle point
    public Vector2 bounds() {
      return dancers.Select(d => d.location).Aggregate((v1, v2) =>
                new Vector2(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y)));
    }

    public void analyze() {
      dancers.ForEach(d => {
        d.animateToEnd();
        d.data.beau = false;
        d.data.belle = false;
        d.data.leader = false;
        d.data.trailer = false;
        d.data.partner = null;
        d.data.center = false;
        d.data.end = false;
        d.data.verycenter = false;
      });
      bool istidal = false;
      dancers.ForEach(d1 => {
        Dancer bestleft = null;
        Dancer bestright = null;
        int leftcount = 0;
        int rightcount = 0;
        int frontcount = 0;
        int backcount = 0;
        dancers.Where(d2 => d2 != d1).ForEach(d2 => {
          //  Count dancers to the left and right,
          //  and find the closest on each side
          if (isRight(d1)(d2)) {
            rightcount++;
            if (bestright == null || distance(d1, d2) < distance(d1, bestright))
              bestright = d2;
          } else if (isLeft(d1)(d2)) {
            leftcount++;
            if (bestleft == null || distance(d1, d2) < distance(d1, bestleft))
              bestleft = d2;
          }
          //  Also count dancers in front and in back
          else if (isInFront(d1)(d2))
            frontcount++;
          else if (isInBack(d1)(d2))
            backcount++;
        });
        //  Use the results of the counts to assign belle/beau/leader/trailer
        //  and partner
        if (leftcount % 2 == 1 && rightcount % 2 == 0 && distance(d1,bestleft) < 3) {
          d1.data.partner = bestleft;
          d1.data.belle = true;
        }
        else if (rightcount % 2 == 1 && leftcount % 2 == 0 && distance(d1,bestright) < 3) {
          d1.data.partner = bestright;
          d1.data.beau = true;
        }
        if (frontcount % 2 == 0 && backcount % 2 == 1)
          d1.data.leader = true;
        else if (frontcount % 2 == 1 && backcount % 2 == 0)
          d1.data.trailer = true;
        //  Assign ends
        if (rightcount == 0 && leftcount > 1)
          d1.data.end = true;
        else if (leftcount == 0 && rightcount > 1)
          d1.data.end = true;
        //  The very centers of a tidal wave are ends
        //  Remember this special case for assigning centers later
        if (rightcount == 3 && leftcount == 4 ||
            rightcount == 4 && leftcount == 3) {
          d1.data.end = true;
          istidal = true;
        }
      });
      //  Analyze for centers and very centers
      //  Sort dancers by distance from center
      var dorder = dancers.OrderBy(d => d.location.Length()).ToArray();
      //  The 2 dancers closest to the center
      //  are centers (4 dancers) or very centers (8 dancers)
      if (!((double)dorder[1].location.Length()).isApprox((double)dorder[2].location.Length())) {
        if (dancers.Count == 4) {
          dorder[0].data.center = true;
          dorder[1].data.center = true;
        } else {
          dorder[0].data.verycenter = true;
          dorder[1].data.verycenter = true;
        }
      }
      // If tidal, then the next 4 dancers are centers
      if (istidal)
        Enumerable.Range(2, 4).ForEach(i => dorder[i].data.center = true);
      //  Otherwise, if there are 4 dancers closer to the center than the other 4,
      //  they are the centers
      else if (dancers.Count > 4 && !distance(dorder[3]).isApprox(distance(dorder[4])))
        Enumerable.Range(0, 4).ForEach(i => dorder[i].data.center = true);
    }

  }

}
