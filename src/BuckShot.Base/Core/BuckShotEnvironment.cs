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

using System;
using System.IO;

namespace BuckShot.Base.Core
{
    /// <summary>
    /// Holds runtime environment state for BuckShot that is determined at startup
    /// and shared across the application.
    /// </summary>
    public static class BuckShotEnvironment
    {
        /// <summary>
        /// Gets a value indicating whether BuckShot is running in PortableApp (PAF) mode.
        /// Set to <c>true</c> when the standard PortableApp directory structure
        /// (<c>App\BuckShot</c> next to the executable) is detected at startup.
        /// </summary>
        public static bool IsPortable { get; set; }

        /// <summary>
        /// Gets the full path of the active <c>buckshot.ini</c> configuration file.
        /// Resolved lazily from <see cref="Dapplo.Ini.IniConfigRegistry"/> so that
        /// it is correct even when read before the initial <c>Load()</c> call completes.
        /// </summary>
        public static string ConfigLocation => Dapplo.Ini.IniConfigRegistry.Get()?.LoadedFromPath ?? string.Empty;

        /// <summary>
        /// The directory for the files BuckShot keeps on this computer: %LocalAppData%\BuckShot
        /// </summary>
        public static string LocalDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProductIdentity.StorageName);

        /// <summary>
        /// True for the PortableApp and the .zip (buckshot.ini next to BuckShot.exe), these leave nothing on the system
        /// </summary>
        public static bool LeavesNoTraces
        {
            get
            {
                string configLocation = ConfigLocation;
                return IsPortable || (!string.IsNullOrEmpty(configLocation)
                    && string.Equals(Path.GetDirectoryName(configLocation)?.TrimEnd('\\'), AppContext.BaseDirectory.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
