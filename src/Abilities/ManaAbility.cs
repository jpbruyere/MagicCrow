//
//  Ability.cs
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

namespace MagicCrow.Abilities
{
	[Serializable]
	public class ManaAbility : Ability
	{
		public Cost Produced;
		public IntegerValue Amount;

		public ManaAbility ()
		{
		}
		public override bool TrySetParameter (string paramName, string value)
		{			
			switch (paramName) {
			case "Produced":
				Produced = Cost.Parse (value);
				return true;
			case "Amount":
				SetInteger ("Amount", value);
				return true;
			default:
				return base.TrySetParameter (paramName, value);
			}
		}
		public override void Resolve (AbilityActivation source)
		{
			source.Player.ManaPool += Produced;
			source.Player.NotifyValueChange ("ManaPoolElements", source.Player.ManaPoolElements);

			if (source.CardSource.HasType(CardTypes.Land))
				MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(Triggers.Mode.TapsForMana, source));
		}
	}
}

