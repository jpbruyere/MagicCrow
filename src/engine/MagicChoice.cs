//
//  MagicChoice.cs
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
using System.Collections.Generic;

namespace MagicCrow
{
	public class MagicChoice : MagicStackElement
	{
		public List<MagicStackElement> Choices = new List<MagicStackElement>();
		Player _player;
		bool isMandatory = false;
		public MagicChoice ()
		{
		}

		#region implemented abstract members of MagicStackElement
		public override string Title {
			get { return "Choose one:"; }
		}
		public override string Message {
			get { return ""; }
		}
		public override string[] MSECostElements {get { return null; }}
		public override string[] MSEOtherCostElements {get { return null; }}
		public override Player Player {
			get { return _player; }
			set { _player = value; }
		}
		public override bool IsMandatory {
			get { return isMandatory; }
		}
		#endregion
	}
}

