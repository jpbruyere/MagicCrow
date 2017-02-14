using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Crow;
using System.Collections;
using System.Threading;
using GGL;

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
				MagicCard c = null;
				if (!MagicData.TryGetCardFromZip (l.name, ref c)) {
					Debug.WriteLine ("DCK: {0} => Card not found: {1}", Name, l.name);
					continue;
				}
				MagicData.CacheCard (c);
			}
		}

		public override string ToString ()
		{
			return Name;
		}
	}

	public class Deck
	{
		public List<CardInstance> Cards = new List<CardInstance>();
		public Player Player;

		public Deck(){			
		}
		DeckFile inputDck = null;

		public Deck(DeckFile df, Player player){
			Player = player;
			player.Deck = this;
			inputDck = df;
			Player.ProgressMax = inputDck.CardEntries.Count;
			Player.ProgressValue = 0;
		}
		public void LoadCards(){
			foreach (MainLine l in inputDck.CardEntries) {
				MagicCard c = null;
				if (!MagicData.TryGetCardFromCache (l.name, ref c)) {
					if (!MagicData.TryGetCardFromZip (l.name, ref c)) {
						Debug.WriteLine ("DCK: {0} => Card not found: {1}", inputDck.Name, l.name);
						return;
					}
				}
				for (int i = 0; i < l.count; i++) {
					AddCard (c, l.code);
				}
				Player.ProgressValue++;
			}			
			inputDck = null;
		}
		public CardInstance AddCard(MagicCard mc, string edition = "")
		{
			CardInstance tmp = new CardInstance (mc) { Edition = edition };
			Cards.Add(tmp);
			return tmp;
		}

		public bool HasAnimatedCards
		{
			get {
				return 
					Animation.AnimationList.Select (al => al.AnimatedInstance).
					OfType<CardInstance> ().Where (c => c.Controler == this.Player).Count() > 0 ?
					true : false;
			}
		}

	}
	class MainLine
	{
		public int count = 0;
		public string name = "unamed";
		public string code = "";
		public string ExpansionImg {
			get { return "#MagicCrow.images.expansions." + code + ".svg"; }
		}
		public override string ToString ()
		{
			return code + ":" + name;
		}
		public MagicCard Card {
			get { 
				MagicCard c = null;
				MagicData.TryGetCardFromZip (name, ref c);
				return c;
			}
		}
	}
}
