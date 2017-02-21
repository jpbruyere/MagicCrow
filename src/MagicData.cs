using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using GGL;
using Cairo;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL;
using System.Runtime.Serialization.Formatters.Binary;
using MagicCrow.Effects;

namespace MagicCrow
{
	public static class MagicData
	{
		public const float CardWidth = 1f;
		public const float CardHeight = 1.425f;

		public static Rectangle<float> CardBounds = new Rectangle<float> (-0.5f,-0.7125f, 1f, 1.425f);
		public static vaoMesh CardMesh;
		public static vaoMesh AbilityMesh;
		public static vaoMesh PointsMesh;
		public static vaoMesh UIQuad;

		public static int CardBack = 0;

		public static Rsvg.Handle hSVGsymbols;
		public static Rsvg.Handle hSVGManas;

		public static AttributGroup<ManaTypes> ParseMultipleColors(string strColors)
		{
			AttributGroup<ManaTypes> result = new AttributGroup<ManaTypes> (AttributeType.Composite);
			string[] tmp = strColors.Split (',');
			foreach (string s in tmp) {
				result += (ManaTypes)Enum.Parse (typeof(ManaTypes), s);	
			}
			return result;
		}
		public static void Init(){
			hSVGsymbols = loadRessourceSvg ("MagicCrow.images.abilities.svg");
			hSVGManas = loadRessourceSvg ("MagicCrow.images.Mana2.svg");
		}
//		public static void InitCardModel()
//		{			
//			if (!Directory.Exists (cardsArtPath))
//				Directory.CreateDirectory (cardsArtPath);
//
//			hSVGsymbols = loadRessourceSvg ("MagicCrow.image2.abilities.svg");
//
//			CardBack = new Texture(@"images/card_back.jpg");
//
//			MagicData.CardMesh = new vaoMesh(0, 0, 0, MagicData.CardWidth, MagicData.CardHeight);
//			MagicData.AbilityMesh = new vaoMesh(0, 0, 0.002f, 1.0f, 0.2f);
//			MagicData.PointsMesh = new vaoMesh(0, 0, 0.002f, 0.50f, 0.2f);
//		}
//		public static void DesinitCardModel()
//		{
////			if (hSVGsymbols != null)
////				hSVGsymbols.Dispose ();
//			GL.DeleteTexture(CardBack);
//			CardMesh.Dispose ();
//			AbilityMesh.Dispose ();
//			PointsMesh.Dispose ();
//		}

		public static List<MagicCard> MissingPicToDownload = new List<MagicCard>();

		static void PictureDownloader()
		{
			while (true)
			{
				int count = 0;
				lock (MissingPicToDownload)
				{
					count = MissingPicToDownload.Count;
				}
				if (count == 0)
					Thread.Sleep(30000);
				else
				{
					MagicCard mc = null;
					string path = "";
					string name = "";
					lock (MissingPicToDownload)
					{
						mc = MissingPicToDownload[0];
						MissingPicToDownload.RemoveAt(0);
					}

					lock (mc)
					{
						path = mc.picturePath;
						name = mc.Name;
						mc.DownloadingTextureInProgress = true;
						mc.DownloadingTryCount++;
					}

					Debug.WriteLine("downloading:" + "http:" + path);
					WebClient webClient = new WebClient();
					string localPath = System.IO.Path.Combine (cardsArtPath, name + ".full.jpg");
					try
					{
						webClient.DownloadFile("http:" + path, localPath);
						Debug.WriteLine("->ok");
					}
					catch (Exception)
					{
						if (File.Exists(localPath))
							File.Delete(localPath);
					}
					lock (mc)
					{
						mc.DownloadingTextureInProgress = false;
					}

				}
			}
		}
		public const string MAGICZIP = "cardsfolder.zip";
		public static string cardsArtPath = 
			System.IO.Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
				@".cache/forge/pics");
		public static string cachingPath = System.IO.Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache/MagicCrow");
		
		const int maxCard = 1000;

