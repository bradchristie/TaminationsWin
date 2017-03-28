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

  class CallContext
  {

    //  Angle of d2 as viewed from d1
    //  If angle is 0 then d2 is in front of d1
    //  Angle returned is in the range -pi to pi
    static public double angle(Dancer d1, Dancer d2) {
      return d2.location.concatenate(d1.tx.Inverse()).Angle();
    }

    //  Distance between two dancers
    static public double distance(Dancer d1, Dancer d2) {
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
    static public Func<Dancer, Func<Dancer, bool>> isInFront = d1 => d2 => {
      return d1 != d2 && angle(d1, d2).angleEquals(0);
    };
    static public Func<Dancer, Func<Dancer, bool>> isInBack = d1 => d2 => {
      return d1 != d2 && angle(d1, d2).angleEquals(Math.PI);
    };
    static public Func<Dancer, Func<Dancer, bool>> isLeft = d1 => d2 => {
      return d1 != d2 && angle(d1, d2).angleEquals(Math.PI / 2);
    };
    static public Func<Dancer, Func<Dancer, bool>> isRight = d1 => d2 => {
      return d1 != d2 && angle(d1, d2).angleEquals(3 * Math.PI / 2);
    };


    string callname = "";
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
      for (uint i=0; i<fds.Count; i++) {
        var fd = fds.Item(i);
        //  Assume square geometry
        var m = Matrix.CreateRotation(double.Parse(fd.attr("angle")) * Math.PI / 180);
        m = m * Matrix.CreateTranslation(double.Parse(fd.attr("x")), double.Parse(fd.attr("y")));
        dancers.Add(new Dancer($"{i * 2 + 1}", $"{i + 1}", 
          genderMap(fd.attr("gender")), Colors.White, m, 
          GeometryMaker.makeOne(GeometryType.SQUARE, 0), new List<Movement>()));
        dancers.Add(new Dancer($"{i * 2 + 2}", $"{i + 1}", 
          genderMap(fd.attr("gender")), Colors.White, m,
          GeometryMaker.makeOne(GeometryType.SQUARE, 1), new List<Movement>()));
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
          //  Then look for a code match
          try {
            success = matchXMLcall(callname);
          }
          catch (CallError err2) {
            err = err2;
          }
          try {
            success = success || matchCodedCall(callname);
          }
          catch (CallError err2) {
            err = err2;
          }
          if (success) {
            //  Remove the words we matched, break out of and
            //  the chopped loop, and continue if any words left
            calltext = calltext.Replace(callname, "").Trim();
          }
          return success;
        }))
          //  Every combination from callwords.chopped failed
          throw err;
      }
      return this;
    }

    bool matchXMLcall(string calltext) {
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
      var callfiles = TamUtils.calllistdata.Where(x => Regex.Match(x.text, callquery).Success);
      //  Found xml file with call, now look through each animation
      //  First read and extract all the animations to a list
      var tams = callfiles.SelectMany(d => TamUtils.getXMLAsset(d.link).SelectNodes("/tamination/tam")).ToList();
      found = tams.Count > 0;
      //  Now find the animations that match the name and formation
      tams.Where(tam => Regex.Match(tam.attr("title").ToLower().ReplaceAll("\\W",""), callquery).Success)
        .Any(tam => {
          var f = tam.hasAttr("formation")
                    ? TamUtils.getFormation(tam.attr("formation"))
                    : tam.SelectNodes("formation").First();
          var sexy = tam.hasAttr("gender-specific");
          //  Try to match the formation to the current dancer positions
          var mm = matchFormations(ctx, new CallContext(f), sexy);
          if (mm != null) {
            matches = true;
            // add XMLCall object to the call stack
            ctx0.callstack.Add(new XMLCall(tam, mm, ctx));
            ctx0.callname = callname + tam.attr("title") + " ";
          }
          return matches;
        });
      if (found && !matches)
        //  Found the call but formations did not match
        throw new FormationNotFoundError(calltext);
      return matches;
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

    int dancerRelation(Dancer d1, Dancer d2) {
      //  TODO fuzzy cases
      return angleBin(angle(d1, d2));
    }

    int[] matchFormations(CallContext ctx1, CallContext ctx2, bool sexy) {
      if (ctx1.dancers.Count != ctx2.dancers.Count)
        return null;
      //  Find mapping using DFS
      var mapping = new int[ctx1.dancers.Count];
      for (int i = 0; i < ctx1.dancers.Count; i++)
        mapping[i] = -1;
      var mapindex = 0;
      while (mapindex >= 0 && mapindex < ctx1.dancers.Count) {
        var nextmapping = mapping[mapindex] + 1;
        var found = false;
        while (!found && nextmapping < ctx2.dancers.Count) {
          mapping[mapindex] = nextmapping;
          mapping[mapindex + 1] = nextmapping ^ 1;
          if (testMapping(ctx1, ctx2, mapping, mapindex, sexy))
            found = true;
          else
            nextmapping++;
        }
        if (nextmapping >= ctx2.dancers.Count) {
          //  No more mappings for this dancer
          mapping[mapindex] = -1;
          mapping[mapindex + 1] = -1;
          mapindex -= 2;
        } else {
          //  Mapping found
          mapindex += 2;
        }
      }
      return mapindex < 0 ?  null : mapping;
    }

    bool testMapping(CallContext ctx1, CallContext ctx2, int[] mapping, int i, bool sexy) {
      if (sexy && (ctx1.dancers[i].gender != ctx2.dancers[mapping[i]].gender))
        return false;
      return Enumerable.Range(0, ctx1.dancers.Count).All(j => {
        if (mapping[j] < 0 || i == j)
          return true;
        var relq1 = dancerRelation(ctx1.dancers[i], ctx1.dancers[j]);
        var relt1 = dancerRelation(ctx2.dancers[mapping[i]], ctx2.dancers[mapping[j]]);
        var relq2 = dancerRelation(ctx1.dancers[j], ctx1.dancers[i]);
        var relt2 = dancerRelation(ctx2.dancers[mapping[j]], ctx2.dancers[mapping[i]]);
        //  If dancers are side-by-side, make sure handholding matches by checking distance
        var d1 = 0.0;
        var d2 = 0.0;
        if (relq1 == 2 || relq1 == 6) {
          d1 = distance(ctx1.dancers[i], ctx1.dancers[j]);
          d2 = distance(ctx2.dancers[mapping[i]], ctx2.dancers[mapping[j]]);
        }
        return relq1 == relt1 && relq2 == relt2 && (d1 < 2.1) == (d2 < 2.1);
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
      callstack.ForEach((Call c, int i) => c.preProcess(this, i));
      //  Core calls primarly use the performCall method
      callstack.ForEach((Call c, int i) => c.performCall(this, i));
      callstack.ForEach((Call c, int i) => c.postProcess(this, i));
    }

    //  This is used to match XML calls
    public int[] matchShapes(CallContext ctx2) {
      var ctx1 = this;
      if (ctx1.dancers.Count != ctx2.dancers.Count)
        return null;
      var mapping = new int[ctx1.dancers.Count];
      var reversemap = new int[ctx1.dancers.Count];
      ctx1.dancers.ForEach((d1, i) => {
        var bestd2 = -1;
        var bestdistance = 100.0;
        var v1 = d1.location;
        ctx2.dancers.ForEach((d2, j) => {
          var d = (double)(v1 - d2.location).Length();
          if (d.isApprox(bestdistance)) {
            bestd2 = -1;
          } else if (d < bestdistance) {
            bestdistance = d;
            bestd2 = j;
          }
        });
        if (bestd2 >= 0) {
          mapping[i] = bestd2;
          reversemap[bestd2] = i;
        }
      });
      //  Make sure we have a 1:1 mapping
      return mapping.All(i => i >= 0) && reversemap.All(i => i >= 0) ? mapping : null;
    }

    //  Re-center dancers
    public void center() {
      var xave = dancers.Sum(d => d.location.X) / dancers.Count;
      var yave = dancers.Sum(d => d.location.Y) / dancers.Count;
      dancers.ForEach(d => {
        d.starttx = d.starttx * Matrix.CreateTranslation(xave, yave);
      });
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
