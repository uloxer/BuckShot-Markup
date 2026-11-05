using System;
using System.IO;
using System.Linq;
using Greenshot.Base.Core;
using Greenshot.Base.Recipes;
using Greenshot.Ipc;
using Greenshot.Ipc.Cli;
using Greenshot.Recipes;
using Xunit;

namespace Greenshot.Tests
{
    public class BuckShotIdentityTests
    {
        [Fact]
        public void ProductVersionAndRuntimeStorageAreIndependentFromGreenshot()
        {
            Assert.Equal("BuckShot Markup", EditionInfo.ProductName);
            Assert.Equal("0.8.666", ProductIdentity.Version);
            Assert.Equal(ProductIdentity.Version, EnvironmentInfo.GetGreenshotVersion());
            Assert.Equal(ProductIdentity.Version, EnvironmentInfo.GetGreenshotVersion(true));
            Assert.Equal("buckshot-markup.ini", IniConfigHelper.NormalConfigName);
            Assert.EndsWith(Path.DirectorySeparatorChar + ProductIdentity.StorageName, GreenshotEnvironment.LocalDataDirectory);
            Assert.StartsWith("BuckShot-Markup_", NamedPipeEndpoint.GetPipeName());
            Assert.Equal(ProductIdentity.SourceUrl + "/releases", Greenshot.Helpers.UpdateService.DownloadsUri.AbsoluteUri);
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
