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

namespace TaminationsWin
{
  public struct Callouts
  {
    public static Action<string> SetTitle = (string title) => { };
    public static Action<string> SetLevel = (string level) => { };
    public static Action<double> progressCallback = (double beat) => { };
    public static Action settingsChanged = () => { };
    public static Action<string> LevelButtonAction = (string level) => { };
    public static Action AboutAction = () => { };
    public static Action LevelSettingsAction = () => { };
    public static Action<string> CallSelected = (string link) => { };
    public static Action<AnimListItem> AnimationSelected = (AnimListItem item) => { };
    public static Action<AnimListItem> FirstAnimationReady = (AnimListItem item) => { };
    public static Action AnimationSettingsAction = () => { };
    public static Action<string> AnimationDefinitionAction = (string link) => { };
    public static Action AnimationFinished = () => { };
    public static Action<int> AnimationPart = (int part) => { };
  }

}
