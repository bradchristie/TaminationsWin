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

using System.Text.RegularExpressions;

namespace TaminationsWin.Calls
{
  abstract class CodedCall : Call
  {

    delegate CodedCall CodedCallCreator(string callname);
    struct CallKey {
      public string regex;
      public CodedCallCreator creator;
      public CallKey(string regex, CodedCallCreator creator) {
        this.regex = regex;
        this.creator = creator;
      }
    }

    static CallKey[] codedCalls = {
      new CallKey("allemande left", callname => new AllemandeLeft()),
      new CallKey("and roll", callname => new Roll()),
      new CallKey("and spread", callname => new Spread()),
      new CallKey("beaus", callname => new Beaus()),
      new CallKey("belles", callname => new Belles()),
      new CallKey("box counter rotate", callname => new BoxCounterRotate()),
      new CallKey("box the gnat", callname => new BoxtheGnat()),
      new CallKey("boys?", callname => new Boys()),
      new CallKey("centers?", callname => new Centers()),
      new CallKey("circulate", callname => new Circulate()),
      new CallKey("cross run", callname => new CrossRun()),
      new CallKey("ends?", callname => new Ends()),
      new CallKey("explode and", callname => new ExplodeAnd()),
      new CallKey("face (in|out|left|right)", callname => new FaceIn(callname)),
      new CallKey("girls?", callname => new Girls()),
      new CallKey("half", callname => new Half()),
      new CallKey("heads?", callname => new Heads()),
      new CallKey("hinge", callname => new Hinge()),
      new CallKey("leaders?", callname => new Leaders()),
      new CallKey("left touch a quarter", callname => new LeftTouchAQuarter()),
      new CallKey("(onc?e and a half)|(1 1/2)", callname => new OneAndaHalf()),
      new CallKey("pass thru", callname => new PassThru()),
      new CallKey("quarter (in|out)", callname => new QuarterIn(callname)),
      new CallKey("run", callname => new Run()),
      new CallKey("sides?", callname => new Sides()),
      new CallKey("slide thru", callname => new SlideThru()),
      new CallKey("slip", callname => new Slip()),
      new CallKey("star thru", callname => new StarThru()),
      new CallKey("(left )?touch a quarter", callname => new TouchAQuarter(callname)),
      new CallKey("trade", callname => new Trade()),
      new CallKey("trailers?", callname => new Trailers()),
      new CallKey("turn back", callname => new TurnBack()),
      new CallKey("turn thru", callname => new TurnThru()),
      new CallKey("very centers", callname => new VeryCenters()),
      new CallKey("wheel around", callname => new WheelAround()),
      new CallKey("z[ai]g", callname => new Zig(callname)),
      new CallKey("z[ai]g z[ai]g", callname => new ZigZag(callname)),
      new CallKey("zoom", callname => new Zoom()),
    };

    public static CodedCall getCodedCall(string calltext) {
      var callname = calltext.ToLower();
      for (int i = 0; i<codedCalls.Length; i++)
        if (Regex.Match(callname,"^("+codedCalls[i].regex+")$").Success)
          return codedCalls[i].creator(callname);
      return null;
    }

    public override void postProcess(CallContext ctx,int i = 0) {
      base.postProcess(ctx,i);
      ctx.matchStandardFormation();
    }

  }
}
