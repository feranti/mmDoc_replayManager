using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Replays;
using System.IO;

namespace MMDoCHistoryV2
{
    public sealed class Filter
    {
        public Filter()
        {
        }

        /// <summary>
        /// Name of filter (for user interface).
        /// </summary>
        public string Name = "Unnamed filter";

        /// <summary>
        /// Current options for filter.
        /// </summary>
        public FilterFlags Flags = FilterFlags.None;

        /// <summary>
        /// List of conditions that will be applied to replay file.
        /// </summary>
        public readonly List<Condition> Conditions = new List<Condition>();

        /// <summary>
        /// Test if replay file passes this filter.
        /// </summary>
        /// <param name="replay">Replay to test.</param>
        /// <returns></returns>
        public bool Test(Replay replay)
        {
            if((this.Flags & FilterFlags.Disabled) != FilterFlags.None)
                return true;

            bool wasfalse = false;

            for(int i = 0; i < this.Conditions.Count; i++)
            {
                if(i > 0 && (this.Conditions[i - 1].Flags & ConditionFlags.Or) != ConditionFlags.None && !wasfalse)
                    continue;

                bool r = this.Conditions[i].Test(replay);
                if((this.Conditions[i].Flags & ConditionFlags.Invert) != ConditionFlags.None)
                    r = !r;

                if(!r)
                {
                    wasfalse = true;
                    if((this.Conditions[i].Flags & ConditionFlags.Or) != ConditionFlags.None)
                        continue;
                    return false;
                }
                else
                    wasfalse = false;
            }

            return !wasfalse;
        }

        internal static void _Save(Filter filter, BinaryWriter f)
        {
            f.Write(filter.Name);
            f.Write((uint)filter.Flags);
            f.Write(filter.Conditions.Count);
            foreach(Condition c in filter.Conditions)
                Condition._Save(c, f);
        }

        internal static Filter _Load(BinaryReader f)
        {
            Filter filter = new Filter();
            filter.Name = f.ReadString();
            filter.Flags = (FilterFlags)f.ReadUInt32();
            int count = f.ReadInt32();
            for(int i = 0; i < count; i++)
                filter.Conditions.Add(Condition._Load(f));
            return filter;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /*
     * TODO:
     * condition: card go to graveyard
     * copy filters & conditions
     * basic win lose % and stats in statusbar
     */

    [Flags]
    public enum FilterFlags : uint
    {
        None = 0,

        Disabled = 1,
    }
}
