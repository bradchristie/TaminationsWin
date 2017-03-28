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
using System.Numerics;
using System.Text;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;

namespace TaminationsWin
{
  public enum TouchAction {
    BEGAN = 1,
    ENDED = 2,
    MOVED = 3
  }

  public class InteractiveDancer : Dancer
  {
    const float LEFTSENSITIVITY = 0.02f;
    const float RIGHTSENSITIVITY = 0.02f;
    const float DIRECTIONALPHA = 0.9f;
    const float DIRECTIONTHRESHOLD = 0.01f;

    public bool onTrack = true;
    static Point PointZero = new Point(0, 0);
    Point primaryTouch = PointZero;
    Point primaryMove = PointZero;
    Point secondaryTouch = PointZero;
    Point secondaryMove = PointZero;
    Vector2 primaryDirection = Vector.Create(0, 0);
    uint primaryid = 0;
    uint secondaryid = 0;

    private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

    public InteractiveDancer(string number, string number_couple, Gender gender,
      Color fillColor, Matrix3x2 mat, Geometry geom, List<Movement> moves) 
          : base(number,number_couple,gender,fillColor,mat,geom,moves) { }

    public Matrix3x2 computeMatrix(double beat) {
      var savetx = tx;
      base.animate(beat);
      var computetx = tx;
      tx = savetx;
      return computetx;
    }

    override public void animate(double beat) {
      fillColor = beat <= 0 || onTrack ? drawColor.veryBright() : Colors.Gray;
      if (beat <= -2.0) {
        tx = starttx;
      } else {
        //  Apply any additional movement and angle from the user
        //  This processes left and right touches
        if (primaryMove != PointZero) {
          var dx = -(primaryMove.Y - primaryTouch.Y) * LEFTSENSITIVITY;
          var dy = -(primaryMove.X - primaryTouch.X) * LEFTSENSITIVITY;
          tx = tx * Matrix.CreateTranslation(dx, dy);
          primaryTouch = primaryMove;
          if (secondaryid == 0) {
            //  Right finger is up - rotation follows movement
            if (primaryDirection == Vector.Create(0,0)) {
              primaryDirection = Vector.Create(dx, dy);
            } else {
              primaryDirection = Vector.Create(
                DIRECTIONALPHA * primaryDirection.X + (1 - DIRECTIONALPHA) * dx,
                DIRECTIONALPHA * primaryDirection.Y + (1 - DIRECTIONALPHA) * dy);
            }
            if (primaryDirection.Length() >= DIRECTIONTHRESHOLD) {
              var a1 = tx.Angle();
              var a2 = primaryDirection.Angle();
              tx = Matrix.CreateRotation(a2 - a1) * tx;
            }
          }
        }
        if (secondaryid != 0) {
          //  Rotation follow right finger
          //  Get the vector of the user's finger
          var dx = -(secondaryMove.Y - secondaryTouch.Y) * RIGHTSENSITIVITY;
          var dy = -(secondaryMove.X - secondaryTouch.X) * RIGHTSENSITIVITY;
          var vf = Vector.Create(dx, dy);
          //  Get the vector the dancer is facing
          var vu = tx.Direction();
          //  Amount of rotation is z of the cross product of the two
          var da = vu.Cross(vf);
          tx = Matrix.CreateRotation(da) * tx;
          secondaryTouch = secondaryMove;
        }
      }
    }

    public void doTouch(PointerPoint pp, TouchAction action, UserControl inView) {
      var s = 500 / inView.ActualHeight;
      if (action == TouchAction.BEGAN) {
        //  Touch down event
        //  Figure out if touching left or right side, and remember the point
        //  Also need to remember the Touch to correlate future move events
        var controller = settings.Values["primarycontroller"]?.ToString();
        if ((pp.Position.X > inView.ActualWidth/2) ^ (controller == "left")) {
          primaryTouch.X = pp.Position.X * s;
          primaryTouch.Y = pp.Position.Y * s;
          primaryMove = primaryTouch;
          primaryid = pp.PointerId;
          primaryDirection = Vector.Create(0, 0);
        } else {
          secondaryTouch.X = pp.Position.X * s;
          secondaryTouch.Y = pp.Position.Y * s;
          secondaryMove = secondaryTouch;
          secondaryid = pp.PointerId;
        }
      }
      else if (action == TouchAction.ENDED) {
        //  Touch up event
        //  Stop moving and rotating
        if (pp.PointerId == primaryid) {
          primaryid = 0;
        } else {
          secondaryid = 0;
        }
      }
      else if (action == TouchAction.MOVED) {
        //  Movements
        if (pp.PointerId == primaryid) {
          primaryMove.X = pp.Position.X * s;
          primaryMove.Y = pp.Position.Y * s;
        } else {
          secondaryMove.X = pp.Position.X * s;
          secondaryMove.Y = pp.Position.Y * s;
        }
      }
    }

  }
}
