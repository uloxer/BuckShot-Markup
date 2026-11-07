using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.ServiceModel.Security;
using System.Text;
using BuckShot.Base.Interfaces.Drawing;
using BuckShot.Editor.Configuration;
using BuckShot.Editor.Drawing;
using BuckShot.Editor.Drawing.Fields;
using BuckShot.Editor.FileFormatHandlers;
using BuckShot.Editor.Helpers;
using Xunit;

namespace BuckShot.Tests.Editor;

public class LegacyProjectCompatibilityTests
{
    public LegacyProjectCompatibilityTests() => TestEnvironment.EnsureInitialized();
    private static string Fixture(string extension) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "TestData", "LegacyProjects", "pre-rename-0.9.673." + extension);

    private static void CheckElements(IDrawableContainerList elements)
    {
        Assert.Equal(3, elements.Count);
        var box = Assert.IsType<RectangleContainer>(elements[0]);
        var polygon = Assert.IsType<PolygonContainer>(elements[1]);
        var image = Assert.IsType<ImageContainer>(elements[2]);
        Assert.Equal(Color.Aquamarine, box.GetFieldValueAsColor(FieldType.FILL_COLOR));
        Assert.NotEqual(Guid.Empty, box.GroupId);
        Assert.Equal(box.GroupId, polygon.GroupId);
        Assert.Equal(3, polygon.Vertices.Length);
        Assert.Equal(Color.Magenta.ToArgb(), ((Bitmap)image.Image).GetPixel(0, 0).ToArgb());
    }

    [Fact]
    public void PublishedLegacyProjectLoadsAndNewSaveUsesRenamedAssemblies()
    {
        DimensionFormatTests.Sta(() =>
        {
            var original = File.ReadAllBytes(Fixture("greenshot"));
            Assert.Contains("Greenshot.Editor", Encoding.UTF8.GetString(original));
            using var input = new MemoryStream(original);
            using var loaded = (Surface)new BuckShotFileFormatHandler().LoadSurface(input);
            CheckElements(loaded.Elements);
            using var output = new MemoryStream();
            Assert.True(new BuckShotFileFormatHandler().TrySaveToStream((Bitmap)loaded.Image, output, ".buckshot", loaded));
            Assert.Contains("BuckShot.Editor", Encoding.UTF8.GetString(output.ToArray()));
            output.Position = 0;
            using var roundtrip = (Surface)new BuckShotFileFormatHandler().LoadSurface(output);
            CheckElements(roundtrip.Elements);
        });
    }

    [Fact]
    public void PublishedLegacyClipboardAndCurrentClipboardRemainEditable()
    {
        DimensionFormatTests.Sta(() =>
        {
            var old = DrawableContainerClipboard.Deserialize(File.ReadAllBytes(Fixture("clipboard")));
            Assert.NotNull(old);
            CheckElements(old);
            var current = DrawableContainerClipboard.Deserialize(DrawableContainerClipboard.Serialize(old));
            Assert.NotNull(current);
            CheckElements(current);
        });
    }

    [Theory]
    [InlineData("Greenshot.Drawing.RectangleContainer", typeof(RectangleContainer))]
    [InlineData("Greenshot.Editor.Drawing.RectangleContainer", typeof(RectangleContainer))]
    [InlineData("Greenshot.Plugin.Drawing.IField", typeof(IField))]
    [InlineData("GreenshotPlugin.Interfaces.Drawing.IFieldHolder", typeof(IFieldHolder))]
    [InlineData("System.Collections.Generic.List`1[[Greenshot.Base.Interfaces.Drawing.IField, Greenshot.Base, Version=0.9.0.0]]", typeof(List<IField>))]
    [InlineData("System.Collections.Generic.List`1[[GreenshotPlugin.Interfaces.Drawing.IFieldHolder, GreenshotPlugin]]", typeof(List<IFieldHolder>))]
    public void HistoricalNamesResolveOnlyToTheExistingAllowlist(string name, Type expected)
    {
        Assert.Equal(expected, new BinaryFormatterHelper().BindToType("Greenshot.Editor", name));
    }

    [Fact]
    public void LegacySettingsValuesMigrateAndUnknownTypesStayRejected()
    {
        var converter = new BuckShotEditorObjectValueConverter();
        var status = converter.ConvertFromString("Greenshot.Base.Interfaces.Drawing.EditStatus,Greenshot.Base:IDLE");
        Assert.Equal(EditStatus.IDLE, status);
        Assert.StartsWith("BuckShot.Base.Interfaces.Drawing.EditStatus,BuckShot.Base:", converter.ConvertToString(status));
        Assert.Throws<SecurityAccessDeniedException>(() => new BinaryFormatterHelper().BindToType("Greenshot.Editor", "Greenshot.Editor.Drawing.UnapprovedPayload"));
        Assert.Throws<SecurityAccessDeniedException>(() => new BinaryFormatterHelper().BindToType("mscorlib", "System.Diagnostics.Process"));
    }
}
