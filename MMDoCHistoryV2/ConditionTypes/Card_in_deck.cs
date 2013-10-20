using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Replays;

namespace MMDoCHistoryV2.ConditionTypes
{
    public class Card_in_deck : Condition
    {
        public Card_in_deck()
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
            Dictionary<string, int> cardsMe = null;
            Dictionary<string, int> cardsOp = null;

            bool any = this.Values[3].Test("Any");
            bool me = this.Values[3].Test("Me");
            bool op = this.Values[3].Test("Opponent");
            string key = this.Values[0].Value as string;
            if(string.IsNullOrEmpty(key))
                return false;

            Player plr = null;

            int total = 0;
            if(any || me)
            {
                plr = new Player(replay, Form1.Loader);
                plr.Parse();

                cardsMe = plr.CalculateDeck(replay.OwnerPlayer);
                foreach(KeyValuePair<string, int> ce in cardsMe)
                {
                    if(key.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        if(this.Values[1].Test(ce.Key))
                            total += ce.Value;
                    }
                    else
                    {
                        Replays.CardData card = Form1.Loader.GetCardByName(ce.Key);
                        if(card != null && this.Values[1].Test(card.GetValue("Card." + key)))
                            total += ce.Value;
                    }
                }
            }
            if(any || op)
            {
                if(plr == null)
                {
                    plr = new Player(replay, Form1.Loader);
                    plr.Parse();
                }
                cardsOp = plr.CalculateDeck(replay.OwnerPlayer == 0 ? 1 : 0);
                foreach(KeyValuePair<string, int> ce in cardsOp)
                {
                    if(key.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        if(this.Values[1].Test(ce.Key))
                            total += ce.Value;
                    }
                    else
                    {
                        Replays.CardData card = Form1.Loader.GetCardByName(ce.Key);
                        if(card != null && this.Values[1].Test(card.GetValue("Card." + key)))
                            total += ce.Value;
                    }
                }
            }

            return this.Values[2].Test(total);
        }

        private static readonly Regex deckEntry = new Regex(@"^(\d+),(.+)$", RegexOptions.Compiled);
    }
}
