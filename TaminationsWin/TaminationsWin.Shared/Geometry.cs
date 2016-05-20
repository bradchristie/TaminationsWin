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

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using Windows.UI;

namespace TaminationsWin {

  public struct GeometryType {
    public const int BIGON = 1;
    public const int SQUARE = 2;
    public const int HEXAGON = 3;
  }

  public interface Geometry {
    Matrix3x2 startMatrix(Matrix3x2 mat);
    Matrix3x2 pathMatrix(Matrix3x2 starttx, Matrix3x2 tx, double beat);
    void drawGrid(double s, CanvasDrawingSession ds);
    int geometry();
    Geometry clone();
  }

  public class GeometryMaker {

    public static Geometry[] makeAll(int type) {
      switch (type) {
        case GeometryType.BIGON: return new Geometry[] { new BigonGeometry() };
        case GeometryType.HEXAGON: return new Geometry[] { new HexagonGeometry(0), new HexagonGeometry(1), new HexagonGeometry(2) };
        default: return new Geometry[] { new SquareGeometry(0), new SquareGeometry(1) };
      }
    }

    public static Geometry makeOne(int g, int r=0) {
      switch (g) {
        case GeometryType.BIGON: return new BigonGeometry();
        case GeometryType.HEXAGON: return new HexagonGeometry(r);
        default: return new SquareGeometry(r);
      }
    }

    public static Geometry makeOne(string gstr, int r=0) {
      switch (gstr) {
        case "Bigon": return new BigonGeometry();
        case "Hexagon": return new HexagonGeometry(r);
        default: return new SquareGeometry(r);
      }
    }

  }

  //////////////////////////////////////////////////////////////////////////
  public class BigonGeometry : Geometry {
    public BigonGeometry() { }
    private double prevangle = 0.0;
    public Geometry clone() {
      return new BigonGeometry();
    }

    public int geometry() {
      return GeometryType.BIGON;
    }

    /**
     * Generate a transform to apply to a dancer's start position
     */
    public Matrix3x2 startMatrix(Matrix3x2 mat) {
      var x = mat.M31;
      var y = mat.M32;
      var r = Math.Sqrt(x * x + y * y);
      var startangle = Math.Atan2(mat.M12, mat.M22);
      var angle = Math.Atan2(y, x) + Math.PI;
      var bigangle = angle * 2 - Math.PI;
      var x2 = r * Math.Cos(bigangle);
      var y2 = r * Math.Sin(bigangle);
      return Matrix.CreateRotation(startangle + angle) * Matrix.CreateTranslation(x2, y2);
    }

    /**
     * Convert transform for a dancer's current position
     */
    public Matrix3x2 pathMatrix(Matrix3x2 starttx, Matrix3x2 tx, double beat) {
      var x = starttx.M31;
      var y = starttx.M32;
      var a0 = Math.Atan2(y, x);
      var x2 = tx.M31;
      var y2 = tx.M32;
      var a1 = Math.Atan2(y2, x2);
      if (beat <= 0.0)
        prevangle = a1;
      var wrap = Math.Round((a1 - prevangle) / (Math.PI * 2));
      var a2 = a1 - wrap * Math.PI * 2;
      var a3 = a2 - a0;
      prevangle = a2;
      return Matrix.CreateRotation(a3);
    }

    public void drawGrid(double s, CanvasDrawingSession ds) {
      CanvasPathBuilder pathBuilder = new CanvasPathBuilder(ds);
      for (float x1 = -7.5f; x1 <= 7.5f; x1 += 1) {
        pathBuilder.BeginFigure(Math.Abs(x1), 0);
        for (float y1 = 0.2f; y1 <= 7.5f; y1 += 0.2f) {
          var a = 2.0 * Math.Atan2(y1, x1);
          var r = Math.Sqrt(x1 * x1 + y1 * y1);
          var x = r * Math.Cos(a);
          var y = r * Math.Sin(a);
          pathBuilder.AddLine((float)x, (float)y);
        }
        pathBuilder.EndFigure(CanvasFigureLoop.Open);
      }
      var onep = CanvasGeometry.CreatePath(pathBuilder);
      ds.DrawGeometry(onep, Colors.DarkGray, 1 / (float)s);
      ds.DrawGeometry(onep.Transform(Matrix.CreateScale(-1, 1)), Colors.DarkGray, 1 / (float)s);
    }

  }

