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
using System.ComponentModel;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace MagicCrow
{
	public class CardDataGrid : GraphicObject
	{
		
		public CardDataGrid (): base ()
		{
		}

		#region events
		public event EventHandler<SelectionChangeEventArgs> SelectedItemChanged;
		#endregion

		const int scrBarWidth = 10;
		IList datas;
		double lineHeight = 1;
		int selectedIndex = -1, hoverIndex = -1, lastSelectedIndex = -1,
		lastHoverIndex = -1, visibleLines = 0, lastVisibleLines = 0, scroll = 0, lastScroll = 0, scrollSpeed = 10;
		Crow.Color selBackground, selForeground, hoverBackground, hoverForeground;
		bool mouseScrolling = false;
		protected Crow.Point mousePos;

		FontExtents fe;

		object mutex = new object();

		[XmlAttributeAttribute]public virtual IList Datas {
			get { return datas; }
			set {
				if (datas == value)
					return;

				datas = value;

				NotifyValueChanged ("Datas", datas);
				RegisterForGraphicUpdate ();
			}
		}
		[XmlAttributeAttribute()][DefaultValue(0)]
		public virtual int Scroll {
			get { return scroll; }
			set {
				if (scroll == value)
					return;
				lock (mutex) {
					scroll = value;

					if (scroll + visibleLines > Datas.Count)
						scroll = Datas.Count - visibleLines;
					if (scroll < 0)
						scroll = 0;
				}
				NotifyValueChanged ("Scroll", scroll);
				RegisterForGraphicUpdate ();
			}
		}
		[XmlAttributeAttribute][DefaultValue(-1)]public int SelectedIndex{
			get { return selectedIndex; }
			set {
				if (value == selectedIndex)
					return;
				lock (mutex) {
					if (datas == null)
						selectedIndex = -1;
					else if (value < 0)
						selectedIndex = 0;
					else if (selectedIndex >= datas.Count)
						selectedIndex = datas.Count - 1;
					else
						selectedIndex = value;
				
					NotifyValueChanged ("SelectedIndex", selectedIndex);
					NotifyValueChanged ("SelectedItem", SelectedItem);
					SelectedItemChanged.Raise (this, new SelectionChangeEventArgs (SelectedItem));
				}
				RegisterForRedraw ();
			}
		}
		[XmlAttributeAttribute][DefaultValue(-1)]public int HoverIndex{
			get { return hoverIndex; }
			set {
				if (value == hoverIndex)
					return;
				lock (mutex) {
					if (datas == null)
						hoverIndex = -1;
					else if (value < 0)
						hoverIndex = 0;
					else if (hoverIndex >= datas.Count)
						hoverIndex = datas.Count - 1;
					else
						hoverIndex = value;
				}

				NotifyValueChanged ("HoverIndex", selectedIndex);
				RegisterForRedraw ();
			}
		}
		[XmlIgnore]public virtual object SelectedItem{
			get { return datas == null ? null : selectedIndex < 0 ? null : (datas[selectedIndex] as string).Split(';')[0]; }
//			set {
//				lock (mutex) {
//					if (value == SelectedItem || datas == null )
//						return;
//					IEnumerator e = datas.GetEnumerator ();
//					int idx = 0;
//					string v = value as string;
//					while (e.MoveNext ()) {
//						string c = e.Current as string;
//						if (c.StartsWith (v, StringComparison.OrdinalIgnoreCase)) {
//							SelectedIndex = idx;
//							NotifyValueChanged ("SelBegin", new Crow.Point (c.Length - v.Length, 0));
//							NotifyValueChanged ("SelEnd", new Crow.Point (c.Length, 0));
//							break;
//						}
//						idx++;
//					}
//				}
//			}
		}
		[XmlAttributeAttribute][DefaultValue("SteelBlue")]
		public virtual Crow.Color SelectionBackground {
			get { return selBackground; }
			set {
				if (value == selBackground)
					return;
				selBackground = value;
				NotifyValueChanged ("SelectionBackground", selBackground);
				RegisterForRedraw ();
			}
		}
		[XmlAttributeAttribute][DefaultValue("White")]
		public virtual Crow.Color SelectionForeground {
			get { return selForeground; }
			set {
				if (value == selForeground)
					return;
				selForeground = value;
				NotifyValueChanged ("SelectionForeground", selForeground);
				RegisterForRedraw ();
			}
		}
		[XmlAttributeAttribute][DefaultValue("BlueCrayola")]
		public virtual Crow.Color HoverBackground {
			get { return hoverBackground; }
			set {
				if (value == hoverBackground)
					return;
				hoverBackground = value;
				NotifyValueChanged ("HoverBackground", hoverBackground);
				RegisterForRedraw ();
			}
		}
		[XmlAttributeAttribute][DefaultValue("White")]
		public virtual Crow.Color HoverForeground {
			get { return hoverForeground; }
			set {
				if (value == hoverForeground)
					return;
				hoverForeground = value;
				NotifyValueChanged ("HoverForeground", hoverForeground);
				RegisterForRedraw ();
			}
		}

		public override void OnLayoutChanges (LayoutingType layoutType)
		{
			base.OnLayoutChanges (layoutType);

			if (layoutType == LayoutingType.Height) {
				using (ImageSurface img = new ImageSurface (Format.Argb32, 10, 10)) {
					using (Context gr = new Context (img)) {
						//Cairo.FontFace cf = gr.GetContextFontFace ();

						gr.SelectFontFace (Font.Name, Font.Slant, Font.Wheight);
						gr.SetFontSize (Font.Size);

						fe = gr.FontExtents;
					}
				}

				lineHeight = fe.Height;
				visibleLines = (int)Math.Floor ((double)ClientRectangle.Height / fe.Height);
				System.Diagnostics.Debug.WriteLine ("Visible lines: {0})", visibleLines);
				if (Scroll + visibleLines > Datas?.Count)
					Scroll = Datas.Count - visibleLines;
			}
		}
		protected override void RecreateCache ()
		{
			lock (mutex) {
				int stride = 4 * Slot.Width;

				int bmpSize = Math.Abs (stride) * Slot.Height;
				if (lastVisibleLines > 0) {
					byte[] newBmp = new byte[bmpSize];

					using (ImageSurface draw =
						      new ImageSurface (newBmp, Format.Argb32, Slot.Width, Slot.Height, stride)) {
						using (Context gr = new Context (draw)) {
							using (ImageSurface lastDraw =
								      new ImageSurface (bmp, Format.Argb32, LastPaintedSlot.Width, LastPaintedSlot.Height,
									      LastPaintedSlot.Width * 4)) {
								gr.SetSource (lastDraw, 0, (lastScroll - scroll) * lineHeight);
								gr.Paint ();
							}
						}
					}
					bmp = newBmp;
				} else
					bmp = new byte[bmpSize];					
				IsDirty = false;

				using (ImageSurface draw =
					      new ImageSurface (bmp, Format.Argb32, Slot.Width, Slot.Height, stride)) {
					using (Context gr = new Context (draw)) {
						gr.Antialias = Interface.Antialias;
						onDraw (gr);
					}
					draw.Flush ();
				}
			}
		}
		double[] cols = new double[] { 0.45, 0.20, 0.35 };

		void drawDataLine(Context gr, int ptr, double x, double y, double lineWidth, double lineHeight){
			//System.Diagnostics.Debug.WriteLine ("drawLine {0} -> ({1},{2})", ptr, x, y);
			if (ptr == SelectedIndex) {
				gr.SetSourceColor (SelectionBackground);
				gr.Rectangle (x, y, lineWidth, fe.Height);
				gr.Fill ();
				gr.SetSourceColor (SelectionForeground);
			}else if (ptr == hoverIndex) {
				gr.SetSourceColor (HoverBackground);
				gr.Rectangle (x, y, lineWidth, fe.Height);
				gr.Fill ();
				gr.SetSourceColor (HoverForeground);
			}else
				gr.SetSourceColor (Foreground);

			string[] l = (datas [ptr] as string).Split(';');

			//name
			gr.MoveTo (x+2, y + fe.Ascent);
			gr.ShowText (l[0]);
			gr.Fill ();
			x += lineWidth * cols [0];

			gr.Save ();
			gr.Translate (x+2, y);
			gr.Scale (0.1, 0.1);
			string[] manas = l [1].Split (' ');
			foreach (string m in manas) {
				MagicData.hSVGManas.RenderCairoSub (gr, "#" + m);
				gr.Translate (100, 0);
			}
			gr.Restore ();

			x += lineWidth * cols [1];
			//types
			gr.MoveTo (x+2, y + fe.Ascent);
			gr.ShowText (l[2]);
			gr.Fill ();
		}
		protected override void onDraw (Cairo.Context gr)
		{			
			if (datas == null)
				return;

			lock (mutex) {				
				gr.SelectFontFace (Font.Name, Font.Slant, Font.Wheight);
				gr.SetFontSize (Font.Size);

				Crow.Rectangle scrBar = ClientRectangle;
				if (datas?.Count > visibleLines) {					
					scrBar.X += ClientRectangle.Width - scrBarWidth;
					scrBar.Width = scrBarWidth;
				}
				Crow.Rectangle r = ClientRectangle;
				r.Width -= scrBarWidth;

				int diffScroll = lastScroll - scroll;
				int diffVisible = visibleLines - lastVisibleLines;

				int limInf = 0;
				int limSup = diffScroll + diffVisible;

				if (diffScroll < 0) {
					limInf = lastVisibleLines + diffScroll;
					limSup = visibleLines;
				}

				gr.Save ();
				gr.ResetClip ();

				gr.Rectangle (r.X, r.Y + limInf * fe.Height, r.Width, (limSup - limInf) * fe.Height);
				if (lastHoverIndex != HoverIndex) {
					if (lastHoverIndex >= 0)
						gr.Rectangle (r.X, r.Y + (lastHoverIndex - Scroll) * fe.Height, r.Width, fe.Height);
					if (HoverIndex >= 0)
						gr.Rectangle (r.X, r.Y + (HoverIndex - Scroll) * fe.Height, r.Width, fe.Height);
				}
				if (lastSelectedIndex != SelectedIndex) {
					if (lastSelectedIndex >= 0)
						gr.Rectangle (r.X, r.Y + (lastSelectedIndex - Scroll) * fe.Height, r.Width, fe.Height);
					if (selectedIndex >= 0)
						gr.Rectangle (r.X, r.Y + (SelectedIndex - Scroll) * fe.Height, r.Width, fe.Height);
				}
				if (datas?.Count > visibleLines && diffScroll != 0)
					gr.Rectangle (scrBar);

				gr.ClipPreserve ();
				gr.Operator = Operator.Clear;
				gr.Fill ();
				gr.Operator = Operator.Over;

				base.onDraw (gr);

				Foreground.SetAsSource (gr);

				int i = limInf;
				while (i < limSup) {
					if (i + Scroll >= Datas.Count)
						break;
					drawDataLine (gr, i + Scroll, r.X, r.Y + i * fe.Height, (double)r.Width, fe.Height);
					i++;
				}
				int redrawnLines = limSup - limInf;
				System.Diagnostics.Debug.WriteLine ("draw {0} lines from {1} to {2}", redrawnLines, limInf, limSup - 1);
				if (lastHoverIndex != HoverIndex) {
					if (lastHoverIndex >= 0 && (redrawnLines == 0 || (lastHoverIndex < limInf + Scroll || lastHoverIndex >= limSup + Scroll)))
						drawDataLine (gr, lastHoverIndex, r.X, r.Y + (lastHoverIndex - Scroll) * fe.Height, (double)r.Width, fe.Height);
					if (HoverIndex >= 0 && (redrawnLines == 0 || (HoverIndex < limInf + Scroll|| HoverIndex >= limSup + Scroll)))
						drawDataLine (gr, HoverIndex, r.X, r.Y + (HoverIndex - Scroll) * fe.Height, (double)r.Width, fe.Height);
				}
				if (lastSelectedIndex != SelectedIndex) {
					if (lastSelectedIndex >= 0 && (redrawnLines == 0 || (lastSelectedIndex < limInf  + Scroll|| lastSelectedIndex >= limSup + Scroll)))
						drawDataLine (gr, lastSelectedIndex, r.X, r.Y + (lastSelectedIndex - Scroll) * fe.Height, (double)r.Width, fe.Height);
					if (SelectedIndex >= 0 && (redrawnLines == 0 || (SelectedIndex < limInf + Scroll || SelectedIndex >= limSup + Scroll)))
						drawDataLine (gr, SelectedIndex, r.X, r.Y + (SelectedIndex - Scroll) * fe.Height, (double)r.Width, fe.Height);
				}

				double x = ClientRectangle.X;
				gr.LineWidth = 1;
				for (i = 0; i < cols.Length; i++) {
					x += (double)r.Width * cols [i];
					gr.MoveTo (Math.Floor (x) + 0.5, 0);
					gr.LineTo (Math.Floor (x) + 0.5, r.Height);
				}
				gr.Stroke ();

				lastScroll = Scroll;
				lastHoverIndex = HoverIndex;
				lastSelectedIndex = SelectedIndex;
				lastVisibleLines = visibleLines;

				if (Datas?.Count <= visibleLines) {
					gr.Restore ();
					return;
				}
				if (mouseScrolling)
					new SolidColor (Crow.Color.Jet.AdjustAlpha (0.5)).SetAsSource (gr);
				else
					new SolidColor (Crow.Color.LightGray.AdjustAlpha (0.5)).SetAsSource (gr);
				CairoHelpers.CairoRectangle (gr, scrBar, 0.0);
				gr.Fill ();
				new SolidColor (Crow.Color.BlueCrayola.AdjustAlpha (0.7)).SetAsSource (gr);
				scrBar.Y += (int)((double)scrBar.Height * (double)Scroll / (double)Datas.Count);
				scrBar.Height = (int)((double)scrBar.Height * (double)visibleLines / (double)Datas.Count);
				CairoHelpers.CairoRectangle (gr, scrBar, 2.0);
				gr.Fill ();
				gr.Restore ();
			}
		}
		public override void onMouseWheel (object sender, MouseWheelEventArgs e)
		{
			base.onMouseWheel (sender, e);
			if (e.Delta > 0)
				Scroll-=scrollSpeed;
			else
				Scroll+=scrollSpeed;
			updateMouseLocalPos (e.Position);
			HoverIndex = (int)Math.Floor (mousePos.Y / lineHeight) + scroll;
			RegisterForRedraw ();
		}
		public override void onMouseMove (object sender, MouseMoveEventArgs e)
		{
			base.onMouseMove (sender, e);
			updateMouseLocalPos (e.Position);

			if (mouseScrolling) {
				Scroll = (int)((double)mousePos.Y / (double)ClientRectangle.Height * (double)datas.Count);
			}else
				HoverIndex = (int)Math.Floor (mousePos.Y / lineHeight) + scroll;
			RegisterForRedraw ();
		}
		public override void onMouseDown (object sender, MouseButtonEventArgs e)
		{
			base.onMouseDown (sender, e);
			if (mousePos.X + scrBarWidth >= ClientRectangle.Width)
				mouseScrolling = true;
			else
				SelectedIndex = hoverIndex;
		}
		public override void onMouseUp (object sender, MouseButtonEventArgs e)
		{
			base.onMouseUp (sender, e);
			mouseScrolling = false;
		}
		public override void onKeyDown (object sender, KeyboardKeyEventArgs e)
		{
			base.onKeyDown (sender, e);
			if (e.Key == Key.Home) {
				SelectedIndex = 0;
				Scroll = 0;
			} else if (e.Key == Key.End) {
				Scroll = Datas.Count - visibleLines;
				SelectedIndex = datas.Count - 1;
			}else if (e.Key == Key.Down) {
				SelectedIndex++;
				if (SelectedIndex >= Scroll + visibleLines)
					Scroll = SelectedIndex - visibleLines + 1;
			} else if (e.Key == Key.Up) {				
				SelectedIndex--;
				if (SelectedIndex < Scroll)
					Scroll = SelectedIndex;
			} else if (e.Key == Key.PageDown) {
				SelectedIndex += visibleLines;
				if (SelectedIndex >= Scroll + visibleLines)
					Scroll = SelectedIndex - visibleLines + 1;
			} else if (e.Key == Key.PageUp) {
				SelectedIndex-= visibleLines;
				if (SelectedIndex < Scroll)
					Scroll = SelectedIndex;
			}
		}

		protected virtual void updateMouseLocalPos(Crow.Point mPos){
			Crow.Rectangle r = ScreenCoordinates (Slot);
			Crow.Rectangle cb = ClientRectangle;
			mousePos = mPos - r.Position;

			mousePos.X = Math.Max(cb.X, mousePos.X);
			mousePos.X = Math.Min(cb.Right, mousePos.X);
			mousePos.Y = Math.Max(cb.Y, mousePos.Y);
			mousePos.Y = Math.Min(cb.Bottom, mousePos.Y);
		}
	}
}

