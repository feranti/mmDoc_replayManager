using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Card_was_played : Condition
    {
        public Card_was_played()
            : base()
        {
            this.Values = new ConditionValue[] 
            {
                new ConditionValue()
                {
                    Name = "Card key",
                    AllowComparison = ConditionComparison.Equal,
                    Comparison = ConditionComparison.Equal,
                    Restricted = null,
                    Type = ConditionValueTypes.String,
                    Value = "Name"
                },
                new ConditionValue()
                {
                    Name = "Card value",
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Contains | ConditionComparison.NotContains | ConditionComparison.Greater | ConditionComparison.Less,
                    Comparison = ConditionComparison.Contains,
                    Restricted = null,
                    Type = ConditionValueTypes.Dynamic,
                    Value = "enter name here"
                },
                new ConditionValue()
                {
                    Name = "Card count",
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual | ConditionComparison.Greater | ConditionComparison.Less,
                    Comparison = ConditionComparison.Greater | ConditionComparison.Equal,
                    Restricted = null,
                    Type = ConditionValueTypes.Integer,
                    Value = 1,
                }
            };
        }

        public override bool Test(Replays.Replay replay)
        {
            int startIndex = 0;
            int total = 0;
            string key = this.Values[0].Value as string;
            do
            {
                KeyValuePair<Match, int> next = this.Find(replay, playPattern, true, 0, startIndex);
                if(next.Key == null)
                    break;

                startIndex = next.Value + 1;

                if(next.Value <= 0)
                    continue;

                int cardslot;
                if(!int.TryParse(next.Key.Groups[1].Value, out cardslot))
                    continue;

                Match prevMatch = prevPattern.Match(replay.ReplayCommandList[next.Value - 1]);
                if(!prevMatch.Success || prevMatch.Groups[1].Value != prevMatch.Groups[3].Value || prevMatch.Groups[2].Value == "0")
                    continue;

                int cardid;
                if(!int.TryParse(prevMatch.Groups[2].Value, out cardid))
                    continue;

                if(next.Value < replay.ReplayCommandList.Count - 1 && nextPattern.Match(replay.ReplayCommandList[next.Value + 1]).Success)
                    continue;

                Replays.CardData card = Form1.Loader.GetCard(cardid);
                if(card == null)
                    continue;

                if(key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    if(this.Values[1].Test(Form1.Loader.GetCardName(cardid) ?? "Unknown card"))
                        total++;
                }
                else
                {
                    if(this.Values[1].Test(card.GetValue("Card." + key)))
                        total++;
                }
            } while(true);

            return this.Values[2].Test(total);
        }

        private static readonly Regex playPattern = new Regex(@"^\d+\|GENERIC SAction U[01] Sclick/ c(\d+)$", RegexOptions.Compiled);
        private static readonly Regex prevPattern = new Regex(@"^\d+\|RevealToOther (\d+) (\d+) (\d+) 10000[26]$", RegexOptions.Compiled);
        private static readonly Regex nextPattern = new Regex(@"^\d+\|GENERIC SAction U[01] Srclick$", RegexOptions.Compiled);
    }
}
