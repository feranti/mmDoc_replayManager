using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Replays;

namespace MMDoCHistoryV2
{
    public partial class GameLog : Form
    {
        public GameLog(Form1 f)
        {
            form = f;

            InitializeComponent();
        }

        private readonly Form1 form;

        internal Replay setted = null;

        private CustomRTF rtf = null;

        internal void SetReplay(Replays.Replay replay)
        {
            setted = replay;
            Replays.Player player = new Replays.Player(replay, Form1.Loader);
            List<string> game = player.Parse();
            rtf.Clear(true);
            foreach(string x in game)
            {
                rtf.AddLine("|cffffffff" + x, true);
            }
            rtf.Update();
            this.Text = "Game log " + (replay.ToString() ?? "");
            rtf.ScrollToEnd();
        }

        private void GameLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.ClosedForm(this);
        }

        private void GameLog_Load(object sender, EventArgs e)
        {
            rtf = new CustomRTF(richTextBox1);
        }
    }
}
