//
//  CardListView.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2017 jp
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
using Crow;
using System.Xml.Serialization;
using System.IO;
using Cairo;

namespace MagicCrow
{
	public class CardView : GraphicObject
	{
		public CardView (): base ()
		{
		}

		string cardName;
		[XmlAttributeAttribute]	public virtual string CardName {
			get { return cardName; }
			set {
				if (cardName == value)
					return;
				cardName = value; 
				NotifyValueChanged ("CardName", cardName);
				RegisterForRedraw ();
			}
		} 

		protected override void onDraw (Cairo.Context gr)
		{
			base.onDraw (gr);

			if (string.IsNullOrEmpty(cardName))
				return;

			Crow.Rectangle r = ClientRectangle;
			//int zoom = 50;
			string imgPath = cardName + ".full.jpg";
			string[] imgsPath = Directory.GetFiles (Magic.cardImgsBasePath, imgPath, SearchOption.AllDirectories);

			if (imgsPath.Length == 0)
				return;

			System.Drawing.Bitmap bmp = null;
			using (Stream s = new FileStream (imgsPath[0], FileMode.Open)) {
				bmp = new System.Drawing.Bitmap (s);
			}

			if (bmp == null)
				return;
			
			System.Drawing.Imaging.BitmapData data = bmp.LockBits
			(new System.Drawing.Rectangle (0, 0, bmp.Width, bmp.Height),
				                                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			int stride = data.Stride;
						

			using (ImageSurface imgSurf = new ImageSurface (data.Scan0, Format.Argb32, 
				                             bmp.Width, bmp.Height, 4 * bmp.Width)) {
				gr.SetSourceSurface (imgSurf, r.Width / 2 - bmp.Width / 2, r.Height /2 - bmp.Height/2);
				gr.Paint ();
			}
			bmp.UnlockBits (data);
		}
	}
}

