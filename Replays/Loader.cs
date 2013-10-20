using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;

namespace Replays
{
    /// <summary>
    /// This is a helper class that can load all cards and replays for you.
    /// </summary>
    public sealed class Loader
    {
        /// <summary>
        /// Create a new loader.
        /// </summary>
        public Loader()
        {
        }

        /// <summary>
        /// Add directories for replay loading here.
        /// </summary>
        public readonly List<string> ReplayDirectories = new List<string>();

        /// <summary>
        /// Add directories for card loading here.
        /// </summary>
        public readonly List<string> CardDirectories = new List<string>();

        /// <summary>
        /// Add directories for localization loading here.
        /// </summary>
        public readonly List<string> LocalizationDirectories = new List<string>();

        /// <summary>
        /// Only load missing information. When we StartLoad again.
        /// </summary>
        public bool LoadMissingOnly = true;

        /// <summary>
        /// If we have an unfinished replay then reload the file to see if it has any updates?
        /// Otherwise skip this replay file if only loading missing.
        /// </summary>
        public bool ReloadUnfinishedReplays = true;

        private long _status = 0;
        private long _stop = 0;

        private bool ShouldStop
        {
            get
            {
                return Interlocked.Read(ref _stop) != 0;
            }
        }

        /// <summary>
        /// Start loading on a new thread. Loading is completed when this.IsLoading is false.
        /// </summary>
        public void StartLoad(bool replace)
        {
            if(this.IsLoading)
            {
                if(replace)
                    this.StopLoad();
                else
                    return;
            }

            Interlocked.Exchange(ref _status, 1);
            Interlocked.Exchange(ref _stop, 0);

            Thread t = new Thread(Load);
            t.Start();
        }

