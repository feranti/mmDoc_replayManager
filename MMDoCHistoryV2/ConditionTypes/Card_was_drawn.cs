using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Card_was_drawn : Condition
    {
        public Card_was_drawn()
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
            int startIndex = 0;
            int total = 0;
            string key = this.Values[0].Value as string;
            do
            {
                KeyValuePair<Match, int> next = this.Find(replay, drawnPattern, true, 0, startIndex);
                if(next.Key == null)
                    break;

                startIndex = next.Value + 1;

                if(next.Value <= 0)
                    continue;

                int d1, d2, d3;
                if(!int.TryParse(next.Key.Groups[1].Value, out d1) || !int.TryParse(next.Key.Groups[1].Value, out d2) ||
                    !int.TryParse(next.Key.Groups[1].Value, out d3))
                    continue;

                // card was not drawn, but maybe it's possible that d1 == d3 when card was drawn due to random.
                if(d1 == d3)
                    continue;

                if(!this.Values[3].Test("Any"))
                {
                    bool p0 = next.Key.Groups[4].Value == "2";
                    if(this.Values[3].Test("Me"))
                    {
                        if((replay.OwnerPlayer == 0) != p0)
                            continue;
                    }
                    else
                    {
                        if((replay.OwnerPlayer == 0) == p0)
                            continue;
                    }
                }

                Replays.CardData card = Form1.Loader.GetCard(d2);
                if(card == null)
                    continue;

                if(key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    if(this.Values[1].Test(Form1.Loader.GetCardName(d2) ?? "Unknown card"))
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

        private static readonly Regex drawnPattern = new Regex(@"^\d+\|RevealToOther (\d+) (\d+) (\d+) 10000([26])$", RegexOptions.Compiled);
    }
}
