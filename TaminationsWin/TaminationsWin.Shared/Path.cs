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

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TaminationsWin {
  public class Path
  {

    public List<Movement> movelist = null;
    public List<Matrix3x2> transformlist = null;

    public void recalculate() {
      var tx = Matrix3x2.Identity;
      transformlist = movelist.Select(movement => {
        tx = movement.translate() * tx;
        tx = movement.rotate() * tx;
        return tx; // makes a copy since tx is a struct
      }).ToList();
    }

    public Path() {
      movelist = new List<Movement>();
      recalculate();
    }

    public Path(List<Movement> move) {
      movelist = move.Select(m => m).ToList();
      recalculate();
    }

    public Path(Movement m) {
      movelist = new List<Movement>();
      movelist.Add(m);
      recalculate();
    }

    public Path copy() {
      return new Path(movelist);
    }

    public void clear() {
      movelist = null;
      transformlist = null;
    }

    public Path add(Path p) {
      movelist = movelist.Concat(p.movelist).ToList();
      recalculate();
      return this;
    }

    public Path add(Movement m) {
      movelist.Add(m);
      recalculate();
      return this;
    }

    public Movement pop() {
      var m = movelist.Last();
      movelist.RemoveAt(movelist.Count - 1);
      recalculate();
      return m;
    }

    public void reflect() {
      movelist = movelist.Select(m => m.reflect()).ToList();
      recalculate();
    }

    public double beats {
      get {
        return movelist.Sum(m => m.beats);
      }
    }

    public Path changebeats(double newbeats) {
      var factor = newbeats / beats;
      movelist = movelist.Select(m => m.time(m.beats * factor)).ToList();
      //  no need to recalculate, transformlist doesn't depend on beats
      return this;
    }

    public Path changehands(int hands) {
      movelist = movelist.Select(m => m.useHands(hands)).ToList();
      return this;
    }

    public Path scale(double x, double y) {
      movelist = movelist.Select(m => m.scale(x, y)).ToList();
      recalculate();
      return this;
    }

    public Path skew(double x, double y) {
      //  Apply the skew to just the last movement
      if (movelist != null && movelist.Count > 0) {
        var m = pop();
        m = m.skew(x, y);
        add(m);
      }
      return this;
    }

    /**
     * Return a transform for a specific point of time
     */
    public Matrix3x2 animate(double b) {
      var bv = b;
      var tx = Matrix3x2.Identity;
      Movement m = null;
      foreach (int i in Enumerable.Range(0,movelist.Count)) {
        if (m == null) {
          m = movelist.ElementAt(i);
          if (bv >= m.beats) {
            tx = transformlist.ElementAt(i);
            bv = bv - m.beats;
            m = null;
          }
        }
      }
      //  Apply movement in progress
      if (m != null)
        tx = m.rotate(bv) * m.translate(bv) * tx;
      return tx;
    }

    /**
     * Return the current hand at a specific point in time
     */
    public int hands(double b) {
      if (b < 0 || b > beats)
        return (int)Hands.BOTHHANDS;
      else {
        var bv = b;
        return (int)movelist.Aggregate(Hands.BOTHHANDS, (h, m) => {
          if (bv < 0)
            return h;
          bv = bv - m.beats;
          return m.hands;
        });
      }
    }

  }
}