        private void Load()
        {
            DirectoryInfo dir;
            FileInfo[] files;

            bool shouldLoadCards = false;
            lock(this.LoadedCards)
            {
                if(this.LoadedCards.Count == 0 || !this.LoadMissingOnly)
                    shouldLoadCards = true;
            }
            bool shouldLoadReplays = true;
            bool shouldLoadLocals = false;
            lock(this.LoadedLocalizations)
            {
                if(this.LoadedLocalizations.Count == 0 || !this.LoadMissingOnly)
                    shouldLoadLocals = true;
            }

            if(shouldLoadCards)
            {
                List<CardData> cardInfo = new List<CardData>();
                foreach(string x in this.CardDirectories)
                {
                    if(this.ShouldStop)
                        break;

                    try
                    {
                        dir = new DirectoryInfo(x);
                        if(!dir.Exists)
                            continue;

                        files = dir.GetFiles("cards_*.xml");
                    }
                    catch
                    {
                        continue;
                    }

                    foreach(FileInfo f in files)
                    {
                        if(this.ShouldStop)
                            break;

                        StreamReader sr;
                        try
                        {
                            sr = new StreamReader(f.FullName);
                        }
                        catch
                        {
                            continue;
                        }

                        string xmlString = sr.ReadToEnd();
                        sr.Close();

                        try
                        {
                            // Create settings and XML reader to parse string.
                            XmlReaderSettings rset = new XmlReaderSettings();
                            rset.ConformanceLevel = ConformanceLevel.Fragment;
                            XmlReader reader = XmlReader.Create(new StringReader(xmlString), rset);

                            while(reader.Read())
                            {
                                if(reader.NodeType == XmlNodeType.Element && reader.Name.Equals("card", StringComparison.OrdinalIgnoreCase))
                                    cardInfo.Add(new CardData(reader));
                            }

                            reader.Close();
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                lock(this.LoadedCards)
                {
                    if(cardInfo.Count != 0)
                        this.LoadedCards = cardInfo;
                }
            }

            List<Replay> replayInfo = new List<Replay>();
            if(this.LoadMissingOnly)
            {
                lock(this.LoadedReplays)
                {
                    replayInfo = this.LoadedReplays.ToList();
                }
            }
            DateTime nao = DateTime.Now;
            foreach(string x in this.ReplayDirectories)
            {
                if(this.ShouldStop)
                    break;

                try
                {
                    dir = new DirectoryInfo(x);
                    if(!dir.Exists)
                        continue;

                    files = dir.GetFiles("*.replay");
                }
                catch
                {
                    continue;
                }

                //int j = files.Length;
                foreach(FileInfo f in files)
                {
                    if(this.ShouldStop)
                        break;

                    // Debug to load faster - only load last X replays
                    /*if(--j >= 200)
                        continue;*/

                    if(this.LoadMissingOnly)
                    {
                        if(this.loadedFilenames.ContainsKey(f.FullName))
                        {
                            if(!this.ReloadUnfinishedReplays)
                                continue;

                            Replay rp = loadedFilenames[f.FullName];

                            if(rp.Finished)
                                continue;

                            if(rp.Time < nao && (nao - rp.Time) > new TimeSpan(8, 0, 0))
                                continue;

                            replayInfo.Remove(rp);
                        }
                    }

                    try
                    {
                        Replay re = new Replay(f.FullName, f.LastWriteTime);
                        replayInfo.Add(re);
                        loadedFilenames[f.FullName] = re;
                    }
                    catch
                    {
                    }
                }
            }

            lock(this.LoadedReplays)
            {
                if(replayInfo.Count != 0)
                    this.LoadedReplays = replayInfo;
            }

            if(shouldLoadLocals)
            {
                List<Localization> locInfo = new List<Localization>();
                this.LoadedLocalizations.Clear();
                foreach(string x in this.LocalizationDirectories)
                {
                    if(this.ShouldStop)
                        break;

                    try
                    {
                        dir = new DirectoryInfo(x);
                        if(!dir.Exists)
                            continue;

                        files = dir.GetFiles("*.bof");
                    }
                    catch
                    {
                        continue;
                    }

                    foreach(FileInfo f in files)
                    {
                        if(this.ShouldStop)
                            break;

                        try
                        {
                            locInfo.Add(new Localization(f.FullName));
                        }
                        catch
                        {
                        }
                    }
                }

                lock(this.LoadedLocalizations)
                {
                    if(locInfo.Count != 0)
                        this.LoadedLocalizations = locInfo;
                }
            }

            // Must reinit cache always.
            this.Cache = null;
            CardCache cache = new CardCache(this);
            this.Cache = cache;

            Interlocked.Exchange(ref _status, 0);
        }

        /// <summary>
        /// Stop loading if it is currently in progress and wait until thread has stopped.
        /// </summary>
        public void StopLoad()
        {
            Interlocked.Exchange(ref _stop, 1);
            while(this.IsLoading)
                Thread.Sleep(1);
        }

        /// <summary>
        /// Check if loader is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return Interlocked.Read(ref _status) != 0;
            }
        }

        /// <summary>
        /// All cards that were loaded by this loader.
        /// </summary>
        public List<CardData> LoadedCards = new List<CardData>();

        /// <summary>
        /// All replays that were loaded by this loader.
        /// </summary>
        public List<Replay> LoadedReplays = new List<Replay>();

        /// <summary>
        /// All localizations that were loaded by this loader.
        /// </summary>
        public List<Localization> LoadedLocalizations = new List<Localization>();

        /// <summary>
        /// Select your localization here. This says which localization file we will get card names from.
        /// </summary>
        public string SelectedLocalization = "English";

        public CardCache Cache = null;

        /// <summary>
        /// Get card by replay id. This only works if we have loaded card data.
        /// </summary>
        /// <param name="id">Id of card in a replay file.</param>
        /// <returns></returns>
        public CardData GetCard(int id)
        {
            id &= 0xFFFF;

            if(this.Cache != null)
                return this.Cache.GetCard(id);

            foreach(CardData c in this.LoadedCards)
            {
                int ri;
                if(int.TryParse(c.GetValue("Card.ID"), out ri) && ri == id)
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Get card by it's name. It depends on which localization you have currently selected.
        /// </summary>
        /// <param name="name">Name of card exactly as it is. This is not case sensitive.</param>
        /// <returns></returns>
        public CardData GetCardByName(string name)
        {
            foreach(CardData d in this.LoadedCards)
            {
                int ri;
                if(int.TryParse(d.GetValue("Card.ID"), out ri))
                {
                    if((this.GetCardName(ri) ?? "").Equals(name, StringComparison.OrdinalIgnoreCase))
                        return d;
                }
            }

            return null;
        }

        /// <summary>
        /// Get display name of card by id. This returns null if card was not found or there is no information on it.
        /// </summary>
        /// <param name="id">Replay ID of card (or regular ID).</param>
        /// <returns></returns>
        public string GetCardName(int id)
        {
            if(this.Cache != null)
                return this.Cache.GetName(id);

            CardData c = GetCard(id);
            if(c != null)
                return this.GetLocalizationString(c.GetValue("Card.Name") + "_Name", 2) ?? c.GetValue("Card.DisplayName");
            return null;
        }

        /// <summary>
        /// Get a string from currently selected localization file.
        /// </summary>
        /// <param name="key">Search this string in localization file.</param>
        /// <param name="mod">Return string where index is searched string's index + mod.</param>
        /// <returns></returns>
        public string GetLocalizationString(string key, int mod)
        {
            if(this.LoadedLocalizations.Count != 0)
            {
                foreach(Localization z in this.LoadedLocalizations)
                {
                    if(z.Name.Equals(this.SelectedLocalization, StringComparison.OrdinalIgnoreCase))
                        return z.GetValue(key, mod);
                }
            }
            return null;
        }

        /// <summary>
        /// This isn't necessary but it speeds up reloading replays.
        /// </summary>
        private readonly Dictionary<string, Replay> loadedFilenames = new Dictionary<string, Replay>();
    }
}