		public static void CacheCard(MagicCard c){
			if (!Directory.Exists (cachingPath))
				Directory.CreateDirectory (cachingPath); 
			string cardPath = System.IO.Path.Combine (cachingPath, c.Name + ".bin");
			using (FileStream ms = new FileStream (cardPath, FileMode.Create)) {
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize (ms, c);
			}
		}
		public static bool TryLoadCard(string name, ref MagicCard c){			
			return
				tryGetCardFromCache (name, ref c) ? true :
				TryGetCardFromZip (name, ref c);
		}
		static bool tryGetCardFromCache(string name, ref MagicCard c){
			string cardPath = System.IO.Path.Combine (cachingPath, name + ".bin");
			if (!File.Exists (cardPath))
				return false;
			using (Stream stream = GGL.FileSystemHelpers.GetStreamFromPath (cardPath)) {
				BinaryFormatter formatter = new BinaryFormatter();
				c = (MagicCard)formatter.Deserialize (stream);
			}
			return true;
		}
		public static bool TryGetCardFromZip(string name, ref MagicCard c){
			string cfn = name.Substring(0,1).ToLower() + "/" + name.Trim ().Replace (' ', '_').Replace("\'",string.Empty).ToLower () + ".txt";
			Stream cardStream = GetCardDataStream (cfn);
			if (cardStream == null)
				return false;
			
			try {
				MemoryStream ms = new MemoryStream();
				cardStream.CopyTo (ms);
				ms.Seek(0,SeekOrigin.Begin);
				c = LoadCardData(ms);
				cardStream.Seek(0,SeekOrigin.Begin);
				using (StreamReader tr = new StreamReader(cardStream)){
					c.RawCardData = tr.ReadToEnd();
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				return false;
			}
			return true;
		}
		public static MagicCard LoadCardData(Stream s)
		{
			MagicCard c = new MagicCard();

			using (StreamReader sr = new StreamReader(s))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();

					string[] tmp = line.Split(new char[] { ':' });

					switch (tmp[0].ToLower())
					{
					case "name":
						c.Name = tmp [1];
						break;
					case "manacost":
						c.Cost = Mana.Parse(tmp[1]);
						break;
					case "types":
						string[] types = tmp[1].Split(new char[] { ' ' });
						foreach (string t in types)
						{
							if (string.IsNullOrEmpty(t))
								continue;
							string tt = t.Replace('\'', '_');
							tt = tt.Replace('-', '_');
							c.Types += (CardTypes)Enum.Parse(typeof(CardTypes), tt, true);
						}
						break;
					case "a":
						c.Abilities.Add(Ability.Parse(tmp[1]));
						break;
					case "oracle":
						c.Oracle = string.Join ("\n", tmp.ToList ().GetRange (1, tmp.Length - 1).ToArray ());
						break;
					case "pt":
						string[] pt = tmp[1].Split(new char[] { '/' });

						int intPT;

						if (int.TryParse(pt[0], out intPT))
							c.Power= intPT;
						else
							Debug.WriteLine("power:" + pt[0]);

						if (int.TryParse(pt[1], out intPT))
							c.Toughness = intPT;
						else
							Debug.WriteLine("toughness:" + pt[1]);
						break;
					case "s":
//							Ability aa = c.Abilities.FirstOrDefault ();
//							if (aa == null) {
//								aa = new Ability ();
//								c.Abilities.Add (aa);
//							} else
//								Debugger.Break;

						c.SpellEffects.Add(EffectGroup.Parse(tmp[1]));
						break;
					case "t":
						c.Triggers.Add(Trigger.Parse(tmp[1]));
						break;
					case "svar":

						if (SVarToResolve.TryToParseAndSetValue (tmp [1], tmp [2]))
							break;
						
						switch (tmp[1].ToLower())
						{
						case "darkeffect":
							break;
						case "darkpower":
							break;
						case "darkmana":
							break;
						case "picture":
							c.picturePath = tmp[3];
							break;
						case "remaideck":
							break;
						case "piccount":
							c.nbrImg = int.Parse(tmp[2]);
							break;
						default:
							Debug.WriteLine ("Unknow SVAR: " + tmp[1]);
							break;
						}
						break;
					case "k":
						Ability a = Ability.ParseKeyword(tmp[1],c);
						if (a != null)
							c.Abilities.Add(a);
						break;
					case "r":
						c.R.Add(tmp[1]);
						break;
					case "deckneeds":
						c.DeckNeeds.Add(tmp[1]);
						break;
					case "text":
						c.TextField.Add(tmp[1]);
						break;
					case "alternatemode":
						c.AlternateMode = tmp[1];
						break;
					case "alternate":
						c.Alternate = true;
						break;
					case "colors":
						c.Colors += (ManaTypes)Enum.Parse(typeof(ManaTypes), tmp[1], true);
						break;
					case "loyalty":
						c.Loyalty = tmp[1];
						break;
					case "handlifemodifier":
						c.HandLifeModifier = tmp[1];
						break;
					case "deckhints":
						c.DeckHints = tmp[1];
						break;
					case "var":
						Debug.WriteLine("var: {0} {1} {2}", c.Name, tmp[1], tmp[2]);
						break;
					default:
						if (tmp[0].StartsWith("#"))
							c.Comments.Add(tmp[0]);
						else if (!string.IsNullOrEmpty(tmp[0]))
						{
							string txt = "";
							for (int i = 0; i < tmp.Length; i++)
								txt += ":" + tmp[i];

							Debug.WriteLine("? => {0} {1}", c.Name, txt);
						}
						break;
					}
				}
				SVarToResolve.UnresolvedSVars.Clear ();
			}

