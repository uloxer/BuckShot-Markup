/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.ComponentModel;
using Dapplo.Ini.Internationalization.Attributes;

namespace BuckShot.Base.Languages
{
    /// <summary>
    /// The texts of the [Welcome] section of buckshot.{ietf}.ini, the English text is shown per property.
    /// Generated from the language files, add a key to the en-US file and a property here.
    /// </summary>
    [IniLanguageSection("Welcome")]
    public interface IWelcomeLanguage : INotifyPropertyChanged
    {
        /// <summary>
        /// What happens with a capture is up to you: open it in the editor, save it, copy it to the clipboard, print it and more. You choose in the settings.
        /// </summary>
        string BasicsAfter { get; }

        /// <summary>
        /// Capture the whole screen
        /// </summary>
        string BasicsFullscreen { get; }

        /// <summary>
        /// Capture the same region again
        /// </summary>
        string BasicsLastregion { get; }

        /// <summary>
        /// Everything is also in the menu of the BuckShot icon, next to the clock.
        /// </summary>
        string BasicsMenu { get; }

        /// <summary>
        /// Select a region or a window
        /// </summary>
        string BasicsRegion { get; }

        /// <summary>
        /// Capturing takes one key
        /// </summary>
        string BasicsTitle { get; }

        /// <summary>
        /// Capture the active window
        /// </summary>
        string BasicsWindow { get; }

        /// <summary>
        /// Back
        /// </summary>
        string ButtonBack { get; }

        /// <summary>
        /// Next
        /// </summary>
        string ButtonNext { get; }

        /// <summary>
        /// Open the settings
        /// </summary>
        string ButtonSettings { get; }

        /// <summary>
        /// Let's go!
        /// </summary>
        string ButtonStart { get; }

        /// <summary>
        /// That's all you need to start. BuckShot waits next to the clock, and everything else can be changed in the settings whenever you like.
        /// </summary>
        string Done { get; }

        /// <summary>
        /// You're all set
        /// </summary>
        string DoneTitle { get; }

        /// <summary>
        /// In a few steps you'll see how capturing works, and BuckShot checks that everything is set up well.
        /// </summary>
        string Intro { get; }

        /// <summary>
        /// OneDrive takes the Print key to save screenshots itself.
        /// </summary>
        string SetupOnedrive { get; }

        /// <summary>
        /// Let BuckShot have the Print key
        /// </summary>
        string SetupOnedriveFix { get; }

        /// <summary>
        /// The Print key starts a capture with BuckShot.
        /// </summary>
        string SetupPrintOk { get; }

        /// <summary>
        /// The Print key opens the Windows Snipping Tool instead of BuckShot.
        /// </summary>
        string SetupSnipping { get; }

        /// <summary>
        /// Let BuckShot have the Print key
        /// </summary>
        string SetupSnippingFix { get; }

        /// <summary>
        /// What BuckShot needs to work as expected, with a fix where something is off
        /// </summary>
        string SetupSub { get; }

        /// <summary>
        /// Your setup
        /// </summary>
        string SetupTitle { get; }

        /// <summary>
        /// Show it next to the clock
        /// </summary>
        string SetupTrayFix { get; }

        /// <summary>
        /// Windows hides the BuckShot icon behind the arrow (^) next to the clock. BuckShot can show it next to the clock for you, or you can switch it on yourself under &quot;Other system tray icons&quot; in the taskbar settings.
        /// </summary>
        string SetupTrayHidden { get; }

        /// <summary>
        /// The BuckShot icon is shown next to the clock.
        /// </summary>
        string SetupTrayOk { get; }

        /// <summary>
        /// BuckShot is checking for a newer version…
        /// </summary>
        string SetupVersionChecking { get; }

        /// <summary>
        /// Download it
        /// </summary>
        string SetupVersionDownload { get; }

        /// <summary>
        /// You have the latest version of BuckShot, {0}.
        /// </summary>
        string SetupVersionOk { get; }

        /// <summary>
        /// BuckShot {0} is available, you have {1}.
        /// </summary>
        string SetupVersionOld { get; }

        /// <summary>
        /// BuckShot couldn't check for a newer version right now.
        /// </summary>
        string SetupVersionUnknown { get; }

        /// <summary>
        /// Step {0} of {1}
        /// </summary>
        string StepOf { get; }

        /// <summary>
        /// Thank you for installing BuckShot! It's running now and waits for you in the notification area, next to the clock.
        /// </summary>
        string Thanks { get; }

        /// <summary>
        /// While capturing, press Space to switch between selecting a region and capturing a window.
        /// </summary>
        string Tip1 { get; }

        /// <summary>
        /// While capturing, the arrow keys move the cursor one pixel at a time, with Ctrl ten pixels.
        /// </summary>
        string Tip2 { get; }

        /// <summary>
        /// Press Z while capturing to show or hide the zoomer, for selections to the pixel.
        /// </summary>
        string Tip3 { get; }

        /// <summary>
        /// Press M while capturing to show or hide the mouse cursor in the screenshot.
        /// </summary>
        string Tip4 { get; }

        /// <summary>
        /// Press F1 while capturing to see all the keys you can use.
        /// </summary>
        string Tip5 { get; }

        /// <summary>
        /// Changed your mind? Esc cancels a capture.
        /// </summary>
        string Tip6 { get; }

        /// <summary>
        /// The editor can pixelate or blur the parts of a screenshot nobody else should see.
        /// </summary>
        string Tip7 { get; }

        /// <summary>
        /// Number the steps of a how-to with the counter of the editor.
        /// </summary>
        string Tip8 { get; }

        /// <summary>
        /// In the settings you choose the folder and the file name pattern of saved screenshots.
        /// </summary>
        string Tip9 { get; }

        /// <summary>
        /// BuckShot is free and open source, made by volunteers. If it helps you, tell your friends about it.
        /// </summary>
        string Tip10 { get; }

        /// <summary>
        /// Another tip
        /// </summary>
        string TipNext { get; }

        /// <summary>
        /// Clicked away a notification of BuckShot? It waits in the Windows notification center: a click on it lets you choose again where the capture goes, even after BuckShot was closed.
        /// </summary>
        string TipNotification { get; }

        /// <summary>
        /// Did you know?
        /// </summary>
        string TipTitle { get; }

        /// <summary>
        /// Welcome to BuckShot
        /// </summary>
        string Title { get; }
    }
}
