using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using GGL;

namespace MagicCrow
{
    public enum CardGroupEnum
    {
		Any,
        Library,
        Hand,
        InPlay,
        Permanent,
        Graveyard,
        Exhiled,
        Attackers,
        Blockers,

		Stack,
		Sideboard,
		Command,
		Reveal,
    }
		
    public class CardGroup : CardLayout
    {       		
		#region CTOR
		public CardGroup(CardGroupEnum groupName) : base()        
		{
			GroupName = groupName;
		}
		#endregion

		public static CardGroupEnum ParseZoneName(string zone)
		{
			string[] tmp = zone.Split (',');
			if (tmp.Count () > 1)
				Debug.WriteLine ("unhandled multiple zone");
			
			switch (tmp[0]) {
//			case "Any":
//				return CardGroupEnum.Any;
			case "Battlefield":
				return CardGroupEnum.InPlay;
			case "Exile":
				return CardGroupEnum.Exhiled;
			case "Ante":
				return CardGroupEnum.Hand;
			case "All":
				return CardGroupEnum.Any;
			case "TopOfLibrary":
				return CardGroupEnum.Library;
//			case "Library":
//				return CardGroupEnum.Library;
//			case "Hand":				
//				return CardGroupEnum.Hand;
//			case "Graveyard":
//				return CardGroupEnum.Graveyard;
			default:
				try {
					return (CardGroupEnum) Enum.Parse (typeof(CardGroupEnum), tmp[0]);
				} catch (Exception ex) {
					Debug.WriteLine ("Unknow zone: " + tmp[0]);
					return CardGroupEnum.Any;					
				}
			}
		}
		public CardGroupEnum GroupName;

        public bool IsVisible = true;
		public bool IsSelected = false;
		public bool DepthTest = true;

		public virtual void AddCard(CardInstance c, bool anim = true)
        {
            if (c == null)
                return;

            c.CurrentGroup = this;
            float hSpace = HorizontalSpacing;

            if (HorizontalSpacing * (Cards.Count + 1) > MaxHorizontalSpace)
                hSpace = MaxHorizontalSpace / (Cards.Count + 1);


			Animation.StartAnimation(new FloatAnimation(c, "x", this.x + hSpace / 2 * Cards.Count, 0.2f));

            float halfWidth = hSpace * (Cards.Count) / 2;

			foreach (CardInstance i in Cards) {
				Animation.StartAnimation (new FloatAnimation (i, "x", this.x - halfWidth + hSpace * Cards.IndexOf (i), 0.2f));
				Animation.StartAnimation(new FloatAnimation(i, "z", this.z + VerticalSpacing * Cards.IndexOf(i), 0.2f));
			}

            Animation.StartAnimation(new FloatAnimation(c, "y", this.y, 0.2f));
			Animation.StartAnimation(new FloatAnimation(c, "z", this.z + VerticalSpacing * (Cards.Count+1), 0.2f));
			Animation.StartAnimation(new AngleAnimation(c, "yAngle", this.yAngle, MathHelper.Pi * 0.1f));
			Animation.StartAnimation(new AngleAnimation(c, "xAngle", this.xAngle, MathHelper.Pi * 0.03f));
            
			Cards.Add(c);
			UpdateLayout (anim);
		}
		public virtual void RemoveCard(CardInstance c, bool anim = true)
        {
            Cards.Remove(c);
			UpdateLayout(anim);
        }   

		public CardInstance TakeTopOfStack
        {
            get
            {
                if (Cards.Count == 0)
                    return null;

                return takeCard(Cards.Count - 1);
            }
        }
        public CardInstance takeCard(int index)
        {
            CardInstance c = Cards[index];
            //c.Position = Vector3.Transform(this.Position, c.Deck.Player.Transformations);
            RemoveCard(c);
            return c;
        }

		public bool PointIsIn(Vector3 _p)
		{
			float chW = MagicData.CardWidth / 2f;
			float chH = MagicData.CardHeight / 2f;

			Rectangle<float> r = new Rectangle<float> (
				this.x - chW, this.y - chH, 
				MagicData.CardWidth, MagicData.CardHeight);

			Point<float> p = new Point<float> (_p.X, _p.Y);
			return r.ContainsOrIsEqual (p);
		}
		public override string ToString ()
		{
			return GroupName.ToString();
		}
	}
}
