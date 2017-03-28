
namespace TaminationsWin.Calls
{
  class HalfSashay : Action {

    public HalfSashay() { name = "Half Sashay"; }

    public override Path performOne(Dancer d,CallContext ctx) {
      if (ctx.isInCouple(d)) {
        return TamUtils.getMove(d.data.beau ? "BackSashay Right" : "Sashay Left");
      }
      throw new CallError("Only Couples can Half Sashay");
    }

  }
}
