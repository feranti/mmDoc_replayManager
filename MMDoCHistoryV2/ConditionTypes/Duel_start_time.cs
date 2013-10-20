using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Duel_start_time : Condition
    {
        public Duel_start_time()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Greater | ConditionComparison.Less,
                    Comparison = ConditionComparison.Equal,
                    Name = "Date & time",
                    Restricted = null,
                    Type = ConditionValueTypes.String,
                    Value = Form1.GetTime(new DateTime(2013, 09, 26, 18, 30, 46))
                }
            };
        }

        public override bool Test(Replays.Replay replay)
        {
            try
            {
                DateTime mt = Form1.GetTime((this.Values[0].Value as string) ?? "");
                if((this.Values[0].Comparison & ConditionComparison.Equal) != ConditionComparison.None &&
                    mt == replay.Time)
                    return true;
                if((this.Values[0].Comparison & ConditionComparison.NotEqual) != ConditionComparison.None &&
                    mt != replay.Time)
                    return true;
                if((this.Values[0].Comparison & ConditionComparison.Less) != ConditionComparison.None &&
                    mt > replay.Time)
                    return true;
                if((this.Values[0].Comparison & ConditionComparison.Greater) != ConditionComparison.None &&
                    mt < replay.Time)
                    return true;
            }
            catch
            {
            }

            return false;
        }
    }
}
