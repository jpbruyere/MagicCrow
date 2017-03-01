﻿//
//  ChangeZoneEffect.cs
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

namespace MagicCrow.Effects
{
	[Serializable]
	public class ChangeZoneEffect : Effect
	{
		public CardGroupEnum Origin;
		public CardGroupEnum Destination;
		public bool Tapped = false;
		public IntegerValue NumCards;

		public ChangeZoneEffect()
		{
			TypeOfEffect = EffectType.ChangeZone;
		}
		public ChangeZoneEffect (CardGroupEnum _destination, CardGroupEnum _origin, bool _tapped = false)
		{
			TypeOfEffect = EffectType.ChangeZone;
			Destination = _destination;
			Origin = _origin;
			Tapped = _tapped;
		}
	}
}
