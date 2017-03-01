using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using GGL;
using OpenTK;
using Crow;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Drawing.Imaging;
using System.Drawing;
using Cairo;
using System.Runtime.Serialization.Formatters.Binary;

using MagicCrow.Triggers;
using MagicCrow.Abilities;
//using GLU = OpenTK.Graphics.Glu;

namespace MagicCrow
{	
    [Serializable]
    public class MagicCard
    {
		//TODO:fix data links
        
		public bool configOk = false;
		public string RawCardData;

		public string FilePath;
        public string Name;
        public Cost Cost;
        public AttributGroup<CardTypes> Types = new AttributGroup<CardTypes>();
		public List<Ability> Abilities = new List<Ability>();
		public List<Trigger> Triggers = new List<Trigger>();
		public List<EvasionKeyword> Keywords = new List<EvasionKeyword> ();
        public List<string> Konstrains = new List<string>();
        public List<string> R = new List<string>();
        public List<string> DeckNeeds = new List<string>();
        public List<string> TextField = new List<string>();
        public List<string> Comments = new List<string>();        

        public string Oracle = "";

        public int Power = 0;
        public int Toughness = 0;        
        
        public string S = "";
        public string AlternateMode = "";
        public AttributGroup<ManaTypes> Colors;
        public string Loyalty = "";
        public string HandLifeModifier = "";
        public string DeckHints = "";

        public string picturePath = "";
		public int nbrImg = 1;
		public bool Alternate = false;

        public string picFileNameWithoutExtension
        {
            get
            {				
                string[] tmp = picturePath.Split(new char[] { '/' });
                string[] f = tmp[tmp.Length - 1].Split(new char[] { '.' });
                return f[0];
            }
        }
		public string ImagePath {
			get {
				string basePath = System.IO.Path.Combine (MagicData.cardsArtPath, "cards");
				return nbrImg == 1 ?
						Directory.GetFiles (basePath, Name + ".full.jpg").FirstOrDefault () :
						Directory.GetFiles (basePath, Name + 1 + ".full.jpg").FirstOrDefault ();
			}
		}
		public String[] CostElements
		{
			get{
				if (Cost == null)
					return null;
				string tmp = Cost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length < 3).ToArray();
			}
		}
		public bool IsOk { 
			get { return configOk; }
			set {
				if (configOk == value)
					return;
				configOk = value;
			}
		}
		public bool IsCreature { get { return Types == CardTypes.Creature; } }
		public Ability[] StaticAbilities {
			get { return Abilities.Where (a => a.Category == AbilityCategory.Static).ToArray(); }
		}
		public Ability[] TriggeredAbilities {
			get { return Abilities.Where (a => a.Category == AbilityCategory.DrawBack).ToArray(); }
		}
		public Ability[] ActivatedAbilities {
			get { return Abilities.Where (a => a.Category == AbilityCategory.Acivated).ToArray(); }
		}


        [NonSerialized]public bool DownloadingTextureInProgress = false;
		[NonSerialized]public int DownloadingTryCount = 0;

		void updateGraphic(string file)
        {
            int width = 100;
            int height = 15;
            int x = 10;
            int y = 6;

            Bitmap bmp = new Bitmap(file);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(x, y, width, height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int stride = 4 * width;

            using (ImageSurface draw =
                new ImageSurface(data.Scan0, Format.Argb32, width, height, stride))
            {
                using (Context gr = new Context(draw))
                {
                    //Rectangle r = new Rectangle(0, 0, renderBounds.Width, renderBounds.Height);
                    gr.SelectFontFace("MagicMedieval", FontSlant.Normal, FontWeight.Bold);
                    gr.SetFontSize(12);
					gr.SetSourceColor(Crow.Color.Black);

                    string test = "Test string";

                    FontExtents fe = gr.FontExtents;
                    TextExtents te = gr.TextExtents(test);
                    double xt = 20;// 0.5 - te.XBearing - te.Width / 2,
                    double yt = fe.Height;

                    gr.MoveTo(xt, yt);
                    gr.ShowText(test);

                    using (ImageSurface imgSurf = new ImageSurface(@"images/manaw.png"))
                    {
                        gr.SetSourceSurface(imgSurf, 0,0);

                        gr.Paint();
                    }
                    draw.Flush();
                }
                //draw.WriteToPng(directories.rootDir + @"test.png");
            }


			imgHelpers.imgHelpers.flipY(data.Scan0, stride, height);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, bmp.Height - y - height, width, height,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

        }

        public string colorComponentInPicName
        {
            get
            {
                string color = picFileNameWithoutExtension.Split(new char[] { '_' }).LastOrDefault();

                color = char.ToUpper(color[0]) + color.Substring(1);

                switch (color)
                {
                    case "Black":
                    case "White":
                    case "Red":
                    case "Blue":
                    case "Green":
                    case "Artifacts":
                    case "Shadow":
                    case "Lands":
                        return color;
                    default:
                        return "none";
                }
            }
        }

		void onSaveInCache (object sender, MouseButtonEventArgs e)
		{			
			MagicData.CacheCard (this);
		}
		public void AddCardToHand(){
			CardInstance tmp = Magic.CurrentGameWin.Players[0].Deck.AddCard(this);

			tmp.CreateGLCard ();

			tmp.Controler = Magic.CurrentGameWin.Players[0];
			tmp.ResetPositionAndRotation();
			tmp.yAngle = MathHelper.Pi;
			tmp.Controler.Hand.AddCard(tmp,true);
		}
		void onAddToHand (object sender, MouseButtonEventArgs e)
		{
			AddCardToHand ();
		}
		void onReparse (object sender, MouseButtonEventArgs e)
		{						
			MagicCard c = this;
			string n = Name;
			reset ();
			if (!MagicData.TryGetCardFromZip (n, ref c)) {
				Debug.WriteLine ("DCK: {0} => Card not found: {1}", Name, n);
				return;
			}
		}
		void reset(){
			configOk = false;
			RawCardData = "";

			FilePath = "";
			Name = "";
			Cost = null;
			Types = new AttributGroup<CardTypes>();
			Abilities = new List<Ability>();
			Triggers = new List<Trigger>();
			Konstrains = new List<string>();
			R = new List<string>();
			DeckNeeds = new List<string>();
			TextField = new List<string>();
			Comments = new List<string>();        

			Oracle = "";

			Power = 0;
			Toughness = 0;        

			S = "";
			AlternateMode = "";
			Colors = null;
			Loyalty = "";
			HandLifeModifier = "";
			DeckHints = "";

			picturePath = "";
			nbrImg = 1;
			Alternate = false;			
		}
		public override string ToString ()
		{
			return string.Format("{0} | {1} | {2}", Name, Types, Cost);
		}
    }

}
