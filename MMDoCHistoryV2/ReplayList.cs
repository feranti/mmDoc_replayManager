using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Replays;
using System.Windows.Forms;
using System.Drawing;

namespace MMDoCHistoryV2
{
    public partial class Form1
    {
        private bool NeedReplayUpdate = false;

        private ReplayUpdater CurrentUpdater = null;

        private readonly Dictionary<string, Replay> DisplayedReplays = new Dictionary<string, Replay>();

        private bool UpdateReplays()
        {
            if(this.CurrentUpdater != null)
                return false;

            if(this.StopReplayUpdater())
                return false;

            if(Form1.Loader.IsLoading)
                return false;

            List<Filter> filters = new List<Filter>();
            foreach(Filter x in this.Filters)
            {
                if((x.Flags & FilterFlags.Disabled) != FilterFlags.None)
                    continue;

                filters.Add(x);
            }
            int limit = (this.checkBox1.Checked ? 0 : 1) |
                (this.checkBox2.Checked ? 0 : 2) |
                (this.checkBox3.Checked ? 0 : 4) |
                (this.checkBox4.Checked ? 0 : 8);
            string nlimit = this.textBox1.Text;
            string hlimit = this.textBox2.Text;
            string vtext = this.numericUpDown1.Text;
            int lastonly;
            if(!int.TryParse(vtext, out lastonly) || lastonly < 0)
                lastonly = 0;
            if(this.checkBox5.Checked)
            {
                if(!string.IsNullOrEmpty(nlimit) ||
                    !string.IsNullOrEmpty(hlimit) ||
                    limit != 0)
                    lastonly = 9999999;
            }
            lock(Loader.LoadedReplays)
            {
                this.CurrentUpdater = new ReplayUpdater(Loader.LoadedReplays, filters, this.listView2.Font, limit, nlimit, lastonly, hlimit);
            }
            this.NeedReplayUpdate = false;
            return true;
        }

        internal bool StopReplayUpdater()
        {
            return this.OpenedForms.Any(e => e is EditFilter || e is EditCondition);
        }

        private void CheckUpdater()
        {
            if(this.CurrentUpdater == null)
                return;

            if(this.StopReplayUpdater())
            {
                this.CurrentUpdater.Stop();
                this.CurrentUpdater = null;
                NeedReplayUpdate = true;
                return;
            }

            if(this.CurrentUpdater.IsRunning)
            {
                if(this.toolStripStatusLabel2.Text == "Filtering.")
                    this.toolStripStatusLabel2.Text = "Filtering..";
                else if(this.toolStripStatusLabel2.Text == "Filtering..")
                    this.toolStripStatusLabel2.Text = "Filtering...";
                else
                    this.toolStripStatusLabel2.Text = "Filtering.";
                return;
            }

            List<ListViewItem> filtered = this.CurrentUpdater.GetResult();
            bool ys;
            if(cfg.vals.ContainsKey("hidebroken") && bool.TryParse(cfg.vals["hidebroken"], out ys) && ys)
            {
                for(int i = filtered.Count - 1; i >= 0; i--)
                {
                    if(string.IsNullOrWhiteSpace(filtered[i].SubItems[1].Text) ||
                        string.IsNullOrWhiteSpace(filtered[i].SubItems[2].Text) ||
                        filtered[i].SubItems[1].Text.Equals("Local_Player_1", StringComparison.OrdinalIgnoreCase) ||
                        filtered[i].SubItems[1].Text.Equals("Local_Player_2", StringComparison.OrdinalIgnoreCase) ||
                        filtered[i].SubItems[2].Text.Equals("Local_Player_1", StringComparison.OrdinalIgnoreCase) ||
                        filtered[i].SubItems[2].Text.Equals("Local_Player_2", StringComparison.OrdinalIgnoreCase))
                        filtered.RemoveAt(i);
                }
            }
            int maxAllow;
            if(int.TryParse(cfg.vals.ContainsKey("maxshow") ? cfg.vals["maxshow"] : "none", out maxAllow) && maxAllow >= 0 && filtered.Count > maxAllow)
            {
                SortedDictionary<DateTime, List<ListViewItem>> sortal = new SortedDictionary<DateTime, List<ListViewItem>>();
                foreach(ListViewItem x in filtered)
                {
                    DateTime tim = Form1.GetTime(x.SubItems[0].Text);
                    if(!sortal.ContainsKey(tim))
                        sortal[tim] = new List<ListViewItem>();
                    sortal[tim].Add(x);
                }

                filtered.Clear();
                while(maxAllow > 0 && sortal.Count > 0)
                {
                    KeyValuePair<DateTime, List<ListViewItem>> x = sortal.ElementAt(sortal.Count - 1);
                    while(maxAllow > 0 && x.Value.Count > 0)
                    {
                        filtered.Add(x.Value[0]);
                        x.Value.RemoveAt(0);
                        maxAllow--;
                    }
                    sortal.Remove(x.Key);
                }
            }

            /*bool mustEdit = false;
            if(this.listView2.Items.Count != filtered.Count)
                mustEdit = true;

            // Only update list if it's necessary otherwise user selections get messed up
            TODO();

            if(mustEdit)*/
            {
                this.listView2.Enabled = false;
                SortOrder ord = this.listView2.Sorting;
                this.listView2.Sorting = SortOrder.None;
                this.listView2.Items.Clear();
                this.listView2.BeginUpdate();
                this.listView2.Items.AddRange(filtered.ToArray());
                this.listView2.EndUpdate();
                this.listView2.Sorting = ord;
                this.listView2.Enabled = true;
                this.listView2.Sort();
            }

            this.DisplayedReplays.Clear();
            int win = 0;
            int lose = 0;
            int draw = 0;
            foreach(ListViewItem itm in filtered)
            {
                Replay rep = this.GetReplay(itm);
                if(rep.Finished)
                {
                    if(rep.Score > 0)
                        win++;
                    else if(rep.Score < 0)
                        lose++;
                    else
                        draw++;
                }
                this.DisplayedReplays[itm.ToolTipText] = rep;
            }

            int total = win + lose + draw;
            this.toolStripStatusLabel2.Text = "Displaying " + this.listView2.Items.Count.ToString() + " replay" + (this.listView2.Items.Count == 1 ? "" : "s") + ".";
            if(total > 0)
                this.toolStripStatusLabel2.Text += " Win: " + ((float)win / (float)total * 100.0f).ToString("0.00") + " % | Lose: " + ((float)lose / (float)total * 100.0f).ToString("0.00") + " % | Draw: " + ((float)draw / (float)total * 100.0f).ToString("0.00") + " %.";

            this.UpdateStatsWindow();

            this.CurrentUpdater = null;
        }

