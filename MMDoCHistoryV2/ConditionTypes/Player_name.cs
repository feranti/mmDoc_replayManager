using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Player_name : Condition
    {
        public Player_name()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    Name = "Name",
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Contains | ConditionComparison.NotContains,
                    Comparison = ConditionComparison.Contains,
                    Restricted = null,
                    Type = ConditionValueTypes.String,
                    Value = "enter name here"
                },
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal,
                    Comparison = ConditionComparison.Equal,
                    Name = "Player",
                    Restricted = new List<object>() { "Any", "Me", "Opponent" },
                    Value = "Any",
                    Type = ConditionValueTypes.String
                }
            };
        }

        public override bool Test(Replays.Replay replay)
        {
            string me = replay.OwnerPlayer == 0 ? replay.NamePlayer1 : replay.NamePlayer2;
            string op = replay.OwnerPlayer == 0 ? replay.NamePlayer2 : replay.NamePlayer1;
            bool any;
            if((any = this.Values[1].Test("Any")) || this.Values[1].Test("Me"))
            {
                if(!string.IsNullOrEmpty(me) && this.Values[0].Test(me))
                    return true;
            }
            if(any || this.Values[1].Test("Opponent"))
            {
                if(!string.IsNullOrEmpty(op) && this.Values[0].Test(op))
                    return true;
            }

            return false;
        }
    }
}
