using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Replays;
using System.Text.RegularExpressions;

namespace MMDoCHistoryV2
{
    public abstract class Condition
    {
        public Condition()
        {
        }

        public ConditionValue[] Values
        {
            get;
            protected set;
        }

        public int MinTurn = -1;

        public int MaxTurn = -1;

        public int MinTime = -1;

        public int MaxTime = -1;

        public ConditionFlags Flags = ConditionFlags.None;

        public abstract bool Test(Replay replay);

        protected KeyValuePair<Match, int> Find(Replay replay, Regex pattern, bool restrict, int skip, int startIndex)
        {
            Match match = null;
            int index = -1;

            int turn = -1;
            int totalTime = 0;
            int mulligan = 0;

            for(int i = 0; i < replay.ReplayCommandList.Count; i++)
            {
                string x = replay.ReplayCommandList[i];
                if(x.Contains('|'))
                {
                    int tempTime;
                    if(int.TryParse(x.Substring(0, x.IndexOf('|')), out tempTime))
                        totalTime += tempTime;
                    x = x.Substring(x.IndexOf('|') + 1);
                    string y = "";
                    if(x.Contains(' '))
                    {
                        y = x.Substring(x.IndexOf(' ')+1).Trim();
                        x = x.Substring(0, x.IndexOf(' '));
                    }

                    if(x.Equals("startgame", StringComparison.OrdinalIgnoreCase))
                        turn = 0;
                    else if(x.Equals("nextphase", StringComparison.OrdinalIgnoreCase))
                        turn++;
                    else if(x.Equals("mulligan", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            int plrid = int.Parse(y.Substring(0, 1));
                            int ans = int.Parse(y.Substring(4, 1));
                            if(ans == 0 && plrid == replay.OwnerPlayer)
                                mulligan = 1;
                        }
                        catch
                        {
                        }
                    }
                }

                if(i < startIndex)
                    continue;

                if(restrict)
                {
                    if(this.MinTurn >= 0 && ((turn / 2) + 1) < this.MinTurn)
                        continue;
                    if(this.MaxTurn >= 0 && ((turn / 2) + 1) > this.MaxTurn)
                        continue;
                    if(this.MinTime >= 0 && totalTime < this.MinTime)
                        continue;
                    if(this.MaxTime >= 0 && totalTime > this.MaxTime)
                        continue;
                    if((this.Flags & ConditionFlags.RequireMyTurn) != ConditionFlags.None &&
                        (replay.OwnerPlayer == 0 ? ((turn % 2) == 1) : ((turn % 2) == 0)))
                        continue;
                    if((this.Flags & ConditionFlags.RequireOpponentTurn) != ConditionFlags.None &&
                        (replay.OwnerPlayer == 1 ? ((turn % 2) == 1) : ((turn % 2) == 0)))
                        continue;
                    if((this.Flags & ConditionFlags.RequireBeforeMulligan) != ConditionFlags.None &&
                        mulligan != 0)
                        continue;
                    if((this.Flags & ConditionFlags.RequireAfterMulligan) != ConditionFlags.None &&
                        mulligan == 0)
                        continue;
                    if((this.Flags & ConditionFlags.RequireBeforeGameStart) != ConditionFlags.None &&
                        turn >= 0)
                        continue;
                    if((this.Flags & ConditionFlags.RequireAfterGameStart) != ConditionFlags.None &&
                        turn < 0)
                        continue;
                }

                match = pattern.Match(replay.ReplayCommandList[i]);
                if(!match.Success)
                {
                    match = null;
                    continue;
                }

                if(skip-- > 0)
                    continue;

                index = i;
                break;
            }

            return new KeyValuePair<Match, int>(match, index);
        }

