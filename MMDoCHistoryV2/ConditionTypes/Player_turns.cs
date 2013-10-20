using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Player_turns : Condition
    {
        public Player_turns()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    Name = "Turns",
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Greater | ConditionComparison.Less,
                    Comparison = ConditionComparison.Equal,
                    Restricted = null,
                    Type = ConditionValueTypes.Integer,
                    Value = (int)5
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
            if(this.Values[1].Test("Any"))
            {
                return this.Values[0].Test(replay.TurnsPlayer1) ||
                    this.Values[0].Test(replay.TurnsPlayer2);
            }
            if(this.Values[1].Test("Me"))
                return this.Values[0].Test(replay.OwnerPlayer == 0 ? replay.TurnsPlayer1 : replay.TurnsPlayer2);
            return this.Values[0].Test(replay.OwnerPlayer == 1 ? replay.TurnsPlayer1 : replay.TurnsPlayer2);
        }
    }
}
