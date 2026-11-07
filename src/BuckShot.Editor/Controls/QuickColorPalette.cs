using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BuckShot.Editor.Configuration;
using BuckShot.Editor.Drawing.Fields;
using BuckShot.Base.Interfaces.Drawing;

namespace BuckShot.Editor.Controls
{
    internal sealed class QuickColorPalette : IDisposable
    {
        private readonly IEditorConfiguration _configuration;
        private readonly ToolStrip _toolbar;
        private bool _hasLine = true, _hasFill = true, _refreshing, _disposed;
        private Size _swatchSize;
        private readonly ToolStripDropDownButton _target = new ToolStripDropDownButton("Line");
        private readonly ToolStripMenuItem _line = new ToolStripMenuItem("Line"), _fill = new ToolStripMenuItem("Fill");
        private readonly List<ToolStripItem> _items = new List<ToolStripItem>();
        private readonly List<ToolStripButton> _recent = new List<ToolStripButton>();
        public IFieldType Target { get; private set; } = FieldType.LINE_COLOR;
        public event Action<Color> ColorSelected;

        public QuickColorPalette(ToolStrip toolbar, IEditorConfiguration configuration)
        {
            _configuration = configuration;
            _toolbar = toolbar;
            _swatchSize = toolbar.ImageScalingSize;
            _target.ToolTipText = "Quick colour target";
            _target.DropDownItems.AddRange(new ToolStripItem[] { _line, _fill });
            _line.Click += (s, e) => SelectTarget(FieldType.LINE_COLOR);
            _fill.Click += (s, e) => SelectTarget(FieldType.FILL_COLOR);
            _items.Add(new ToolStripSeparator());
            _items.Add(_target);
            foreach (var color in new[] { Color.Black, Color.White, Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Magenta })
            {
                var swatch = CreateSwatch();
                SetColor(swatch, color);
                _items.Add(swatch);
            }
            _items.Add(new ToolStripSeparator());
            for (int i = 0; i < 6; i++)
            {
                var swatch = CreateSwatch();
                swatch.Available = false;
                _recent.Add(swatch);
                _items.Add(swatch);
            }
            toolbar.SuspendLayout();
            try
            {
                toolbar.Items.AddRange(_items.ToArray());
                SelectTarget(FieldType.LINE_COLOR);
                toolbar.Layout += ToolbarLayout;
                Refresh(true, true);
            }
            finally { toolbar.ResumeLayout(); }
        }

        private void SelectTarget(IFieldType field)
        {
            Target = field;
            _target.Text = field == FieldType.FILL_COLOR ? "Fill" : "Line";
            _line.Checked = field == FieldType.LINE_COLOR;
            _fill.Checked = field == FieldType.FILL_COLOR;
        }

        private ToolStripButton CreateSwatch()
        {
            var button = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, AutoSize = true, Padding = Padding.Empty, Margin = new Padding(0, 1, 0, 1) };
            button.Click += (s, e) =>
            {
                if (button.Tag is Color color) ColorSelected?.Invoke(color);
            };
            return button;
        }

        private void SetColor(ToolStripButton button, Color color)
        {
            var size = _toolbar.ImageScalingSize;
            if (button.Tag is Color current && current.ToArgb() == color.ToArgb() && button.Image?.Size == size) return;
            var image = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                using var brush = new SolidBrush(color);
                graphics.FillRectangle(brush, 1, 1, image.Width - 2, image.Height - 2);
                graphics.DrawRectangle(Pens.Gray, 0, 0, image.Width - 1, image.Height - 1);
                if (color.A == 0) graphics.DrawLine(Pens.Red, 1, image.Height - 2, image.Width - 2, 1);
            }
            var old = button.Image;
            button.Image = image;
            old?.Dispose();
            button.Tag = color;
            button.ToolTipText = button.AccessibleName = ColorDescription.Describe(color);
        }

        public void Remember(Color color)
        {
            var colors = _configuration.RecentColors ?? new List<Color>();
            if (colors.Count > 0 && colors[0].ToArgb() == color.ToArgb()) return;
            colors.RemoveAll(c => c.ToArgb() == color.ToArgb());
            colors.Insert(0, color);
            _configuration.RecentColors = colors.Take(12).ToList();
            Refresh(_hasLine, _hasFill);
        }

        private void ToolbarLayout(object sender, LayoutEventArgs e)
        {
            if (!_disposed && !_refreshing && _swatchSize != _toolbar.ImageScalingSize)
                Refresh(_hasLine, _hasFill);
        }

        public void Refresh(bool hasLine, bool hasFill)
        {
            if (_disposed || _refreshing) return;
            _refreshing = true;
            _toolbar.SuspendLayout();
            try
            {
                _hasLine = hasLine;
                _hasFill = hasFill;
                _swatchSize = _toolbar.ImageScalingSize;
                _line.Enabled = hasLine;
                _fill.Enabled = hasFill;
                if (Target == FieldType.FILL_COLOR && !hasFill) SelectTarget(FieldType.LINE_COLOR);
                if (Target == FieldType.LINE_COLOR && !hasLine && hasFill) SelectTarget(FieldType.FILL_COLOR);
                bool available = hasLine || hasFill;
                var colors = (_configuration.RecentColors ?? new List<Color>()).GroupBy(c => c.ToArgb()).Select(g => g.First()).Take(6).ToList();
                foreach (var item in _items)
                {
                    int recentIndex = item is ToolStripButton recent ? _recent.IndexOf(recent) : -1;
                    bool show = available && (recentIndex < 0 || recentIndex < colors.Count);
                    if (item.Available != show) item.Available = show;
                    if (item is ToolStripButton swatch)
                    {
                        if (recentIndex >= 0 && recentIndex < colors.Count) SetColor(swatch, colors[recentIndex]);
                        else if (recentIndex < 0 && swatch.Tag is Color color) SetColor(swatch, color);
                    }
                }
            }
            finally
            {
                _toolbar.ResumeLayout();
                _refreshing = false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _toolbar.Layout -= ToolbarLayout;
            _toolbar.SuspendLayout();
            try
            {
                foreach (var item in _items)
                {
                    var image = item.Image;
                    item.Image = null;
                    image?.Dispose();
                    if (!_toolbar.IsDisposed) _toolbar.Items.Remove(item);
                    item.Dispose();
                }
            }
            finally { _toolbar.ResumeLayout(); }
        }
    }
}
