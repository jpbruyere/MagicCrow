//
//  IntegerValue.cs
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
	public class SimpleIntegerValue : IntegerValue
	{
		int _value;

		public SimpleIntegerValue(int v)
		{
			_value = v;
		}


		#region implemented abstract members of IntegerValue
		public override int GetValue (CardInstance _source, object _target = null)
		{
			return _value;
		}
		#endregion

		public static implicit operator SimpleIntegerValue(int v)
		{
			return new SimpleIntegerValue (v);
		}
	}
	[Serializable]
	public abstract class IntegerValue{		
		public abstract int GetValue (CardInstance _source, object _target = null);

		public static implicit operator int(IntegerValue iv)
		{
			return iv.GetValue (null, null);
		}
		public static implicit operator IntegerValue(int v)
		{
			return new SimpleIntegerValue (v) as IntegerValue;
		}
	}
}