        internal static void _Save(Condition c, BinaryWriter f)
        {
            f.Write(c.GetType().FullName);
            f.Write(c.MinTurn);
            f.Write(c.MaxTurn);
            f.Write(c.MinTime);
            f.Write(c.MaxTime);
            f.Write((uint)c.Flags);
            f.Write(c.Values != null && c.Values.Length != 0 ? (int)c.Values.Length : 0);
            if(c.Values != null && c.Values.Length != 0)
            {
                for(int i = 0; i < c.Values.Length; i++)
                    c.Values[i].Save(f);
            }
        }

        internal static Condition _Load(BinaryReader f)
        {
            string fullName = f.ReadString();
            int minturn = f.ReadInt32();
            int maxturn = f.ReadInt32();
            int mintime = f.ReadInt32();
            int maxtime = f.ReadInt32();
            ConditionFlags flags = (ConditionFlags)f.ReadUInt32();
            int count = f.ReadInt32();
            ConditionValue[] ar = new ConditionValue[count];
            for(int i = 0; i < ar.Length; i++)
            {
                ar[i] = new ConditionValue();
                ar[i].Load(f);
            }

            try
            {
                Condition c = Activator.CreateInstance(Type.GetType(fullName)) as Condition;
                c.MinTurn = minturn;
                c.MaxTurn = maxturn;
                c.MinTime = mintime;
                c.MaxTime = maxtime;
                c.Flags = flags;
                c.Values = ar;
                return c;
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name.Replace("_", " ");
        }
    }

    [Flags]
    public enum ConditionFlags : uint
    {
        None = 0,

        Or = 1,

        Invert = 2,

        RequireMyTurn = 4,

        RequireOpponentTurn = 8,

        RequireBeforeMulligan = 0x10,

        RequireAfterMulligan = 0x20,

        RequireBeforeGameStart = 0x40,

        RequireAfterGameStart = 0x80,
    }

    [Flags]
    public enum ConditionComparison : uint
    {
        None = 0,

        Equal = 1,

        NotEqual = 2,

        Greater = 4,

        Less = 8,

        Contains = 0x10,

        NotContains = 0x20,
    }

    public enum ConditionValueTypes : uint
    {
        None = 0,
        Integer = 1,
        Float = 2,
        String = 3,
        Bool = 4,
        Dynamic = 5,
    }

    public sealed class ConditionValue
    {
        public ConditionValue()
        {
        }

        public string Name = "Unknown value";

        public ConditionValueTypes Type = ConditionValueTypes.None;

        public ConditionComparison Comparison = ConditionComparison.None;

        public ConditionComparison AllowComparison = ConditionComparison.None;

        public object Value = null;

        public List<object> Restricted = null;

