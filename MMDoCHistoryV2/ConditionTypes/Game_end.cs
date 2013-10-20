using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Game_end : Condition
    {
        public Game_end() : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual,
                    Comparison = ConditionComparison.Equal,
                    Name = "Game result",
                    Restricted = new List<object>() { "Any", "Win", "Lose", "Draw" },
                    Type = ConditionValueTypes.String,
                    Value = "Any"
                }
            };
        }

        public override bool Test(Replays.Replay replay)
        {
            KeyValuePair<Match, int> r = this.Find(replay, gameResult, true, 0, 0);
            if(r.Key == null)
            {
                return this.Values[0].Test("Unfinished");
            }
            switch(r.Key.Groups[1].Value)
            {
                case "GameWon":
                    return this.Values[0].Test("Win") || this.Values[0].Test("Any");
                case "GameLost":
                    return this.Values[0].Test("Lose") || this.Values[0].Test("Any");
                case "GameDraw":
                    return this.Values[0].Test("Draw") || this.Values[0].Test("Any");
                default:
                    throw new NotImplementedException();
            }
        }

        private static readonly Regex gameResult = new Regex(@"^\d+\|(GameWon|GameLost|GameDraw) ", RegexOptions.Compiled);
    }
}
