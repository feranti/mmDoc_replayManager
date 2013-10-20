using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Player_hero : Condition
    {
        public Player_hero()
            : base()
        {
            this.Values = new ConditionValue[]
            {
                new ConditionValue()
                {
                    AllowComparison = ConditionComparison.Equal | ConditionComparison.NotEqual |
                    ConditionComparison.Contains | ConditionComparison.NotContains,
                    Comparison = ConditionComparison.Contains,
                    Name = "Hero name",
                    Restricted = null,
                    Type = ConditionValueTypes.String,
                    Value = "enter hero name"
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
            string hMe = null;
            string hOp = null;

            if(replay.DeckPlayer1.Count > 0 && replay.DeckPlayer2.Count > 0)
            {
                int id1, id2;
                if(replay.DeckPlayer1[0].Contains(',') && replay.DeckPlayer2[0].Contains(','))
                {
                    if(int.TryParse(replay.DeckPlayer1[0].Substring(replay.DeckPlayer1[0].IndexOf(',') + 1).Trim(), out id1) &&
                        int.TryParse(replay.DeckPlayer2[0].Substring(replay.DeckPlayer2[0].IndexOf(',') + 1).Trim(), out id2))
                    {
                        hMe = Form1.Loader.GetCardName(replay.OwnerPlayer == 0 ? id1 : id2);
                        hOp = Form1.Loader.GetCardName(replay.OwnerPlayer == 1 ? id1 : id2);

                        if(this.Values[1].Test("Any"))
                        {
                            return this.Values[0].Test(hMe) || this.Values[0].Test(hOp);
                        }
                        else if(this.Values[1].Test("Me"))
                        {
                            return this.Values[0].Test(hMe);
                        }
                        else if(this.Values[1].Test("Opponent"))
                        {
                            return this.Values[0].Test(hOp);
                        }
                    }
                }
            }

            return false;
        }
    }
}
