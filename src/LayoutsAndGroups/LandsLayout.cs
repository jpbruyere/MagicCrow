//
//  LandsLayout.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2015 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using OpenTK;
using System.Linq;
using System.Collections.Generic;

namespace MagicCrow
{
	public class LandsLayout : CardLayout
	{
		public override void UpdateLayout (bool anim = true)
		{
			if (IsExpanded)
				return;

			float cX = this.x + MaxHorizontalSpace / 2 + 0.3f;
			float cY = this.y;
			float cZ = this.z;

//			foreach (CardInstance c in Cards).Where(ci => ci.IsTapped)) {
//				Animation.StartAnimation (new FloatAnimation (c, "x", cX, 0.3f));
//				Animation.StartAnimation (new FloatAnimation (c, "y", cY, 0.2f));
//				Animation.StartAnimation (new FloatAnimation (c, "z", cZ + c.AttachedCards.Count * attachedCardsSpacing, 0.2f));
//				Animation.StartAnimation (new AngleAnimation (c, "xAngle", xAngle, MathHelper.Pi * 0.3f));
//				Animation.StartAnimation (new AngleAnimation (c, "yAngle", yAngle, MathHelper.Pi * 0.3f));
//				Animation.StartAnimation (new FloatAnimation (c, "zAngle", -MathHelper.PiOver2, MathHelper.Pi * 0.1f));
//				
//				cX += 0.15f;
//				cZ += VerticalSpacing;
//			}


			IEnumerable<CardInstance> untapped = Cards.Where (ci => !ci.IsTapped);//.OrderBy (cci => cci.Model.Types);
			int groupBy = 1;
			if (untapped.Count () > 6)
				groupBy = 3;
			int i = 0;

			float hSpace = HorizontalSpacing;

			if (HorizontalSpacing * Cards.Count > MaxHorizontalSpace)
				groupBy = (int)(HorizontalSpacing * Cards.Count / MaxHorizontalSpace);

			float halfWidth = hSpace * (Cards.Count/groupBy) / 2;

			cX = this.x - MaxHorizontalSpace / 2.0f;
			cY = this.y;
			cZ = this.z;

			hSpace += (groupBy - 1) * 0.1f;

			foreach (CardInstance c in Cards.OrderBy(cc=>cc.Model.Types)){ //untapped) {
				int subI = i % groupBy;
				GGL.Animation.StartAnimation (new GGL.FloatAnimation (c, "x", cX + subI * 0.1f, 0.3f));
				GGL.Animation.StartAnimation (new GGL.FloatAnimation (c, "y", cY - subI * 0.2f, 0.2f));
				GGL.Animation.StartAnimation (new GGL.FloatAnimation (c, "z", cZ + c.AttachedCards.Count * attachedCardsSpacing, 0.2f));
				GGL.Animation.StartAnimation (new GGL.AngleAnimation (c, "xAngle", xAngle, MathHelper.Pi * 0.3f));
				GGL.Animation.StartAnimation (new GGL.AngleAnimation (c, "yAngle", yAngle, MathHelper.Pi * 0.3f));
				if (c.IsTapped)
					GGL.Animation.StartAnimation (new GGL.FloatAnimation (c, "zAngle", -MathHelper.PiOver2, MathHelper.Pi * 0.1f));
				else
					GGL.Animation.StartAnimation (new GGL.FloatAnimation (c, "zAngle", 0, MathHelper.Pi * 0.1f));
				GGL.Animation.StartAnimation (new GGL.FloatAnimation (c, "Scale", this.Scale, 0.05f));

				i++;

				if ((i % groupBy) == 0) {
					cX += hSpace;
					cZ = this.z;
					cY = this.y;
					continue;
				}

				cZ += VerticalSpacing;
			}

		}
	}
}

