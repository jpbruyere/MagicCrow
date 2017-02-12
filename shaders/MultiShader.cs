//
//  MultiShader.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2016 jp
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
using OpenTK.Graphics.OpenGL;

namespace MagicCrow
{
	
	public class MultiShader : Tetra.Shader
	{
		public MultiShader (string vertResPath, string fragResPath = null, string geomResPath = null)
			: base(vertResPath, fragResPath,geomResPath)
		{
		}
		protected override void BindSamplesSlots ()
		{
			GL.Uniform1(GL.GetUniformLocation (pgmId, "texCards"), 0);
			GL.Uniform1(GL.GetUniformLocation (pgmId, "texMS"), 2);
			GL.Uniform1(GL.GetUniformLocation (pgmId, "texUI"), 10);
		}
		int selCardLoc;
		int fstkCards, fstkCache, fstkUI, fstkSelection, vstkSelection, vstkNormal, fsTeck, vsTeck;
		protected override void GetUniformLocations ()
		{
			base.GetUniformLocations ();
			selCardLoc = GL.GetUniformLocation(pgmId, "selectedIndex");

			fstkCards = GL.GetSubroutineIndex (pgmId, ShaderType.FragmentShader, "cardsPass");
			fstkCache = GL.GetSubroutineIndex (pgmId, ShaderType.FragmentShader, "cachePass");
			fstkUI = GL.GetSubroutineIndex (pgmId, ShaderType.FragmentShader, "uiPass");
			fstkSelection = GL.GetSubroutineIndex (pgmId, ShaderType.FragmentShader, "selectionPass");
			fsTeck = GL.GetSubroutineUniformLocation (pgmId, ShaderType.FragmentShader, "computeColor");
			vsTeck = GL.GetSubroutineUniformLocation (pgmId, ShaderType.VertexShader, "vsTeck");
			vstkSelection = GL.GetSubroutineIndex (pgmId, ShaderType.VertexShader, "selectionPass");
			vstkNormal = GL.GetSubroutineIndex (pgmId, ShaderType.VertexShader, "normalPass");

		}
		int _selIndex = -1;
		public int SelectedIndex {
			set {
				if (_selIndex == value)
					return;
				_selIndex = value;
				GL.Uniform1 (selCardLoc, _selIndex);
			}
			get { return _selIndex; }
		}
		public void SetCachePass(){
			GL.UniformSubroutines (ShaderType.VertexShader, 1, ref vstkNormal);
			GL.UniformSubroutines (ShaderType.FragmentShader, 1, ref fstkCache);
		}
		public void SetUIPass(){
			GL.UniformSubroutines (ShaderType.VertexShader, 1, ref vstkNormal);
			GL.UniformSubroutines (ShaderType.FragmentShader, 1, ref fstkUI);
		}
		public void SetCardsPass(){
			GL.UniformSubroutines (ShaderType.VertexShader, 1, ref vstkNormal);
			GL.UniformSubroutines (ShaderType.FragmentShader, 1, ref fstkCards);
		}
		public void SetSelectionPass(){
			GL.UniformSubroutines (ShaderType.VertexShader, 1, ref vstkSelection);
			GL.UniformSubroutines (ShaderType.FragmentShader, 1, ref fstkSelection);
		}
	}
}

