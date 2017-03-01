//
//  NumericEffect.cs
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
	public class AddOrRemoveCounter : NumericEffect
	{
		public enum CounterType
		{
			P1P1
		}
		public CounterType Type;

		public AddOrRemoveCounter() : base() {}
		public AddOrRemoveCounter(EffectType et) : base(et){}
		public AddOrRemoveCounter(EffectType et, IntegerValue amount) : base(et)
		{
			Amount = amount;
		}

		protected override void ApplySingle (CardInstance _source, object _target)
		{
			CardInstance cardTarget = _target as CardInstance;
			if (cardTarget == null)
				cardTarget = _source;
			
			if (Type == AddOrRemoveCounter.CounterType.P1P1) {
				cardTarget.ChangeCounter ("Power", (this as NumericEffect).Amount);
				cardTarget.ChangeCounter ("Toughness", (this as NumericEffect).Amount);
			}
		}
	}
}

