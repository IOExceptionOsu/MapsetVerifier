using MapsetChecks.checks.compose;
using MapsetChecks.checks.events;
using MapsetChecks.checks.general.audio;
using MapsetChecks.checks.general.files;
using MapsetChecks.checks.general.metadata;
using MapsetChecks.checks.general.resources;
using MapsetChecks.checks.hit_sounds;
using MapsetChecks.checks.settings;
using MapsetChecks.checks.spread;
using MapsetChecks.checks.standard.compose;
using MapsetChecks.checks.standard.settings;
using MapsetChecks.checks.standard.spread;
using MapsetChecks.checks.timing;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            new List<Check>()
            {
                // all modes -> compose
                new CheckAbnormalNodes(),
                new CheckConcurrent(),
                new CheckDrainTime(),
                new CheckInvisibleSlider(),

                // all modes -> events
                new CheckBreaks(),
                new CheckStoryHitSounds(),

                // all modes -> general -> audio
                // new CheckAudioFormat(),
                new CheckAudioInVideo(),
                new CheckAudioUsage(),
                // new CheckBitrate(),
                new CheckCommonFinish(),
                new CheckHitSoundDelay(),
                // new CheckHitSoundFormat(),
                new CheckHitSoundImbalance(),
                new CheckHitSoundLength(),
                new CheckMultipleAudio(),

                // all modes -> general -> files
                new CheckUnusedFiles(),
                new CheckUpdateValidity(),
                new CheckZeroBytes(),

                // all modes -> general -> metadata
                new CheckGenreLanguage(),
                new CheckGuestTags(),
                new CheckInconsistentMetadata(),
                new CheckMarkerFormat(),
                new CheckMarkerSpacing(),
                new CheckSource(),
                new CheckTitleMarkers(),
                new CheckUnicode(),

                // all modes -> general -> resources
                new CheckBgPresence(),
                new CheckBgResolution(),
                new CheckMultipleVideo(),
                new CheckOverlayLayer(),
                new CheckSpriteResolution(),
                new CheckVideoOffset(),
                new CheckVideoResolution(),

                // all modes -> hit sounds
                new CheckHitSounds(),
                new CheckMuted(),

                // all modes -> settings
                new CheckDiffSettings(),
                new CheckInconsistentSettings(),
                new CheckTickRate(),

                // all modes -> spread
                new CheckLowestDiff(),

                // all modes -> timing
                new CheckBeforeLine(),
                new CheckConcurrentLines(),
                new CheckFirstLine(),
                new CheckInconsistentLines(),
                new CheckKiaiUnsnap(),
                new CheckPreview(),
                new CheckUnsnaps(),
                new CheckUnusedLines(),
                new CheckWrongSnapping(),

                // standard -> compose
                new CheckAmbiguity(),
                new CheckBurai(),
                new CheckNinjaSpinner(),
                new CheckObscuredReverse(),
                new CheckOffscreen(),

                // standard -> settings
                new CheckDefaultColours(),
                new CheckLuminosity(),

                // standard -> spread
                new CheckCloseOverlap(),
                new CheckMultipleReverses(),
                new CheckShortSliders(),
                new CheckSpaceVariation(),
                new CheckSpinnerRecovery(),
                new CheckStackLeniency(),
            }.ForEach(CheckerRegistry.RegisterCheck);
        }

        private void launchOpenDialog(object sender, ToolStripItemClickedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\osu!\Songs";
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
