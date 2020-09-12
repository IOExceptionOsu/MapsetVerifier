using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace MapsetVerifier
{
    partial class OpenDialog : Window
    {
        public OpenDialog()
        {
            InitializeComponent();

            var osuSongsFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\osu!";
            // MapsetList = Directory.GetFiles(osuSongsFolder).ToList();
        }
    }
}