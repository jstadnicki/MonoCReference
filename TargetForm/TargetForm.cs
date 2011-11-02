using System.Windows.Forms;

namespace TargetForm
{
    public partial class TargetForm : Form
    {
        public TargetForm()
        {
            InitializeComponent();
        }

        private void TargetForm_MouseDown(object sender, MouseEventArgs e)
        {
            this.targetStatus.Text = string.Format("down: {0}", e.Button.ToString());
        }

        private void TargetForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.targetStatus.Text = string.Format("up: {0}", e.Button.ToString());
        }
    }
}
