using MapsetParser.objects;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace MapsetVerifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CurrentBeatmapSet CurrentBeatmapSet = null;

        public MainWindow()
        {
            InitializeComponent();

            DiffList.ItemsSource = CurrentBeatmapSet;
            // new OpenDialog().ShowDialog();
        }

        private void LaunchOpenDialog(object sender, RoutedEventArgs e)
        {
            new OpenDialog().ShowDialog();
        }
    }

    internal class CurrentBeatmapSet : IEnumerable<Beatmap>
    {
        private BeatmapSet beatmapSet = null;

        public IEnumerator<Beatmap> GetEnumerator()
        {
            if (beatmapSet == null)
            {
                return new List<Beatmap>().GetEnumerator();
            }
            else
            {
                return beatmapSet.beatmaps.GetEnumerator();
            }
            // lmao6
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (beatmapSet == null)
            {
                return new List<Beatmap>().GetEnumerator();
            }
            else
            {
                return beatmapSet.beatmaps.GetEnumerator();
            }
        }
    }
}
