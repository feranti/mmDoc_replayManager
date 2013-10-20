using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Replays
{
    /// <summary>
    /// This class contains localization strings.
    /// </summary>
    public sealed class Localization
    {
        /// <summary>
        /// Load localization strings from file.
        /// </summary>
        /// <param name="fileName">File name to load from.</param>
        public Localization(string fileName)
        {
            // Get localization name for later.
            this.Name = Path.GetFileNameWithoutExtension(fileName);
            FileStream fs = File.OpenRead(fileName);
            BinaryReader wr = new BinaryReader(fs);

            /*
             * All of this is possible thanks for ZenityAlpha !
             */

            try
            {
                wr.BaseStream.Position += 8;
                int len = wr.ReadInt32() * 2;
                this.Loaded = Encoding.Unicode.GetString(wr.ReadBytes(len)).Split(new[] { '\0' });
                wr.Close();
            }
            catch
            {
                wr.Close();
                throw;
            }
        }

        /// <summary>
        /// Name of localization. For example "English".
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Array of all loaded strings.
        /// </summary>
        public readonly string[] Loaded = null;

        /// <summary>
        /// Search value of string and return a new value with offset to that string.
        /// </summary>
        /// <param name="key">Value to search for. This is not case sensitive.</param>
        /// <param name="mod">Modify the index of found string by this and return.</param>
        /// <returns></returns>
        public string GetValue(string key, int mod)
        {
            int i = -1;
            foreach(string x in Loaded)
            {
                i++;
                if(x.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    i += mod;
                    return i >= 0 && i < this.Loaded.Length ? this.Loaded[i] : null;
                }
            }

            // Didn't find string in file.
            return null;
        }
    }
}
