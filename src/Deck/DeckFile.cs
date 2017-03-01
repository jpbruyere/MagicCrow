using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace MagicCrow
{
	[Serializable]
	public class DeckFile
	{
		enum parserState
		{
			init,
			shop,
			metadata,
			main,
			sideboard,
		}

		List<MainLine> cardLines;

		//metadata
		public string Name = "unamed";
		public string DeckType = "";
		public string Description = "";
		public string Set = "";
		public string Image = "";
		//shop
		public int Credits = 0;
		public int MinDifficulty = 0;
		public int MaxDifficulty = 0;

		public string ImgSetPath { get { return "#MagicCrow.images.expansions." + Set + ".svg";	}}

		public IList CardEntries { get { return cardLines;}}

		public DeckFile(string path){
			cardLines = new List<MainLine> ();
			parserState state = parserState.init;

			using (Stream s = new FileStream(path, FileMode.Open))
			{
				using (StreamReader sr = new StreamReader(s))
				{
					while (!sr.EndOfStream)
					{
						string tmp = sr.ReadLine();

						if (tmp.StartsWith("["))
						{
							switch (tmp.ToLower())
							{
							case "[shop]":
								state = parserState.shop;
								continue;
							case "[metadata]":
								state = parserState.metadata;
								continue;
							case "[main]":
								state = parserState.main;
								continue;
							case "[sideboard]":
								state = parserState.sideboard;
								continue;
							default:
								state = parserState.init;
								continue;
							}
						}

						switch (state)
						{
						case parserState.shop:
							break;
						case parserState.metadata:
							string[] tokens = tmp.Split(new char[] { '=' });
							switch (tokens[0].ToLower())
							{
							case "name":
								Name = tokens[1];
								break;
							case "description":
								Description = tokens[1];
								break;
							case "set":
								Set = tokens[1];
								break;
							case "Image":
								Image = tokens[1];
								break;
							case "deck type":
								DeckType = tokens[1];
								break;
							default:
								break;
							}
							break;
						case parserState.main:
							if (string.IsNullOrEmpty (tmp))
								continue;
							MainLine l = new MainLine ();

							string strCount = tmp.Split (new char[] { ' ' }) [0];
							l.count = int.Parse (strCount);
							string strTmp = tmp.Substring (strCount.Length).Trim ();
							string[] ts = strTmp.Split (new char[] { '|' });
							l.name = ts [0];
							l.code = "";
							if (ts.Length > 1)
								l.code = ts [1];

							cardLines.Add (l);
							break;
						case parserState.sideboard:
							break;
						default:
							break;
						}
					}
				}
			}
		}

		public void CacheAllCards(){
			foreach (MainLine l in CardEntries) {				
				MagicCard c = l.Card;
				if (c == null)
					Debug.WriteLine ("DCK: {0} => Card not found: {1}", Name, l.name);
				else
					MagicData.CacheCard (c);
			}
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}