        public bool Test(object v)
        {
            if(this.Type == ConditionValueTypes.Integer)
            {
                if(!(v is int) || !(this.Value is int))
                    throw new ArgumentOutOfRangeException("v");

                if((this.Comparison & ConditionComparison.Equal) != ConditionComparison.None &&
                    v.Equals(this.Value))
                    return true;
                if((this.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None &&
                    !v.Equals(this.Value))
                    return true;
                if((this.Comparison & ConditionComparison.Greater) != ConditionComparison.None &&
                    (int)v > (int)this.Value)
                    return true;
                if((this.Comparison & ConditionComparison.Less) != ConditionComparison.None &&
                    (int)v < (int)this.Value)
                    return true;
                if((this.Comparison & ConditionComparison.Contains) != ConditionComparison.None &&
                    v.ToString().Contains(this.Value.ToString()))
                    return true;
                if((this.Comparison & ConditionComparison.NotContains) != ConditionComparison.None &&
                    !v.ToString().Contains(this.Value.ToString()))
                    return true;
            }
            else if(this.Type == ConditionValueTypes.Float)
            {
                if(!(v is float) || !(this.Value is float))
                    throw new ArgumentOutOfRangeException("v");

                if((this.Comparison & ConditionComparison.Equal) != ConditionComparison.None &&
                    v.Equals(this.Value))
                    return true;
                if((this.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None &&
                    !v.Equals(this.Value))
                    return true;
                if((this.Comparison & ConditionComparison.Greater) != ConditionComparison.None &&
                    (float)v > (float)this.Value)
                    return true;
                if((this.Comparison & ConditionComparison.Less) != ConditionComparison.None &&
                    (float)v < (float)this.Value)
                    return true;
                if((this.Comparison & ConditionComparison.Contains) != ConditionComparison.None &&
                    v.ToString().Contains(this.Value.ToString()))
                    return true;
                if((this.Comparison & ConditionComparison.NotContains) != ConditionComparison.None &&
                    !v.ToString().Contains(this.Value.ToString()))
                    return true;
            }
            else if(this.Type == ConditionValueTypes.String)
            {
                if(!(v is string) || !(this.Value is string))
                    throw new ArgumentOutOfRangeException("v");

                if((this.Comparison & ConditionComparison.Equal) != ConditionComparison.None &&
                    ((string)v).Equals(this.Value as string, StringComparison.OrdinalIgnoreCase))
                    return true;
                if((this.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None &&
                    !((string)v).Equals(this.Value as string, StringComparison.OrdinalIgnoreCase))
                    return true;
                if((this.Comparison & ConditionComparison.Contains) != ConditionComparison.None &&
                    ((string)v).IndexOf(this.Value as string, StringComparison.OrdinalIgnoreCase) != -1)
                    return true;
                if((this.Comparison & ConditionComparison.NotContains) != ConditionComparison.None &&
                    ((string)v).IndexOf(this.Value as string, StringComparison.OrdinalIgnoreCase) == -1)
                    return true;
            }
            else if(this.Type == ConditionValueTypes.Bool)
            {
                if(!(v is bool) || !(this.Value is bool))
                    throw new ArgumentOutOfRangeException("v");

                if((this.Comparison & ConditionComparison.Equal) != ConditionComparison.None &&
                    v.Equals(this.Value))
                    return true;
                if((this.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None &&
                    !v.Equals(this.Value))
                    return true;
            }
            else if(this.Type == ConditionValueTypes.Dynamic)
            {
                if(!(v is string) || !(this.Value is string))
                    throw new ArgumentOutOfRangeException("v");

                if((this.Comparison & ConditionComparison.Equal) != ConditionComparison.None &&
                    ((string)v).Equals(this.Value as string, StringComparison.OrdinalIgnoreCase))
                    return true;
                if((this.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None &&
                    !((string)v).Equals(this.Value as string, StringComparison.OrdinalIgnoreCase))
                    return true;
                if((this.Comparison & ConditionComparison.Contains) != ConditionComparison.None &&
                    ((string)v).IndexOf(this.Value as string, StringComparison.OrdinalIgnoreCase) != -1)
                    return true;
                if((this.Comparison & ConditionComparison.NotContains) != ConditionComparison.None &&
                    ((string)v).IndexOf(this.Value as string, StringComparison.OrdinalIgnoreCase) == -1)
                    return true;
                if((this.Comparison & ConditionComparison.Equal) != ConditionComparison.None)
                {
                    {
                        int dValue;
                        int dv;
                        if(int.TryParse(this.Value as string, out dValue) && int.TryParse(v as string, out dv) && dv == dValue)
                            return true;
                    }
                    {
                        float dValue;
                        float dv;
                        if(float.TryParse(this.Value as string, out dValue) && float.TryParse(v as string, out dv) && dv.Equals(dValue))
                            return true;
                    }
                }
                if((this.Comparison & ConditionComparison.NotEqual) != ConditionComparison.None)
                {
                    {
                        int dValue;
                        int dv;
                        if(int.TryParse(this.Value as string, out dValue) && int.TryParse(v as string, out dv) && dv != dValue)
                            return true;
                    }
                    {
                        float dValue;
                        float dv;
                        if(float.TryParse(this.Value as string, out dValue) && float.TryParse(v as string, out dv) && !dv.Equals(dValue))
                            return true;
                    }
                }
                if((this.Comparison & ConditionComparison.Greater) != ConditionComparison.None)
                {
                    {
                        int dValue;
                        int dv;
                        if(int.TryParse(this.Value as string, out dValue) && int.TryParse(v as string, out dv) && dv > dValue)
                            return true;
                    }
                    {
                        float dValue;
                        float dv;
                        if(float.TryParse(this.Value as string, out dValue) && float.TryParse(v as string, out dv) && dv > dValue)
                            return true;
                    }
                }
                if((this.Comparison & ConditionComparison.Less) != ConditionComparison.None)
                {
                    {
                        int dValue;
                        int dv;
                        if(int.TryParse(this.Value as string, out dValue) && int.TryParse(v as string, out dv) && dv < dValue)
                            return true;
                    }
                    {
                        float dValue;
                        float dv;
                        if(float.TryParse(this.Value as string, out dValue) && float.TryParse(v as string, out dv) && dv < dValue)
                            return true;
                    }
                }
            }

            return false;
        }

        public void Save(BinaryWriter f)
        {
            f.Write(this.Name);
            f.Write((uint)this.Type);
            f.Write((uint)this.Comparison);
            f.Write((uint)this.AllowComparison);
            if(this.Type == ConditionValueTypes.Bool)
                f.Write((byte)((bool)this.Value ? 1 : 0));
            else if(this.Type == ConditionValueTypes.Integer)
                f.Write((int)this.Value);
            else if(this.Type == ConditionValueTypes.Float)
                f.Write((float)this.Value);
            else if(this.Type == ConditionValueTypes.String || this.Type == ConditionValueTypes.Dynamic)
                f.Write((string)this.Value);
            if(this.Restricted == null || this.Restricted.Count == 0)
                f.Write((int)0);
            else
            {
                f.Write(this.Restricted.Count);
                foreach(object x in this.Restricted)
                {
                    if(this.Type == ConditionValueTypes.Bool)
                        f.Write((byte)((bool)x ? 1 : 0));
                    else if(this.Type == ConditionValueTypes.Integer)
                        f.Write((int)x);
                    else if(this.Type == ConditionValueTypes.Float)
                        f.Write((float)x);
                    else if(this.Type == ConditionValueTypes.String || this.Type == ConditionValueTypes.Dynamic)
                        f.Write((string)x);
                    else
                        throw new Exception();
                }
            }
        }

        public void Load(BinaryReader f)
        {
            this.Name = f.ReadString();
            this.Type = (ConditionValueTypes)f.ReadUInt32();
            this.Comparison = (ConditionComparison)f.ReadUInt32();
            this.AllowComparison = (ConditionComparison)f.ReadUInt32();
            if(this.Type == ConditionValueTypes.Bool)
                this.Value = f.ReadByte() != 0;
            else if(this.Type == ConditionValueTypes.Integer)
                this.Value = f.ReadInt32();
            else if(this.Type == ConditionValueTypes.Float)
                this.Value = f.ReadSingle();
            else if(this.Type == ConditionValueTypes.String || this.Type == ConditionValueTypes.Dynamic)
                this.Value = f.ReadString();
            int c = f.ReadInt32();
            if(c > 0)
            {
                this.Restricted = new List<object>();
                for(int i = 0; i < c; i++)
                {
                    if(this.Type == ConditionValueTypes.Bool)
                        this.Restricted.Add(f.ReadByte() != 0);
                    else if(this.Type == ConditionValueTypes.Integer)
                        this.Restricted.Add(f.ReadInt32());
                    else if(this.Type == ConditionValueTypes.Float)
                        this.Restricted.Add(f.ReadSingle());
                    else if(this.Type == ConditionValueTypes.String || this.Type == ConditionValueTypes.Dynamic)
                        this.Restricted.Add(f.ReadString());
                }
            }
        }
    }
}
