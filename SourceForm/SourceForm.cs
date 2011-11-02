using System.Windows.Forms;

namespace SourceForm
{
    public partial class SourceForm : Form
    {
        public SourceForm()
        {
            InitializeComponent();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.sourceStatus.Text = string.Format("I did forget to mention that {0} ends the game.", e.KeyCode.ToString());
            }
            else
            {
                this.sourceStatus.Text = string.Format("Key {0} (down).", e.KeyCode.ToString());
            }

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else
            {
                this.sourceStatus.Text = string.Format("Key {0} (up).", e.KeyCode.ToString());
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            this.sourceStatus.Text = string.Format("Mouse: {0} down", e.Button.ToString());
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            this.sourceStatus.Text = string.Format("Mouse: {0} up", e.Button.ToString());
        }
    }
}