			#region add mana ability to basic lands
			if (c.Types == CardTypes.Land)
			{
				if (c.Abilities.Count == 0)
				{
					Mana m = null;
					switch (c.Name.ToLower())
					{
					case "plains":
					case "snow-covered plains":
						m = ManaTypes.White;
						break;
					case "mountain":
					case "snow-covered mountain":
						m = ManaTypes.Red;
						break;
					case "swamp":
					case "snow-covered swamp":
						m = ManaTypes.Black;
						break;
					case "forest":
					case "snow-covered forest":
						m = ManaTypes.Green;
						break;
					case "island":
					case "snow-covered island":
						m = ManaTypes.Blue;
						break;
					}
					if (m != null)
						c.Abilities.Add(
							new Ability(new ManaEffect(m)) { 
								ActivationCost = CostTypes.Tap 
							});
				}
			}
			#endregion

			return c;
		}
		public static void LoadAllCardsInZip()
		{
			ZipFile zf = null;
			MemoryStream ms = new MemoryStream ();
			try {
				FileStream fs = File.OpenRead (MAGICZIP);
				zf = new ZipFile (fs);
				//	ZipEntry zipEntry = zf.FindEntry(cardPath,true);
				//zf.GetInputStream(
				foreach (ZipEntry ze in zf) {
					if (!ze.IsFile)
						continue;
					Stream s = zf.GetInputStream (ze);
					LoadCardData(s);
					s.Dispose();
				}

			} finally {
				if (zf != null) {
					zf.Close (); 
				}
			}
		}
		public static string[] GetCardNames(){
			List<string> tmp = new List<string> ();
			using (Stream fs = new FileStream(MAGICZIP, FileMode.Open, FileAccess.Read))
			{
				ZipFile zf = new ZipFile (fs);
				foreach (ZipEntry ze in zf)
				{					
					if (ze.IsDirectory)
						continue;
					using (Stream s = zf.GetInputStream (ze)) {
						using (StreamReader sr = new StreamReader(s))
						{
							string[] ls = new string[3];
							while (!sr.EndOfStream)
							{
								string line = sr.ReadLine();
								if (line.StartsWith ("name", StringComparison.OrdinalIgnoreCase))
									ls[0] = line.Substring (5);
								else if (line.StartsWith ("manacost", StringComparison.OrdinalIgnoreCase))
									ls[1] = line.Substring (9);
								else if (line.StartsWith ("types", StringComparison.OrdinalIgnoreCase)) {
									ls[2] = line.Substring (6);
									tmp.Add (string.Join(";", ls));
									break;
								}
							}
						}
					}
				}
				zf.Close ();
			}	
			return tmp.ToArray ();
		}
		public static string[] GetCardDataFileNames(){
			List<string> tmp = new List<string> ();
			using (Stream fs = new FileStream(MAGICZIP, FileMode.Open, FileAccess.Read))
			{
				ZipFile zf = new ZipFile (fs);
				foreach (ZipEntry ze in zf)
				{					
					if (ze.IsDirectory)
						continue;
					
					tmp.Add(ze.Name);
				}
				zf.Close ();
			}	
			return tmp.ToArray ();
		}
		public static Stream GetCardDataStream(string cardPath) {
			ZipFile zf = null;
			MemoryStream ms = new MemoryStream ();

			try {
				FileStream fs = File.OpenRead (MAGICZIP);
				zf = new ZipFile (fs);
				int zipEntry = zf.FindEntry(cardPath,true);
				if (zipEntry < 0){
					ms = null;
				}else{
					Stream s = zf.GetInputStream (zf.FindEntry(cardPath,true));
					s.CopyTo (ms);
					ms.Seek(0,SeekOrigin.Begin);
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			} finally {
				if (zf != null)
					zf.Close ();
			}
			return ms;
		}
		static Rsvg.Handle loadRessourceSvg(string resId)
		{			
			Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resId);
			using (MemoryStream ms = new MemoryStream ()) {
				s.CopyTo (ms);
				return new Rsvg.Handle (ms.ToArray ());
			}
		}
	}
}
	