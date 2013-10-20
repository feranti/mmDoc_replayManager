using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class I_started : Condition
    {
        public I_started()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal,
                    Comparison = ConditionComparison.Equal,
                    Name = "I started",
                    Restricted = new List<object>() { true, false },
                    Type = ConditionValueTypes.Bool,
                    Value = true
                }
            };
        }

        public override bool Test(Replays.Replay replay)
        {
            return (this.Values[0].Test(true) ? 0 : 1) == replay.OwnerPlayer;
        }
    }
}
