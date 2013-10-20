using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using System.Text.RegularExpressions;

namespace Replays
{
    /// <summary>
    /// This class represents one game loaded from a replay file.
    /// </summary>
    public sealed class Replay
    {
        /// <summary>
        /// Load a new replay file.
        /// </summary>
        /// <param name="fileName">Full path to the file being loaded.</param>
        /// <param name="time">Start time of the game. This is parsed from file name if possible or the creation time of the file.</param>
        public Replay(string fileName, DateTime time)
        {
            this.FileName = fileName;
            this.Time = time;

            // Load the file into a XML string that we will parse.
            StreamReader file = new StreamReader(fileName);
            string xmlString = file.ReadToEnd();
            file.Close();

            // Create settings and XML reader to parse string.
            XmlReaderSettings rset = new XmlReaderSettings();
            rset.ConformanceLevel = ConformanceLevel.Fragment;
            XmlReader reader = XmlReader.Create(new StringReader(xmlString), rset);

            string element = "";

            // Read through all elements of XML.
            while(reader.Read())
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        element = reader.Name;
                        break;
                    case XmlNodeType.Text:
                        switch(element.ToLower())
                        {
                            case "randomseed":
                                {
                                    int r;
                                    if(int.TryParse(reader.Value, out r))
                                        this.RandomSeed = r;
                                    else
                                        this.RandomSeed = 0;
                                } break;
                            case "enablelocaldraw":
                                {
                                    this.EnableLocalDraw = reader.Value.ToLower() == "true";
                                } break;
                            case "hotseat":
                                {
                                    this.HotSeat = reader.Value.ToLower() == "true";
                                } break;
                            case "ownerplayer":
                                {
                                    int r;
                                    if(int.TryParse(reader.Value, out r))
                                        this.OwnerPlayer = r;
                                    else
                                        this.OwnerPlayer = -1;
                                } break;
                            case "nameplayer1":
                                {
                                    this.NamePlayer1 = reader.Value;
                                } break;
                            case "nameplayer2":
                                {
                                    this.NamePlayer2 = reader.Value;
                                } break;
                            case "eloplayer1":
                                {
                                    int r;
                                    if(int.TryParse(reader.Value, out r))
                                        this.EloPlayer1 = r;
                                    else
                                        this.EloPlayer1 = 0;
                                } break;
                            case "eloplayer2":
                                {
                                    int r;
                                    if(int.TryParse(reader.Value, out r))
                                        this.EloPlayer2 = r;
                                    else
                                        this.EloPlayer2 = 0;
                                } break;
                            case "deckplayer1":
                                {
                                    this.DeckPlayer1.AddRange(reader.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                                } break;
                            case "deckplayer2":
                                {
                                    this.DeckPlayer2.AddRange(reader.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                                } break;
                            case "replaycommandlist":
                                {
                                    this.ReplayCommandList.AddRange(reader.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                                } break;
                            case "checksumplayer1":
                                {
                                    this.ChecksumPlayer1 = reader.Value;
                                } break;
                            case "checksumplayer2":
                                {
                                    this.ChecksumPlayer2 = reader.Value;
                                } break;
                            default:
                                //throw new NotImplementedException();
                                break;
                        }
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        break;
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.EndElement:
                        element = "";
                        break;
                }
            }

            reader.Close();

            this.Finished = false;
            this.Score = 0;

            int totalTime = 0;
            bool hasEnded = false;
            int pturn = -1;
            for(int i = 0; i < this.ReplayCommandList.Count; i++)
            {
                string n = this.ReplayCommandList[i];
                if(n.Contains('|'))
                {
                    if(!hasEnded)
                    {
                        int tm;
                        if(int.TryParse(n.Substring(0, n.IndexOf('|')), out tm))
                        {
                            totalTime += tm;
                            if(pturn == 0)
                                this.Player1TurnsDuration += tm;
                            else if(pturn == 1)
                                this.Player2TurnsDuration += tm;
                        }
                    }
                    n = n.Substring(n.IndexOf('|') + 1).Trim();
                }
                if(n.StartsWith("GameWon "))
                {
                    hasEnded = true;
                    this.Finished = true;
                    this.Score = 1;
                    this.parseWinnings(n.Substring(8));
                }
                else if(n.StartsWith("GameLost "))
                {
                    hasEnded = true;
                    this.Finished = true;
                    this.Score = -1;
                    this.parseWinnings(n.Substring(9));
                }
                else if(n.StartsWith("GameDraw "))
                {
                    hasEnded = true;
                    this.Finished = true;
                    this.Score = 0;
                    this.parseWinnings(n.Substring(9));
                }
                else if(n.StartsWith("StartGame"))
                {
                    this.TurnsPlayer1 = 1;
                    pturn = 0;
                }
                else if(n.StartsWith("NEXTPHASE"))
                {
                    if(pturn == 0)
                    {
                        pturn = 1;
                        TurnsPlayer2++;
                    }
                    else
                    {
                        pturn = 0;
                        TurnsPlayer1++;
                    }
                }
            }

            this.Duration = totalTime;

            // If you don't plan on reading command list clearing would be wise to save a lot of memory when loading thousands of replays.
            //this.ReplayCommandList.Clear();
        }

        private static readonly Regex patternEndGame = new Regex(@"^(\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+)", RegexOptions.Compiled);

        private void parseWinnings(string m)
        {
            Match q = patternEndGame.Match(m);
            m = m.Trim();
            try
            {
                int xp = int.Parse(m.Substring(0, m.IndexOf(' ')));
                m = m.Substring(m.IndexOf(' ') + 1).Trim();

                int gold = int.Parse(m.Substring(0, m.IndexOf(' ')));

                if(q.Success)
                {
                    int newEloMy;
                    int newEloOp;
                    if(int.TryParse(q.Groups[16].Value, out newEloMy) &&
                        int.TryParse(q.Groups[17].Value, out newEloOp))
                    {
                        if(this.OwnerPlayer == 0)
                        {
                            this.NewEloPlayer1 = newEloMy;
                            this.NewEloPlayer2 = newEloOp;
                        }
                        else
                        {
                            this.NewEloPlayer1 = newEloOp;
                            this.NewEloPlayer2 = newEloMy;
                        }
                    }
                }

                this.EarnedXP = xp;
                this.EarnedGold = gold;
            }
            catch
            {
            }
        }

        /// <summary>
        /// File name of this replay.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Unknown. Possibly used to calculate checksum or player and packets?
        /// </summary>
        public int RandomSeed
        {
            get;
            private set;
        }

        /// <summary>
        /// This possibly sets whether local client should handle the game or not. If playing with AI then the game is not played over
        /// network of ubisoft.
        /// </summary>
        public bool EnableLocalDraw
        {
            get;
            private set;
        }

        /// <summary>
        /// Unknown, perhaps this is a feature that was considered but never added.
        /// </summary>
        public bool HotSeat
        {
            get;
            private set;
        }

        /// <summary>
        /// This is the index of player who created this replay. Possible index is 0 or 1. If index is 0 then player #1 is you. If
        /// index is 1 then player #2 is you.
        /// </summary>
        public int OwnerPlayer
        {
            get;
            private set;
        }

        /// <summary>
        /// Name of player #1.
        /// </summary>
        public string NamePlayer1
        {
            get;
            private set;
        }

        /// <summary>
        /// Name of player #2.
        /// </summary>
        public string NamePlayer2
        {
            get;
            private set;
        }

        /// <summary>
        /// Elo of player #1. This is actual ELO and not tournament elo.
        /// </summary>
        public int EloPlayer1
        {
            get;
            private set;
        }

        /// <summary>
        /// Elo of player #2. This is actual ELO and not tournament elo.
        /// </summary>
        public int EloPlayer2
        {
            get;
            private set;
        }

        /// <summary>
        /// List of cards for player #1.
        /// </summary>
        public readonly List<string> DeckPlayer1 = new List<string>();

        /// <summary>
        /// List of cards for player #2.
        /// </summary>
        public readonly List<string> DeckPlayer2 = new List<string>();

        /// <summary>
        /// This is a list of commands that were sent to players playing this game over network. Everything that happened in the game will be here.
        /// </summary>
        public readonly List<string> ReplayCommandList = new List<string>();

        /// <summary>
        /// Unknown, possibly used to calculate values for packets?
        /// </summary>
        public string ChecksumPlayer1
        {
            get;
            private set;
        }

        /// <summary>
        /// Unknown, possibly used to calculate values for packets?
        /// </summary>
        public string ChecksumPlayer2
        {
            get;
            private set;
        }

        /// <summary>
        /// Score of the game. -1 means lose, 0 means draw and 1 means win. This is only valid if game has finished. This value is
        /// for owner player. If score is win and owner player is 0 then player #1 wins if owner player is 1 then player #2 wins.
        /// </summary>
        public int Score
        {
            get;
            private set;
        }

        /// <summary>
        /// Has the game finished properly? This is false if game is currently being played or the game ended with a crash or a bug.
        /// </summary>
        public bool Finished
        {
            get;
            private set;
        }

        /// <summary>
        /// Start time of game.
        /// </summary>
        public DateTime Time
        {
            get;
            private set;
        }

        /// <summary>
        /// How many turns did player 1 play.
        /// </summary>
        public int TurnsPlayer1
        {
            get;
            private set;
        }

        /// <summary>
        /// How many turns did player 2 play.
        /// </summary>
        public int TurnsPlayer2
        {
            get;
            private set;
        }

        /// <summary>
        /// Duration of duel in milliseconds including mulligan phase but excluding anything that happend after game finished.
        /// </summary>
        public int Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// Duration of all player 1 turns.
        /// </summary>
        public int Player1TurnsDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Duration of all player 2 turns.
        /// </summary>
        public int Player2TurnsDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// How much XP did owner of replay gain total.
        /// </summary>
        public int EarnedXP
        {
            get;
            private set;
        }

        /// <summary>
        /// How much gold did owner of replay gain total.
        /// </summary>
        public int EarnedGold
        {
            get;
            private set;
        }

        /// <summary>
        /// New ELO of player 1.
        /// </summary>
        public int NewEloPlayer1
        {
            get;
            private set;
        }

        /// <summary>
        /// New ELO of player 2.
        /// </summary>
        public int NewEloPlayer2
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return this.NamePlayer1 + " (" + this.EloPlayer1 + ") vs. " + this.NamePlayer2 + " (" + this.EloPlayer2 + ")";
        }
    }

