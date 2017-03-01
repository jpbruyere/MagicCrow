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
	public class Deck
	{
		public volatile bool Loaded = false;
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

			Thread thread = new Thread(() => loadingThread());
			thread.IsBackground = true;
			thread.Start();
		}

		#region deck async loading
		void loadingThread(){
			foreach (MainLine l in inputDck.CardEntries) {
				MagicCard c = null;
				if (!MagicData.TryLoadCard (l.name, ref c)) {
					Debug.WriteLine ("DCK: {0} => Card not found: {1}", inputDck.Name, l.name);
					continue;
				}
				for (int i = 0; i < l.count; i++) {
					AddCard (c, l.code);
				}
				Player.ProgressValue++;
			}			
			inputDck = null;
			Loaded = true;
		}
		#endregion

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
}
