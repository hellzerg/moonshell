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
        FontDialog _fontDialog = new FontDialog();
        ColorDialog _colorDialog = new ColorDialog();
        FontConverter _fontConverter = new FontConverter();

        Color _previewBackColor = Options.CurrentOptions.BackgroundColor;
        Color _previewForeColor = Options.CurrentOptions.ForegroundColor;
        Font _previewFont = Options.CurrentOptions.Font;

        public OptionsForm()
        {
            InitializeComponent();
            Options.ApplyTheme(this);
           
            _fontDialog.ShowEffects = true;

            string font = _fontConverter.ConvertToString(Options.CurrentOptions.Font);

            lblFont.Text = font;
            panelBackColor.BackColor = Options.CurrentOptions.BackgroundColor;
            panelForeColor.BackColor = Options.CurrentOptions.ForegroundColor;
            lblPreview.Font = Options.CurrentOptions.Font;
            lblPreview.ForeColor = Options.CurrentOptions.ForegroundColor;
            lblPreview.BackColor = Options.CurrentOptions.BackgroundColor;
        }

        private void ResetToDefault()
        {
            _previewFont = new Font("Consolas", 11F);
            _previewBackColor = Color.Black;
            _previewForeColor = Color.Lime;

            lblFont.Text = _fontConverter.ConvertToInvariantString(_previewFont);
            panelBackColor.BackColor = _previewBackColor;
            panelForeColor.BackColor = _previewForeColor;
            lblPreview.Font = _previewFont;
            lblPreview.ForeColor = _previewForeColor;
            lblPreview.BackColor = _previewBackColor;
        }

        private void SaveOptions()
        {
            Options.CurrentOptions.Font = _previewFont;
            Options.CurrentOptions.ForegroundColor = _previewForeColor;
            Options.CurrentOptions.BackgroundColor = _previewBackColor;

            this.Close();
        }

        private void PreviewFont()
        {
            if (_fontDialog.ShowDialog() == DialogResult.OK)
            {
                lblFont.Text = _fontConverter.ConvertToInvariantString(_fontDialog.Font);
                lblPreview.Font = _fontDialog.Font;
                
                _previewFont = _fontDialog.Font;
            }
        }

        private void PreviewTextColor()
        {
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                panelForeColor.BackColor = _colorDialog.Color;
                lblPreview.ForeColor = _colorDialog.Color;

                _previewForeColor = _colorDialog.Color;
            }
        }

        private void PreviewBackgroundColor()
        {
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                panelBackColor.BackColor = _colorDialog.Color;
                lblPreview.BackColor = _colorDialog.Color;

                _previewBackColor = _colorDialog.Color;
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
            ResetToDefault();
        }
    }
}
