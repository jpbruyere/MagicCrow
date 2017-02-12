using System;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using GGL;

namespace MagicCrow
{

	public class CardLayout : Layout3d
	{
		protected const float attachedCardsSpacing = 0.01f;
		public List<CardInstance> Cards = new List<CardInstance> ();

		#region CTOR
		public CardLayout(){}
		#endregion

		public override void Render ()
		{
			foreach (CardInstance c in Cards) {
				c.Render ();
			}
		}

		public void ShuffleAndLayoutZ ()
		{
			Cards.Shuffle ();
			float currentZ = this.z;
			foreach (CardInstance c in Cards) {
				Animation a = null;
				if (Animation.GetAnimation (c, "z", ref a))
					a.CancelAnimation ();

				if (c.z != currentZ)
					Animation.StartAnimation (new FloatAnimation (c, "z", currentZ, 0.1f));

				Animation.StartAnimation (new FloatAnimation (c, "x", this.x + 1.5f,0.01f) { Cycle = true});

				currentZ += VerticalSpacing;
			}
		}

		public override  void UpdateLayout (bool anim = true)
		{
//vertical layouting            
			if (IsExpanded)
				return;
			
			float hSpace = HorizontalSpacing;

			if (HorizontalSpacing * Cards.Count > MaxHorizontalSpace)
				hSpace = MaxHorizontalSpace / Cards.Count;


			float halfWidth = hSpace * (Cards.Count) / 2;

			float cX = this.x - halfWidth;
			float cY = this.y;
			float cZ = this.z;

			if (anim) {
				foreach (CardInstance c in Cards) {
					float aX = cX;
					float aY = cY;
					float aZ = cZ + c.AttachedCards.Count * attachedCardsSpacing;


					Animation.StartAnimation (new FloatAnimation (c, "x", aX, 0.2f));
					Animation.StartAnimation (new FloatAnimation (c, "y", aY, 0.2f));

					try {
						if (c.CurrentGroup.GroupName == CardGroupEnum.InPlay && c.HasAbility (AbilityEnum.Flying))
							Animation.StartAnimation (new ShakeAnimation (c, "z", 0.5f, 0.6f));
						else
							Animation.StartAnimation (new FloatAnimation (c, "z", aZ, 0.1f));						
					} catch (Exception ex) {
						Animation.StartAnimation (new FloatAnimation (c, "z", aZ, 0.1f));
					}

					Animation.StartAnimation (new AngleAnimation (c, "xAngle", xAngle, MathHelper.Pi * 0.1f),70);
					Animation.StartAnimation (new AngleAnimation (c, "yAngle", yAngle, MathHelper.Pi * 0.3f));
					if (c.IsTapped)
						Animation.StartAnimation (new FloatAnimation (c, "zAngle", -MathHelper.PiOver2, MathHelper.Pi * 0.1f));
					else
						Animation.StartAnimation (new FloatAnimation (c, "zAngle", 0f, MathHelper.Pi * 0.1f));

					Animation.StartAnimation (new FloatAnimation (c, "Scale", this.Scale, 0.05f));

					cX += hSpace;
					cZ += VerticalSpacing;
				}
				return;
			}
			foreach (CardInstance c in Cards) {
				c.x = cX;
				c.y = cY;
				c.z = cZ + c.AttachedCards.Count * attachedCardsSpacing;
				c.xAngle = xAngle;
				c.yAngle = yAngle;
				if (c.IsTapped)
					c.zAngle = -MathHelper.PiOver2;
				else
					c.zAngle = 0f;


				cX += hSpace;
				cZ += VerticalSpacing;
			}
		}			
		public override void toogleShowAll ()
		{
			IsExpanded = !IsExpanded;
			if (IsExpanded) {
				Vector3 v = Magic.vGroupedFocusedPoint;
				float aCam = Magic.FocusAngle;

				float horizontalLimit = 8f;

				float cX = v.X;
				float cZ = v.Z;

				float hSpace = horizontalLimit / Cards.Count;
				float vSpace = -0.01f;

				if (hSpace > 0.9f)
					hSpace = 0.9f;

				cX -= hSpace * Cards.Count / 2;

				int delay = 0;

				foreach (CardInstance c in Cards) {
					Animation.StartAnimation (new FloatAnimation (c, "x", cX, 0.2f));
					Animation.StartAnimation (new FloatAnimation (c, "y", v.Y, 0.1f));
					Animation.StartAnimation (new FloatAnimation (c, "z", cZ, 0.1f));
					Animation.StartAnimation (new AngleAnimation (c, "xAngle", aCam, MathHelper.Pi * 0.1f), 100);
					Animation.StartAnimation (new AngleAnimation (c, "yAngle", 0, MathHelper.Pi * 0.1f), 100);

					cX += hSpace;
					cZ += vSpace;

					delay += 10;
				}
			} else
				UpdateLayout ();
		}
		public void RevealToUIPlayer()
		{
			IsExpanded = !IsExpanded;

			if (!IsExpanded) {
				UpdateLayout ();
				return;
			}

			Vector3 v = Magic.vGroupedFocusedPoint;
			v = v.Transform(Matrix4.Invert(Cards.FirstOrDefault().Controler.Transformations));

			float aCam = Magic.FocusAngle;

			float horizontalLimit = 5f;

			float cX = v.X;
			float cZ = v.Z;

			float hSpace = horizontalLimit / Cards.Count;
			float vSpace = 0.01f;

			if (hSpace > 0.9f)
				hSpace = 0.9f;

			cX -= hSpace * Cards.Count / 2;

			int delay = 0;

			foreach (CardInstance c in Cards) {
				Animation.StartAnimation (new FloatAnimation (c, "x", cX, 0.2f));
				Animation.StartAnimation (new FloatAnimation (c, "y", v.Y, 1.0f));
				Animation.StartAnimation (new FloatAnimation (c, "z", cZ, 0.5f));
				Animation.StartAnimation (new AngleAnimation (c, "xAngle", aCam, MathHelper.Pi * 0.1f), 100);
				Animation.StartAnimation (new AngleAnimation (c, "yAngle", 0, MathHelper.Pi * 0.1f), 100);
				Animation.StartAnimation(new AngleAnimation(c, "zAngle", -c.Controler.zAngle, MathHelper.Pi * 0.3f));

				cX += hSpace;
				cZ += vSpace;

				delay += 10;
			}
		}

