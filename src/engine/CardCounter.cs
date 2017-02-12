//
//  CardCounter.cs
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
using System.Linq;

namespace MagicCrow
{	
	[Serializable]
	public class CardCounter : IntegerValue
	{
		public MultiformAttribut<Target> CardsToCount = new MultiformAttribut<Target> ();
		public int Multiplier = 1;
		#region implemented abstract members of IntegerValue
		public override int GetValue (CardInstance _source, object _target = null)
		{
			int sum = 0;

			foreach (CardTarget ct in CardsToCount.Values.OfType<CardTarget>()) {
				sum += ct.GetValidTargetsInPlay (_source).Count();
			}
			return sum * Multiplier;
		}
		#endregion
	}
	[Serializable]
	public class KickedOrNotValue : IntegerValue
	{
		int kickedValue = 0;
		int unkickedValue = 0;

		public KickedOrNotValue(int _kicked, int _unkicked)
		{
			kickedValue = _kicked;
			unkickedValue = _unkicked;
		}
		#region implemented abstract members of IntegerValue
		public override int GetValue (CardInstance _source, object _target = null)
		{
			return _source.Kicked ? kickedValue : unkickedValue;
		}
		#endregion
	}
}
