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

  public static class ObjectUtilities {

    public delegate T IfDoDelegate<T>(T obj);
    public static T IfDo<T>(this T obj, bool exp, IfDoDelegate<T> f) {
      return exp ? obj : f(obj);
    }

  }

  public static class DoubleUtilities {

    public static double Sign(this double x) {
      return x < 0 ? -1 : x > 0 ? 1 : 0;
    }

    public static double Floor(this double x) {
      return Math.Floor(x);
    }

    public static double Ceil(this double x) {
      return Math.Ceiling(x);
    }

    public static double Round(this double x) {
      return Math.Round(x);
    }

    public static double Abs(this double x) {
      return Math.Abs(x);
    }

    public static double Sqrt(this double x) {
      return Math.Sqrt(x);
    }

    public static double Sq(this double x) {
      return x * x;
    }

    public static double Sin(this double x) {
      return Math.Sin(x);
    }

    public static double Cos(this double x) {
      return Math.Cos(x);
    }

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
    public static int Abs(this int i) {
      return Math.Abs(i);
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

    public static Matrix3x2 CreateScale(double x,double y) {
      return Matrix3x2.CreateScale((float)x,(float)y);
    }

    public static Matrix3x2 CreateRotation(double r) {
      return Matrix3x2.CreateRotation((float)r);
    }

    public static Matrix3x2 CreateTranslation(double x,double y) {
      return Matrix3x2.CreateTranslation((float)x,(float)y);
    }

    public static Vector2 TransformVector(this Matrix3x2 m,Vector2 v) {
      return new Vector2(m.M11 * v.X + m.M21 * v.Y + m.M31,
                         m.M12 * v.X + m.M22 * v.Y + m.M32);
    }

    public static Vector2 Location(this Matrix3x2 m) {
      return new Vector2(m.M31,m.M32);
    }

    public static Vector2 Direction(this Matrix3x2 m) {
      var mat2 = m;
      mat2.M31 = 0;
      mat2.M32 = 0;
      var pt = new Vector2(1,0);
      pt = mat2.TransformVector(pt);
      return pt;
    }

    public static double Angle(this Matrix3x2 m) {
      return m.Direction().Angle();
    }

    public static Matrix3x2 Inverse(this Matrix3x2 m) {
      var m2 = new Matrix3x2();
      Matrix3x2.Invert(m,out m2);  //  we assume that this works
      return m2;
    }

    public static Matrix3x2 putArray(double[,] a) {
      var m = new Matrix3x2();
      m.M11 = (float)a[0,0];
      m.M12 = (float)a[1,0];
      m.M21 = (float)a[0,1];
      m.M22 = (float)a[1,1];
      m.M31 = a.GetUpperBound(1) > 1 ? (float)a[0,2] : 0;
      m.M32 = a.GetUpperBound(1) > 1 ? (float)a[1,2] : 0;
      return m;
    }

    public static double pythag(double aa,double bb) {
      var a = Math.Abs(aa);
      var b = Math.Abs(bb);
      if (a > b)
        return a * Math.Sqrt(1.0 + (b * b / a / a));
      else if (b == 0.0)
        return a;
      return b * Math.Sqrt(1.0 + (a * a / b / b));
    }

    //  Transpose 3x3 matrix, for SVD result
    public static double[,] transposeX(this double[,] a) {
      return new[,] {
        { a[0, 0], a[1, 0], a[2, 0]},
        { a[0, 1], a[1, 1], a[2, 1]},
        { a[0, 2], a[1, 2], a[2, 2] } };
    }

    public static double[,] transpose(this double[,] a) {
      var x = a.GetUpperBound(1)+1;
      var y = a.GetUpperBound(0)+1;
      var retval = new double[x,y];
      for (int i = 0; i<x; i++)
        for (int j = 0; j<y; j++)
          retval[i,j] = a[j,i];
      return retval;
    }

    public static Tuple<double[,],double[],double[,]> SVD(double[,] A) {
      var a = A[0,0];
      var b = A[0,1];
      var c = A[1,0];
      var d = A[1,1];
      //  Check for trivial case
      var epsilon = 0.0001;
      if (b.Abs() < epsilon && c.Abs() < epsilon) {
        double[,] V = { { (a < 0.0) ? -1.0 : 1.0,0.0 },
                 { 0.0,(d < 0.0) ? -1.0 : 1.0 } };
        double[] Sigma = { a.Abs(),d.Abs() };
        double[,] U = { { 1.0,0.0 },{ 0.0,1.0 } };
        return Tuple.Create(U,Sigma,V);
      }
      else {
        var j = a.Sq() + b.Sq();
        var k = c.Sq() + d.Sq();
        var vc = a*c + b*d;
        //  Check to see if A^T*A is diagonal
        if (vc.Abs() < epsilon) {
          var s1 = j.Sqrt();
          var s2 = ((j-k).Abs() < epsilon) ? s1 : k.Sqrt();
          double[] Sigma = { s1,s2 };
          double[,] V = { { 1.0,0.0 },{ 0.0,1.0 } };
          double[,] U = { { a/s1,b/s1 },{ c/s2,d/s2 } };
          return Tuple.Create(U,Sigma,V);
        }
        else {   //  Otherwise, solve quadratic for eigenvalues
          var atanarg1 = 2 * a * c + 2 * b * d;
          var atanarg2 = a * a + b * b - c * c - d * d;
          var Theta = 0.5 * Math.Atan2(atanarg1,atanarg2);
          double[,] U = { { Theta.Cos(),-Theta.Sin() },
                           { Theta.Sin(),Theta.Cos() } };
          var Phi = 0.5 * Math.Atan2(2*a*b + 2*c*d,a.Sq() - b.Sq() + c.Sq() - d.Sq());
          var s11 = (a * Theta.Cos() + c * Theta.Sin()) * Phi.Cos() +
                    (b * Theta.Cos() + d * Theta.Sin()) * Phi.Sin();
          var s22 = (a * Theta.Sin() - c * Theta.Cos()) * Phi.Sin() +
                    (-b * Theta.Sin() + d * Theta.Cos()) * Phi.Cos();

          var S1 = a.Sq() + b.Sq() + c.Sq() + d.Sq();
          var S2 = ((a.Sq() + b.Sq() - c.Sq() - d.Sq()).Sq() + 4 * (a * c + b * d).Sq()).Sqrt();
          double[] Sigma = { (S1 + S2).Sqrt() / 2,(S1 - S2).Sqrt() / 2 };
          double[,] V = { { s11.Sign() * Phi.Cos(),-s22.Sign() * Phi.Sin() },
                   { s11.Sign() * Phi.Sin(),s22.Sign() * Phi.Cos() } };
          return Tuple.Create(U,Sigma,V);
        }
      }
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

    public static string ToCapCase(this string str) {
      var strs = str.Split().Select(s => s.First().ToString().ToUpper() + s.Skip(1).ToString().ToLower());
      return string.Join(" ",strs);
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
