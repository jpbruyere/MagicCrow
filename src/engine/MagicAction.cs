//
//  MagicAction.cs
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
using System.Linq;

namespace MagicCrow
{
	public abstract class MagicAction : MagicStackElement
	{
		#region CTOR
		public MagicAction(CardInstance _source)
		{
			if (_source == null)
				return;
			CardSource = _source;
			if (CardSource.HasType (CardTypes.Land))
				GoesOnStack = false;
		}
		#endregion

		#region implemented abstract members of MagicStackElement
		public override string[] MSECostElements {
			get{
				if (RemainingCost == null)
					return null;
				string tmp = RemainingCost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length < 3).ToArray();
			}
		}
		public override string[] MSEOtherCostElements {
			get {
				if (RemainingCost == null)
					return null;
				string tmp = RemainingCost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length > 3).ToArray();			}
		}
		Player _sourcePlayer = null;
		public override Player Player{
			get { return _sourcePlayer == null ? 
				CardSource == null ? null : CardSource.Controler
					: _sourcePlayer; 
			}
			set { _sourcePlayer = value; }
		}
		#endregion

		public bool GoesOnStack = true;
		public bool IsCountered = false;
		public CardInstance CardSource;
		public Cost remainingCost;

		public virtual Cost RemainingCost {
			get {
				return remainingCost;
			}
			set {
				remainingCost = value;
			}
		}
		public virtual bool IsComplete {
			get { return Cost.IsNullOrCountIsZero(remainingCost); }
		}

		public virtual void PayCost(ref Cost _amount)
		{
			RemainingCost = RemainingCost.Pay (ref _amount);
		}



		public abstract string NextMessage();

		/// <summary>
		/// Processes the activation of a spell or ability with target selection or cost payment
		/// </summary>
		/// <returns>true if spellActivation has all possible targets and the cost(s) is paid</returns>
		public abstract void Resolve ();
		public abstract bool TryToAddTarget (object c);
		public abstract void Validate ();
		public abstract int RequiredTargetCount { get; }
		public abstract AttributGroup<Target> ValidTargets { get; }
		public abstract List<Object> SelectedTargets { get; }
		public abstract bool IsMandatory { get; }
	}
}

