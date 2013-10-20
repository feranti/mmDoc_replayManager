using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Player_surrendered : Condition
    {
        public Player_surrendered()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal,
                    Comparison = ConditionComparison.Equal,
                    Name = "Surrendered",
                    Restricted = new List<object>() { true, false },
                    Type = ConditionValueTypes.Bool,
                    Value = true
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
            KeyValuePair<Match, int> f = this.Find(replay, sur, true, 0, 0);
            if(f.Key == null)
                return !this.Values[0].Test(true);

            if(this.Values[1].Test("Any"))
                return this.Values[0].Test(true);

            int surrenderer = int.Parse(f.Key.Groups[1].Value);
            if(this.Values[1].Test("Me"))
            {
                if(this.Values[0].Test(true))
                    return surrenderer == replay.OwnerPlayer;
                else
                    return surrenderer != replay.OwnerPlayer;
            }
            if(this.Values[0].Test(true))
                return surrenderer != replay.OwnerPlayer;
            return surrenderer == replay.OwnerPlayer;
        }

        private static readonly Regex sur = new Regex(@"^\d+\|SURRENDER U([01])", RegexOptions.Compiled);
    }
}
