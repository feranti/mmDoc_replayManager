using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Replays;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace MMDoCHistoryV2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private AutoUpdate updater = null;

        internal static Loader Loader = null;

        internal static DateTime GetTime(string timeString)
        {
            return DateTime.Parse(timeString);
        }

        internal static string GetTime(DateTime timeValue)
        {
            return timeValue.ToString();
        }

        internal static TimeSpan GetDuration(string durationString)
        {
            string[] spl = durationString.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            int h = 0;
            int m = 0;
            int s = 0;
            if(spl.Length == 1)
                s = int.Parse(spl[0]);
            else if(spl.Length == 2)
            {
                m = int.Parse(spl[0]);
                s = int.Parse(spl[1]);
            }
            else if(spl.Length == 3)
            {
                h = int.Parse(spl[0]);
                m = int.Parse(spl[1]);
                s = int.Parse(spl[2]);
            }
            else
                throw new Exception();

            return new TimeSpan(h, m, s);
        }

        internal static string GetDuration(TimeSpan durationValue)
        {
            int hr = (int)durationValue.TotalHours;
            if(hr >= 1)
                return hr.ToString("00") + ":" + durationValue.Minutes.ToString("00") + ":" + durationValue.Seconds.ToString("00");
            else
                return durationValue.Minutes.ToString("00") + ":" + durationValue.Seconds.ToString("00");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.listView2.ListViewItemSorter = sorter;
            sorter.Order = SortOrder.Descending;
            sorter.SortColumn = 0;

            toolStripStatusLabel2.Alignment = ToolStripItemAlignment.Right;

            Loader = new Replays.Loader();

            if(!cfg.Load())
                this.GuessPaths();

            cfg.UpdateLoader(this);

            this.LoadFilters();
            this.UpdateFilters();

            // In the end enable timer.
            timer1.Enabled = true;
            timer2.Enabled = true;

            this.CheckUpdates();
        }

        private void GuessPaths()
        {
            string path = null;

            {
                try
                {
                    path = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Ubisoft\MMDoC-PDCLive", "Patch_Dir", null);
                    if(!TestDir(path))
                        path = null;
                }
                catch
                {
                }

                // For me the patch dir was in this location in the registry.
                if(path == null)
                {
                    try
                    {
                        path = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Ubisoft\MMDoC-PDCLive", "Patch_Dir", null);
                        if(!TestDir(path))
                            path = null;
                    }
                    catch
                    {
                    }
                }

                path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"Ubisoft\MMDoC-PDCLive\GameData");
                if(!TestDir(path))
                    path = null;
            }

            // Unable to guess path.
            if(path == null)
                return;

            if(cfg.cardDirs.Count == 0)
                cfg.cardDirs.Add(path);
            if(cfg.replayDirs.Count == 0)
                cfg.replayDirs.Add(Path.Combine(path, "Replay"));
            if(cfg.locDirs.Count == 0)
                cfg.locDirs.Add(Path.Combine(path, "Localization"));
            cfg.UpdateLoader(this);
        }

        private bool TestDir(string path)
        {
            if(string.IsNullOrEmpty(path))
                return false;
            try
            {
                return new DirectoryInfo(path).Exists;
            }
            catch
            {
            }
            return false;
        }

        private readonly ReplaySorter sorter = new ReplaySorter();

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if(e.Column == sorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if(sorter.Order == SortOrder.Ascending)
                {
                    sorter.Order = SortOrder.Descending;
                }
                else
                {
                    sorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                sorter.SortColumn = e.Column;
                sorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView2.Sort();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(this.NeedReplayUpdate)
                this.UpdateReplays();

            if(_doReplayDelete2 > 0)
                _doReplayDelete2 -= timer1.Interval;
            else if(!string.IsNullOrEmpty(_doReplayDelete))
            {
                string fn = _doReplayDelete;
                _doReplayDelete = null;
                try
                {
                    File.Delete(fn);
                }
                catch
                {
                }
            }
        }

        private readonly List<Form> OpenedForms = new List<Form>();

        internal void OpenForm(Type t)
        {
            for(int i = OpenedForms.Count - 1; i >= 0; i--)
            {
                if(OpenedForms[i].GetType()  == t)
                    OpenedForms[i].Close();
            }

            OpenedForms.Add(Activator.CreateInstance(t, this) as Form);
            OpenedForms[OpenedForms.Count - 1].Show();
        }

        internal void ClosedForm(Form f)
        {
            OpenedForms.Remove(f);
        }

        public readonly Config cfg = new Config();

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            while(OpenedForms.Count > 0)
                OpenedForms[0].Close();

            if(this.CurrentUpdater != null)
                this.CurrentUpdater.Stop();

            if(Loader != null)
                Loader.StopLoad();

            this.SaveFilters();

            if(!string.IsNullOrEmpty(_doReplayDelete))
            {
                try
                {
                    File.Delete(_doReplayDelete);
                }
                catch
                {
                }
                _doReplayDelete = null;
            }

            while(Interlocked.Read(ref _isUpdate) != 0)
                Thread.Sleep(1);

            cfg.Save();
        }

        internal static string GetNumberCom(long num)
        {
            return num.ToString("#,#", CultureInfo.InvariantCulture);
        }

        private void statsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm(typeof(Stats));
            this.UpdateStatsWindow();
        }

        private void UpdateStatsWindow()
        {
            foreach(Form x in this.OpenedForms)
            {
                if(x is Stats)
                    ((Stats)x).SetStats(this.DisplayedReplays.Values.ToList());
                else if(x is GameLog)
                {
                    if(((GameLog)x).setted != null)
                    {
                        Replay nr = this.GetReplay(((GameLog)x).setted.FileName);
                        if(nr != null && nr != ((GameLog)x).setted)
                            ((GameLog)x).SetReplay(nr);
                    }
                }
                else if(x is GameDecks)
                {
                    if(((GameDecks)x).setted != null)
                    {
                        Replay nr = this.GetReplay(((GameDecks)x).setted.FileName);
                        if(nr != null && nr != ((GameDecks)x).setted)
                            ((GameDecks)x).SetReplay(nr);
                    }
                }
            }
        }

        private bool wasLoad = false;

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.CheckUpdater();

            if(this.updater != null)
                this.updater.Update();

            if(Loader.IsLoading)
            {
                if(this.toolStripStatusLabel1.Text == "Loading.")
                    this.toolStripStatusLabel1.Text = "Loading..";
                else if(this.toolStripStatusLabel1.Text == "Loading..")
                    this.toolStripStatusLabel1.Text = "Loading...";
                /*else if(this.toolStripStatusLabel1.Text == "Loading...")
                    this.toolStripStatusLabel1.Text = "Loading.";*/
                else
                    this.toolStripStatusLabel1.Text = "Loading.";
                wasLoad = true;
            }
            else if(wasLoad)
            {
                int cardCout = 0;
                lock(Loader.LoadedCards)
                {
                    cardCout = Loader.LoadedCards.Count;
                }
                int repCount = 0;
                lock(Loader.LoadedReplays)
                {
                    repCount = Loader.LoadedReplays.Count;
                }
                this.toolStripStatusLabel1.Text = "Loaded " + repCount + " replays and " + cardCout + " cards.";
                wasLoad = false;
                NeedReplayUpdate = true;
            }

            long prev = Interlocked.Exchange(ref _hasUpdate, 0);
            if(prev != 0)
            {
                if(prev == 1)
                {
                    if(MessageBox.Show("There is a newer version available! Would you like to download it now?", "Update", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("http://www.duckbat.com/gamehistory.zip");
                            this.Close();
                        }
                        catch
                        {
                            MessageBox.Show("Couldn't download! Try manually" + Environment.NewLine + "http://www.duckbat.com/gamehistory.zip");
                        }
                    }
                }
                else
                {
                    if(notifyNextUpdate)
                    {
                        notifyNextUpdate = false;
                        MessageBox.Show("You have the latest version already.", "Update");
                    }
                }
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.cfg.cardDirs.Count == 0 || cfg.locDirs.Count == 0 || cfg.replayDirs.Count == 0)
            {
                this.OpenOptions();
                MessageBox.Show("Unable to load because some directories were missing, please enter the correct paths.");
                return;
            }

            this.LoadReplays();
        }

        private void OpenOptions()
        {
            if(this.OpenedForms.Any(e => e.GetType() == typeof(Options)))
                this.OpenedForms.Where(e => e.GetType() == typeof(Options)).ElementAt(0).Show();
            else
            {
                this.OpenedForms.Add(new Options(this));
                this.OpenedForms[this.OpenedForms.Count - 1].Show();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.UnloadReplays();

            wasLoad = true; // this will update status bar.
            this.NeedReplayUpdate = true;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenOptions();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem itm in this.listView1.SelectedItems)
                itm.Checked = !itm.Checked;
        }

        private void newFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Filters.Add(new Filter());
            this.UpdateFilters();

            OpenedForms.Add(new EditFilter(this));
            ((EditFilter)OpenedForms[OpenedForms.Count - 1]).SetFilter(this.Filters[this.Filters.Count - 1]);
            OpenedForms[OpenedForms.Count - 1].Show();
        }

        private void editFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count == 0)
                return;

            int ind = this.listView1.Items.IndexOf(this.listView1.SelectedItems[0]);

            List<Form> forms = this.OpenedForms.Where(q => q.GetType() == typeof(EditFilter)).ToList();
            foreach(Form x in forms)
                x.Close();

            OpenedForms.Add(new EditFilter(this));
            ((EditFilter)OpenedForms[OpenedForms.Count - 1]).SetFilter(this.Filters[ind]);
            OpenedForms[OpenedForms.Count - 1].Show();
        }

        private void deleteFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count == 0)
                return;

            SortedDictionary<int, ListViewItem> toremove = new SortedDictionary<int, ListViewItem>();
            foreach(ListViewItem itm in this.listView1.SelectedItems)
            {
                toremove[this.listView1.Items.IndexOf(itm)] = itm;
            }

            while(toremove.Count > 0)
            {
                KeyValuePair<int, ListViewItem> x = toremove.ElementAt(toremove.Count - 1);
                this.Filters.RemoveAt(x.Key);
                toremove.Remove(x.Key);
            }

            this.UpdateFilters();
        }

        private void moveFilterUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count == 0)
                return;

            SortedDictionary<int, Filter> toremove = new SortedDictionary<int, Filter>();
            foreach(ListViewItem itm in this.listView1.SelectedItems)
            {
                toremove[this.listView1.Items.IndexOf(itm)] = Filters[this.listView1.Items.IndexOf(itm)];
            }

            while(toremove.Count > 0)
            {
                KeyValuePair<int, Filter> x = toremove.ElementAt(toremove.Count - 1);
                int curin = this.Filters.IndexOf(x.Value);
                if(curin > 0)
                {
                    this.Filters.RemoveAt(curin);
                    this.Filters.Insert(curin - 1, x.Value);
                }
                toremove.Remove(x.Key);
            }

            this.UpdateFilters();
        }

        private void moveFilterDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count == 0)
                return;

            SortedDictionary<int, Filter> toremove = new SortedDictionary<int, Filter>();
            foreach(ListViewItem itm in this.listView1.SelectedItems)
            {
                toremove[this.listView1.Items.IndexOf(itm)] = Filters[this.listView1.Items.IndexOf(itm)];
            }

            while(toremove.Count > 0)
            {
                KeyValuePair<int, Filter> x = toremove.ElementAt(0);
                int curin = this.Filters.IndexOf(x.Value);
                if(curin < this.Filters.Count - 1)
                {
                    this.Filters.RemoveAt(curin);
                    this.Filters.Insert(curin + 1, x.Value);
                }
                toremove.Remove(x.Key);
            }

            this.UpdateFilters();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count == 0)
                return;

            int ind = this.listView1.Items.IndexOf(this.listView1.SelectedItems[0]);

            List<Form> forms = this.OpenedForms.Where(q => q.GetType() == typeof(EditFilter)).ToList();
            foreach(Form x in forms)
                x.Close();

            OpenedForms.Add(new EditFilter(this));
            ((EditFilter)OpenedForms[OpenedForms.Count - 1]).SetFilter(this.Filters[ind]);
            OpenedForms[OpenedForms.Count - 1].Show();
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int index = listView1.Items.IndexOf(e.Item);
            this.Filters[index].Flags &= ~FilterFlags.Disabled;
            if(!e.Item.Checked)
                this.Filters[index].Flags |= FilterFlags.Disabled;
            NeedReplayUpdate = true;
        }

        private void ViewGameLog(Replay replay)
        {
            List<Form> forms = this.OpenedForms.Where(q => q.GetType() == typeof(GameLog)).ToList();
            foreach(Form x in forms)
                x.Close();

            GameLog log = new GameLog(this);
            OpenedForms.Add(log);
            log.Show();
            log.SetReplay(replay);
        }

        private void ViewGameDecks(Replay replay)
        {
            List<Form> forms = this.OpenedForms.Where(q => q.GetType() == typeof(GameDecks)).ToList();
            foreach(Form x in forms)
                x.Close();

            GameDecks log = new GameDecks(this);
            OpenedForms.Add(log);
            log.Show();
            log.SetReplay(replay);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if(this.listView2.SelectedItems.Count == 0)
                return;

            Replay replay = this.GetReplay(this.listView2.SelectedItems[0]);
            if(replay != null)
                this.ViewGameLog(replay);
        }

        private Replay GetReplay(ListViewItem itm)
        {
            lock(Loader.LoadedReplays)
            {
                foreach(Replay r in Loader.LoadedReplays)
                {
                    if(r.FileName == itm.ToolTipText)
                        return r;
                }
            }
            return null;
        }

        private Replay GetReplay(string fileName)
        {
            lock(Loader.LoadedReplays)
            {
                foreach(Replay r in Loader.LoadedReplays)
                {
                    if(r.FileName == fileName)
                        return r;
                }
            }
            return null;
        }

        private void removeFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ListViewItem> rem = new List<ListViewItem>();
            foreach(ListViewItem x in this.listView2.SelectedItems)
                rem.Add(x);

            while(rem.Count > 0)
            {
                listView2.Items.Remove(rem[0]);
                rem.RemoveAt(0);
            }
        }

        private void viewDecksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView2.SelectedItems.Count == 0)
                return;

            Replay replay = this.GetReplay(this.listView2.SelectedItems[0]);
            if(replay != null)
                this.ViewGameDecks(replay);
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            if(this.listView2.SelectedItems.Count == 0)
            {
                viewDecksToolStripMenuItem.Enabled = false;
                removeFromListToolStripMenuItem.Enabled = false;
                toolStripMenuItem3.Enabled = false;
            }
            else
            {
                viewDecksToolStripMenuItem.Enabled = true;
                removeFromListToolStripMenuItem.Enabled = true;
                toolStripMenuItem3.Enabled = true;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if(listView1.SelectedItems.Count == 0)
            {
                toolStripMenuItem1.Enabled = false;
                editFilterToolStripMenuItem.Enabled = false;
                deleteFilterToolStripMenuItem.Enabled = false;
                moveFilterDownToolStripMenuItem.Enabled = false;
                moveFilterUpToolStripMenuItem.Enabled = false;
            }
            else
            {
                toolStripMenuItem1.Enabled = true;
                editFilterToolStripMenuItem.Enabled = true;
                deleteFilterToolStripMenuItem.Enabled = true;
                moveFilterDownToolStripMenuItem.Enabled = true;
                moveFilterUpToolStripMenuItem.Enabled = true;
            }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            int amt;
            if(int.TryParse(toolStripTextBox1.Text, out amt))
            {
                if(!cfg.vals.ContainsKey("maxshow") || cfg.vals["maxshow"] != amt.ToString())
                {
                    cfg.vals["maxshow"] = amt.ToString();
                    NeedReplayUpdate = true;
                }
            }
        }

        private void toolStripMenuItem5_CheckedChanged(object sender, EventArgs e)
        {
            if(!cfg.vals.ContainsKey("hidebroken") || cfg.vals["hidebroken"] != toolStripMenuItem5.Checked.ToString())
            {
                cfg.vals["hidebroken"] = toolStripMenuItem5.Checked.ToString();
                NeedReplayUpdate = true;
            }
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if(this.updater == null)
            {
                this.updater = new AutoUpdate(Form1.Loader);
                int trys;
                if(int.TryParse(toolStripTextBox2.Text, out trys))
                    this.updater.Interval = Math.Max(2, trys) * 1000;
            }

            if(this.updater != null)
            {
                if(toolStripMenuItem6.Text.IndexOf("start", StringComparison.OrdinalIgnoreCase) != -1)
                    this.updater.Start();
                else
                    this.updater.Stop();
            }
            fileToolStripMenuItem.DropDown.Close();
        }

        private void toolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            if(this.updater == null)
                return;

            int trys;
            if(int.TryParse(toolStripTextBox2.Text, out trys))
                this.updater.Interval = Math.Max(2, trys) * 1000;
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if(this.updater == null || !this.updater.IsRunning)
                toolStripMenuItem6.Text = "Start replay file updater";
            else
                toolStripMenuItem6.Text = "Stop replay file updater";
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenForm(typeof(HelpForm));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void numericUpDown1_TextChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            notifyNextUpdate = true;

            this.CheckUpdates();
        }

        private bool notifyNextUpdate = false;

        private void CheckUpdates()
        {
            if(Interlocked.Exchange(ref _isUpdate, 1) != 0)
                return;

            Thread t = new Thread(_checkUpdates);
            t.Start();
        }

        private void _checkUpdates()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.duckbat.com/gamehistoryversion.txt");

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // we will read data via the response stream
                Stream resStream = response.GetResponseStream();
                string tempString = null;
                int count = 0;
                byte[] buf = new byte[1024];
                StringBuilder sb = new StringBuilder();

                do
                {
                    // fill the buffer with data
                    count = resStream.Read(buf, 0, buf.Length);

                    // make sure we read some data
                    if(count != 0)
                    {
                        // translate from bytes to ASCII text
                        tempString = Encoding.ASCII.GetString(buf, 0, count);

                        // continue building the string
                        sb.Append(tempString);
                    }
                }
                while(count > 0); // any more data to read?

                int ver;
                if(int.TryParse(sb.ToString(), out ver))
                {
                    if(ver > Form1.Version)
                        Interlocked.Exchange(ref _hasUpdate, 1);
                    else
                        Interlocked.Exchange(ref _hasUpdate, 2);
                }
            }
            catch
            {
            }

            Interlocked.Exchange(ref _isUpdate, 0);
        }

        private long _hasUpdate = 0;
        private long _isUpdate = 0;

        public const int Version = 4;

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            this.NeedReplayUpdate = true;
        }

        private string _doReplayDelete = null;
        private long _doReplayDelete2 = 0;

        private void viewReplayInGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView2.SelectedItems.Count == 0)
                return;

            Replay replay = this.GetReplay(this.listView2.SelectedItems[0]);
            FileInfo file = null;
            try
            {
                file = new FileInfo(replay.FileName);
            }
            catch
            {
            }
            if(replay == null || file == null)
            {
                MessageBox.Show("Couldn't find replay file!");
                return;
            }

            string gameDir = null;
            foreach(string x in cfg.cardDirs)
            {
                if(!string.IsNullOrEmpty(x) && x.IndexOf("gamedata", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    gameDir = x;
                    break;
                }
            }

            if(gameDir == null)
            {
                MessageBox.Show("Couldn't find game directory! Make sure you have set the card directory properly in options.");
                return;
            }

            if(MessageBox.Show("In order to view the replay with game you must close the game AND launcher first! Click \"OK\" when you have done that.", "Warning", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                if(File.Exists(Path.Combine(gameDir, file.Name)))
                    File.Delete(Path.Combine(gameDir, file.Name));
                File.Copy(file.FullName, Path.Combine(gameDir, file.Name));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error while copying replay file to game directory! Try starting this application in administrator mode.\r\nInfo (" + ex.GetType().FullName + "):\r\n" + ex.Message);
                return;
            }

            int fl;
            if(int.TryParse(cfg.vals["formflags"], out fl) && (fl & 1) == 0)
            {
                fl |= 1;
                cfg.vals["formflags"] = fl.ToString();

                if(cfg.vals["limitdelay"] == "0")
                    MessageBox.Show("If you want to reduce the delays while playing replay files, you can do so in Options.");
            }

            int ld;
            if(int.TryParse(cfg.vals["limitdelay"], out ld) && ld > 0)
            {
                try
                {
                    StreamReader frl = new StreamReader(Path.Combine(gameDir, file.Name));
                    string fre = frl.ReadToEnd();
                    frl.Close();

                    StringBuilder str = new StringBuilder();

                    int ind = fre.IndexOf("<ReplayCommandList>");
                    int mind = fre.IndexOf("</ReplayCommandList>");
                    if(ind >= 0 && mind >= 0)
                    {
                        ind += "<ReplayCommandList>".Length;
                        str.Append(fre.Substring(0, ind));

                        string[] cmd = fre.Substring(ind, mind - ind).Split(new string[] { "&#x0A;" }, StringSplitOptions.RemoveEmptyEntries);
                        for(int i = 0; i < cmd.Length; i++)
                        {
                            if(cmd[i].IndexOf('|') == -1)
                                continue;

                            int delay;
                            if(int.TryParse(cmd[i].Substring(0, cmd[i].IndexOf('|')), out delay) && delay > ld)
                                cmd[i] = ld.ToString() + cmd[i].Substring(cmd[i].IndexOf('|'));
                        }

                        str.Append(string.Join("&#x0A;", cmd) + "&#x0A;");

                        str.Append(fre.Substring(mind));

                        StreamWriter frw = new StreamWriter(Path.Combine(gameDir, file.Name), false);
                        frw.Write(str.ToString());
                        frw.Close();
                    }
                }
                catch
                {
                }
            }

            try
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.FileName = Path.Combine(gameDir, "Game.exe");
                proc.Arguments = Path.Combine(gameDir, file.Name);
                proc.WorkingDirectory = gameDir;
                Process.Start(proc);

                MessageBox.Show("Starting game - log in and the replay will start to play.\r\n\r\nDO NOT CLICK OK until the game has started to play the replay file or game will crash!");
                _doReplayDelete = Path.Combine(gameDir, file.Name);
                _doReplayDelete2 = 5000;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Couldn't start game! Try running this application in administrator mode.\r\nInfo (" + ex.GetType().FullName + "):\r\n" + ex.Message);
            }
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Thanks to ZenityAlpha for explaining how localization file format works.\r\nThanks to Diinsdale for idea of limiting replay file delays.\r\nAnd thanks for all people who reported bugs and crashes. :)");
        }
    }

    public sealed class Config
    {
        public Config()
        {
            vals["hidebroken"] = true.ToString();
            vals["maxshow"] = "100000";
            vals["localization"] = "english";
            vals["checkupdates"] = "1";
            vals["limitdelay"] = "0";
            vals["formflags"] = "0";
        }

        public readonly List<string> cardDirs = new List<string>();

        public readonly List<string> replayDirs = new List<string>();

        public readonly List<string> locDirs = new List<string>();

        public readonly Dictionary<string, string> vals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void Save()
        {
            try
            {
                StreamWriter f = new StreamWriter("config.txt", false);
                f.WriteLine("gamedata:");
                foreach(string x in cardDirs)
                    f.WriteLine(x);
                f.WriteLine();
                f.WriteLine("replays:");
                foreach(string x in replayDirs)
                    f.WriteLine(x);
                f.WriteLine();
                f.WriteLine("localization:");
                foreach(string x in locDirs)
                    f.WriteLine(x);
                f.WriteLine();
                f.WriteLine("vals:");
                foreach(KeyValuePair<string, string> x in vals)
                    f.WriteLine(x.Key + " = " + x.Value);
                f.Close();
            }
            catch
            {
            }
        }

        public void UpdateLoader(Form1 f)
        {
            Form1.Loader.CardDirectories.Clear();
            Form1.Loader.ReplayDirectories.Clear();
            Form1.Loader.LocalizationDirectories.Clear();

            Form1.Loader.CardDirectories.AddRange(cardDirs);
            Form1.Loader.ReplayDirectories.AddRange(replayDirs);
            Form1.Loader.LocalizationDirectories.AddRange(locDirs);

            Form1.Loader.SelectedLocalization = vals.ContainsKey("localization") ? vals["localization"] : "english";

            int ms;
            if(int.TryParse(vals.ContainsKey("maxshow") ? vals["maxshow"] : "none", out ms))
                f.toolStripTextBox1.Text = ms.ToString();

            bool ys;
            if(bool.TryParse(vals.ContainsKey("hidebroken") ? vals["hidebroken"] : "none", out ys))
                f.toolStripMenuItem5.Checked = ys;
        }

        public bool Load()
        {
            this.cardDirs.Clear();
            this.replayDirs.Clear();
            this.locDirs.Clear();

            try
            {
                StreamReader f = new StreamReader("config.txt");
                string l;
                string index = "";
                while((l = f.ReadLine()) != null)
                {
                    l = l.Trim();
                    if(string.IsNullOrEmpty(l))
                        continue;

                    if(l.EndsWith(":"))
                    {
                        index = l.Substring(0, l.IndexOf(':'));
                        continue;
                    }
                    if(index.Equals("gamedata", StringComparison.OrdinalIgnoreCase))
                        cardDirs.Add(l);
                    else if(index.Equals("replays", StringComparison.OrdinalIgnoreCase))
                        replayDirs.Add(l);
                    else if(index.Equals("localization", StringComparison.OrdinalIgnoreCase))
                        locDirs.Add(l);
                    else if(index.Equals("vals", StringComparison.OrdinalIgnoreCase))
                    {
                        Match m = valpattern.Match(l);
                        if(!m.Success)
                            continue;

                        vals[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim();
                    }
                }

                f.Close();
                return locDirs.Count != 0 && replayDirs.Count != 0 && cardDirs.Count != 0;
            }
            catch
            {
            }
            return false;
        }

        private static readonly Regex valpattern = new Regex(@"^(.+)=(.+)$", RegexOptions.Compiled);
    }
}
