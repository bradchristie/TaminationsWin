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

namespace TaminationsWin.Calls
{
  abstract class CodedCall : Call
  {

    public static CodedCall getCodedCall(string callname) {
      switch (callname.ToLower()) {
        case "allemande left": return new AllemandeLeft();
        case "and roll": return new Roll();
        case "and spread": return new Spread();
        case "beaus": return new Beaus();
        case "belles": return new Belles();
        case "box counter rotate": return new BoxCounterRotate();
        case "box the gnat": return new BoxtheGnat();
        case "boys": return new Boys();
        case "centers": return new Centers();
        case "circulate": return new Circulate();
        case "cross run": return new CrossRun();
        case "ends": return new Ends();
        case "explode and": return new ExplodeAnd();
        case "face in": return new FaceIn();
        case "face left": return new FaceLeft();
        case "face out": return new FaceOut();
        case "face right": return new FaceRight();
        case "girls": return new Girls();
        case "half": return new Half();
        case "hinge": return new Hinge();
        case "leaders": return new Leaders();
        case "one and a half": return new OneAndaHalf();
        case "pass thru": return new PassThru();
        case "quarter in": return new QuarterIn();
        case "quarter out": return new QuarterOut();
        case "run": return new Run();
        case "slide thru": return new SlideThru();
        case "slip": return new Slip();
        case "star thru": return new StarThru();
        case "touch a quarter": return new TouchAQuarter();
        case "trade": return new Trade();
        case "trailers": return new Trailers();
        case "turn back": return new TurnBack();
        case "turn thru": return new TurnThru();
        case "very centers": return new VeryCenters();
        case "wheel around": return new WheelAround();
        case "zig": return new Zig();
        case "zag": return new Zag();
        case "zig zig": return new ZigZig();
        case "zig zag": return new ZigZag();
        case "zag zig": return new ZagZig();
        case "zag zag": return new ZagZag();
        case "zoom": return new Zoom();
        default: return null;
      }
    }

    public override void postProcess(CallContext ctx,int i = 0) {
      base.postProcess(ctx,i);
      ctx.matchStandardFormation();
    }

  }
}