        private void LoadReplays()
        {
            /*int cardCount = 0;
            int locCount = 0;
            int repCount = 0;
            lock(Loader.LoadedCards)
            {
                cardCount = Loader.LoadedCards.Count;
            }
            lock(Loader.LoadedReplays)
            {
                repCount = Loader.LoadedReplays.Count;
            }
            lock(Loader.LoadedLocalizations)
            {
                locCount = Loader.LoadedLocalizations.Count;
            }
            if(cardCount != 0 || locCount != 0 ||
                repCount != 0)
                return;*/

            Loader.StartLoad(false);
        }

        private void UnloadReplays()
        {
            Loader.StopLoad();
            Loader = new Loader();
            this.cfg.UpdateLoader(this);
        }
    }

    /// <summary>
    /// Filtering replays is an expensive process so we will do it on another thread or the user interface
    /// becomes nonresponsive.
    /// </summary>
    internal sealed class ReplayUpdater
    {
        internal ReplayUpdater(List<Replay> replays, List<Filter> filters, Font font, int limit, string nlimit, int lastonly, string hlimit)
        {
            this.Replays = new SortedDictionary<DateTime, Replay>();
            foreach(Replay x in replays)
                this.Replays[x.Time] = x;
            //if(lastonly != 0 && this.Replays.Count > lastonly)
            {
                DateTime[] keys = this.Replays.Keys.ToArray();
                int i = 0;
                while(this.Replays.Count > lastonly)
                    this.Replays.Remove(keys[i++]);
                for(; i < keys.Length; i++)
                {
                    Replay r = this.Replays[keys[i]];
                    if((limit & 1) != 0 && r.Score > 0)
                    {
                        this.Replays.Remove(r.Time);
                        continue;
                    }
                    if((limit & 2) != 0 && r.Score < 0)
                    {
                        this.Replays.Remove(r.Time);
                        continue;
                    }
                    if((limit & 4) != 0 && r.Score == 0 && r.Finished)
                    {
                        this.Replays.Remove(r.Time);
                        continue;
                    }
                    if((limit & 8) != 0 && !r.Finished)
                    {
                        this.Replays.Remove(r.Time);
                        continue;
                    }
                    if(!string.IsNullOrEmpty(nlimit))
                    {
                        if((r.NamePlayer1 == null || r.NamePlayer1.IndexOf(nlimit, StringComparison.OrdinalIgnoreCase) == -1) &&
                            (r.NamePlayer2 == null || r.NamePlayer2.IndexOf(nlimit, StringComparison.OrdinalIgnoreCase) == -1))
                        {
                            this.Replays.Remove(r.Time);
                            continue;
                        }
                    }
                    if(!string.IsNullOrEmpty(hlimit))
                    {
                        string myhero = null;
                        if(r.OwnerPlayer == 0)
                        {
                            if(r.DeckPlayer1.Count > 0)
                                myhero = r.DeckPlayer1[0];
                        }
                        else if(r.OwnerPlayer == 1)
                        {
                            if(r.DeckPlayer2.Count > 0)
                                myhero = r.DeckPlayer2[0];
                        }
                        if(myhero != null && myhero.Contains(','))
                        {
                            myhero = myhero.Substring(myhero.IndexOf(',') + 1).Trim();
                            int myid;
                            if(int.TryParse(myhero, out myid))
                            {
                                string hn = Form1.Loader.GetCardName(myid);
                                if(!string.IsNullOrEmpty(hn) && hn.IndexOf(hlimit, StringComparison.OrdinalIgnoreCase) == -1)
                                {
                                    this.Replays.Remove(r.Time);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            this.Filters = filters;
            lvfont = font;

            thread = new Thread(Run);
            thread.Start();
        }

        private Font lvfont;
        private Thread thread;
        private readonly SortedDictionary<DateTime, Replay> Replays;
        private readonly List<Filter> Filters;
        private long _isRunning = 1;
        private long _isStopping = 0;
        private readonly List<ListViewItem> Result = new List<ListViewItem>();

        private void Run()
        {
            //if(this.Filters.Count != 0)
            {
                foreach(KeyValuePair<DateTime, Replay> y in this.Replays)
                {
                    if(Interlocked.Read(ref _isStopping) != 0)
                        break;

                    Replay replay = y.Value;
                    bool allow = true;
                    for(int x = 0; x < this.Filters.Count; x++)
                    {
                        Filter f = this.Filters[x];
                        if(!f.Test(replay))
                        {
                            allow = false;
                            break;
                        }
                    }

                    if(allow)
                    {
                        Replay x = replay; // lazy

                        string myHero = "";
                        string opHero = "";
                        if(x.DeckPlayer1.Count != 0)
                        {
                            try
                            {
                                string h = Form1.Loader.GetCardName(int.Parse(x.DeckPlayer1[0].Substring(x.DeckPlayer1[0].IndexOf(',') + 1).Trim()));
                                if(h != null)
                                {
                                    if(x.OwnerPlayer == 0)
                                        myHero = h;
                                    else
                                        opHero = h;
                                }
                            }
                            catch
                            {
                            }
                        }
                        if(x.DeckPlayer2.Count != 0)
                        {
                            try
                            {
                                string h = Form1.Loader.GetCardName(int.Parse(x.DeckPlayer2[0].Substring(x.DeckPlayer2[0].IndexOf(',') + 1).Trim()));
                                if(h != null)
                                {
                                    if(x.OwnerPlayer == 1)
                                        myHero = h;
                                    else
                                        opHero = h;
                                }
                            }
                            catch
                            {
                            }
                        }

                        ListViewItem itm = new ListViewItem(new string[] {
                        Form1.GetTime(x.Time), x.OwnerPlayer == 0 ? x.NamePlayer1 : x.NamePlayer2,
                        x.OwnerPlayer == 0 ? x.NamePlayer2 : x.NamePlayer1,
                        x.OwnerPlayer == 0 ? x.EloPlayer1.ToString() :
                        x.EloPlayer2.ToString(), x.OwnerPlayer == 0 ? x.EloPlayer2.ToString() :
                        x.EloPlayer1.ToString(), 
                        !x.Finished ? "" : (x.Score > 0 ? "Win" : (x.Score < 0 ? "Lose" : "Draw")),
                        myHero, opHero, x.OwnerPlayer == 0 ? "Yes" : "No",
                        x.OwnerPlayer == 0 ? x.TurnsPlayer1.ToString() : x.TurnsPlayer2.ToString(),
                        Form1.GetDuration(new TimeSpan(0, 0, x.Duration / 1000)),
                        Form1.GetDuration(new TimeSpan(0, 0, (x.OwnerPlayer == 0 ? x.Player1TurnsDuration : x.Player2TurnsDuration) / 1000)),
                        Form1.GetDuration(new TimeSpan(0, 0, (x.OwnerPlayer == 1 ? x.Player1TurnsDuration : x.Player2TurnsDuration) / 1000))}, -1, Color.Black,
                        GetReplayColor(x), lvfont);
                        itm.ToolTipText = x.FileName;
                        this.Result.Add(itm);
                    }
                }
            }

            // Last line.
            this.IsRunning = false;
        }

        private Color GetReplayColor(Replay replay)
        {
            if(!replay.Finished)
                return Color.White;
            if(replay.Score < 0)
                return Color.FromArgb(255, 225, 225);
            if(replay.Score > 0)
                return Color.FromArgb(225, 255, 225);
            return Color.FromArgb(225, 225, 225);
        }

        internal bool IsRunning
        {
            get
            {
                return Interlocked.Read(ref _isRunning) != 0;
            }
            private set
            {
                Interlocked.Exchange(ref _isRunning, value ? 1 : 0);
            }
        }

        internal void Stop()
        {
            Interlocked.Exchange(ref _isStopping, 1);

            while(IsRunning)
                Thread.Sleep(1);
        }

        internal List<ListViewItem> GetResult()
        {
            if(this.IsRunning)
                return null;

            return this.Result;
        }
    }
}
