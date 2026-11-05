using System.Collections.Generic;
using System.Drawing;
using Dapplo.Windows.Dpi;
using Greenshot.Base.Wpf;

namespace Greenshot.Editor.Forms
{
    public partial class ImageEditorForm
    {
        private HashSet<Image> _buckShotImages = new HashSet<Image>();

        private void BuckShotThemeChanged()
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            BeginInvoke(new System.Windows.Forms.MethodInvoker(RefreshBuckShotIcons));
        }

        private Image GetEditorImage(string name)
        {
            var size = DpiCalculator.ScaleWithDpi(coreConfiguration.IconSize, DeviceDpi);
            // WPF settings may use a dark theme while WinForms toolbar chrome remains light.
            var background = toolsToolStrip?.BackColor ?? SystemColors.Control;
            var image = EditorIconFactory.Create(name, size, background.GetBrightness() < .5f);
            _buckShotImages.Add(image);
            return image;
        }

        private void RefreshBuckShotIcons()
        {
            if (IsDisposed || Disposing) return;
            var previous = _buckShotImages;
            _buckShotImages = new HashSet<Image>();
            ApplyImages();
            btnEmoji.Image = GetEditorImage("emoji.Image");
            if (_polygonButton != null) _polygonButton.Image = GetEditorImage("btnPolygon.Image");
            if (_groupButton != null) _groupButton.Image = GetEditorImage("btnGroup.Image");
            if (_ungroupButton != null) _ungroupButton.Image = GetEditorImage("btnUngroup.Image");
            if (_rotateObjectButton != null) _rotateObjectButton.Image = GetEditorImage("btnObjectRotate.Image");
            if (_scaleButton != null) _scaleButton.Image = GetEditorImage("calibrate.Image");
            if (_stepLabelIconName != null)
            {
                _stepLabelIcon = GetEditorImage(_stepLabelIconName);
                btnStepLabel.Image = _stepLabelIcon;
                addCounterToolStripMenuItem.Image = _stepLabelIcon;
            }
            foreach (var image in previous) image.Dispose();
        }

        private void DisposeBuckShotIcons()
        {
            WpfThemeHelper.ThemeChanged -= BuckShotThemeChanged;
            foreach (var image in _buckShotImages) image.Dispose();
            _buckShotImages.Clear();
        }
    }
}
