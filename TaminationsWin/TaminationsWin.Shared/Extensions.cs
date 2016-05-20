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
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.Data.Xml.Dom;
using System.Numerics;
using System.Text.RegularExpressions;

namespace TaminationsWin {
  public static class DoubleUtilities {
    public static double toRadians(this double angle) {
      return angle * Math.PI / 180;
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

  }

  public class Matrix {

    public static Matrix3x2 CreateScale(double x, double y) {
      return Matrix3x2.CreateScale((float)x, (float)y);
    }

    public static Matrix3x2 CreateRotation(double r) {
      return Matrix3x2.CreateRotation((float)r);
    }

    public static Matrix3x2 CreateTranslation(double x, double y) {
      return Matrix3x2.CreateTranslation((float)x, (float)y);
    }
  }

  public static class StringUtilities {

    public static string ReplaceAll(this string source, string pattern, string repl) {
      return Regex.Replace(source, pattern, repl);
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
