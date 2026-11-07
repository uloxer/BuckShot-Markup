using System.Collections.Generic;
using System.Windows.Forms;

namespace BuckShot.Editor.Forms
{
    public partial class ImageEditorForm
    {
        // The designer, runtime tools, icons and translated labels all change preferred sizes.
        // Keep their layout pending until the complete initial control tree is ready.
        private List<Control> SuspendStartupLayout()
        {
            var controls = new List<Control>();
            void Suspend(Control control)
            {
                control.SuspendLayout();
                controls.Add(control);
                foreach (Control child in control.Controls) Suspend(child);
            }
            Suspend(this);
            return controls;
        }

        private static void ResumeStartupLayout(List<Control> controls)
        {
            for (int index = controls.Count - 1; index >= 0; index--)
                controls[index].ResumeLayout(false);
            // Settle the children once before their containing panels and the form.
            for (int index = controls.Count - 1; index >= 0; index--)
                controls[index].PerformLayout();
        }
    }
}
