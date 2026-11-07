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
using BuckShot.Base.Core;
using BuckShot.Base.Core.FileFormat;
using BuckShot.Base.Interfaces;
using BuckShot.Base.Recipes.Pipeline;
using BuckShot.Base.Threading;
using BuckShot.Editor;
using BuckShot.Editor.Configuration;
using BuckShot.Editor.Drawing;
using BuckShot.Base.Languages;

namespace BuckShot.Tests
{
    public static class TestEnvironment
    {
        private static readonly object SyncLock = new object();
        private static bool _initialized;

        public static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (SyncLock)
            {
                if (_initialized) return;

                IniConfigHelper.EnsureInitialized();
                IniConfigHelper.EnsureSection<IEditorConfiguration>(() => new EditorConfigurationImpl());
                IniConfigHelper.EnsureSection<BuckShot.Plugin.ExternalCommand.IExternalCommandConfiguration>(() => new BuckShot.Plugin.ExternalCommand.ExternalCommandConfigurationImpl());

                var supportedFileFormatRegistry = SimpleServiceProvider.Current.GetInstance<IFileFormatRegistry>(true);
                if (supportedFileFormatRegistry == null)
                {
                    supportedFileFormatRegistry = new FileFormatRegistry();
                    SimpleServiceProvider.Current.AddService<IFileFormatRegistry>(supportedFileFormatRegistry);
                }
                CoreFileFormats.RegisterCoreFileFormats(supportedFileFormatRegistry);

                // Like CapturePipeline: a Surface created on a pool thread must not install the WinForms SynchronizationContext there,
                // an await in the test would continue on a message loop which never runs (xunit.v3 runs the tests without a context)
                CapturePayload.DefaultSurfaceFactory = capture => WinFormsContextGuard.CreateWithoutContext<ISurface>(() => new Surface(capture) { Modified = true });

                if (SimpleServiceProvider.Current.GetInstance<Func<ISurface>>(isOptional: true) == null)
                {
                    SimpleServiceProvider.Current.AddService<Func<ISurface>>(() => WinFormsContextGuard.CreateWithoutContext<ISurface>(() => new Surface()));
                }

                EditorInitialize.Initialize();
                // The tests check English texts, whatever the language of Windows is
                Texts.SystemLanguage = "en-US";
                Texts.SetLanguage("en-US");
                // The plugins register their texts when they start, the tests need them without starting the plugins
                Texts.Register<BuckShot.Plugin.Box.IBoxLanguage>(new BuckShot.Plugin.Box.BoxLanguageImpl());
                Texts.Register<BuckShot.Plugin.Atlassian.Confluence.IConfluenceLanguage>(new BuckShot.Plugin.Atlassian.Confluence.ConfluenceLanguageImpl());
                Texts.Register<BuckShot.Plugin.CustomUploader.ICustomUploaderLanguage>(new BuckShot.Plugin.CustomUploader.CustomUploaderLanguageImpl());
                Texts.Register<BuckShot.Plugin.Dropbox.IDropboxLanguage>(new BuckShot.Plugin.Dropbox.DropboxLanguageImpl());
                Texts.Register<BuckShot.Plugin.ExternalCommand.IExternalCommandLanguage>(new BuckShot.Plugin.ExternalCommand.ExternalCommandLanguageImpl());
                Texts.Register<BuckShot.Plugin.Atlassian.Jira.IJiraLanguage>(new BuckShot.Plugin.Atlassian.Jira.JiraLanguageImpl());
                Texts.Register<BuckShot.Plugin.Office.IOfficeLanguage>(new BuckShot.Plugin.Office.OfficeLanguageImpl());
                Texts.Register<BuckShot.Plugin.Pdf.IPdfLanguage>(new BuckShot.Plugin.Pdf.PdfLanguageImpl());
                Texts.Register<BuckShot.Plugin.RecipeEditor.IRecipeEditorLanguage>(new BuckShot.Plugin.RecipeEditor.RecipeEditorLanguageImpl());
                Texts.Register<BuckShot.Plugin.Stamps.IStampsLanguage>(new BuckShot.Plugin.Stamps.StampsLanguageImpl());
                Texts.Register<BuckShot.Plugin.Zxing.IZxingLanguage>(new BuckShot.Plugin.Zxing.ZxingLanguageImpl());

                _initialized = true;
            }
        }
    }
}
