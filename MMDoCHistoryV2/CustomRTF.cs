using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MMDoCHistoryV2
{
    public class CustomRTF
    {
        public CustomRTF(RichTextBox box)
        {
            this.rtf = box;
            this.rtf.BackColor = Color.Black;
            this.rtf.WordWrap = true;
            this.rtf.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
        }

        public bool Timestamps = false;

        public int MaxSize = 100000;

        public void Clear(bool skipUpdate)
        {
            this.Msg.Clear();
            if(!skipUpdate)
                this.Update();
        }

        public const string QuestionColor = "|cff99ccff";
        public const string AnswerColor = "|cffdddddd";
        public const string CardCommonColor = "|cffffffff";
        public const string CardUncommonColor = "|cff1eff00";
        public const string CardRareColor = "|cff66ffff";
        public const string CardEpicColor = "|cffff8000";
        public const string CardHeroColor = "|cff9999ff";
        public const string CardUnknownColor = "|cff666666";
        public const string MyPlayerColor = "|cffc0ffc0";
        public const string EnemyColor = "|cffffc0c0";

        private readonly RichTextBox rtf;

        private readonly Dictionary<uint, int> ColorCodes = new Dictionary<uint, int>();

        private void GetColor(uint c, out byte r, out byte g, out byte b)
        {
            b = (byte)(c & 0xFF);
            g = (byte)((c >> 8) & 0xFF);
            r = (byte)((c >> 16) & 0xFF);
        }

        private uint GetColor(byte r, byte g, byte b)
        {
            return b | ((uint)g << 8) | ((uint)r << 16);
        }

        private uint GetColor(string hex)
        {
            if(hex.Length == 8)
                hex = hex.Substring(2);
            if(hex.Length != 6)
                throw new Exception("Invalid hex value!");

            return GetColor(Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
        }

        private void BuildHeader(StringBuilder str)
        {
            // Set the font
            str.Append("{\\rtf1\\ansi\\ansicpg1257\\deff0\\deflang1061{\\fonttbl{\\f0\\fnil\\fcharset186 arial;}}");

            // Build color table
            ColorCodes.Clear();

            str.Append("{\\colortbl ;");
            str.Append("\\red255\\green255\\blue255;");
            ColorCodes[0xFFFFFF] = 1;
            int i = 2;
            foreach(Message m in Msg)
            {
                if(m.Colors != null)
                {
                    foreach(uint x in m.Colors)
                    {
                        if(ColorCodes.ContainsKey(x))
                            continue;

                        byte r;
                        byte g;
                        byte b;
                        GetColor(x, out r, out g, out b);
                        str.Append("\\red" + r + "\\green" + g + "\\blue" + b + ";");
                        ColorCodes[x] = i++;
                    }
                }
            }

            str.Append("}");
        }

        private void BuildBody(StringBuilder str)
        {
            int j = 0;
            foreach(Message msg in Msg)
            {
                if(this.Timestamps)
                    str.Append("\\viewkind4\\uc1\\pard\\b\\f0\\fs20\\cf1 [" + string.Format("{0:D2}", msg.Timestamp.Hour) + ":" + string.Format("{0:D2}", msg.Timestamp.Minute) + ":" + string.Format("{0:D2}", msg.Timestamp.Second) + "] ");
                string txt = msg.Msg.Replace("||", "\t");
                Match m;
                while((m = _msgColor.Match(txt)).Success)
                {
                    txt = txt.Remove(m.Index, m.Length);
                    uint c = GetColor(m.Groups[1].Value);
                    if(ColorCodes.ContainsKey(c))
                        txt = txt.Insert(m.Index, "\\viewkind4\\uc1\\pard\\b\\f0\\fs20\\cf" + ColorCodes[c] + " ");
                    else
                        txt = txt.Insert(m.Index, "<MISSING_COLOR>");
                }

                txt = txt.Replace("\t", "|");
                if(j != 0)
                    str.Append("\\par ");
                str.Append(txt);
                j++;
            }
        }

        private static Regex _msgColor = new Regex(@"\|[cC]..(.{6})", RegexOptions.Compiled);

        private readonly List<Message> Msg = new List<Message>();

        public void AddLine(string msg, bool skipUpdate)
        {
            this.Add(new Message(msg), skipUpdate);
        }

        private void Add(Message msg, bool noUpdate)
        {
            Msg.Add(msg);
            while(Msg.Count > this.MaxSize)
                Msg.RemoveAt(0);
            if(!noUpdate)
                this.Update();
        }

        public void Update()
        {
            StringBuilder bld = new StringBuilder();
            BuildHeader(bld);
            BuildBody(bld);

            rtf.Rtf = bld.ToString();

            // TODO: scroll to end option?
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const int WM_VSCROLL = 277;
        const int SB_BOTTOM = 7;

        private void ScrollToEnd(RichTextBox rtb)
        {
            IntPtr ptrWparam = new IntPtr(SB_BOTTOM);
            IntPtr ptrLparam = new IntPtr(0);
            SendMessage(rtb.Handle, WM_VSCROLL, ptrWparam, ptrLparam);
        }

        public void ScrollToEnd()
        {
            this.ScrollToEnd(rtf);
        }

        private class Message
        {
            internal Message(string msg)
            {
                msg = msg.Replace("\\", "\\\\");
                msg = msg.Replace("{", "\\{");
                msg = msg.Replace("}", "\\}");

                // Get colors
                {
                    List<uint> color = new List<uint>();
                    Match m;
                    int i = 0;
                    while((m = _msgColor.Match(msg, i)).Success)
                    {
                        i = m.Index + m.Length;
                        try
                        {
                            uint c = (uint)Convert.ToInt32(m.Groups[1].Value, 16);
                            if(!color.Contains(c))
                                color.Add(c);
                        }
                        catch
                        {
                        }
                    }

                    Colors = color.ToArray();
                }

                Msg = msg;
                Timestamp = DateTime.Now;
            }

            /// <summary>
            /// Text message where text has already been escaped for RTF
            /// </summary>
            public readonly string Msg;

            /// <summary>
            /// Timestamp when this message was created
            /// </summary>
            public readonly DateTime Timestamp;

            /// <summary>
            /// Colors present in this message
            /// </summary>
            public readonly uint[] Colors;
        }
    }
}
