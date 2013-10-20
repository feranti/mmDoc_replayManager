using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace MMDoCHistoryV2
{
    public partial class EditCondition : Form
    {
        public EditCondition(Form1 f, EditFilter fil)
        {
            form = f;
            filform = fil;

            InitializeComponent();
        }

        private readonly Form1 form;
        private readonly EditFilter filform;

        internal Condition condition = null;

        private Filter filter = null;

        private Condition cur = null;

        internal void SetData(Filter f, Condition c)
        {
            filter = f;
            condition = c;
            cur = condition;

            this.Reload();
        }

        private bool enEvent = false;

        private readonly Dictionary<ConditionValue, Panel> econtrols = new Dictionary<ConditionValue, Panel>();

        internal void Reload()
        {
            if(cur == null)
            {
                comboBox1.SelectedIndex = 0;
                return;
            }

            enEvent = false;

            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(cur.GetType().Name.Replace("_", " "));
            numericUpDown1.Value = cur.MinTurn;
            numericUpDown2.Value = cur.MaxTurn;
            numericUpDown4.Value = cur.MinTime;
            numericUpDown3.Value = cur.MaxTime;
            checkBox1.Checked = (cur.Flags & ConditionFlags.RequireMyTurn) != ConditionFlags.None;
            checkBox2.Checked = (cur.Flags & ConditionFlags.RequireOpponentTurn) != ConditionFlags.None;
            checkBox4.Checked = (cur.Flags & ConditionFlags.Or) != ConditionFlags.None;
            checkBox3.Checked = (cur.Flags & ConditionFlags.Invert) != ConditionFlags.None;
            checkBox5.Checked = (cur.Flags & ConditionFlags.RequireAfterMulligan) != ConditionFlags.None;
            checkBox6.Checked = (cur.Flags & ConditionFlags.RequireBeforeMulligan) != ConditionFlags.None;
            checkBox7.Checked = (cur.Flags & ConditionFlags.RequireAfterGameStart) != ConditionFlags.None;
            checkBox8.Checked = (cur.Flags & ConditionFlags.RequireBeforeGameStart) != ConditionFlags.None;

            foreach(KeyValuePair<ConditionValue, Panel> x in this.econtrols)
                this.groupBox1.Controls.Remove(x.Value);
            this.econtrols.Clear();
            this.eventIndexes.Clear();

            if(cur.Values != null && cur.Values.Length != 0)
            {
                int na = -1;
                foreach(ConditionValue x in cur.Values)
                {
                    na++;
                    if(x.AllowComparison == ConditionComparison.None ||
                        x.Type == ConditionValueTypes.None)
                        continue;

                    Panel p = new Panel();
                    p.Size = new System.Drawing.Size(10, 32);
                    p.Dock = DockStyle.Top;
                    Label l = new Label();
                    l.Text = x.Name + ":";
                    l.Location = new Point(10, 8);
                    l.Size = new System.Drawing.Size(60, 20);
                    p.Controls.Add(l);

                    ComboBox cb = new ComboBox();
                    if((x.AllowComparison & ConditionComparison.Equal) != ConditionComparison.None)
                        cb.Items.Add("Is equal to");
                    if((x.AllowComparison & ConditionComparison.NotEqual) != ConditionComparison.None)
                        cb.Items.Add("Is not equal to");
                    if((x.AllowComparison & ConditionComparison.Greater) != ConditionComparison.None)
                        cb.Items.Add("Is greater than");
                    if((x.AllowComparison & (ConditionComparison.Greater | ConditionComparison.Equal)) == (ConditionComparison.Greater | ConditionComparison.Equal))
                        cb.Items.Add("Is equal to or greater than");
                    if((x.AllowComparison & ConditionComparison.Less) != ConditionComparison.None)
                        cb.Items.Add("Is less than");
                    if((x.AllowComparison & (ConditionComparison.Less | ConditionComparison.Equal)) == (ConditionComparison.Less | ConditionComparison.Equal))
                        cb.Items.Add("Is equal to or less than");
                    if((x.AllowComparison & ConditionComparison.Contains) != ConditionComparison.None)
                        cb.Items.Add("Contains");
                    if((x.AllowComparison & ConditionComparison.NotContains) != ConditionComparison.None)
                        cb.Items.Add("Does not contain");
                    if((x.Comparison & ConditionComparison.Equal) != ConditionComparison.None)
                    {
                        if((x.Comparison & ConditionComparison.Greater) != ConditionComparison.None)
                            cb.SelectedIndex = cb.Items.IndexOf("Is equal to or greater than");
                        else
                            cb.SelectedIndex = cb.Items.IndexOf("Is equal to");
                    }
                    else if((x.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None)
                    {
                        cb.SelectedIndex = cb.Items.IndexOf("Is not equal to");
                    }
                    else if((x.Comparison & ConditionComparison.Contains) != ConditionComparison.None)
                    {
                        cb.SelectedIndex = cb.Items.IndexOf("Contains");
                    }
                    else if((x.Comparison & ConditionComparison.NotContains) != ConditionComparison.None)
                    {
                        cb.SelectedIndex = cb.Items.IndexOf("Does not contain");
                    }
                    else if((x.Comparison & ConditionComparison.Greater) != ConditionComparison.None)
                    {
                        cb.SelectedIndex = cb.Items.IndexOf("Is greater than");
                    }
                    else if((x.Comparison & ConditionComparison.Less) != ConditionComparison.None)
                    {
                        cb.SelectedIndex = cb.Items.IndexOf("Is less than");
                    }
                    else
                        cb.SelectedIndex = 0;
                    int offset = -40;
                    cb.Size = new Size(120, 21);
                    cb.Location = new Point(120 + offset, 8);
                    cb.SelectedIndexChanged += new EventHandler(cb_SelectedIndexChanged);
                    eventIndexes[cb] = na;
                    p.Controls.Add(cb);

                    if(x.Restricted != null && x.Restricted.Count != 0)
                    {
                        ComboBox cb2 = new ComboBox();
                        foreach(object y in x.Restricted)
                        {
                            cb2.Items.Add(y.ToString());
                            if(x.Value.ToString().Equals(y))
                                cb2.SelectedIndex = cb2.Items.Count - 1;
                        }
                        cb2.Location = new Point(260 + offset, 8);
                        cb2.Size = new Size(120, 21);
                        //cb2.SelectedIndex = 0;
                        cb2.SelectedIndexChanged += new EventHandler(cb2_SelectedIndexChanged);
                        eventIndexes[cb2] = na;
                        p.Controls.Add(cb2);
                    }
                    else
                    {
                        switch(x.Type)
                        {
                            case ConditionValueTypes.Bool:
                                {
                                    ComboBox cb2 = new ComboBox();
                                    cb2.Items.Add("Yes");
                                    cb2.Items.Add("No");
                                    cb2.Location = new Point(260 + offset, 8);
                                    cb2.Size = new Size(120, 21);
                                    cb2.SelectedIndex = (x.Value is bool && ((bool)x.Value)) ? 0 : 1;
                                    cb2.SelectedIndexChanged += new EventHandler(cb2_SelectedIndexChanged2);
                                    eventIndexes[cb2] = na;
                                    p.Controls.Add(cb2);
                                }break;

                            case ConditionValueTypes.String:
                            case ConditionValueTypes.Dynamic:
                                {
                                    TextBox tb = new TextBox();
                                    tb.Location = new Point(260 + offset, 8);
                                    tb.Size = new Size(120, 21);
                                    tb.Text = x.Value as string;
                                    tb.TextChanged += new EventHandler(tb_TextChanged);
                                    eventIndexes[tb] = na;
                                    p.Controls.Add(tb);
                                }break;

                            case ConditionValueTypes.Integer:
                                {
                                    NumericUpDown nb = new NumericUpDown();
                                    nb.Minimum = int.MinValue;
                                    nb.Maximum = int.MaxValue;
                                    nb.Location = new Point(260 + offset, 8);
                                    nb.Size = new Size(120, nb.Size.Height);
                                    nb.Value = ((int)x.Value);
                                    nb.ValueChanged += new EventHandler(nb_ValueChanged);
                                    eventIndexes[nb] = na;
                                    p.Controls.Add(nb);
                                }break;

                            case ConditionValueTypes.Float:
                                {
                                    NumericUpDown nb = new NumericUpDown();
                                    nb.Minimum = int.MinValue;
                                    nb.Maximum = int.MaxValue;
                                    nb.Location = new Point(260 + offset, 8);
                                    nb.DecimalPlaces = 4;
                                    nb.Size = new Size(120, nb.Size.Height);
                                    nb.Value = (decimal)((float)x.Value);
                                    nb.ValueChanged += new EventHandler(nb_ValueChanged2);
                                    eventIndexes[nb] = na;
                                    p.Controls.Add(nb);
                                }break;
                        }
                    }

                    groupBox1.Controls.Add(p);
                    p.BringToFront();
                    econtrols[x] = p;
                }
            }

            enEvent = true;
        }

        void nb_ValueChanged(object sender, EventArgs e)
        {
            if(!(sender is Control))
                return;

            Control co = sender as Control;
            if(!eventIndexes.ContainsKey(co))
                return;

            this.cur.Values[eventIndexes[co]].Value = (int)((NumericUpDown)co).Value;
        }

        void nb_ValueChanged2(object sender, EventArgs e)
        {
            if(!(sender is Control))
                return;

            Control co = sender as Control;
            if(!eventIndexes.ContainsKey(co))
                return;

            this.cur.Values[eventIndexes[co]].Value = (float)((NumericUpDown)co).Value;
        }

        void tb_TextChanged(object sender, EventArgs e)
        {
            if(!(sender is Control))
                return;

            Control co = sender as Control;
            if(!eventIndexes.ContainsKey(co))
                return;

            this.cur.Values[eventIndexes[co]].Value = ((TextBox)co).Text;
        }

        void cb2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!(sender is Control))
                return;

            Control co = sender as Control;
            if(!eventIndexes.ContainsKey(co) || ((ComboBox)co).SelectedIndex == -1)
                return;

            try
            {
                if(cur.Values[eventIndexes[co]].Type == ConditionValueTypes.String ||
                    cur.Values[eventIndexes[co]].Type == ConditionValueTypes.Dynamic)
                    cur.Values[eventIndexes[co]].Value = ((ComboBox)co).Items[((ComboBox)co).SelectedIndex] as string;
                else if(cur.Values[eventIndexes[co]].Type == ConditionValueTypes.Integer)
                    cur.Values[eventIndexes[co]].Value = int.Parse(((ComboBox)co).Items[((ComboBox)co).SelectedIndex] as string);
                else if(cur.Values[eventIndexes[co]].Type == ConditionValueTypes.Float)
                    cur.Values[eventIndexes[co]].Value = float.Parse(((ComboBox)co).Items[((ComboBox)co).SelectedIndex] as string);
                else if(cur.Values[eventIndexes[co]].Type == ConditionValueTypes.Bool)
                    cur.Values[eventIndexes[co]].Value = bool.Parse(((ComboBox)co).Items[((ComboBox)co).SelectedIndex] as string);
            }
            catch
            {
            }
        }

        void cb2_SelectedIndexChanged2(object sender, EventArgs e)
        {
            if(!(sender is Control))
                return;

            Control co = sender as Control;
            if(!eventIndexes.ContainsKey(co) || ((ComboBox)co).SelectedIndex == -1)
                return;

            try
            {
                cur.Values[eventIndexes[co]].Value = (((ComboBox)co).Items[((ComboBox)co).SelectedIndex] as string).Equals("yes", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
            }
        }

        private readonly Dictionary<Control, int> eventIndexes = new Dictionary<Control, int>();

        void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!(sender is Control))
                return;

            Control co = sender as Control;
            if(!eventIndexes.ContainsKey(co) || ((ComboBox)co).SelectedIndex == -1)
                return;

            ConditionComparison comp = ConditionComparison.None;
            string val = ((ComboBox)co).Items[((ComboBox)co).SelectedIndex] as string;
            if(val == "Is equal to")
            {
                comp = ConditionComparison.Equal;
            }
            else if(val == "Is not equal to")
            {
                comp = ConditionComparison.NotEqual;
            }
            else if(val == "Is greater than")
            {
                comp = ConditionComparison.Greater;
            }
            else if(val == "Is equal to or greater than")
            {
                comp = ConditionComparison.Equal | ConditionComparison.Greater;
            }
            else if(val == "Is less than")
            {
                comp = ConditionComparison.Less;
            }
            else if(val == "Is equal to or less than")
            {
                comp = ConditionComparison.Less | ConditionComparison.Equal;
            }
            else if(val == "Contains")
            {
                comp = ConditionComparison.Contains;
            }
            else if(val == "Does not contain")
            {
                comp = ConditionComparison.NotContains;
            }
            else
                return;

            cur.Values[eventIndexes[co]].Comparison = comp;
        }

        private void EditCondition_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(cur != null)
            {
                int i = filter.Conditions.IndexOf(condition);
                if(i != -1)
                {
                    filter.Conditions.RemoveAt(i);
                    filter.Conditions.Insert(i, cur);
                }
            }

            if(filter != null)
                filform.SetFilter(filter);
            filform.conForm = null;

            form.ClosedForm(this);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            string typeName = comboBox1.Items[comboBox1.SelectedIndex] as string;
            typeName = typeName.Replace(" ", "_");

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach(Type t in allTypes)
            {
                if(t.IsAbstract || !t.IsSubclassOf(typeof(Condition)) || !t.IsPublic)
                    continue;

                if(!t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if(cur.GetType() != t)
                {
                    cur = Activator.CreateInstance(t) as Condition;
                    this.Reload();
                }
                break;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
                cur.MinTurn = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
                cur.MaxTurn = (int)numericUpDown2.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
                cur.MinTime = (int)numericUpDown4.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
                cur.MaxTime = (int)numericUpDown3.Value;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.Or;
                if(checkBox4.Checked)
                    cur.Flags |= ConditionFlags.Or;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.Invert;
                if(checkBox3.Checked)
                    cur.Flags |= ConditionFlags.Invert;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.RequireMyTurn;
                if(checkBox1.Checked)
                    cur.Flags |= ConditionFlags.RequireMyTurn;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.RequireOpponentTurn;
                if(checkBox2.Checked)
                    cur.Flags |= ConditionFlags.RequireOpponentTurn;
            }
        }

        private void EditCondition_Load(object sender, EventArgs e)
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach(Type t in types)
            {
                if(t.IsAbstract || !t.IsPublic || !t.IsSubclassOf(typeof(Condition)))
                    continue;

                if(t.Name.IndexOf("disabled", StringComparison.OrdinalIgnoreCase) != -1)
                    continue;

                comboBox1.Items.Add(t.Name.Replace("_", " "));
            }

            enEvent = true;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.RequireBeforeMulligan;
                if(checkBox6.Checked)
                    cur.Flags |= ConditionFlags.RequireBeforeMulligan;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.RequireAfterMulligan;
                if(checkBox5.Checked)
                    cur.Flags |= ConditionFlags.RequireAfterMulligan;
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.RequireBeforeGameStart;
                if(checkBox8.Checked)
                    cur.Flags |= ConditionFlags.RequireBeforeGameStart;
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if(!enEvent)
                return;

            if(cur != null)
            {
                cur.Flags &= ~ConditionFlags.RequireAfterGameStart;
                if(checkBox7.Checked)
                    cur.Flags |= ConditionFlags.RequireAfterGameStart;
            }
        }
    }
}
