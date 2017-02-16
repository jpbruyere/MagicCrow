//
//  AbilityActivation.cs
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
using System.Diagnostics;

namespace MagicCrow
{
	public class AbilityActivation : MagicAction
	{
		#region CTOR
		public AbilityActivation(CardInstance _source, Ability a, bool isInChoice = false) : base(_source)
		{
			Source = a;
			if (!Cost.IsNullOrCountIsZero (Source.ActivationCost)) {
				remainingCost = Source.ActivationCost.Clone ();
				remainingCost.OrderFirst (Source.ActivationCost.GetDominantMana ());
			}

			//			else//if it's a spell ab, no cost and no message
			//				return;

			if (CardSource != null && !isInChoice) {
				if (Source.Mandatory)
					Magic.AddLog (CardSource.Model.Name + " ability activation.");
				else
					Magic.AddLog (Source.Message);

				if (CardSource.Controler.ManaPool != null && remainingCost != null) {
					PayCost (ref CardSource.Controler.ManaPool);
					CardSource.Controler.NotifyValueChange ("ManaPoolElements", CardSource.Controler.ManaPoolElements);
				}
			}

			if (Source.Category == AbilityCategory.Spell)
				return;
			if (IsComplete && GoesOnStack)
				MagicEngine.CurrentEngine.GivePriorityToNextPlayer ();
		}
		#endregion

		#region implemented abstract members of MagicStackElement
		public override string Title {
			get { return CardSource == null ? "Engine Request:" :
				CardSource.Model.Name + " Ability Activation"; }
		}
		public override string Message {
			get { return WaitForTarget ? Source.TargetPrompt : Source.Description; }
		}
		#endregion

		bool validated = false;

		List<object> selectedTargets = new List<object> ();
		public Ability Source;


		/// <summary>
		/// True as long as adding targets is valid
		/// </summary>
		public bool WaitForTarget
		{
			get
			{
				return SelectedTargets.Count < Source.PossibleTargetCount ? true : false;
			}
		}

		#region MagicAction implementation
		public override List<object> SelectedTargets {
			get {
				return selectedTargets;
			}
		}
		public override AttributGroup<Target> ValidTargets {
			get {
				return Source.ValidTargets;
			}
		}
		public override int RequiredTargetCount {
			get {
				return Source.RequiredTargetCount;
			}
		}
		public override bool IsMandatory {
			get {
				return Source.Mandatory;
			}
		}
		public override bool IsComplete
		{
			get {
				return (SelectedTargets.Count < RequiredTargetCount) ||
					!base.IsComplete ? false :					
					WaitForTarget && !validated ? false : true;
			}
		}
		public override string NextMessage ()
		{			
			if (CardSource == null)
				return "";
			
			//show library cards if needeed
			if (WaitForTarget && ValidTargets != null) {
				Library library = CardSource.Controler.Library;
				if (ValidTargets.OfType<CardTarget> ().Where
				(cct => cct.ValidGroup == CardGroupEnum.Library).Count () > 0) {
					if (!library.IsExpanded)
						library.toogleShowAll ();
				} else if (library.IsExpanded)
					library.toogleShowAll ();
	
				return Source.TargetPrompt;			
			}

			return "";
		}
		/// <summary>
		/// action will be complete if MinTarget <= targets <= MaxTarget 
		/// </summary>
		public override void Validate ()
		{			
			validated = true;
			if (!IsComplete && MagicEngine.CurrentEngine.MagicStack.NextActionOnStack == this)
				MagicEngine.CurrentEngine.MagicStack.CancelLastActionOnStack ();
		}
		public override bool TryToAddTarget (object target)
		{
			if (!WaitForTarget) {
				if (RemainingCost != null) {
					if (RemainingCost.RequiredTargetCount > 0 && target is CardInstance) {
						CardInstance cat = target as CardInstance;
						RemainingCost = RemainingCost.Pay (ref cat, CardSource);
						if (cat == null)
							return true;
					}
				}
				return false;
			}

			if (target is CardInstance) {
				CardInstance ci = target as CardInstance;
				if (ci.BindedAction != null)
					target = ci.BindedAction;
			}

			//other target group are possible, should change
			foreach (Target ct in ValidTargets)
			{
				if (ct.Accept(target, CardSource)){
					if (Source.Effects.AcceptTarget (target)) {
						SelectedTargets.Add (target);
						return true;
					}
				}
			}

			Magic.AddLog ("Invalid target: " + target.ToString());
			return false;
		}

		public override void Resolve ()
		{			
			if (IsCountered)
				return;

			switch (Source.AbilityType) {
			case AbilityEnum.Attach:
				if (CardSource.IsAttached)
					CardSource.AttachedTo.DetacheCard (CardSource);
				(selectedTargets.FirstOrDefault() as CardInstance).AttachCard (CardSource);
				break;
			case AbilityEnum.Enchant:
				(selectedTargets.FirstOrDefault() as CardInstance).AttachCard (CardSource);
				break;
			case AbilityEnum.Equip:
				(selectedTargets.FirstOrDefault() as CardInstance).AttachCard (CardSource);
				break;
			case AbilityEnum.Instant:
				break;
			case AbilityEnum.Interrupt:
				break;
			case AbilityEnum.Kicker:
				CardSource.Kicked = true;
				break;
			default:
				//triggered Ability ou activated
				if (!(this.Source.IsActivatedAbility||this.Source.IsTriggeredAbility)) {
					Debug.WriteLine ("unset static ability");
				}
				this.Source.Activate(CardSource, SelectedTargets);
				break;
			}
			MagicEngine.CurrentEngine.RaiseMagicEvent (new ActivatedAbilityEventArg (Source, CardSource));	
			//MagicEngine.CurrentEngine.UpdateOverlays ();
		}
		#endregion

		public override string ToString ()
		{
			return "Ability Activation";
		}
	}

}

