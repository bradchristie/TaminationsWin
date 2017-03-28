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

using Windows.UI;

namespace TaminationsWin {

  public class LevelData {
    public string name;
    public string dir;
    public string selector;
    public Color color;
    public LevelData(string name, string dir, string selector, uint color) {
      this.name = name;
      this.dir = dir;
      this.selector = selector;
      this.color = ColorUtilities.ColorFromHex(color);
    }

    public static LevelData find(string what) {
      LevelData retval = null;
      foreach (LevelData d in data) {
        if (d.name == what || d.dir == what)
          retval = d;
      }
      return retval;
    }

    static LevelData[] data = {
    new LevelData("Basic and Mainstream","bms","level='Basic and Mainstream' and @sublevel!='Styling'",0xffc0c0ff),
    new LevelData("Basic 1","b1","sublevel='Basic 1'",0xffe0e0ff),
    new LevelData("Basic 2","b2","sublevel='Basic 2'",0xffe0e0ff),
    new LevelData("Mainstream","ms","sublevel='Mainstream'",0xffe0e0ff),
    new LevelData("Plus","plus","level='Plus'",0xffc0ffc0),
    new LevelData("Advanced","adv","level='Advanced'",0xffffe080),
    new LevelData("A-1","a1","sublevel='A-1'",0xfffff0c0),
    new LevelData("A-2","a2","sublevel='A-2'",0xfffff0c0),
    new LevelData("Challenge","challenge","level='Challenge'",0xffffc0c0),
    new LevelData("C-1","c1","sublevel='C-1'",0xffffe0e0),
    new LevelData("C-2","c2","sublevel='C-2'",0xffffe0e0),
    new LevelData("C-3A","c3a","sublevel='C-3A'",0xffffe0e0),
    new LevelData("C-3B","c3b","sublevel='C-3B'",0xffffe0e0),
    new LevelData("All Calls","all","level!='Info' and @sublevel!='Styling'",0xffc0c0c0),
    new LevelData("Index of All Calls","all","level!='Info' and @sublevel!='Styling'",0xffc0c0c0)
    };

  }
}
