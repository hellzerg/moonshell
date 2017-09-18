using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoonShell
{
    public partial class OptionsForm : Form
    {
        FontDialog fontDialog = new FontDialog();
        ColorDialog colorDialog = new ColorDialog();
        FontConverter c = new FontConverter();

        Color previewBackColor = Options.CurrentOptions.BackgroundColor;
        Color previewForeColor = Options.CurrentOptions.ForegroundColor;
        Font previewFont = Options.CurrentOptions.Font;

        public OptionsForm()
        {
            InitializeComponent();
            Options.ApplyTheme(this);
            //fontDialog.ShowColor = true;
            fontDialog.ShowEffects = true;

            string font = c.ConvertToString(Options.CurrentOptions.Font);

            lblFont.Text = font;
            panelBackColor.BackColor = Options.CurrentOptions.BackgroundColor;
            panelForeColor.BackColor = Options.CurrentOptions.ForegroundColor;
            lblPreview.Font = Options.CurrentOptions.Font;
            lblPreview.ForeColor = Options.CurrentOptions.ForegroundColor;
            lblPreview.BackColor = Options.CurrentOptions.BackgroundColor;
        }

        private void Default()
        {
            previewFont = new Font("Consolas", 11F);
            previewBackColor = Color.Black;
            previewForeColor = Color.Lime;

            lblFont.Text = c.ConvertToInvariantString(previewFont);
            panelBackColor.BackColor = previewBackColor;
            panelForeColor.BackColor = previewForeColor;
            lblPreview.Font = previewFont;
            lblPreview.ForeColor = previewForeColor;
            lblPreview.BackColor = previewBackColor;
        }

        private void SaveOptions()
        {
            Options.CurrentOptions.Font = previewFont;
            Options.CurrentOptions.ForegroundColor = previewForeColor;
            Options.CurrentOptions.BackgroundColor = previewBackColor;

            this.Close();
        }

        private void PreviewFont()
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                lblFont.Text = c.ConvertToInvariantString(fontDialog.Font);
                lblPreview.Font = fontDialog.Font;
                //panelForeColor.BackColor = fontDialog.Color;
                //lblPreview.ForeColor = fontDialog.Color;

                //previewForeColor = fontDialog.Color;
                previewFont = fontDialog.Font;
            }
        }

        private void PreviewTextColor()
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                panelForeColor.BackColor = colorDialog.Color;
                lblPreview.ForeColor = colorDialog.Color;

                previewForeColor = colorDialog.Color;
            }
        }

        private void PreviewBackgroundColor()
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                panelBackColor.BackColor = colorDialog.Color;
                lblPreview.BackColor = colorDialog.Color;

                previewBackColor = colorDialog.Color;
            }
        }

        private void Options_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            PreviewFont();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PreviewTextColor();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveOptions();
        }

        private void lblFont_Click(object sender, EventArgs e)
        {
            PreviewFont();
        }

        private void panelForeColor_Click(object sender, EventArgs e)
        {
            PreviewTextColor();
        }

        private void panelBackColor_Click(object sender, EventArgs e)
        {
            PreviewBackgroundColor();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PreviewBackgroundColor();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Default();
        }
    }
}
