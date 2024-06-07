using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ComPort.Scanner
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            var halfWidth = this.Width / 2;
            lblName.Left = halfWidth - (lblName.Width / 2);
            lblVersion.Left = halfWidth - (lblVersion.Width / 2);
            
            btCoffee.Left = halfWidth - (btCoffee.Width / 2);
            btClose.Left = halfWidth - (btClose.Width / 2);
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OnCoffeeClick(object sender, EventArgs e)
        {
            var sInfo = new ProcessStartInfo(@"https://www.buymeacoffee.com/Tsessebe");
            Process.Start(sInfo);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            lblVersion.Text = $"Version: {Application.ProductVersion}";
        }
    }
}