  //////////////////////////////////////////////////////////////////////////
  public class SquareGeometry : Geometry {
    int rotnum;
    public SquareGeometry(int rotnum) { this.rotnum = rotnum; }
    public Geometry clone() {
      return new SquareGeometry(rotnum);
    }

    public void drawGrid(double s, CanvasDrawingSession ds) {
      for (float x = -7.5f; x <= 7.5f; x += 1) {
        var m11 = ds.Transform.M11;
        ds.DrawLine(x, -7.5f, x, 7.5f, Colors.DarkGray, 1/(float)s);
        ds.DrawLine(-7.5f, x, 7.5f, x, Colors.DarkGray, 1/(float)s);
      }
    }

    public int geometry() {
      return GeometryType.SQUARE;
    }

    public Matrix3x2 pathMatrix(Matrix3x2 starttx, Matrix3x2 tx, double beat) {
      //  No transform needed
      return Matrix3x2.Identity;
    }

    public Matrix3x2 startMatrix(Matrix3x2 mat) {
      return mat * Matrix.CreateRotation(Math.PI * rotnum);
    }
  }

  //////////////////////////////////////////////////////////////////////////
  public class HexagonGeometry : Geometry {
    int rotnum;
    double prevangle = 0;
    public HexagonGeometry(int rotnum) { this.rotnum = rotnum; }
    public Geometry clone() {
      return new HexagonGeometry(rotnum);
    }

    /**
     * Generate a transform to apply to a dancer's start position
     */
    public Matrix3x2 startMatrix(Matrix3x2 mat) {
      var a = (Math.PI * 2.0 / 3.0) * rotnum;
      var x = mat.M31;
      var y = mat.M32;
      var r = Math.Sqrt(x * x + y * y);
      var startangle = Math.Atan2(mat.M12, mat.M22);
      var angle = Math.Atan2(y, x);
      var dangle = angle < 0 ? -(Math.PI + angle) / 3.0 : (Math.PI - angle) / 3.0;
      var x2 = r * Math.Cos(angle + dangle + a);
      var y2 = r * Math.Sin(angle + dangle + a);
      var startangle2 = startangle + a + dangle;
      return Matrix.CreateRotation(startangle2) * Matrix.CreateTranslation(x2, y2);
    }

    /**
     * Convert transform for a dancer's current position
     */
    public Matrix3x2 pathMatrix(Matrix3x2 starttx, Matrix3x2 tx, double beat) {
      //  Get dancer's start angle and current angle
      var x = starttx.M31;
      var y = starttx.M32;
      var a0 = Math.Atan2(y, x);
      var x2 = tx.M31;
      var y2 = tx.M32;
      var a1 = Math.Atan2(y2, x2);
      //  Correct for wrapping around +/- pi
      if (beat <= 0)
        prevangle = a1;
      var wrap = Math.Round((a1 - prevangle) / (Math.PI * 2));
      var a2 = a1 - wrap * Math.PI * 2;
      var a3 = -(a2 - a0) / 3;
      prevangle = a2;
      return Matrix.CreateRotation(a3);
    }

    public void drawGrid(double s, CanvasDrawingSession ds) {
      CanvasPathBuilder pathBuilder = new CanvasPathBuilder(ds);
      for (float x0=0.5f; x0<8.5f; x0+=1) {
        pathBuilder.BeginFigure(0, x0);
        for (float y0=0.5f; y0<8.5; y0+=0.5f) {
          var a = Math.Atan2(y0, x0) * 2 / 3;
          var r = Math.Sqrt(x0 * x0 + y0 * y0);
          var x = r * Math.Sin(a);
          var y = r * Math.Cos(a);
          pathBuilder.AddLine((float)x, (float)y);
        }
        pathBuilder.EndFigure(CanvasFigureLoop.Open);
      }
      //  rotate and reflect the result
      var onep = CanvasGeometry.CreatePath(pathBuilder);
      for (int a=0; a<6; a++) {
        var m = Matrix3x2.Identity * Matrix.CreateRotation(Math.PI / 6 + a * Math.PI / 3);
        ds.DrawGeometry(onep.Transform(m), Colors.DarkGray, 1 / (float)s);
        m = m * Matrix.CreateScale(1, -1);
        ds.DrawGeometry(onep.Transform(m), Colors.DarkGray, 1 / (float)s);
      }
    }

    public int geometry() {
      return GeometryType.HEXAGON;
    }

  }

}
