using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MMDoCHistoryV2.ConditionTypes;

namespace MMDoCHistoryV2
{
    public partial class EditFilter : Form
    {
        public EditFilter(Form1 f)
        {
            form = f;

            InitializeComponent();
        }

        private readonly Form1 form;

        private Filter filter = null;

        private bool enEvent = true;

        internal EditCondition conForm = null;

        internal void SetFilter(Filter f)
        {
            enEvent = false;
            this.listView1.Enabled = false;
            this.listView1.Items.Clear();
            this.listView1.Enabled = true;

            filter = f;

            if(filter != null)
            {
                textBox1.Text = filter.Name;
                this.Text = "Filter \"" + filter.Name + "\"";
                this.listView1.Enabled = false;
                foreach(Condition c in filter.Conditions)
                {
                    this.listView1.Items.Add(new ListViewItem(new string[] { c.ToString(), (c.Flags & ConditionFlags.Or) != ConditionFlags.None ? "Y" : "N" }));
                }
                this.listView1.Enabled = true;
            }

            enEvent = true;
        }

        private void OpenEditCondition(Condition con)
        {
            if(conForm != null)
                conForm.Close();

            conForm = new EditCondition(form, this);
            conForm.Show();
            conForm.SetData(filter, con);
        }

        private void DeleteConditions(IEnumerable<Condition> cons)
        {
            int index = -1;

            if(conForm != null)
            {
                if(conForm.condition != null)
                {
                    if(cons.Contains(conForm.condition))
                        index = this.filter.Conditions.IndexOf(conForm.condition);
                }
                conForm.Close();
            }

            if(index != -1)
                this.filter.Conditions.RemoveAt(index);

            this.filter.Conditions.RemoveAll(e => cons.Contains(e));
            this.SetFilter(filter);
        }

        private void NewCondition()
        {
            if(conForm != null)
                conForm.Close();

            Game_end con = new Game_end();
            this.filter.Conditions.Add(con);
            conForm = new EditCondition(form, this);
            conForm.Show();
            conForm.SetData(filter, con);
        }

        private void EditFilter_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.ClosedForm(this);
            form.UpdateFilters();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(filter != null)
            {
                filter.Name = textBox1.Text;
                this.Text = "Filter \"" + filter.Name + "\"";
            }
        }

        private void newConditionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewCondition();
        }

        private void editConditionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count != 0)
                this.OpenEditCondition(this.filter.Conditions[this.listView1.Items.IndexOf(this.listView1.SelectedItems[0])]);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count != 0)
            {
                List<Condition> toremove = new List<Condition>();
                foreach(ListViewItem itm in this.listView1.SelectedItems)
                {
                    toremove.Add(this.filter.Conditions[this.listView1.Items.IndexOf(itm)]);
                }

                this.DeleteConditions(toremove);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if(this.listView1.SelectedItems.Count > 0)
            {
                deleteToolStripMenuItem.Enabled = true;
                editConditionToolStripMenuItem.Enabled = true;
                toolStripMenuItem1.Enabled = true;
                toolStripMenuItem2.Enabled = true;
            }
            else
            {
                deleteToolStripMenuItem.Enabled = false;
                editConditionToolStripMenuItem.Enabled = false;
                toolStripMenuItem1.Enabled = false;
                toolStripMenuItem2.Enabled = false;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SortedDictionary<int, Condition> moveup = new SortedDictionary<int, Condition>();
            foreach(ListViewItem itm in this.listView1.SelectedItems)
            {
                int index = this.listView1.Items.IndexOf(itm);
                moveup[index] = this.filter.Conditions[index];
            }
            while(moveup.Count > 0)
            {
                KeyValuePair<int, Condition> last = moveup.ElementAt(moveup.Count - 1);
                int nowindex = filter.Conditions.IndexOf(last.Value);
                if(nowindex > 0)
                {
                    filter.Conditions.RemoveAt(nowindex);
                    filter.Conditions.Insert(nowindex - 1, last.Value);
                }
                moveup.Remove(last.Key);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SortedDictionary<int, Condition> moveup = new SortedDictionary<int, Condition>();
            foreach(ListViewItem itm in this.listView1.SelectedItems)
            {
                int index = this.listView1.Items.IndexOf(itm);
                moveup[index] = this.filter.Conditions[index];
            }
            while(moveup.Count > 0)
            {
                KeyValuePair<int, Condition> last = moveup.ElementAt(0);
                int nowindex = filter.Conditions.IndexOf(last.Value);
                if(nowindex < filter.Conditions.Count - 1)
                {
                    filter.Conditions.RemoveAt(nowindex);
                    filter.Conditions.Insert(nowindex + 1, last.Value);
                }
                moveup.Remove(last.Key);
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if(this.listView1.SelectedItems.Count != 0)
                this.OpenEditCondition(this.filter.Conditions[this.listView1.Items.IndexOf(this.listView1.SelectedItems[0])]);
        }
    }
}
