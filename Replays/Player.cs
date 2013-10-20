using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Replays
{
    /// <summary>
    /// This class can be used to read a game. This class can also be overrided to change functionality.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Initialize a replay parser.
        /// </summary>
        /// <param name="replay">Replay file to parse.</param>
        /// <param name="loader">Loader that has card and replay data.</param>
        public Player(Replay replay, Loader loader)
        {
            this.Replay = replay;
            this.Loader = loader;
        }

        /// <summary>
        /// Replay file that is being parsed.
        /// </summary>
        public readonly Replay Replay;

        /// <summary>
        /// Loader that loaded this replay.
        /// </summary>
        public readonly Loader Loader;

        /// <summary>
        /// Write timestamps when parseing.
        /// </summary>
        public bool Timestamps = true;

        private int totalTime = 0;

        /// <summary>
        /// Cardslots read during the game. This will fill the card info in decks that it saw in game.
        /// </summary>
        public readonly Dictionary<int, string> CardSlots = new Dictionary<int, string>();

        /// <summary>
        /// How many cards does player 1 have in their deck. Including all.
        /// </summary>
        public int Player1CardCount = 0;

        /// <summary>
        /// How many cards does player 2 have in their deck. Including all.
        /// </summary>
        public int Player2CardCount = 0;

        /// <summary>
        /// This will calculate the composition of deck based on game. Only valid after parse function
        /// was used.
        /// </summary>
        /// <param name="playerId">Player ID of deck (0 or 1).</param>
        /// <returns></returns>
        public virtual Dictionary<string, int> CalculateDeck(int playerId)
        {
            Dictionary<string, int> deck = new Dictionary<string, int>();
            if(this.CardSlots.Count == 0)
                return deck;

            for(int i = 1; i <= (playerId == 0 ? this.Player1CardCount : this.Player2CardCount); i++)
            {
                int key = 10000 + i + (playerId == 0 ? 0 : this.Player1CardCount);
                if(!CardSlots.ContainsKey(key))
                    continue;

                if(!deck.ContainsKey(this.CardSlots[key]))
                    deck[this.CardSlots[key]] = 1;
                else
                    deck[this.CardSlots[key]]++;
            }

            return deck;
        }

        /// <summary>
        /// ;)
        /// </summary>
        private bool CanCheat = false;

        /// <summary>
        /// Parse this replay file into text.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> Parse()
        {
            // This is where we write the result.
            List<string> result = new List<string>();

            // Clear all cards.
            this.CardSlots.Clear();

            // Not allowed during game.
            if(this.Replay.Finished || (DateTime.Now > this.Replay.Time && (DateTime.Now - this.Replay.Time) > new TimeSpan(1, 0, 0)))
                this.CanCheat = true;

            // Starting card slot index.
            int cardSlot = 10001;

            // Deck data will be loaded with this pattern.
            Regex deckEntry = new Regex(@"^(\d+),(.+)$", RegexOptions.Compiled);

            // Read player #1 deck.
            foreach(string x in this.Replay.DeckPlayer1)
            {
                Match m = deckEntry.Match(x);
                if(!m.Success)
                    continue;

                int count;
                if(!int.TryParse(m.Groups[1].Value, out count))
                    continue;

                string card = m.Groups[2].Value;
                int cardid;
                if(int.TryParse(card, out cardid))
                    card = this.Loader.GetCardName(cardid) ?? cardid.ToString();

                if(!this.CanCheat)
                {
                    if(card.Equals("CreatureCard", StringComparison.OrdinalIgnoreCase) ||
                        card.Equals("SpellCard", StringComparison.OrdinalIgnoreCase) ||
                        card.Equals("FortuneCard", StringComparison.OrdinalIgnoreCase))
                        card = "Unknown card";
                }

                for(int i = 0; i < count; i++)
                    CardSlots[cardSlot++] = card;
            }

            this.Player1CardCount = cardSlot - 10001;

            // Read player #2 deck.
            foreach(string x in this.Replay.DeckPlayer2)
            {
                Match m = deckEntry.Match(x);
                if(!m.Success)
                    continue;

                int count;
                if(!int.TryParse(m.Groups[1].Value, out count))
                    continue;

                string card = m.Groups[2].Value;
                int cardid;
                if(int.TryParse(card, out cardid))
                    card = this.Loader.GetCardName(cardid) ?? cardid.ToString();

                if(!this.CanCheat)
                {
                    if(card.Equals("CreatureCard", StringComparison.OrdinalIgnoreCase) ||
                        card.Equals("SpellCard", StringComparison.OrdinalIgnoreCase) ||
                        card.Equals("FortuneCard", StringComparison.OrdinalIgnoreCase))
                        card = "Unknown card";
                }

                for(int i = 0; i < count; i++)
                    CardSlots[cardSlot++] = card;
            }

            this.Player2CardCount = cardSlot - 10001 - this.Player1CardCount;

            int j = -1;

            // Load all replay commands and write what we can.
            foreach(string x in this.Replay.ReplayCommandList)
            {
                j++;

                if(string.IsNullOrEmpty(x))
                    continue;

                // Only parse time if command is valid.
                if(char.IsNumber(x[0]) && x.Contains('|'))
                {
                    int time;

                    // Add to time because command is executed AFTER this read time.
                    if(int.TryParse(x.Substring(0, x.IndexOf('|')), out time))
                        totalTime += time;
                }

                // Parse this line into new data.
                string[] r = this.Get(x, j).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Didn't parse anything.
                if(r.Length == 0)
                    continue;

                // Write the parsed lines into result.
                foreach(string z in r)
                {
                    // This shouldn't happen.
                    if(string.IsNullOrEmpty(z))
                        continue;

                    // Add timestamp if necessary.
                    string q = z;
                    if(this.Timestamps)
                    {
                        // Timestamps are [hh:mm:ss] since start of replay file (not start of game).
                        TimeSpan span = new TimeSpan(0, 0, totalTime / 1000);
                        q = "[" + span.Hours.ToString("00") + ":" + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00") + "] " + q;
                    }

                    result.Add(q);
                }
            }

            for(int i = 0; i < result.Count; i++)
            {
                while(result[i].Contains("$$$"))
                {
                    string ns = result[i].Substring(result[i].IndexOf("$$$") + 3);
                    ns = ns.Substring(0, ns.IndexOf("$$$"));

                    int ni;
                    if(int.TryParse(ns, out ni))
                        result[i] = result[i].Replace("$$$" + ns + "$$$", this.GetCardNameBySlot(ni) ?? "Unknown card");
                    else
                        result[i] = result[i].Replace("$$$" + ns + "$$$", "Unknown card");
                }
            }

            return result;
        }

        /// <summary>
        /// This function will parse a single line.
        /// </summary>
        /// <param name="line">Line from replaycommandlist that is parsed.</param>
        /// <param name="index">Index of line in that replay command list.</param>
        /// <returns></returns>
        protected virtual string Get(string line, int index)
        {
            Match m;

            // I divided this into different functions for simpler understanding.
            if((m = patternMulligan.Match(line)).Success)
                return GetMulligan(m);
            if((m = patternReveal.Match(line)).Success)
                return GetReveal(m);
            if((m = patternStart.Match(line)).Success)
                return GetStart(m);
            if((m = patternClick.Match(line)).Success)
                return GetClick(m);
            if((m = patternEndTurn.Match(line)).Success)
                return GetEndTurn(m);
            if((m = patternEndGame.Match(line)).Success)
                return GetEndGame(m);

            // None of the functions parsed this line.
            return "UNKNOWN";
        }

        private string GetMulligan(Match m)
        {
            StringBuilder str = new StringBuilder();
            str.Append(this.GetPlayerName(int.Parse(m.Groups[2].Value)));
            if(m.Groups[4].Value == "1")
                str.Append(" was offered mulligan.");
            else
            {
                if(m.Groups[3].Value == "1")
                    str.Append(" did not choose mulligan.");
                else
                    str.Append(" chose to take mulligan.");
            }
            return str.ToString();
        }

        private void WriteWhoseTurn(StringBuilder str)
        {
            if(str.Length > 0)
                str.AppendLine();
            str.Append("It is now ");
            str.Append(this.GetPlayerName(this.turnStatus % 2) + "'s ");
            str.Append(((turnStatus / 2) + 1).ToString() + getNth((turnStatus / 2) + 1) + " turn.");
        }

        private string getNth(int n)
        {
            if(n >= 10 && n <= 13)
                return "th";

            if((n % 10) == 1)
                return "st";
            if((n % 10) == 2)
                return "nd";
            if((n % 10) == 3)
                return "rd";
            return "th";
        }

        private string GetStart(Match m)
        {
            StringBuilder str = new StringBuilder();
            str.Append("The game has started.");
            this.WriteWhoseTurn(str);
            return str.ToString();
        }

        private string GetEndTurn(Match m)
        {
            StringBuilder str = new StringBuilder();
            str.Append(this.GetPlayerName(turnStatus % 2));
            str.Append(" has ended turn.");
            turnStatus++;
            this.WriteWhoseTurn(str);
            return str.ToString();
        }

        private string GetEndGame(Match m)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("The game has ended.");
            if(m.Groups[2].Value.Equals("gamewon", StringComparison.OrdinalIgnoreCase))
            {
                str.Append(this.GetPlayerName(this.Replay.OwnerPlayer));
                str.AppendLine(" has won the game.");
                str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer == 0 ? 1 : 0) + " has lost the game.");
            }
            else if(m.Groups[2].Value.Equals("gamelost", StringComparison.OrdinalIgnoreCase))
            {
                str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer == 0 ? 1 : 0) + " has won the game.");
                str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer == 1 ? 1 : 0) + " has lost the game.");
            }
            else
            {
                str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer == 0 ? 1 : 0) + " has drawn the game.");
                str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer == 1 ? 1 : 0) + " has drawn the game.");
            }

            str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer) + " has earned " + m.Groups[3].Value + " experience.");
            str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer) + " has earned " + m.Groups[4].Value + " gold.");

            int elodiff1 = this.Replay.OwnerPlayer == 1 ? this.Replay.EloPlayer2 : this.Replay.EloPlayer1;
            int elodiff2 = this.Replay.OwnerPlayer == 0 ? this.Replay.EloPlayer2 : this.Replay.EloPlayer1;

            int elo1;
            if(int.TryParse(m.Groups[18].Value, out elo1))
                elodiff1 = elo1 - elodiff1;
            if(int.TryParse(m.Groups[19].Value, out elo1))
                elodiff2 = elo1 - elodiff2;

            str.AppendLine(this.GetPlayerName(this.Replay.OwnerPlayer) + " has " + (elodiff1 >= 0 ? "gained" : "lost") + " " + Math.Abs(elodiff1).ToString() + " ELO.");
            str.Append(this.GetPlayerName(this.Replay.OwnerPlayer == 0 ? 1 : 0) + " has " + (elodiff2 >= 0 ? "gained" : "lost") + " " + Math.Abs(elodiff2).ToString() + " ELO.");
            return str.ToString();
        }

        private string GetReveal(Match m)
        {
            int d1, d2, d3, d4;
            if(!int.TryParse(m.Groups[2].Value, out d1))
            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }
            if(!int.TryParse(m.Groups[3].Value, out d2))
            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }
            if(!int.TryParse(m.Groups[4].Value, out d3))
            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }
            if(!int.TryParse(m.Groups[5].Value, out d4))
            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }

            if(d2 != 0)
                this.CardSlots[d3] = this.GetCardNameById(d2);

            switch(d4)
            {
                case 100002:
                case 100006:
                    {
                        if(d1 == d3)
                            return this.GetPlayerName(d4 == 100002 ? 0 : 1) + " has revealed \"$$$" + d3.ToString() + "$$$\".";

                        if(this.CanCheat || this.Replay.OwnerPlayer == (d4 == 100002 ? 0 : 1))
                            return this.GetPlayerName(d4 == 100002 ? 0 : 1) + " has drawn \"$$$" + d3.ToString() + "$$$\".";
                        else
                            return this.GetPlayerName(d4 == 100002 ? 0 : 1) + " has drawn \"Unknown card\".";
                    }
                case 100009:
                    {
                        return "Event card \"" + this.GetCardNameById(d2) + "\" is added.";
                    }
                case 100003:
                case 100007:
                    {
                        return this.GetPlayerName(d4 == 100003 ? 0 : 1) + " is looking in deck and sees \"$$$" + d3.ToString() + "$$$\".";
                    }
                case 100012:
                    {
                        return "Hero \"" + this.GetCardNameById(d2) + "\" is set to " + this.GetPlayerName(d3 == 10001 ? 0 : 1) + ".";
                    }
                case 100004:
                case 100008:
                    {
                        return "Card \"" + this.GetCardNameBySlot(d3) + "\" has gone to " + this.GetPlayerName(d4 == 100008 ? 1 : 0) + "'s graveyard.";
                    }
                default:
                    {
#if DEBUG
                throw new Exception();
#else
                        return "";
#endif
                    }
            }
        }

        private string GetClick(Match m)
        {
            int playerId;
            if(!int.TryParse(m.Groups[2].Value, out playerId))
            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }

            string[] actions = m.Groups[3].Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < actions.Length; i++)
            {
                actions[i] = actions[i].Trim();
                if(actions[i].EndsWith("/"))
                    actions[i] = actions[i].Substring(0, actions[i].Length - 1);
            }
            if(actions.Length == 0)
            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }

            if(actions[0] == "Srclick")
                return this.GetPlayerName(playerId) + " has closed the menu from X.";

            if(actions[1].StartsWith("o"))
            {
                int op;
                if(int.TryParse(actions[1].Substring(1), out op))
                {
                    if(actions.Length != 2)
                    {
#if DEBUG
                throw new Exception();
#else
                        return "";
#endif
                    }
                    return this.GetPlayerName(playerId) + " has chosen option " + (op + 1).ToString() + " from menu.";
                }
                {
#if DEBUG
                throw new Exception();
#else
                    return "";
#endif
                }
            }
            else if(actions[1].StartsWith("c"))
            {
                int ca;
                if(int.TryParse(actions[1].Substring(1), out ca))
                {
                    if(actions.Length == 2)
                        return this.GetPlayerName(playerId) + " has clicked on \"$$$" + ca.ToString() + "$$$\".";
                    else if(actions.Length == 3)
                    {
                        if(actions[2].StartsWith("b"))
                        {
                            Match mp = patternPosition.Match(actions[2]);
                            if(!mp.Success)
                            {
#if DEBUG
                throw new Exception();
#else
                                return "";
#endif
                            }

                            int r, l;
                            if(int.TryParse(mp.Groups[1].Value, out r) &&
                                int.TryParse(mp.Groups[2].Value, out l))
                            {
                                if(r == 4)
                                {
                                    if(l <= 1)
                                        return this.GetPlayerName(playerId) + " has clicked on \"$$$" + ca + "$$$\" in " + this.GetPlayerName(1) + "'s " + (l == 0 ? "back" : "front") + " line.";
                                    else if(l <= 3)
                                        return this.GetPlayerName(playerId) + " has clicked on \"$$$" + ca + "$$$\" in " + this.GetPlayerName(0) + "'s " + (l == 2 ? "front" : "back") + " line.";
                                }
                                else if(l == 4)
                                {
                                    return this.GetPlayerName(playerId) + " has clicked on \"$$$" + ca + "$$$\" in row " + (r + 1).ToString() + ".";
                                }
                                l = this.Replay.OwnerPlayer == 0 ? (3 - l) : l;
                                if(l <= 1)
                                    return this.GetPlayerName(playerId) + " has clicked on \"$$$" + ca + "$$$\" in row " + (r + 1).ToString() + " in " + this.GetPlayerName(1) + "'s " + (l == 0 ? "back" : "front") + " line.";
                                else
                                    return this.GetPlayerName(playerId) + " has clicked on \"$$$" + ca + "$$$\" in row " + (r + 1).ToString() + " in " + this.GetPlayerName(0) + "'s " + (l == 2 ? "front" : "back") + " line.";
                            }
                        }
                    }
                }
            }
            else if(actions[1].StartsWith("b"))
            {
                Match mp = patternPosition.Match(actions[1]);
                if(!mp.Success)
                {
#if DEBUG
                throw new Exception();
#else
                    return "";
#endif
                }

                int r, l;
                if(int.TryParse(mp.Groups[1].Value, out r) &&
                    int.TryParse(mp.Groups[2].Value, out l))
                {
                    if(r == 4)
                    {
                        if(l <= 1)
                            return this.GetPlayerName(playerId) + " has clicked on " + this.GetPlayerName(1) + "'s " + (l == 0 ? "back" : "front") + " line.";
                        else if(l <= 3)
                            return this.GetPlayerName(playerId) + " has clicked on " + this.GetPlayerName(0) + "'s " + (l == 2 ? "front" : "back") + " line.";
                    }
                    else if(l == 4)
                    {
                        return this.GetPlayerName(playerId) + " has clicked on row " + (r + 1).ToString() + ".";
                    }
                    l = this.Replay.OwnerPlayer == 0 ? (3 - l) : l;
                    if(l <= 1)
                        return this.GetPlayerName(playerId) + " has clicked on row " + (r + 1).ToString() + " in " + this.GetPlayerName(1) + "'s " + (l == 0 ? "back" : "front") + " line.";
                    else
                        return this.GetPlayerName(playerId) + " has clicked on row " + (r + 1).ToString() + " in " + this.GetPlayerName(0) + "'s " + (l == 2 ? "front" : "back") + " line.";
                }
            }

            {
#if DEBUG
                throw new Exception();
#else
                return "";
#endif
            }
        }

        /// <summary>
        /// Get player name from replay by ID.
        /// </summary>
        /// <param name="id">ID of player, this is either 0 or 1.</param>
        /// <returns></returns>
        protected virtual string GetPlayerName(int id)
        {
            return id == 0 ? this.Replay.NamePlayer1 : this.Replay.NamePlayer2;
        }

        /// <summary>
        /// Get my player name from replay file.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetMyPlayerName()
        {
            return this.Replay.OwnerPlayer == 0 ? this.Replay.NamePlayer1 : this.Replay.NamePlayer2;
        }

        /// <summary>
        /// Get card's name by its ID. This can also be replay ID where premium and other flags are set.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual string GetCardNameById(int id)
        {
            return this.Loader.GetCardName(id);
        }

        /// <summary>
        /// Get card name by it's slot in deck list. This includes the 10000 value.
        /// </summary>
        /// <param name="slot">Card slot. This includes 10000 value.</param>
        /// <returns></returns>
        protected virtual string GetCardNameBySlot(int slot)
        {
            return this.CardSlots.ContainsKey(slot) ? this.CardSlots[slot] : "Error";
        }

        private int turnStatus = 0;

        private static readonly Regex patternMulligan = new Regex(@"^(\d+)\|Mulligan (\d+) (\d+) (\d+) (\d+)$", RegexOptions.Compiled);
        private static readonly Regex patternReveal = new Regex(@"^(\d+)\|RevealToOther (\d+) (\d+) (\d+) (\d+)$", RegexOptions.Compiled);
        private static readonly Regex patternStart = new Regex(@"^(\d+)\|StartGame$", RegexOptions.Compiled);
        private static readonly Regex patternClick = new Regex(@"^(\d+)\|GENERIC SAction U([01])(( [^\s/$]+/?)*)$", RegexOptions.Compiled);
        private static readonly Regex patternEndTurn = new Regex(@"^(\d+)\|NEXTPHASE (\d+)", RegexOptions.Compiled);
        private static readonly Regex patternEndGame = new Regex(@"^(\d+)\|(GameWon|GameLost|GameDraw) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+)", RegexOptions.Compiled);
        private static readonly Regex patternPosition = new Regex(@"b\(([01234]),([01234])\)", RegexOptions.Compiled);
    }
}
