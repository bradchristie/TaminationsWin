/*

    Taminations Square Dance Animations App for Android
    Copyright (C) 2016 Brad Christie

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

namespace TaminationsWin {

  public class Handhold : IComparable {
    public Dancer dancer1;
    public Dancer dancer2;
    public int hold1;
    public int hold2;
    public double angle1;
    public double angle2;
    public double distance;
    public double score;

    public Handhold(Dancer dancer1, Dancer dancer2, int hold1, int hold2,
      double angle1, double angle2, double distance, double score) {
      this.dancer1 = dancer1;
      this.dancer2 = dancer2;
      this.hold1 = hold1;
      this.hold2 = hold2;
      this.angle1 = angle1;
      this.angle2 = angle2;
      this.distance = distance;
      this.score = score;
    }

    public bool inCenter {
      get {
        return dancer1.inCenter && dancer2.inCenter;
      }
    }

    public static Handhold Create(Dancer d1, Dancer d2, int geometry) {
      if (!d1.hidden && !d2.hidden) {
        //  Turn off grips if not specified in current movement
        if ((d1.hands & Hands.GRIPRIGHT) != Hands.GRIPRIGHT)
          d1.rightgrip = null;
        if ((d1.hands & Hands.GRIPLEFT) != Hands.GRIPLEFT)
          d1.leftgrip = null;
        if ((d2.hands & Hands.GRIPRIGHT) != Hands.GRIPRIGHT)
          d2.rightgrip = null;
        if ((d2.hands & Hands.GRIPLEFT) != Hands.GRIPLEFT)
          d2.leftgrip = null;

        //  Check distance
        var x1 = d1.tx.M31;
        var y1 = d1.tx.M32;
        var x2 = d2.tx.M31;
        var y2 = d2.tx.M32;
        var dx = x2 - x1;
        var dy = y2 - y1;
        var dfactor1 = 0.1;  // for distance up to 2.0
        var dfactor2 = 2.0;  // for distance past 2.0
        var cutover = geometry == GeometryType.HEXAGON ? 2.5
          : geometry == GeometryType.BIGON ? 3.7 : 2.0;
        var d = Math.Sqrt(dx * dx + dy * dy);
        var dfactor0 = geometry == GeometryType.HEXAGON ? 1.15 : 1.0;
        var d0 = d * dfactor0;
        var score1 = d0 > cutover ? (d0 - cutover) * dfactor2 + 2 * dfactor1 : d0 * dfactor1;
        var score2 = score1;
        //  Angle between dancers
        var a0 = Math.Atan2(dy, dx);
        //  Angle each dancer is facing
        var a1 = Math.Atan2(d1.tx.M12, d1.tx.M22);
        var a2 = Math.Atan2(d2.tx.M12, d2.tx.M22);
        //  For each dancer, try left and right hands
        int h1 = 0;
        int h2 = 0;
        double ah1 = 0;
        double ah2 = 0;
        double afactor1 = 0.2;
        double afactor2 = geometry == GeometryType.BIGON ? 0.6 : 1.0;

        //  Dancer 1
        var a = Math.Abs(Math.IEEERemainder(Math.Abs(a1 - a0 + Math.PI * 3.0 / 2.0), Math.PI * 2.0));
        var ascore = a > Math.PI / 6.0 ? (a - Math.PI / 6.0) * afactor2 + Math.PI / 6.0 * afactor1 : a * afactor1;
        if (score1+ascore < 1 && (d1.hands & Hands.RIGHTHAND) != 0 &&
          d1.rightgrip==null || d1.rightgrip== d2) {
          score1 = d1.rightgrip == d2 ? 0 : score1 + ascore;
          h1 = Hands.RIGHTHAND;
          ah1 = a1 - a0 + Math.PI * 3.0 / 2.0;
        } else {
          a = Math.Abs(Math.IEEERemainder(Math.Abs(a1 - a0 + Math.PI / 2.0), Math.PI * 2.0));
          ascore = (a > Math.PI / 6.0) ? (a - Math.PI / 6.0) * afactor2 + Math.PI / 6.0 * afactor1 : a * afactor1;
          if (score1 + ascore < 1.0 && (d1.hands & Hands.LEFTHAND) != 0 &&
            d1.leftgrip == null || d1.leftgrip == d2) {
            score1 = d1.leftgrip == d2 ? 0.0 : score1 + ascore;
            h1 = Hands.LEFTHAND;
            ah1 = a1 - a0 + Math.PI / 2.0;
          } else
            score1 = 10.0;
        }

        //  Dancer 2
        a = Math.Abs(Math.IEEERemainder(Math.Abs(a2 - a0 + Math.PI / 2.0), Math.PI * 2));
        ascore = a > Math.PI / 6 ? (a - Math.PI / 6) * afactor2 + Math.PI / 6 * afactor1 : a * afactor1;
        if (score2 + ascore < 1 && (d2.hands & Hands.RIGHTHAND) != 0 &&
          d2.rightgrip == null || d2.rightgrip == d1) {
          score2 = d2.rightgrip == d1 ? 0 : score2 + ascore;
          h2 = Hands.RIGHTHAND;
          ah2 = a2 - a0 + Math.PI / 2;
        } else {
          a = Math.Abs(Math.IEEERemainder(Math.Abs(a2 - a0 + Math.PI *3.0 / 2.0), Math.PI * 2));
          ascore = (a > Math.PI / 6) ? (a - Math.PI / 6) * afactor2 + Math.PI / 6 * afactor1 : a * afactor1;
          if (score2 + ascore < 1.0 && (d2.hands & Hands.LEFTHAND) != 0 &&
            d2.leftgrip == null || d2.leftgrip == d1) {
            score2 = d2.leftgrip == d1 ? 0.0 : score2 + ascore;
            h2 = Hands.LEFTHAND;
            ah2 = a2 - a0 + Math.PI * 3.0 / 2.0;
          } else
            score2 = 10.0;
        }

        //  Generate return value
        if (d1.rightgrip == d2 && d2.rightgrip == d1)
          return new Handhold(d1, d2, Hands.RIGHTHAND, Hands.RIGHTHAND, ah1, ah2, d, 0.0);
        else if (d1.rightgrip == d2 && d2.leftgrip == d1)
          return new Handhold(d1, d2, Hands.RIGHTHAND, Hands.LEFTHAND, ah1, ah2, d, 0.0);
        else if (d1.leftgrip == d2 && d2.rightgrip == d1)
          return new Handhold(d1, d2, Hands.LEFTHAND, Hands.RIGHTHAND, ah1, ah2, d, 0.0);
        else if (d1.leftgrip == d2 && d2.leftgrip == d1)
          return new Handhold(d1, d2, Hands.LEFTHAND, Hands.LEFTHAND, ah1, ah2, d, 0.0);
        else if (score1 > 1.0 || score2 > 1.0 || score1 + score2 > 1.2)
          return null;
        else
          return new Handhold(d1, d2, h1, h2, ah1, ah2, d, score1 + score2);

      }
      return null;  // hidden dancer
    }

    public int CompareTo(object obj) {
      return ((IComparable)score).CompareTo(((Handhold)obj).score);
    }
  }


}
