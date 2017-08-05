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
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.Data.Xml.Dom;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace TaminationsWin {

  public static class DoubleUtilities {

    public static double toRadians(this double angle) {
      return angle * Math.PI / 180;
    }

    public static bool isApprox(this double x, double y, double delta = 0.1) {
      return Math.Abs(x - y) < delta;
    }
    public static bool isApprox(this float x, double y, double delta = 0.1) {
      return ((double)x).isApprox(y,delta);
    }
    public static double angleDiff(this double x, double a2) {
      return ((((x - a2) % (Math.PI * 2)) + (Math.PI * 3)) % (Math.PI * 2)) - Math.PI;
    }
    public static bool angleEquals(this double x, double a2) {
      return x.angleDiff(a2).isApprox(0);
    }

  }

  public static class IntUtilities {
    public static IEnumerable<int> until(this int i, int j)
    {
      return Enumerable.Range(i, j - i);
    }
  }

  public static class ListUtilities {

    //  Iterate over a sequence, doing a computation on each item which does not return a result
    public static void ForEach<TSource>(this IEnumerable<TSource> list, Action<TSource> fun) {
      foreach (TSource item in list)
        fun(item);
    }
    //  Same as above, passing an additional index to the function
    public static void ForEach<TSource>(this IEnumerable<TSource> list, Action<TSource,int> fun) {
      int i = 0;
      foreach (TSource item in list)
        fun(item,i++);
    }

  }

  public static class Vector {

    public static Vector2 Create(double x, double y) {
      return new Vector2((float)x, (float)y);
    }

    //  Just returns the z-component of the cross product, which is the only non-zero value
    public static float Cross(this Vector2 v1, Vector2 v2) {
      return v1.X * v2.Y - v1.Y * v2.X;
    }

    public static double Length(this Vector2 v) {
      return Math.Sqrt(v.X * v.X + v.Y * v.Y);
    }

    public static double Angle(this Vector2 v) {
      return Math.Atan2(v.Y, v.X);
    }

    public static double angleDiff(double a1, double a2) {
      return ((a1 - a2 + Math.PI * 3) % (Math.PI * 2)) - Math.PI;
    }

    public static double angleDiff(this Vector2 v1, Vector2 v2) {
      return Vector.angleDiff(v1.Angle(), v2.Angle());
    }

    public static Vector2 vectorTo(this Vector2 v1, Vector2 v2) {
      return v2 - v1;
    }

    public static Vector2 Rotate(this Vector2 v1, double th) {
      var d = v1.Length();
      var a = v1.Angle() + th;
      return Vector.Create(d * Math.Cos(a), d * Math.Sin(a));
    }

    public static Vector2 concatenate(this Vector2 v, Matrix3x2 m) {
      return (Matrix.CreateTranslation(v.X, v.Y) * m).Location();
    }

    public static Vector2 preConcatenate(this Vector2 v, Matrix3x2 m) {
      return (m * Matrix.CreateTranslation(v.X, v.Y)).Location();
    }

  }

  public static class Matrix {

/*
Table for matrix fields iOS and Java
iOS                         Java                                     Win Matrix3x2
a    b     0          MSCALE_X(0)   MSKEW_Y(3)     MPERSP_0(6)       M11    M12    0
c    d     0          MSKEW_X(1)    MSCALE_Y(4)    MPERSP_1(7)       M21    M22    0
tx   ty    1          MTRANS_X(2)   MTRANS_Y(5)    MPERSP_2(8)       M31    M32    1
*/

    public static Matrix3x2 CreateScale(double x, double y) {
      return Matrix3x2.CreateScale((float)x, (float)y);
    }

    public static Matrix3x2 CreateRotation(double r) {
      return Matrix3x2.CreateRotation((float)r);
    }

    public static Matrix3x2 CreateTranslation(double x, double y) {
      return Matrix3x2.CreateTranslation((float)x, (float)y);
    }

    public static Vector2 TransformVector(this Matrix3x2 m, Vector2 v) {
      return new Vector2(m.M11 * v.X + m.M21 * v.Y + m.M31,
                         m.M12 * v.X + m.M22 * v.Y + m.M32);
    }

    public static Vector2 Location(this Matrix3x2 m) {
      return new Vector2(m.M31, m.M32);
    }

    public static Vector2 Direction(this Matrix3x2 m) {
      var mat2 = m;
      mat2.M31 = 0;
      mat2.M32 = 0;
      var pt = new Vector2(1, 0);
      pt = mat2.TransformVector(pt);
      return pt;
    }

    public static double Angle(this Matrix3x2 m) {
      return m.Direction().Angle();
    }

    public static Matrix3x2 Inverse(this Matrix3x2 m) {
      var m2 = new Matrix3x2();
      Matrix3x2.Invert(m, out m2);  //  we assume that this works
      return m2;
    }

    public static Matrix3x2 putArray(double [,] a) {
      var m = new Matrix3x2();
      m.M11 = (float)a[0,0];
      m.M12 = (float)a[0,1];
      m.M21 = (float)a[1,0];
      m.M22 = (float)a[1,1];
      m.M31 = (float)a[2,0];
      m.M32 = (float)a[2,1];
      return m;
    }

    public static double pythag(double aa, double bb) {
      var a = Math.Abs(aa);
      var b = Math.Abs(bb);
      if (a > b)
        return a * Math.Sqrt(1.0 + (b * b / a / a));
      else if (b == 0.0)
        return a;
      return b * Math.Sqrt(1.0 + (a * a / b / b));
    }

    //  Transpose 3x3 matrix, for SVD result
    public static double[,] transpose(this double [,] a)
    {
      return new [,] {
        { a[0, 0], a[1, 0], a[2, 0]},
        { a[0, 1], a[1, 1], a[2, 1]},
        { a[0, 2], a[1, 2], a[2, 2] } };
    }

    public static Tuple<double[,], double[], double[,]> SVD(double[,] u)
    {
      var temp = 0.0;
      //Compute the thin SVD from G. H. Golub and C. Reinsch, Numer. Math. 14, 403-420 (1970)
      var epsilon = 2.220446049250313e-16;
      var prec = epsilon; //Math.pow(2,-52) // assumes double prec
      var tolerance = 1.0e-64 / prec;
      var itmax = 50;
      double c;
      var l = 0;

      int m = 3;  // only three rows of matrix are used for 2-D SVD
      var n = 3;  //

      double[] e = { 0, 0, 0 };
      double[] q = { 0, 0, 0 };
      double[,] v = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, };

      //  Householder's reduction to bidiagonal form

      double f;
      double g = 0.0;
      double h;
      double x = 0.0;
      double y;
      double z;
      double s;

      for (int i = 0; i < n; i++)
      {
        e[i] = g;
        l = i + 1;
        s = 0.until(3).Select(a => u[i, a] * u[i, a]).Sum();
        if (s <= tolerance)
          g = 0.0;
        else
        {
          f = u[i, i];
          g = Math.Sqrt(s);
          if (f >= 0.0)
            g = -g;
          h = f * g - s;
          u[i, i] = f - g;
          for (int j = l; j < n; j++)
          {
            s = i.until(m).Select(a => u[i, a] * u[j, a]).Sum();
            f = s / h;
            for (int k = i; k < m; k++)
              u[k, j] += f * u[k, i];
          }
        }
        q[i] = g;
        s = l.until(m).Select(j => u[i, j] * u[i, j]).Sum();
        if (s <= tolerance)
          g = 0.0;
        else
        {
          f = u[i, i + 1];
          g = Math.Sqrt(s);
          if (f >= 0.0)
            g = -g;
          h = f * g - s;
          u[i, i + 1] = f - g;
          for (int j = l; j < n; j++)
            e[j] = u[i, j] / h;
          for (int j = l; j < m; j++)
          {
            s = l.until(n).Select(k => u[j, k] * u[i, k]).Sum();
            for (int k = l; k < n; k++)
              u[j, k] += s * e[k];
          }
        }
        y = Math.Abs(q[i]) + Math.Abs(e[i]);
        if (y > x)
          x = y;
      }

      // accumulation of right hand transformations
      for (int i = n - 1; i >= 0; i--)
      {
        if (g != 0.0)
        {
          h = g * u[i, i + 1];
          foreach (int j in l.until(n))
            v[j, i] = u[i, j] / h;
          foreach (int j in l.until(n))
          {
            s = l.until(n).Select(k => u[i, k] * v[k, j]).Sum();
            foreach (int k in l.until(n))
              v[k, j] += (s * v[k, i]);
          }
        }
        foreach (int j in l.until(n))
        {
          v[i, j] = 0.0;
          v[j, i] = 0.0;
        }
        v[i, i] = 1.0;
        g = e[i];
        l = i;
      }

      // accumulation of left hand transformations
      for (int i=n-1; i>=0; i--) {
        l = i + 1;
        g = q[i];
        foreach (int j in l.until(n))
          u[i, j] = 0.0;
        if (g != 0.0) {
          h = u[i, i] * g;
          foreach (int j in l.until(n)) {
            s = l.until(m).Select(a => u[a, i] * u[a, j]).Sum();
            f = s / h;
            foreach (int k in i.until(m))
              u[k, j] += f * u[k, i];
          }
          foreach (int j in i.until(m))
            u[j, i] = u[j, i] / g;
        }
        else
          foreach (int j in i.until(m))
            u[j, i] = 0.0;
        u[i, i] += 1.0;
      }

      // diagonalization of the bidiagonal form
      prec *= x;
      for (int k=n-1; k>=0; k--) {
        for (int iteration=0; iteration<itmax; iteration++) {
          // test f splitting
          var test_convergence = false;
          var el = k;
          for (int ella=k; ella>=0; ella--) {
            el = ella;
            if (Math.Abs(e[ella]) <= prec) {
              test_convergence = true;
              break;
            }
            if (Math.Abs(q[ella - 1]) <= prec)
              break;
          }
          if (!test_convergence) {
            // cancellation of e[l] if l>0
            c = 0.0;
            s = 1.0;
            var l1 = el - 1;
            foreach (int i in el.until(k + 1)) {
              f = s * e[i];
              e[i] = c * e[i];
              if (Math.Abs(f) <= prec)
                break;
              g = q[i];
              h = pythag(f, g);
              q[i] = h;
              c = g / h;
              s = -f / h;
              foreach (int j in 0.until(m))
              {
                y = u[j, l1];
                z = u[j, i];
                u[j, l1] = y * c + (z * s);
                u[j, i] = -y * s + (z * c);
              }
            }
          }
          // test f convergence
          z = q[k];
          if (el == k)
          {
            //convergence
            if (z < 0.0) {
              //q[k] is made non-negative
              q[k] = -z;
              foreach (int j in 0.until(n))
                v[j, k] = -v[j, k];
            }
            break;  //break out of iteration loop and move on to next k value
          }
          if (iteration >= itmax - 1)
            throw new Exception("Error: no convergence.");
          // shift from bottom 2x2 minor
          x = q[el];
          y = q[k - 1];
          g = e[k - 1];
          h = e[k];
          f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0 * h * y);
          g = pythag(f, 1.0);
          if (f < 0.0)
            f = ((x - z) * (x + z) + h * (y / (f - g) - h)) / x;
          else
            f = ((x - z) * (x + z) + h * (y / (f + g) - h)) / x;
          // next QR transformation
          c = 1.0;
          s = 1.0;
          foreach (int i in (el + 1).until(k + 1)) {
            g = e[i];
            y = q[i];
            h = s * g;
            g *= c;
            z = pythag(f, h);
            e[i - 1] = z;
            c = f / z;
            s = h / z;
            f = x * c + g * s;
            g = -x * s + g * c;
            h = y * s;
            y *= c;
            foreach (int j in 0.until(n)) {
              x = v[j, i - 1];
              z = v[j, i];
              v[j, i - 1] = x * c + z * s;
              v[j, i] = -x * s + z * c;
            }
            z = pythag(f, h);
            q[i - 1] = z;
            c = f / z;
            s = h / z;
            f = c * g + s * y;
            x = -s * g + c * y;
            foreach (int j in 0.until(m))
            {
              y = u[j, i - 1];
              z = u[j, i];
              u[j, i - 1] = y * c + z * s;
              u[j, i] = -y * s + z * c;
            }
          }
          e[el] = 0.0;
          e[k] = f;
          q[k] = x;
        }
      }

      //vt= transpose(v)
      //return (u,q,vt)
      q = q.Select(a => a < prec ? 0.0 : a).ToArray();

      //sort eigenvalues
      var ii = 1;
      while (ii < n)
      {
        //writeln(q)
        for (int j = ii - 1; j >= 0; j--)
        {
          if (q[j] < q[ii])
          {
            //  writeln(i,'-',j)
            c = q[j];
            q[j] = q[ii];
            q[ii] = c;
            foreach (int k in 0.until(2))
            {
              temp = u[k, ii]; u[k, ii] = u[k, j]; u[k, j] = temp;
            }
            foreach (int k in 0.until(2))
            {
              temp = v[k, ii]; v[k, ii] = v[k, j]; v[k, j] = temp;
            }
            //     u.swapCols(i,j)
            //     v.swapCols(i,j)
            ii = j;
          }
        }
        ii++;
      }
      return Tuple.Create(u, q, v);
    }

  }

  public static class StringUtilities {

    //  The standard double.Parse(str) uses the user's localization
    //  We don't want that, so this function is used instead
    public static double toDouble(this string value) {
      return double.Parse(value,new CultureInfo("en-US"));
    }

    public static string ReplaceAll(this string source, string pattern, string repl) {
      return Regex.Replace(source, pattern, repl);
    }

    public static string[] Split(this string str) {
      return str.Split(null);
    }

    // Returns an array of strings, starting with the entire string,
    // and each subsequent string chopping one word off the end
    public static List<string> chopped(this string str) {
      var ss = new List<string>();
      return str.Split().Select((string s) => {
        ss.Add(s);
        return ss.Aggregate("", (s1, s2) => $"{s1} {s2}").Trim();
      }).Reverse().ToList();
    }

    // Return an array of strings, each removing one word from the start
    public static List<string> diced(this string str) {
      var ss = new List<string>();
      return str.Split().Reverse().Select((string s) => {
        ss.Insert(0, s);
        return ss.Aggregate("", (s1, s2) => $"{s1} {s2}").Trim();
      }).Reverse().ToList();
    }

    /**
     *   Return all combinations of words from a string
     */
    public static List<string> minced(this string str) {
      return str.chopped().SelectMany((string s) => s.diced()).ToList();
    }

  }

  public static class ColorUtilities {

    public static Color ColorFromHex(uint c) {
      return Color.FromArgb(
        (byte)(c >> 24),
        (byte)((c & 0x00ff0000) >> 16),
        (byte)((c & 0x0000ff00) >> 8),
        (byte)(c & 0x000000ff)
        );
    }

    public static Color invert(this Color c) {
      return Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
    }

    public static Color darker(this Color c, double f = 0.7) {
      return Color.FromArgb(c.A, (byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f));
    }

    public static Color brighter(this Color c, double f = 0.7) {
      return c.invert().darker(f).invert();
    }

    public static Color veryBright(this Color c) {
      return c.brighter(0.24);
    }

  }

  public static class PageUtilities {

    public static Dictionary<Type, Dictionary<string, string>> intents =
      new Dictionary<Type, Dictionary<string, string>>();

    public static void Navigate(this Page page, Type pagetype, Dictionary<string, string> intent) {
      intents[pagetype] = intent;
      page.Frame.Navigate(pagetype);
    }

    public static Dictionary<String, String> Intent(this Page page) {
      return intents[page.GetType()];
    }

  }

  public static class FrameUtilities {
    public static void Navigate(this Frame frame, Type pagetype, Dictionary<string, string> intent) {
      PageUtilities.intents[pagetype] = intent;
      frame.Navigate(pagetype);
    }
  }


  public static class XmlNodeUtilities {

    public static bool hasAttr(this IXmlNode node, string item) {
      return node.Attributes.GetNamedItem(item) != null;
    }

    public static string attr(this IXmlNode node, string item) {
      var n = node.Attributes.GetNamedItem(item);
      return n == null ? "" : (string)n.NodeValue;
    }

  }

}
