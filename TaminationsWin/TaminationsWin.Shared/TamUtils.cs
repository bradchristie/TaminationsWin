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
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace TaminationsWin {

  public class CallListDatum {
    public string title;
    public string text;
    public string link;
    public string sublevel;
    public string languages;
    public CallListDatum(string title, string text, string link, string sublevel, string languages) {
      this.title = title;
      this.text = text;
      this.link = link;
      this.sublevel = sublevel;
      this.languages = languages;
    } 
  }

  static class TamUtils {

    static XmlDocument fdoc = null;
    static XmlDocument mdoc = null;
    public static List<CallListDatum> calllistdata;


    public static async void init() {
      //  Read the global list of calls and save in a local list
      //  to speed up searching
      var index = await getXMLAssetAsync("src/callindex.xml");
      var nodelist = index.SelectNodes("/calls/call[@level!='Info']");
      calllistdata = nodelist.Select(n => new CallListDatum(
        n.attr("title"),
        n.attr("text"),
        n.attr("link"),
        n.attr("sublevel"),
        n.attr("languages"))).ToList();
      //  Read other definition files
      fdoc = await getXMLAssetAsync("src/formations.xml");
      mdoc = await getXMLAssetAsync("src/moves.xml");
    }

    /**
     *   Convenience method to retrieve a XML document
     * @param name file name
     * @return document
     */
    private static async Task<XmlDocument> getXMLAssetAsync(String name) {
      var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
      var file = await folder.GetFileAsync(@"Assets\" + name.ReplaceAll("ms/", "ms0/").Replace('/', '\\').ReplaceAll(@"\..*", "") + ".xml");
      return await XmlDocument.LoadFromFileAsync(file, new XmlLoadSettings { ProhibitDtd = false });
    }
    public static XmlDocument getXMLAsset(String name) {
      var task = Task.Run(() => getXMLAssetAsync(name));
      return task.Result;
    }

    /**
     *    Returns list of animations from an xml document
     */
    public static XmlNodeList tamList(XmlDocument xdoc) {
      return xdoc.SelectNodes("//tam | //tamxref");
    }

    /**
     *  Returns animation element, looking up cross-reference if needed.
     */
    public static IXmlNode tamXref(IXmlNode tam) {
      if (tam.hasAttr("xref-link")) {
        var link = tam.attr("xref-link") + ".xml";
        var xdoc = getXMLAsset(link);
        var s = "//tam";
        if (tam.hasAttr("xref-title"))
          s += "[@title='" + tam.attr("xref-title") + "']";
        if (tam.hasAttr("xref-from"))
          s += "[@from='" + tam.attr("xref-from") + "']";
        return xdoc.SelectNodes(s).First();
      } else {
        return tam;
      }
    }

    //  Return the main title from an animation xml doc
    public static string getTitle(string link) {
      var doc = getXMLAsset(link);
      var tamination = doc.SelectNodes("tamination").ElementAt(0);
      return tamination.attr("title");
    }

    public static IXmlNode getFormation(string fname) {
      return fdoc.SelectNodes($"/formations/formation[@name='{fname}']").First();
    }

    //  From a tam element, look up any cross-references, then
    //  return all the processed paths
    public static List<Path> getPaths(IXmlNode tam) {
      return (tamXref(tam)).SelectNodes("path").Select(m => new Path(translatePath(m))).ToList();
    }

    public static List<Movement> translate(IXmlNode elem) {
      switch (elem.NodeName) {
        case "path" : return translatePath(elem);
        case "move" : return translateMove(elem);
        case "movement" : return translateMovement(elem);
        default : return null;
      }
    }

    //  Takes a path, which is an XML element with children that
    //  are moves or movements.
    //  Returns an array of movements
    public static List<Movement> translatePath(IXmlNode pathelem) {
      var nodelist = pathelem.SelectNodes("*");
      return nodelist.SelectMany(translate).ToList();
    }

    //  Accepts a movement element from a XML file, either an animation definition
    //  or moves.xml
    //  Returns an array of a single movement
    public static List<Movement> translateMovement(IXmlNode move) {
      var list = new List<Movement>();
      list.Add(new Movement(move));
      return list;
    }

    //  Takes a move, which is an XML element that references another XML
    //  path with its "select" attribute
    //  Returns an array of movements
    public static List<Movement> translateMove(IXmlNode move) {
      //  First retrieve the requested path
      var movename = move.attr("select");
      var pathelem = mdoc.SelectNodes($"//path[@name='{movename}']").First();
      //  Get the list of movements
      var movements = translatePath(pathelem);
      //  Get any modifications
      var scaleX = move.hasAttr("scaleX") ? double.Parse(move.attr("scaleX")) : 1;
      var scaleY = (move.hasAttr("scaleY") ? double.Parse(move.attr("scaleY")) : 1) *
                   (move.hasAttr("reflect") ? -1 : 1);
      var offsetX = move.hasAttr("offsetX") ? double.Parse(move.attr("offsetX")) : 0;
      var offsetY = move.hasAttr("offsetY") ? double.Parse(move.attr("offsetY")) : 0;
      var hands = move.attr("hands");
      //  Sum up the total beats so if beats is given as a modification
      //  we know how much to change each movement
      var oldbeats = movements.Sum(m => m.beats);
      var beatfactor = move.hasAttr("beats") ? double.Parse(move.attr("beats"))/oldbeats : 1;
      //  Now go through the movements applying the modifications
      //  The resulting list is the return value
      return movements.Select(m => m.useHands(hands.Length > 0 ? Movement.getHands(hands) : m.hands)
                                    .scale(scaleX, scaleY)
                                    .skew(offsetX, offsetY)
                                    .time(m.beats * beatfactor)).ToList();
    }

    /**
     *   Gets a named path (move) from the file of moves
     */
    public static Path getMove(string name) {
      return new Path(translate(mdoc.SelectNodes($"/moves/path[@name='{name}']").First()));
    }

    /**
     *   Returns an array of numbers to use numering the dancers
     */
    public static string[] getNumbers(IXmlNode tam) {
      var paths = tam.SelectNodes("path");
      var retval = new string[paths.Count * 2];
      var np = Math.Min(paths.Count, 4);
      for (int i = 0; i < paths.Count; i++) {
        var p = paths.ElementAt(i);
        var n = p.attr("numbers");
        if (n.Length >= 3) { // numbers supplied in animation XML
          retval[i * 2] = n.Substring(0, 1);
          retval[i * 2 + 1] = n.Substring(2, 1);
        } else if (i > 3) { // phantoms
          retval[i * 2] = " ";
          retval[i * 2 + 1] = " ";
        } else {   // default numbers
          retval[i * 2] = (i + 1).ToString();
          retval[i * 2 + 1] = (i + 1 + np).ToString();
        }
      }
      return retval;
    }

    public static string[] getCouples(IXmlNode tam) {
      var retval = new string[] { "1","3","1","3",
                                  "2","4","2","4",
                                  "5","6","5","6",
                                  " "," "," "," " };
      var paths = tam.SelectNodes("path");
      for (int i=0; i<paths.Count; i++) {
        var c = paths.ElementAt(i).attr("couples");
        if (c.Length > 0) {
          retval[i * 2] = c.Substring(0, 1);
          retval[i * 2 + 1] = c.Substring(2, 1);
        }
      }
      return retval;
    }

    public static string callnameQuery(string query) {
      return query.ToLower()
    //  Use upper case and dup numbers while building regex
    //  so expressions don't get compounded
    //  Through => Thru
    .ReplaceAll("\\bthrou?g?h?\\b", "THRU")
    //  Process fractions 1/2 3/4 1/4 2/3
    .ReplaceAll("\\b1/2|(one.)?half\\b", "(HALF|1122)")
    .ReplaceAll("\\b(three.quarters?|3/4)\\b", "(THREEQUARTERS|3344)")
    .ReplaceAll("\\b((one.)?quarter|1/4)\\b", "((ONE)?QUARTER|1144)")
    .ReplaceAll("\\btwo.thirds?\\b", "(TWOTHIRDS|2233)")
    //  One and a half
    .ReplaceAll("\\b1.5\\b", "ONEANDAHALF")
    //  Process any other numbers
    .ReplaceAll("\\b(1|onc?e)\\b", "(11|ONE)")
    .ReplaceAll("\\b(2|two)\\b", "(22|TWO)")
    .ReplaceAll("\\b(3|three)\\b", "(33|THREE)")
    .ReplaceAll("\\b(4|four)\\b", "(44|FOUR)")
    .ReplaceAll("\\b(5|five)\\b", "(55|FIVE)")
    .ReplaceAll("\\b(6|six)\\b", "(66|SIX)")
    .ReplaceAll("\\b(7|seven)\\b", "(77|SEVEN)")
    .ReplaceAll("\\b(8|eight)\\b", "(88|EIGHT)")
    .ReplaceAll("\\b(9|nine)\\b", "(99|NINE)")
    //  Accept single and plural forms of some words
    .ReplaceAll("\\bboys?\\b", "BOYS?")
    .ReplaceAll("\\bgirls?\\b", "GIRLS?")
    .ReplaceAll("\\bends?\\b", "ENDS?")
    .ReplaceAll("\\bcenters?\\b", "CENTERS?")
    .ReplaceAll("\\bheads?\\b", "HEADS?")
    .ReplaceAll("\\bsides?\\b", "SIDES")
    //  Accept optional "dancers" e.g. "head dancers" == "heads"
    .ReplaceAll("\\bdancers?\\b", "(DANCERS?)?")
    //  Misc other variations
    .ReplaceAll("\\bswap(\\s+around)?\\b", "SWAP (AROUND)?")

    //  Finally repair the upper case and dup numbers
    //  and make spaces optional
    .ToLower().ReplaceAll("([0-9])\\1", "$1").ReplaceAll("\\s+", "\\s*");
    }


  }

}
