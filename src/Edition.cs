using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MagicCrow
{
    public enum Rarities
    {
        Unknown,
        BasicLand,
        Common,
        Uncommon,
        Rare,
        Mythic,
        Token,
        Special
    }

    public enum MagicColors
    {
        NotSet,
        White,
        Blue,
        Black,
        Red,
        Green,
        Artefact,
        Land,
        Multicolor,
        SplitCards
    }

    public enum EditionTypes
    {
        Core,
        Starter,
        Expansion,
        Reprint,
        Duel_Decks,
        From_The_Vault,
        Premium_Deck_Series,
        Other,
    }

    public class Edition
    {
        public static List<Edition> EditionsDB = new List<Edition>();
		static string editionsPath = @"/mnt/data2/downloads/forge-gui-desktop-1.5.31/res/editions";

        public string Name;
        public string Code;
        public string Code2;
        public EditionTypes Type;
        public string Date;
        public string Foil;
        public MagicColors Border = MagicColors.NotSet;


        public List<MagicCardEdition> Cards = new List<MagicCardEdition>();

        enum parserState
        {
            init,
            metadata,
            cards
        }

        public static void LoadEditionsDatabase()
        {
            string[] editions = Directory.GetFiles(editionsPath, "*.txt");

            foreach (string f in editions)
            {
                Edition e = new Edition();
                parserState state = parserState.init;

                using (Stream s = new FileStream(f, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        while (!sr.EndOfStream)
                        {
                            string tmp = sr.ReadLine();

                            switch (state)
                            {
                                case parserState.init:
                                    if (tmp == "[metadata]")
                                        state = parserState.metadata;
                                    break;
                                case parserState.metadata:
                                    if (tmp == "[cards]")
                                    {
                                        state = parserState.cards;
                                        continue;
                                    }
                                    
                                    if (string.IsNullOrEmpty(tmp))
                                        continue;

                                    string[] tokens = tmp.Split(new char[] { '=' });
                                    switch (tokens[0].ToLower())
                                    {
                                        case "code":
                                            e.Code = tokens[1];
                                            break;
                                        case "name":
                                            e.Name = tokens[1];
                                            break;
                                        case "code2":
                                            e.Code2 = tokens[1];
                                            break;
                                        case "type":
                                            e.Type = (EditionTypes)Enum.Parse(typeof(EditionTypes),tokens[1],true);
                                            break;
                                        case "foil":
                                            e.Foil = tokens[1];
                                            break;
                                        case "date":
                                            e.Date = tokens[1];
                                            break;
                                        case "boostercover":
                                        case "boostercovers":
                                        case "booster":
                                        case "alias":
                                        case "foilalwaysincommonslot":
                                        case "foilchanceinbooster":
                                            break;
                                        case "border":
                                            e.Border = (MagicColors)Enum.Parse(typeof(MagicColors), tokens[1], true);
                                            break;
                                        default:
                                            break;
                                    }

                                    break;
                                case parserState.cards:
                                    MagicCardEdition me = new MagicCardEdition();

                                    char rarityToken = tmp[0];

                                    switch (rarityToken)
                                    {
                                        case 'C':
                                            me.Rarity = Rarities.Common;
                                            break;
                                        case 'U':
                                            me.Rarity = Rarities.Uncommon;
                                            break;
                                        case 'R':
                                            me.Rarity = Rarities.Rare;
                                            break;
                                        case 'M':
                                            me.Rarity = Rarities.Mythic;
                                            break;
                                        case 'L':
                                            me.Rarity = Rarities.BasicLand;
                                            break;
                                        case 'T':
                                            me.Rarity = Rarities.Token;
                                            break;
                                        case 'S':
                                            me.Rarity = Rarities.Special;
                                            break;
                                        default:
                                            break;
                                    }

                                    me.Name = tmp.Substring(2);

                                    e.Cards.Add(me);
                                    break;
                                default:
                                    break;
                            }


                        }
                    }
                }

                EditionsDB.Add(e);
            }
        }
    }

    public class MagicCardEdition
    {
        public Rarities Rarity = Rarities.Unknown;
        public string Name;

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }
    }


}
