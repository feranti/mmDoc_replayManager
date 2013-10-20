using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MMDoCHistoryV2
{
    public partial class Options : Form
    {
        public Options(Form1 f)
        {
            form = f;

            InitializeComponent();
        }

        private readonly Form1 form;

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.ClosedForm(this);
        }

        private void Options_Load(object sender, EventArgs e)
        {
            foreach(string x in form.cfg.replayDirs)
                listBox1.Items.Add(x);

            foreach(string x in form.cfg.cardDirs)
                listBox2.Items.Add(x);

            foreach(string x in form.cfg.locDirs)
                listBox3.Items.Add(x);

            textBox1.Text = form.cfg.vals["localization"];
            int d;
            if(int.TryParse(form.cfg.vals["checkupdates"] ?? "", out d))
                checkBox1.Checked = d != 0;

            int ldel = 0;
            if(!int.TryParse(form.cfg.vals["limitdelay"], out ldel) || ldel < 0)
                ldel = 0;
            numericUpDown1.Value = ldel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.AddNewDir(0);
        }

        private void AddNewDir(int index)
        {
            FolderBrowserDialog op = new FolderBrowserDialog();
            op.Description = "Choose folder...";
            op.ShowNewFolderButton = false;
            if(op.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if(index == 0)
            {
                if(form.cfg.replayDirs.Contains(op.SelectedPath, StringComparer.OrdinalIgnoreCase))
                    return;
                form.cfg.replayDirs.Add(op.SelectedPath);
                form.cfg.UpdateLoader(form);
                listBox1.Items.Add(op.SelectedPath);
                return;
            }

            if(index == 1)
            {
                if(form.cfg.cardDirs.Contains(op.SelectedPath, StringComparer.OrdinalIgnoreCase))
                    return;
                form.cfg.cardDirs.Add(op.SelectedPath);
                form.cfg.UpdateLoader(form);
                listBox2.Items.Add(op.SelectedPath);
                return;
            }

            if(index == 2)
            {
                if(form.cfg.locDirs.Contains(op.SelectedPath, StringComparer.OrdinalIgnoreCase))
                    return;
                form.cfg.locDirs.Add(op.SelectedPath);
                form.cfg.UpdateLoader(form);
                listBox3.Items.Add(op.SelectedPath);
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.AddNewDir(1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.AddNewDir(2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ListBox l = listBox1;
            if(l.SelectedIndex >= 0 && l.SelectedIndex < l.Items.Count)
            {
                string dir = l.Items[l.SelectedIndex] as string;
                form.cfg.replayDirs.Remove(dir);
                form.cfg.UpdateLoader(form);
                l.Items.RemoveAt(l.SelectedIndex);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ListBox l = listBox2;
            if(l.SelectedIndex >= 0 && l.SelectedIndex < l.Items.Count)
            {
                string dir = l.Items[l.SelectedIndex] as string;
                form.cfg.cardDirs.Remove(dir);
                form.cfg.UpdateLoader(form);
                l.Items.RemoveAt(l.SelectedIndex);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ListBox l = listBox3;
            if(l.SelectedIndex >= 0 && l.SelectedIndex < l.Items.Count)
            {
                string dir = l.Items[l.SelectedIndex] as string;
                form.cfg.locDirs.Remove(dir);
                form.cfg.UpdateLoader(form);
                l.Items.RemoveAt(l.SelectedIndex);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            form.cfg.vals["localization"] = textBox1.Text;
            form.cfg.UpdateLoader(form);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            form.cfg.vals["checkupdates"] = checkBox1.Checked ? "1" : "0";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            form.cfg.vals["limitdelay"] = numericUpDown1.Value.ToString();
        }
    }
}
