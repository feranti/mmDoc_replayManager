using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace MMDoCHistoryV2
{
    public partial class Form1
    {
        private readonly List<Filter> Filters = new List<Filter>();

        private const string filterDataFileName = "filters.hdata";

        private void SaveFilters()
        {
            if(this.Filters.Count == 0)
            {
                try
                {
                    File.Delete(filterDataFileName);
                }
                catch
                {
                }
                return;
            }

            FileStream fs = null;
            try
            {
                fs = File.OpenWrite(filterDataFileName);
            }
            catch
            {
                return;
            }

            try
            {
                BinaryWriter wr = new BinaryWriter(fs);
                try
                {
                    wr.Write(this.Filters.Count);
                    foreach(Filter x in Filters)
                    {
                        Filter._Save(x, wr);
                    }
                }
                catch
                {
                    this.Filters.Clear();
                }
                wr.Close();
            }
            catch
            {
                this.Filters.Clear();
                fs.Close();
            }

            if(this.Filters.Count == 0)
                this.SaveFilters();
        }

        private void LoadFilters()
        {
            this.Filters.Clear();

            FileStream fs = null;
            try
            {
                fs = File.OpenRead(filterDataFileName);
            }
            catch
            {
                return;
            }

            try
            {
                BinaryReader wr = new BinaryReader(fs);
                try
                {
                    int count = wr.ReadInt32();
                    for(int i = 0; i < count; i++)
                    {
                        Filter fi = Filter._Load(wr);
                        if(fi != null)
                            this.Filters.Add(fi);
                    }
                }
                catch
                {
                }
                wr.Close();
            }
            catch
            {
                fs.Close();
            }
        }

        internal void UpdateFilters()
        {
            List<Form> forms = this.OpenedForms.Where(q => q.GetType() == typeof(EditFilter)).ToList();
            foreach(Form x in forms)
                x.Close();

            listView1.Enabled = false;

            listView1.SelectedItems.Clear();

            listView1.Items.Clear();

            foreach(Filter x in Filters)
            {
                string name = x.ToString();
                listView1.Items.Add(new ListViewItem(new string[] { name }));
                listView1.Items[listView1.Items.Count - 1].Checked = (x.Flags & FilterFlags.Disabled) == FilterFlags.None;
            }

            listView1.Enabled = true;

            NeedReplayUpdate = true;
        }
    }
}