    /*
     * File format: XML
     * 
     * Nodes:
     * RandomSeed (Int32): Random value, possibly for generating checksums.
     * EnableLocalDraw (Bool): Unknown, perhaps this is used in single player matches.
     * Hotseat (Bool): Unknown, perhaps a remnant of a considered feature that was never implemented or enabled.
     * OwnerPlayer (Int32): This is either 0 or 1. If 0 then player #1 is who created this replay, if 1 then player #2 is
     *     the one who created replay.
     * NamePlayer1 (String): Name of player #1.
     * NamePlayer2 (String): Name of player #2.
     * EloPlayer1 (Int32): ELO of player #1. This is always ladder ELO and not tournament ELO.
     * EloPlayer2 (Int32): ELO of player #2.
     * DeckPlayer1 (String): Deck of player #1. This is a list of cards separated by newline "\r\n". First entry is always hero. Next event
     *     cards. Then creature, then spell then fortune. This is in format <n>,<c> where n is how many of this card and c is the card
     *     ID or type. If this is the OwnerPlayer deck then this will always be card IDs, otherwise it is only hero who has ID and others
     *     have "EventCard", "CreatureCard", "SpellCard", "FortuneCard". It does include the amount of those cards though.
     * DeckPlayer2 (String): See above.
     * ChecksumPlayer1 (String): Some kind of hashes for the owner player only. Length is 32 bytes for each hash separated by ";".
     * ChecksumPlayer2 (String): See above.
     * ReplayCommandList (String): List of commands that represent the whole game. I believe these commands are also not only used to
     *     build up replay but also used to send over network.
     *     
     * Commands:
     * <n>|Command <args>
     * <n> is how many milliseconds have passed since the last command. This is used to represent time in the replay file.
     * <args> have different format and meaning for each command.
     * 
     * RevealToOther <arg1> <card id> <arg3> <arg4>
     * This will let client know what card is being handled. If card id is 0 then it is unknown card, but the card does exist. This
     * is used to represent opponent's cards for example. arg1 and arg3 could be some kind of slot id's or assigned id's in the game.
     * They seem to be always n + 10000 unless it is a hero card in which case this is 0.
     * It's possible for arg1 == arg3 and also arg1 != arg3. arg4 is the type of reveal.
     * arg3 = slot that the card was assigned to and arg1 is slot where it was assigned from?
     * arg4?
     * 12 - set hero at start of game
     * 2 - player 0 draws from deck or reveals
     * 6 - player 1 draws from deck
     * 6 - player 1 reveals card from hand (to choose now)
     * 9 - set event card
     * 3 - player 0 choosing something from deck so you can see the whole deck
     * 8 - go to graveyard of player 1?
     * 7 - player 1 choosing from deck?
     * arg3 is the slot in the player's decklist player1 slots start from player0 cardcount + 1
     * slot 1 is player 0 hero, player 1 hero is player 0 card count + 1
     * arg1 is unknown, when card id is reveaeled to opponent then arg1 == arg3 otherwise they don't seem to be related.
     * It would be better to say instead of RevealToOther it is an assigment of card to certain slot or assign card ID to a slot.
     * 
     * Mulligan <player id> <kept hand> <ask> <arg4 seems always 0>
     * This is sent 4 times. Only sent before StartGame. Examples:
     * Mulligan 0 0 1 0 <- here is mulligan option for player 0
     * Mulligan 0 1 0 0 <- player 0 chose to keep hand
     * Mulligan 1 0 1 0 <- here is mulligan option for player 1
     * Mulligan 1 0 0 0 <- player 1 took mulligan
     * ask is 0 if it's response and 1 if it's question.
     * 
     * StartGame
     * No arguments. This will mean mulligans and initialization are all done and game started.
     * 
     * NEXTPHASE <arg1> [<arg2>]
     * This means turn ended. arg1 is either a big value or 0 taking turns. unknown what the value means.
     * arg2 was introduced in forgotten wars expansion
     * 
     * GENERIC <arg1> <arg2> <arg3>
     * Some kind of interface action? arg1 seems to be always SAction. arg2 is either U0 or U1, possibly the player ID who is taking
     * the action. arg3 is the action that was taken, arg3 examples:
     * Sclick/ c10001 - card in slot 10001 was clicked (hero of player 0).
     * Sclick/ o1 - option 2 was clicked in a menu (raise magic).
     * Sclick/ b(0,3) - battleground position is clicked, the position here is most upper row and line is back line of player 0 (yes it's reversed).
     * Srclick - a menu was closed from X.
     * Sclick/ c10085/ b(2,0) - a card on battleground position (3rd row from up and player 1 back line) was clicked.
     * 
     * GameWon <arg1 780> <arg2 702> <arg3 400> <arg4 380> <arg5 0> <arg6 450> <arg7 3> <arg8 252> <arg9 0> <arg10 0> <arg11 0> <arg12 0> <arg13 0> <arg14 0> <arg15 0> <arg16 1507> <arg17 1182> [<arg18 1>]
     * GameLost <same as GameWon args>
     * GameDraw <same as GameWon args>
     * Game has ended and this is the result for owner player of replay.
     * arg18 was introduced in forgotten wars expansion
     * 
     * SURRENDER <arg1>
     * arg1 is either U0 or U1 this is the player id who surrendered.
     * 
     * There are plenty more commands.
     */
}
