using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Replays;
using System.Text.RegularExpressions;

namespace MMDoCHistoryV2
{
    public partial class Stats : Form
    {
        public Stats(Form1 f)
        {
            this.form = f;

            InitializeComponent();
        }

        private readonly Form1 form;

        internal void SetStats(List<Replay> replays)
        {
            textBox1.Text = replays.Count.ToString();
            textBox2.Text = replays.Count(e => e.Score > 0 && e.Finished).ToString();
            textBox4.Text = replays.Count(e => e.Score < 0 && e.Finished).ToString();
            textBox3.Text = replays.Count(e => e.Score == 0 && e.Finished).ToString();
            int total = replays.Count(e => e.Finished);
            textBox7.Text = total == 0 ? "0.00 %" : (((float)replays.Count(e => e.Score > 0 && e.Finished) / (float)total * 100.0f).ToString("0.00") + " %");
            textBox5.Text = total == 0 ? "0.00 %" : (((float)replays.Count(e => e.Score < 0 && e.Finished) / (float)total * 100.0f).ToString("0.00") + " %");
            textBox6.Text = total == 0 ? "0.00 %" : (((float)replays.Count(e => e.Score == 0 && e.Finished) / (float)total * 100.0f).ToString("0.00") + " %");

            textBox31.Text = replays.Count(e => e.Score > 0 && e.OwnerPlayer == 0 && e.Finished).ToString();
            textBox30.Text = replays.Count(e => e.Score > 0 && e.OwnerPlayer == 1 && e.Finished).ToString();
            textBox29.Text = total == 0 ? "0.00 %" : (((float)replays.Count(e => e.Score > 0 && e.OwnerPlayer == 0 && e.Finished) / (float)total * 100.0f).ToString("0.00") + " %");
            textBox28.Text = total == 0 ? "0.00 %" : (((float)replays.Count(e => e.Score > 0 && e.OwnerPlayer == 1 && e.Finished) / (float)total * 100.0f).ToString("0.00") + " %");

            textBox8.Text = replays.Count == 0 ? "0.00" : replays.Average(e => e.OwnerPlayer == 0 ? e.EloPlayer1 : e.EloPlayer2).ToString("0.00");
            textBox9.Text = replays.Count == 0 ? "0.00" : replays.Average(e => e.OwnerPlayer == 1 ? e.EloPlayer1 : e.EloPlayer2).ToString("0.00");
            textBox10.Text = replays.Count == 0 ? "0" : Form1.GetNumberCom(replays.Sum(e => e.EarnedXP));
            textBox11.Text = replays.Count == 0 ? "0" : Form1.GetNumberCom(replays.Sum(e => e.EarnedGold));

            // avg duration
            long totaldur = 0;
            long mydur = 0;
            long opdur = 0;
            int mt = 0;
            int ot = 0;
            {
                int c = 0;
                foreach(Replay r in replays)
                {
                    if(!r.Finished)
                        continue;

                    c++;
                    totaldur += r.Duration;
                    mydur += r.OwnerPlayer == 0 ? r.Player1TurnsDuration : r.Player2TurnsDuration;
                    opdur += r.OwnerPlayer == 1 ? r.Player1TurnsDuration : r.Player2TurnsDuration;
                    mt += r.OwnerPlayer == 0 ? r.TurnsPlayer1 : r.TurnsPlayer2;
                    ot += r.OwnerPlayer == 1 ? r.TurnsPlayer1 : r.TurnsPlayer2;
                }

                long dur = 0;
                if(c > 0)
                    dur = totaldur / c;

                dur /= 1000;
                TimeSpan ad = new TimeSpan(0, 0, (int)dur);
                textBox12.Text = Form1.GetDuration(ad);
            }

            // avg turns
            int myEloInc = 0;
            int myEloDec = 0;
            int opEloInc = 0;
            int opEloDec = 0;
            {
                int tur = 0;
                int c = 0;
                foreach(Replay r in replays)
                {
                    if(!r.Finished)
                        continue;

                    c++;
                    tur += r.OwnerPlayer == 0 ? r.TurnsPlayer1 : r.TurnsPlayer2;
                    if(r.OwnerPlayer == 0)
                    {
                        myEloInc += Math.Max(0, r.NewEloPlayer1 - r.EloPlayer1);
                        myEloDec += Math.Min(0, r.NewEloPlayer1 - r.EloPlayer1);
                        opEloInc += Math.Max(0, r.NewEloPlayer2 - r.EloPlayer2);
                        opEloDec += Math.Min(0, r.NewEloPlayer2 - r.EloPlayer2);
                    }
                    else
                    {
                        opEloInc += Math.Max(0, r.NewEloPlayer1 - r.EloPlayer1);
                        opEloDec += Math.Min(0, r.NewEloPlayer1 - r.EloPlayer1);
                        myEloInc += Math.Max(0, r.NewEloPlayer2 - r.EloPlayer2);
                        myEloDec += Math.Min(0, r.NewEloPlayer2 - r.EloPlayer2);
                    }
                }

                if(c > 0)
                    tur /= c;

                textBox13.Text = tur.ToString();
            }

            textBox14.Text = Form1.GetNumberCom(myEloInc);
            textBox15.Text = Form1.GetNumberCom(-myEloDec);
            textBox16.Text = Form1.GetNumberCom(opEloInc);
            textBox17.Text = Form1.GetNumberCom(-opEloDec);

            textBox18.Text = Form1.GetDuration(new TimeSpan(0, 0, (int)(totaldur / 1000)));
            textBox19.Text = (replays.Count > 0 ? ((float)replays.Count(e => e.OwnerPlayer == 0) / (float)replays.Count * 100.0f) : 0.0f).ToString("0.00") + " %";

            textBox22.Text = Form1.GetDuration(new TimeSpan(0, 0, (int)(mydur / 1000)));
            textBox23.Text = Form1.GetDuration(new TimeSpan(0, 0, (int)(opdur / 1000)));
            textBox25.Text = Form1.GetDuration(new TimeSpan(0, 0, mt == 0 ? 0 : (int)((mydur / mt) / 1000)));
            textBox24.Text = Form1.GetDuration(new TimeSpan(0, 0, ot == 0 ? 0 : (int)((opdur / ot) / 1000)));

            // highest win streak
            SortedDictionary<DateTime, Replay> sortTime = new SortedDictionary<DateTime, Replay>();
            foreach(Replay x in replays)
            {
                sortTime[x.Time] = x;
            }

            {
                int curstreak = 0;
                int highest = 0;

                foreach(KeyValuePair<DateTime, Replay> x in sortTime)
                {
                    if(!x.Value.Finished)
                        continue;

                    if(x.Value.Score > 0)
                    {
                        if(++curstreak > highest)
                            highest = curstreak;
                    }
                    else
                    {
                        curstreak = 0;
                    }
                }

                textBox20.Text = highest.ToString();
            }

            // highest lose streak
            {
                int curstreak = 0;
                int highest = 0;

                foreach(KeyValuePair<DateTime, Replay> x in sortTime)
                {
                    if(!x.Value.Finished)
                        continue;

                    if(x.Value.Score < 0)
                    {
                        if(++curstreak > highest)
                            highest = curstreak;
                    }
                    else
                    {
                        curstreak = 0;
                    }
                }

                textBox21.Text = highest.ToString();
            }

            // mulligans
            {
                int me0 = 0;
                int me1 = 0;
                int op0 = 0;
                int op1 = 0;

                foreach(Replay r in replays)
                {
                    bool did = false;

                    byte d = 0;
                    foreach(string x in r.ReplayCommandList)
                    {
                        if(!x.Contains('|'))
                            continue;

                        string y = x.Substring(x.IndexOf('|') + 1).ToLower();
                        if(y == "startgame")
                        {
                            did = true;
                            break;
                        }
                        if(y.StartsWith("mulligan"))
                        {
                            Match m = mull.Match(y);
                            if(m.Success)
                            {
                                int plrid = int.Parse(m.Groups[1].Value);
                                int took = int.Parse(m.Groups[2].Value);
                                if(r.OwnerPlayer == plrid)
                                {
                                    if(took == 0)
                                        d |= 1;
                                }
                                else
                                {
                                    if(took == 0)
                                        d |= 2;
                                }
                            }
                        }
                    }

                    if(!did)
                        continue;

                    if((d & 1) != 0)
                        me1++;
                    else
                        me0++;

                    if((d & 2) != 0)
                        op1++;
                    else
                        op0++;
                }

                textBox26.Text = ((me0 + me1) == 0 ? "0.00" : ((float)me1 / (float)(me0 + me1) * 100.0f).ToString("0.00")) + " %";
                textBox27.Text = ((op0 + op1) == 0 ? "0.00" : ((float)op1 / (float)(op0 + op1) * 100.0f).ToString("0.00")) + " %";
            }
        }

        private static readonly Regex mull = new Regex(@"^mulligan ([01]) ([01]) 0 ", RegexOptions.Compiled);

        private void Stats_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.ClosedForm(this);
        }

        private void label23_Click(object sender, EventArgs e)
        {

        }
    }
}
