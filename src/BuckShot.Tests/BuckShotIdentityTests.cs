using System;
using System.IO;
using System.Linq;
using BuckShot.Base.Core;
using BuckShot.Base.Recipes;
using BuckShot.Ipc;
using BuckShot.Ipc.Cli;
using BuckShot.Recipes;
using Xunit;

namespace BuckShot.Tests
{
    public class BuckShotIdentityTests
    {
        [Fact]
        public void ExecutableFileDescriptionUsesBuckShotWindowsDisplayName()
        {
            var assembly = typeof(BuckShotMain).Assembly;
            var title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(System.Reflection.AssemblyTitleAttribute));
            Assert.Equal(ProductIdentity.Name, title.Title);
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Assert.Equal(ProductIdentity.Name, info.FileDescription);
            Assert.Equal(ProductIdentity.Name, info.ProductName);
        }

        [Fact]
        public void ProductVersionAndRuntimeStorageAreIndependentFromBuckShot()
        {
            Assert.Equal("BuckShot Markup", EditionInfo.ProductName);
            Assert.Equal("0.9.674", ProductIdentity.Version);
            Assert.Equal(ProductIdentity.Version, EnvironmentInfo.GetBuckShotVersion());
            Assert.Equal(ProductIdentity.Version, EnvironmentInfo.GetBuckShotVersion(true));
            Assert.Equal("buckshot-markup.ini", IniConfigHelper.NormalConfigName);
            Assert.EndsWith(Path.DirectorySeparatorChar + ProductIdentity.StorageName, BuckShotEnvironment.LocalDataDirectory);
            Assert.StartsWith("BuckShot-Markup_", NamedPipeEndpoint.GetPipeName());
            Assert.Equal(ProductIdentity.SourceUrl + "/releases", BuckShot.Helpers.UpdateService.DownloadsUri.AbsoluteUri);
        }

        [Theory]
        [InlineData("LIST_RECIPES")]
        [InlineData("RUN_RECIPE")]
        [InlineData("DESCRIBE_RECIPE")]
        [InlineData("INSTALL_RECIPE")]
        [InlineData("RECIPE_EDITOR")]
        [InlineData("RECIPE_MANAGER")]
        [InlineData("RECIPE_CATALOG")]
        [InlineData("VALIDATE_RECIPE")]
        [InlineData("PROPOSE_RECIPE")]
        public void PublicRecipeCommandsAreRejectedForEveryExternalSource(string command)
        {
            foreach (var source in new[] { IpcSources.Cli, IpcSources.Mcp, IpcSources.UrlScheme, IpcSources.OpenWith })
                Assert.False(IpcSecurityDispatcher.IsCommandAllowedForSource(command, source));
        }

        [Fact]
        public void RecipeAuthoringIsDisabledButBuiltInCapturePipelineRemainsAvailable()
        {
            TestEnvironment.EnsureInitialized();
            Assert.False(RecipeConfigHelper.IsRecipeFeatureEnabled());
            Assert.False(RecipeManager.RecipeFilesSupported);
            Assert.Contains(RecipeManager.Instance.GetAllRecipes(), recipe => recipe.IsBuiltIn);
            Assert.True(IpcSecurityDispatcher.IsCommandAllowedForSource("OPEN_FILE", IpcSources.Cli));
            Assert.Null(CliCommandParser.Parse(new[] { "--recipe", "anything" }, IpcSources.Cli, Directory.GetCurrentDirectory()).Envelope);
        }
    }
}
