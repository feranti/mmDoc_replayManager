using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Player_elo : Condition
    {
        public Player_elo()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    Name = "ELO",
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Less | ConditionComparison.Greater,
                    Comparison = ConditionComparison.Equal,
                    Restricted = null,
                    Type = ConditionValueTypes.Integer,
                    Value = (int)1
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
            int me = replay.OwnerPlayer == 0 ? replay.EloPlayer1 : replay.EloPlayer2;
            int op = replay.OwnerPlayer == 0 ? replay.EloPlayer2 : replay.EloPlayer1;
            bool any;
            if((any = this.Values[1].Test("Any")) || this.Values[1].Test("Me"))
            {
                if(this.Values[0].Test(me))
                    return true;
            }
            if(any || this.Values[1].Test("Opponent"))
            {
                if(this.Values[0].Test(op))
                    return true;
            }

            return false;
        }
    }
}
