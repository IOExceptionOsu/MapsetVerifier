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
        private string setPath;
        private ulong setId;
        private Dictionary<string, List<Issue>> lastRunIssues = null;
        private string currentlySelectedTab = null;
        private Dictionary<string, string> rtfCache = new Dictionary<string, string>();

        private TableLayoutPanel panel;
        private ListBox diffList;
        private ToolStrip toolbar;
        private ToolStripButton openMapsetInExplorerBtn, openMapsetInBrowserBtn;
        private RichTextBox tb;

        public MapsetTab(string path)
        {
            setPath = path;
            set = new BeatmapSet(path);
            var map = set.beatmaps.Find(bmap => bmap.metadataSettings.beatmapId > 0 && bmap.metadataSettings.beatmapSetId > 0);
            setId = (ulong) map.metadataSettings.beatmapSetId;

            #region setting up controls
            panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;

            diffList = new ListBox();
            diffList.Dock = DockStyle.Left;
            diffList.Items.Add("General");
            diffList.SelectedIndexChanged += changeSelectedDifficulty;
            foreach (var diff in set.beatmaps) {
                diffList.Items.Add($"[{diff.metadataSettings.version}]");
            }
            panel.Controls.Add(diffList, 0, 1);

            toolbar = new ToolStrip();
            openMapsetInExplorerBtn = new ToolStripButton();
            openMapsetInExplorerBtn.Text = "Open in Explorer";
            openMapsetInExplorerBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            openMapsetInExplorerBtn.Click += openMapsetInExplorer;
            toolbar.Items.Add(openMapsetInExplorerBtn);

            openMapsetInBrowserBtn = new ToolStripButton();
            openMapsetInBrowserBtn.Text = "Open in Browser";
            openMapsetInBrowserBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            openMapsetInBrowserBtn.Enabled = setId > 0;
            openMapsetInBrowserBtn.Click += openMapsetInBrowser;
            toolbar.Items.Add(openMapsetInBrowserBtn);
            panel.Controls.Add(toolbar, 0, 0);
            panel.SetColumnSpan(toolbar, 2);

            tb = new RichTextBox();
            tb.Text = "loading...";
            tb.Multiline = true;
            tb.LinkClicked += handleOpenLink;
            tb.Dock = DockStyle.Fill;
            panel.Controls.Add(tb, 1, 1);

            Controls.Add(panel);
            #endregion

            // start running checks
            _ = Task.Run(runChecks);
        }

        private void openMapsetInExplorer(object sender, System.EventArgs e)
        {
            Trace.WriteLine($"opening {setPath} in explorer...");
            Process.Start("explorer.exe", setPath);
        }

        private void openMapsetInBrowser(object sender, System.EventArgs e)
        {
            var uri = $"https://osu.ppy.sh/beatmapsets/{setId}";
            Trace.WriteLine("opening " + uri);
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = uri;
            Process.Start(psi);
        }

        private void changeSelectedDifficulty(object sender, System.EventArgs e)
        {
            if (lastRunIssues == null) return;
            if (diffList.SelectedItem == null) return;
            changeTabTo((string)diffList.SelectedItem);
        }

        private void changeTabTo(string tab)
        {
            List<Issue> issues;
            var found = lastRunIssues.TryGetValue(tab, out issues);
            if (!found) return;

            string rtf;
            found = rtfCache.TryGetValue(tab, out rtf);
            if (!found)
            {
                tb.Text = "loading...";
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    rtf = constructRtf(issues);
                    rtfCache.Add(tab, rtf);
                    Invoke(new FinishConstructingRTF(finishConstructingRtf), rtf);
                });
            }
            else
            {
                tb.Rtf = rtf;
            }
        }

        private delegate void FinishConstructingRTF(string rtf);
        private void finishConstructingRtf(string rtf)
        {
            tb.Rtf = rtf;
        }

        private void handleOpenLink(object sender, LinkClickedEventArgs e)
        {
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = e.LinkText;
            Process.Start(psi);
        }


        private void runChecks()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var issues = Checker.GetBeatmapSetIssues(set);
                Trace.WriteLine("# of issues: " + issues.Count);
                Invoke(new FinishRunningChecksDelegate(finishRunningChecks), issues);
            });
        }

        private delegate void FinishRunningChecksDelegate(List<Issue> issues);
        private void finishRunningChecks(List<Issue> issues)
        {
            tb.Clear();
            lastRunIssues = new Dictionary<string, List<Issue>>();
            foreach (var issue in issues)
            {
                var tabName = "General";
                if (issue.beatmap != null) 
                    tabName = $"[{issue.beatmap.metadataSettings.version}]";
                if (!lastRunIssues.ContainsKey(tabName))
                {
                    lastRunIssues.Add(tabName, new List<Issue>());
                }
                lastRunIssues[tabName].Add(issue);
            }
         
            if (currentlySelectedTab != null)
            {
                changeTabTo(currentlySelectedTab);
            }
        }
        
        private static Regex editorLinks = new Regex(
            @"(osu:\/\/edit\/)?(?<entire>(?<timestamp>\d+:\d{2}:\d{3})( \([^\)]*\))?( -)?)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
        private string constructRtf(IEnumerable<Issue> issues)
        {
            var doc = new StringBuilder();
            doc.AppendLine(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Arial;}}");
            foreach (var issue in issues)
            {
                var matches = editorLinks.Matches(issue.message);
                Trace.WriteLine(matches.Count + " " + matches.ToString());
                var text = editorLinks.Replace(issue.message, match => 
                    $@"{{\field{{\*\fldinst HYPERLINK ""osu://edit/{match.Groups["entire"]}""}}{{\fldrslt {match.Groups["entire"]}}}}}");
                doc.AppendLine($@"{{\pard\sa200 - {text} \par}}");
            }
            doc.AppendLine(@"}");
            return doc.ToString();
        }
    }
}