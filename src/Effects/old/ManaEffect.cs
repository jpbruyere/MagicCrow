//
//  ManaEffect.cs
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
	public class ManaEffect : Effect
	{
		public Cost ProducedMana;

		public ManaEffect () { TypeOfEffect = EffectType.ProduceMana; }
		public ManaEffect (Cost _producedMana)
		{
			TypeOfEffect = EffectType.ProduceMana;
			ProducedMana = _producedMana;
		}

		protected override void ApplySingle (CardInstance _source, object _target)
		{
			Player player = _target as Player;
			CardInstance cardTarget = _target as CardInstance;
			if (cardTarget == null)
				cardTarget = _source;
			if (player == null && _source != null)
				player = _source.Controler;
			
			if (TypeOfEffect == EffectType.ProduceMana) {
				player.ManaPool += (this as ManaEffect).ProducedMana.Clone ();
				player.NotifyValueChange ("ManaPoolElements", player.ManaPoolElements);
			}

		}
	}
}

