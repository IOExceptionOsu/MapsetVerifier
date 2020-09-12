using MapsetParser.objects;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapsetVerifier
{
    internal class MapsetTab : TabPage
    {
        private BeatmapSet set;

        private TableLayoutPanel panel;
        private ListBox diffList;
        private ToolStrip toolbar;
        private ToolStripButton openMapsetInBrowserBtn;
        private RichTextBox tb;

        public MapsetTab(string path)
        {
            set = new BeatmapSet(path);

            #region setting up controls
            panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;

            diffList = new ListBox();
            diffList.Dock = DockStyle.Left;
            diffList.Items.Add("General");
            foreach (var diff in set.beatmaps) {
                diffList.Items.Add(diff.metadataSettings.version);
            }
            panel.Controls.Add(diffList, 0, 1);

            toolbar = new ToolStrip();
            openMapsetInBrowserBtn = new ToolStripButton();
            openMapsetInBrowserBtn.Text = "Open in Browser";
            openMapsetInBrowserBtn.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            openMapsetInBrowserBtn.Image = SystemIcons.Information.ToBitmap();
            openMapsetInBrowserBtn.Enabled = false;
            toolbar.Items.Add("Open In Browser");
            panel.Controls.Add(toolbar, 0, 0);
            panel.SetColumnSpan(toolbar, 2);

            tb = new RichTextBox();
            tb.Multiline = true;
            tb.LinkClicked += handleOpenLink;
            tb.Dock = DockStyle.Fill;
            panel.Controls.Add(tb, 1, 1);

            Controls.Add(panel);
            #endregion

            // start running checks
            _ = Task.Run(runChecks);
        }

        private void handleOpenLink(object sender, LinkClickedEventArgs e)
        {
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = e.LinkText;
            Process.Start(psi);
        }

        private delegate void FinishRunningChecksDelegate(List<Issue> issues);

        private void runChecks()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var issues = Checker.GetBeatmapSetIssues(set);
                Trace.WriteLine("# of issues: " + issues.Count);
                Invoke(new FinishRunningChecksDelegate(finishRunningChecks), issues);
            });
        }

        private void finishRunningChecks(List<Issue> issues)
        {
            tb.Clear();
            tb.Rtf = constructRtf(issues);
            /* tb.Rtf = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Arial;}}
{\field{\*\fldinst HYPERLINK ""URL""}{\fldrslt{\ul\cf1Text to display}}}
}"; */
        }

        // 01:38:518 (1) - 
        // 01:38:518 (1,2) - 
        private static Regex editorLinks = new Regex(
            @"(osu:\/\/edit\/)?(?<entire>(?<timestamp>\d+:\d{2}:\d{3})( \([^\)]*\))?( -)?)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
        private string constructRtf(List<Issue> issues)
        {
            var doc = new StringBuilder();
            doc.AppendLine(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Arial;}}");
            foreach (var issue in issues)
            {
                var matches = editorLinks.Matches(issue.message);
                Trace.WriteLine(matches.Count + " " + matches.ToString());
                var text = editorLinks.Replace(issue.message, match => 
                    $@"{{\field{{\*\fldinst HYPERLINK ""osu://edit/{match.Groups["entire"]}""}}{{\fldrslt {match.Groups["entire"]}}}}}");
                doc.AppendLine(text);
            }
            doc.AppendLine(@"}");
            return doc.ToString();
        }
    }
}