		public void UpdateDefendersLayout ()
		{            
			//vertical layouting            

			float hSpace = 0.25f;
			float vSpace = -0.25f;
			float zSpace = 0.001f;

			foreach (CardInstance c in Cards) {
				if (c.BlockedCreature == null)
					continue;
				int idx = c.BlockedCreature.BlockingCreatures.IndexOf (c);

				Vector3 v = c.BlockedCreature.Position;
				v = v.Transform (Matrix4.Invert (c.BlockedCreature.Controler.Transformations));

				float cX = v.X + hSpace * idx;
				float cY = this.y + vSpace * idx;
				float cZ = this.z + zSpace * idx;

				Animation.StartAnimation (new FloatAnimation (c, "x", cX, 0.2f));
				Animation.StartAnimation (new FloatAnimation (c, "y", cY, 0.2f));
				Animation.StartAnimation (new FloatAnimation (c, "z", cZ, 0.2f));
				Animation.StartAnimation (new AngleAnimation (c, "xAngle", xAngle, MathHelper.Pi * 0.3f));
				Animation.StartAnimation (new AngleAnimation (c, "yAngle", yAngle, MathHelper.Pi * 0.3f));

				float aX = cX;
				float aY = cY;
				float aZ = cZ;
				foreach (CardInstance ac in c.AttachedCards) {
					aX += 0.15f;
					aY += 0.15f;
					aZ -= 0.001f;

					//					Animation.StartAnimation(new FloatAnimation(ac, "x", aX, 0.2f));
					//					Animation.StartAnimation(new FloatAnimation(ac, "y", aY, 0.2f));
					//					Animation.StartAnimation(new FloatAnimation(ac, "z", aZ, 0.2f));
					//					Animation.StartAnimation(new AngleAnimation(ac, "xAngle", xAngle, MathHelper.Pi * 0.3f));
					//					Animation.StartAnimation(new AngleAnimation(ac, "yAngle", yAngle, MathHelper.Pi * 0.3f));
				}
			}
		}			
	}
}

