using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Replays
{
    /// <summary>
    /// This class will load all card data from an XML file which can be queried later.
    /// </summary>
    public sealed class CardData
    {
        public CardData(XmlReader reader)
        {
            string element;
            this.Info = new CardInfo();
            CardInfo ci = this.Info;
            bool end = false;
            do
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        element = reader.Name;
                        if(!ci.Info.ContainsKey(element))
                        {
                            ci.Info[element] = new CardInfo();
                            ci.Info[element].Parent = ci;
                        }
                        ci = ci.Info[element];
                        if(reader.MoveToFirstAttribute())
                        {
                            ci.Info[reader.Name] = new CardInfo();
                            ci.Info[reader.Name].Key = reader.Value;
                            while(reader.MoveToNextAttribute())
                            {
                                ci.Info[reader.Name] = new CardInfo();
                                ci.Info[reader.Name].Key = reader.Value;
                            }
                            reader.MoveToElement();
                        }
                        if(reader.IsEmptyElement && ci.Parent != null)
                            ci = ci.Parent;
                        if(reader.IsEmptyElement && reader.Name.ToLower() == "card")
                            end = true;
                        break;
                    case XmlNodeType.Text:
                        ci.Key = reader.Value;
                        break;
                    case XmlNodeType.EndElement:
                        if(ci.Parent != null)
                            ci = ci.Parent;
                        if(reader.Name.ToLower() == "card")
                            end = true;
                        break;
                }
            } while(!end && reader.Read());
        }

        private CardInfo Info = null;

        /// <summary>
        /// Get value by key. For example "Card.Name". The key is not case sensitive. The keys are directly what is written in XML file.
        /// So that Card.Name might return Cre_Haven_005 but Card.DisplayName will return the actual name of card that you may be looking for.
        /// If the value is not specified or invalid then null will be returned.
        /// </summary>
        /// <param name="key">Key for value. For example "Card.Name" this is not case sensitive.</param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            CardInfo n = this.Info;

            string[] spl = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < spl.Length; i++)
            {
                if(!n.Info.ContainsKey(spl[i]))
                    return null;

                n = n.Info[spl[i]];
            }

            if(n != null)
                return n.Key;

            return null;
        }
    }

    internal sealed class CardInfo
    {
        internal CardInfo Parent = null;

        internal string Key = null;

        internal readonly Dictionary<string, CardInfo> Info = new Dictionary<string, CardInfo>(StringComparer.OrdinalIgnoreCase);
    }
}
