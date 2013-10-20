using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Game_duration : Condition
    {
        public Game_duration()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Greater | ConditionComparison.Less,
                    Name = "Duration in seconds",
                    Comparison = ConditionComparison.Equal,
                    Restricted = null,
                    Type = ConditionValueTypes.Integer,
                    Value = (int)300
                }
            };
        }

        public override bool Test(Replays.Replay replay)
        {
            return this.Values[0].Test(replay.Duration / 1000);
        }
    }
}
