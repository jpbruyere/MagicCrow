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
	public class Animate : Ability
	{
		/// <summary>(required), the power to assign to the animated card</summary>
		public IntegerValue Power;
		/// <summary>(required), the toughness to assign to the animated card</summary>
		public IntegerValue Toughness;
		/// <summary>(optional),  set to True if this effect lasts indefinitely </summary>
		public bool Permanent;
		/// <summary>(optional) - the additional types to give the animated card; comma delimited</summary>
		public AttributGroup<CardTypes> Types;
		/// <summary>(optional) - set to True if the animated being should have these types instead as opposed to in addition to</summary>
		public AttributGroup<CardTypes>OverwriteTypes;
		/// <summary>(optional) - a list of types to Remove from the animated card</summary>
		public AttributGroup<CardTypes>RemoveTypes;
		/// <summary>(optional) - overrides types before it and just will add the ChosenType</summary>
		public AttributGroup<CardTypes>ChosenType;
		/// <summary>(optional) - a " & " delimited list of keywords to give the animated being (just like AB$Pump)</summary>
		public string Keywords;
		/// <summary>(optional) - a " & " delimited list of hidden keywords to give the animated being (just like AB$Pump)</summary>
		public string HiddenKeywords;
		/// <summary>(optional) - a " & " delimited list of keywords to remove from the animated being (just like AB$Debuff)</summary>
		public string RemoveKeywords;
		/// <summary>(optional) - a comma-delimited list of Colors to give to the animated being (capitalized and spelled out) (ChosenColor accepted)</summary>
		public string Colors;
		/// <summary>(optional) - a comma-delimited list of SVar names which contain abilities that should be granted to the animated being</summary>
		public string Abilities;
		/// <summary>(optional) - a comma-delimited list of SVar names which contain triggers that should be granted to the animated being</summary>
		public string Triggers;
		/// <summary>(optional) - a comma-delimited list of SVar names which contain static abilities that should be granted to the animated being</summary>
		public string staticAbilities;
		/// <summary>- boolean Remove all Abilities, Triggers, Statics, and Replacement effects</summary>
		public bool RemoveAllAbilities;
		/// <summary>(optional) - a comma-delimited list of SVars that should be granted to the animated being</summary>
		public string sVars;
		///<summary>(optional) - set to True if the effect should last only until End of Combat instead of End of Turn
		public bool UntilEndOfCombat;
		/// <summary>(optional) - set to True if the effect should last as long as the host is still in play
		public bool UntilHostLeavesPlay;
		public bool UntilYourNextUpkeep;
		public bool UntilControllerNextUntap;
		public bool UntilYourNextTurn;

		public Animate ()
		{
		}

		public override bool TrySetParameter (string paramName, string value)
		{			
			switch (paramName) {
			case "Power":
			case "Toughness":
				SetInteger (paramName, value);
				return true;
			case "Permanent":
				Permanent = bool.Parse (value);
				return true;
			case "Types":
				Types = Target.ParseTargets (value);
				return true;
			case "OverwriteTypes":
				OverwriteTypes = Target.ParseTargets (value);
				return true;
			case "RemoveTypes":
				RemoveTypes = Target.ParseTargets (value);
				return true;
			case "ChosenType":
				Permanent = bool.Parse (value);
				return true;
			case "Keywords":
				Keywords = value;
				return true;
			case "HiddenKeywords":
				HiddenKeywords = value;
				return true;				
			case "RemoveKeywords":
				RemoveKeywords = value;
				return true;
			case "Colors":
				Colors = value;
				return true;
			case "Abilities":
				Abilities = value;
				return true;
			case "Triggers":
				Triggers = value;
				return true;
			case "staticAbilities":
				staticAbilities = value;
				return true;
			case "RemoveAllAbilities":
				RemoveAllAbilities = bool.Parse (value);
				return true;
			case "sVars":
				sVars = value;
				return true;
			case "UntilEndOfCombat":
				UntilEndOfCombat = bool.Parse (value);
				return true;
			case "UntilHostLeavesPlay":
				UntilHostLeavesPlay = bool.Parse (value);
				return true;
			case "UntilYourNextUpkeep":
				UntilYourNextUpkeep = bool.Parse (value);
				return true;
			case "UntilControllerNextUntap":
				UntilControllerNextUntap = bool.Parse (value);
				return true;
			case "UntilYourNextTurn": 
				UntilYourNextTurn = bool.Parse (value);
				return true;
			default:
				return base.TrySetParameter (paramName, value);
			}
		}
	}
}

