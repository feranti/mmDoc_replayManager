using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Replays;

namespace MMDoCHistoryV2
{
    public partial class GameDecks : Form
    {
        public GameDecks(Form1 f)
        {
            form = f;

            InitializeComponent();
        }

        private readonly Form1 form;

        private CustomRTF rtf1;
        private CustomRTF rtf2;

        private void GameDecks_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.ClosedForm(this);
        }

        internal Replay setted = null;

        internal void SetReplay(Replays.Replay replay)
        {
            setted = replay;
            Replays.Player player = new Replays.Player(replay, Form1.Loader);
            List<string> game = player.Parse();
            Dictionary<string, int> deckMe = player.CalculateDeck(replay.OwnerPlayer);
            Dictionary<string, int> deckOp = player.CalculateDeck(replay.OwnerPlayer == 0 ? 1 : 0);
            this.WriteDeck(rtf1, replay.OwnerPlayer, replay, deckMe);
            this.WriteDeck(rtf2, replay.OwnerPlayer == 0 ? 1 : 0, replay, deckOp);
            
            this.Text = "Game decks " + (replay.ToString() ?? "");
        }

        private void WriteDeck(CustomRTF rtf, int playerId, Replay replay, Dictionary<string, int> deck)
        {
            rtf.Clear(true);
            if(replay.OwnerPlayer == playerId)
                rtf.AddLine(CustomRTF.AnswerColor + "Your deck:", true);
            else
            {
                if(!replay.Finished && replay.Time < DateTime.Now && DateTime.Now - replay.Time < new TimeSpan(1, 0, 0))
                {
                    rtf.AddLine(CustomRTF.AnswerColor + "Unknown, finish game first.", false);
                    return;
                }
                rtf.AddLine(CustomRTF.AnswerColor + "Opponent's deck:", true);
            }

            Dictionary<string, SortedDictionary<string, int>> sorted = new Dictionary<string, SortedDictionary<string, int>>();
            foreach(KeyValuePair<string, int> x in deck)
            {
                CardData card = Form1.Loader.GetCardByName(x.Key);
                string header = this.GetCardHeader(x.Key, card);

                if(!sorted.ContainsKey(header))
                    sorted[header] = new SortedDictionary<string, int>();
                sorted[header][x.Key] = x.Value;
            }

            if(sorted.ContainsKey("Hero"))
            {
                rtf.AddLine("", true);
                rtf.AddLine(CustomRTF.AnswerColor + "Hero (" + sorted["Hero"].Sum(e => e.Value) + "):", true);
                foreach(KeyValuePair<string, int> x in sorted["Hero"])
                {
                    CardData card = Form1.Loader.GetCardByName(x.Key);
                    string rarity = "";
                    if(card != null)
                        rarity = (card.GetValue("Card.Rarity") ?? "").ToLower();
                    switch(rarity)
                    {
                        case "common":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardCommonColor + x.Key, true);
                            break;
                        case "uncommon":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardUncommonColor + x.Key, true);
                            break;
                        case "rare":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardRareColor + x.Key, true);
                            break;
                        case "epic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardEpicColor + x.Key, true);
                            break;
                        case "heroic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardHeroColor + x.Key, true);
                            break;
                        default:
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x |cff909090" + x.Key, true);
                            break;
                    }
                }

                sorted.Remove("Hero");
            }

            if(sorted.ContainsKey("Events"))
            {
                rtf.AddLine("", true);
                rtf.AddLine(CustomRTF.AnswerColor + "Events (" + sorted["Events"].Sum(e => e.Value) + "):", true);
                foreach(KeyValuePair<string, int> x in sorted["Events"])
                {
                    CardData card = Form1.Loader.GetCardByName(x.Key);
                    string rarity = "";
                    if(card != null)
                        rarity = (card.GetValue("Card.Rarity") ?? "").ToLower();
                    switch(rarity)
                    {
                        case "common":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardCommonColor + x.Key, true);
                            break;
                        case "uncommon":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardUncommonColor + x.Key, true);
                            break;
                        case "rare":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardRareColor + x.Key, true);
                            break;
                        case "epic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardEpicColor + x.Key, true);
                            break;
                        case "heroic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardHeroColor + x.Key, true);
                            break;
                        default:
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x |cff909090" + x.Key, true);
                            break;
                    }
                }

                sorted.Remove("Events");
            }

            if(sorted.ContainsKey("Creatures"))
            {
                rtf.AddLine("", true);
                rtf.AddLine(CustomRTF.AnswerColor + "Creatures (" + sorted["Creatures"].Sum(e => e.Value) + "):", true);
                foreach(KeyValuePair<string, int> x in sorted["Creatures"])
                {
                    CardData card = Form1.Loader.GetCardByName(x.Key);
                    string rarity = "";
                    if(card != null)
                        rarity = (card.GetValue("Card.Rarity") ?? "").ToLower();
                    switch(rarity)
                    {
                        case "common":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardCommonColor + x.Key, true);
                            break;
                        case "uncommon":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardUncommonColor + x.Key, true);
                            break;
                        case "rare":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardRareColor + x.Key, true);
                            break;
                        case "epic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardEpicColor + x.Key, true);
                            break;
                        case "heroic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardHeroColor + x.Key, true);
                            break;
                        default:
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x |cff909090" + x.Key, true);
                            break;
                    }
                }

                sorted.Remove("Creatures");
            }

            if(sorted.ContainsKey("Spells"))
            {
                rtf.AddLine("", true);
                rtf.AddLine(CustomRTF.AnswerColor + "Spells (" + sorted["Spells"].Sum(e => e.Value) + "):", true);
                foreach(KeyValuePair<string, int> x in sorted["Spells"])
                {
                    CardData card = Form1.Loader.GetCardByName(x.Key);
                    string rarity = "";
                    if(card != null)
                        rarity = (card.GetValue("Card.Rarity") ?? "").ToLower();
                    switch(rarity)
                    {
                        case "common":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardCommonColor + x.Key, true);
                            break;
                        case "uncommon":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardUncommonColor + x.Key, true);
                            break;
                        case "rare":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardRareColor + x.Key, true);
                            break;
                        case "epic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardEpicColor + x.Key, true);
                            break;
                        case "heroic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardHeroColor + x.Key, true);
                            break;
                        default:
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x |cff909090" + x.Key, true);
                            break;
                    }
                }

                sorted.Remove("Spells");
            }

            if(sorted.ContainsKey("Fortunes"))
            {
                rtf.AddLine("", true);
                rtf.AddLine(CustomRTF.AnswerColor + "Fortunes (" + sorted["Fortunes"].Sum(e => e.Value) + "):", true);
                foreach(KeyValuePair<string, int> x in sorted["Fortunes"])
                {
                    CardData card = Form1.Loader.GetCardByName(x.Key);
                    string rarity = "";
                    if(card != null)
                        rarity = (card.GetValue("Card.Rarity") ?? "").ToLower();
                    switch(rarity)
                    {
                        case "common":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardCommonColor + x.Key, true);
                            break;
                        case "uncommon":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardUncommonColor + x.Key, true);
                            break;
                        case "rare":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardRareColor + x.Key, true);
                            break;
                        case "epic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardEpicColor + x.Key, true);
                            break;
                        case "heroic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardHeroColor + x.Key, true);
                            break;
                        default:
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x |cff909090" + x.Key, true);
                            break;
                    }
                }

                sorted.Remove("Fortunes");
            }

            while(sorted.Count > 0)
            {
                rtf.AddLine("", true);
                KeyValuePair<string, SortedDictionary<string, int>> z = sorted.ElementAt(0);
                rtf.AddLine(CustomRTF.AnswerColor + z.Key + " (" + sorted[z.Key].Sum(e => e.Value) + "):", true);
                foreach(KeyValuePair<string, int> x in sorted[z.Key])
                {
                    CardData card = Form1.Loader.GetCardByName(x.Key);
                    string rarity = "";
                    if(card != null)
                        rarity = (card.GetValue("Card.Rarity") ?? "").ToLower();
                    switch(rarity)
                    {
                        case "common":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardCommonColor + x.Key, true);
                            break;
                        case "uncommon":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardUncommonColor + x.Key, true);
                            break;
                        case "rare":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardRareColor + x.Key, true);
                            break;
                        case "epic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardEpicColor + x.Key, true);
                            break;
                        case "heroic":
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x " + CustomRTF.CardHeroColor + x.Key, true);
                            break;
                        default:
                            rtf.AddLine("|cffffffff" + x.Value + "|cffc0c0c0x |cff909090" + x.Key, true);
                            break;
                    }
                }

                sorted.Remove(z.Key);
            }

            rtf.Update();
        }

        private string GetCardHeader(string name, CardData card)
        {
            if(name.Equals("spellcard", StringComparison.OrdinalIgnoreCase))
                return "Spells";
            if(name.Equals("fortunecard", StringComparison.OrdinalIgnoreCase))
                return "Fortunes";
            if(name.Equals("creaturecard", StringComparison.OrdinalIgnoreCase))
                return "Creatures";
            if(name.Equals("eventcard", StringComparison.OrdinalIgnoreCase))
                return "Events";
            if(name.Equals("herocard", StringComparison.OrdinalIgnoreCase))
                return "Hero";

            if(card == null)
            {
                return "Unknown cards";
            }

            string type = (card.GetValue("Card.Type") ?? "").ToLower().Trim();
            switch(type)
            {
                case "hero":
                    return "Hero";
                case "event":
                    return "Events";
                case "spell":
                    return "Spells";
                case "fortune":
                    return "Fortunes";
                case "creature":
                case "unit":
                    return "Creatures";
                default:
                    return "Unknown cards";
            }
        }

        private void GameDecks_Load(object sender, EventArgs e)
        {
            rtf1 = new CustomRTF(richTextBox1);
            rtf2 = new CustomRTF(richTextBox2);
        }
    }
}
