//
//  TokenEffect.cs
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

namespace MagicCrow
{
	[Serializable]
	public class TokenEffect : Effect
	{		

		public IntegerValue Amount;
		public IntegerValue Power;
		public IntegerValue Toughness;
		public string Name;
		public MultiformAttribut<CardTypes> Types = new MultiformAttribut<CardTypes>();
		public ControlerType Owner;
		public MultiformAttribut<ManaTypes> Colors = new MultiformAttribut<ManaTypes>();
		public string Image;

		public TokenEffect ()
		{
			TypeOfEffect = EffectType.Token;
		}

		public static ControlerType ParseTokenOWnew(string tkowner)
		{
			switch (tkowner) {
			case "Player.Opponent":
				return ControlerType.Opponent;
			case "Each":
				return ControlerType.All;
			case "Player.IsNotRemembered":
				return ControlerType.IsNotRemembered;
			case "Player.Other":
				return ControlerType.Opponent;
			default:
				return (ControlerType)Enum.Parse(typeof(ControlerType),tkowner);
			}
		}
	}

}

