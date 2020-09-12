using MapsetChecks.checks.compose;
using MapsetChecks.checks.hit_sounds;
using MapsetChecks.checks.timing;
using MapsetVerifierFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapsetVerifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            addAllChecks();
        }

        private void addAllChecks()
        {
            CheckerRegistry.RegisterCheck(new CheckAbnormalNodes());
            CheckerRegistry.RegisterCheck(new CheckHitSounds());
            CheckerRegistry.RegisterCheck(new CheckWrongSnapping());
        }

        private void launchOpenDialog(object sender, ToolStripItemClickedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.SelectedPath= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\osu!\Songs";
            dialog.ShowDialog();

            if (Directory.Exists(dialog.SelectedPath))
            {
                addNewTab(dialog.SelectedPath);
            }
        }

        private void addNewTab(string path)
        {
            path = Path.GetFullPath(path);
            var directoryContents = Directory.GetFiles(path).ToList();
            var hasOsuFiles = directoryContents.Any(file => file.EndsWith(".osu"));
            if (!hasOsuFiles)
            {
                MessageBox.Show("This folder does not contain any .osu files!", "No .osu files", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var tab = new MapsetTab(path);
            tab.Text = Path.GetFileName(path);
            mainView.Controls.Add(tab);
        }
    }
}
