using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Replays
{
    public sealed class CardCache
    {
        public CardCache(Loader loader)
        {
            foreach(CardData x in loader.LoadedCards)
            {
                int xid;
                if(!int.TryParse(x.GetValue("Card.ID") ?? "", out xid))
                    continue;

                this.Cards[xid] = x;
                string name = loader.GetCardName(xid) ?? "Unknown card";
                this.Names[xid] = name;
            }
        }

        public readonly Dictionary<int, CardData> Cards = new Dictionary<int, CardData>();

        public readonly Dictionary<int, string> Names = new Dictionary<int, string>();

        public CardData GetCard(int id)
        {
            id &= 0xFFFF;
            return this.Cards.ContainsKey(id) ? this.Cards[id] : null;
        }

        public string GetName(int id)
        {
            id &= 0xFFFF;
            return this.Names.ContainsKey(id) ? this.Names[id] : null;
        }
    }
}
