//
//  MagicStack.cs
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
using Crow;
using System.Diagnostics;
using OpenTK;
using System.Linq;

namespace MagicCrow
{
	public class MagicStack : Stack<MagicStackElement> , IValueChange
	{
		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged(string MemberName, object _value)
		{
			ValueChanged.Raise(this, new ValueChangeEventArgs(MemberName, _value));			
		}
		#endregion

		MagicEngine engine;
		//public CardLayout SpellStackLayout = new CardLayout ();

		#region CTOR
		public MagicStack (MagicEngine _engine) : base()
		{
			engine = _engine;

//			SpellStackLayout.Position = new Vector3 (0, 0, 2);//Magic.vGroupedFocusedPoint;
//			SpellStackLayout.HorizontalSpacing = 0.1f;
//			SpellStackLayout.VerticalSpacing = 0.3f;
//			SpellStackLayout.MaxHorizontalSpace = 3f;
			//SpellStackLayout.xAngle = Magic.FocusAngle;
		}
		#endregion


		#region Interface
		public bool UIActionIsChoice{
			get { 
				if (this.Count == 0)
					return false;
				MagicStackElement mse = this.Peek ();
				if (mse.Player != engine.ip)
					return false;				
				return mse is MagicChoice;
			}
		}
		public bool UIPlayerActionIsOnStack {
			get { 
				return this.Count == 0 ? false : 
					((this.Peek ()).Player == engine.ip);
			}
		}
		public string UIPlayerTitle {
			get { return UIPlayerActionIsOnStack ? 
				this.Peek().Title	: "";
			}
		}
		public string UIPlayerMessage {
			get { return UIPlayerActionIsOnStack ? 
				this.Peek().Message	: "";
			}
		}
		public String[] CostElements { get{ return UIPlayerActionIsOnStack ? this.Peek ().MSECostElements : null; }}
		public bool CostIsNotNull { get { return CostElements != null; }}
		public String[] OtherCostElements { get{ return UIPlayerActionIsOnStack ? this.Peek ().MSEOtherCostElements : null; }}
		public bool MessageIsNotNull { get { return !string.IsNullOrEmpty(UIPlayerMessage); }}

		void notifyStackElementChange(){
			bool uipaios = UIPlayerActionIsOnStack;
			NotifyValueChanged ("UIPlayerActionIsOnStack", uipaios);
			if (!uipaios)
				return;
			NotifyValueChanged ("UIPlayerTitle", UIPlayerTitle);
			NotifyValueChanged ("UIPlayerMessage", UIPlayerMessage);
			NotifyValueChanged ("CostElements", CostElements);
			NotifyValueChanged ("OtherCostElements", OtherCostElements);
			NotifyValueChanged ("CostIsNotNull", CostIsNotNull);
			NotifyValueChanged ("MessageIsNotNull", MessageIsNotNull);
			NotifyValueChanged ("UIActionIsChoice", UIActionIsChoice);
			if (UIActionIsChoice)
				NotifyValueChanged ("Choices", Choices);
		}
		void Done_MouseClick (object sender, Crow.MouseButtonEventArgs e)
		{			
			if (NextActionOnStack != null) {
				NextActionOnStack.Validate ();
			} else if (UIActionIsChoice) {
				PopMagicStackElement ();
			}

			//MagicEngine.CurrentEngine.CancelLastActionOnStack ();
		}
		void onChoiceMade (object sender, SelectionChangeEventArgs e)
		{
			if (e.NewValue == null)
				return;
			PopMagicStackElement ();
			PushOnStack (e.NewValue as MagicStackElement);
		}
		#endregion

		#region Stack managment
		public MagicAction NextActionOnStack {
			get { return this.Count == 0 ? null : this.Peek () as MagicAction; }
		}
		public List<MagicStackElement> Choices {
			get { return UIActionIsChoice ? 
				(this.Peek() as MagicChoice).Choices:new List<MagicStackElement>(); }
		}			
		public void PushOnStack (MagicStackElement s)
		{			
			this.Push (s);
			notifyStackElementChange ();
			Magic.CurrentGameWin.NotifyValueChanged ("MagicStack", this.ToList());
		}
		public MagicStackElement PopMagicStackElement()
		{
			MagicStackElement tmp = this.Pop ();
			notifyStackElementChange ();
			Magic.CurrentGameWin.NotifyValueChanged ("MagicStack", this.ToList());
			return tmp;
		}
		public void Resolve ()
		{
			while (Count > 0) {
				if (this.Peek () is Damage)
					Debugger.Break ();
				
				if (Peek () is MagicAction) {

					if (!(Peek () as MagicAction).IsComplete)
						Debugger.Break ();

					MagicAction ma = PopMagicStackElement() as MagicAction;
					ma.Resolve ();
				}
			}
		}
		/// <summary>
		/// Check completeness of last action on stack.
		/// </summary>
//		public void Process ()
//		{
//			if (this.Count == 0)
//				return;
//
//			MagicAction ma = this.Peek () as MagicAction;
//
//			if (ma == null)
//				return;
//
//			if (ma.CardSource != null){
//				if (ma.CardSource.Controler != engine.pp)
//					return;
//			}
//			//Magic.CurrentGameWin.CursorVisible = true;
//			if (!ma.IsComplete) {
//				if (ma.remainingCost == CostTypes.Tap) {
//					ma.remainingCost = null;
//					ma.CardSource.IsTapped = true;
//				} else if ((engine.pp.AvailableManaOnTable + engine.pp.ManaPool) < ma.RemainingCost?.ManaCost) {
//					Magic.AddLog ("Not enough mana available");
//					CancelLastActionOnStack ();
//					return;
//				} else if (engine.pp.ManaPool != null && ma.RemainingCost?.ManaCost != null) {
//					string lastRemCost = ma.RemainingCost.ToString ();
//					ma.PayCost (ref engine.pp.ManaPool);
//					bool skipUpdateUI = false;
//					if (ma.RemainingCost != null) {
//						if (string.Equals (ma.RemainingCost.ToString (), lastRemCost, StringComparison.Ordinal))
//							skipUpdateUI = true;
//					}
//					if (!skipUpdateUI){
//						engine.pp.NotifyValueChange ("ManaPoolElements", engine.pp.ManaPoolElements);
//						notifyStackElementChange ();
//					}
//				}
//
//				//				if (ma.IsComplete && ma.GoesOnStack)
//				//					GivePriorityToNextPlayer ();				
//
//			}
//			if (ma.IsComplete){
//				if (ma.GoesOnStack) {
//					//should show spell to player...
//					//UpdateStackLayouting();
//					engine.GivePriorityToNextPlayer ();				
//				} else {
//					PopMagicStackElement ();
//					ma.Resolve ();
//				}
//				return;
//			}
//			//			AbilityActivation aa = ma as AbilityActivation;
//			//			//mana doest go on stack
//			//			if (aa != null){
//			//				if (aa.Source.AbilityType == AbilityEnum.Mana) {
//			//					MagicEvent (new AbilityEventArg (aa.Source, aa.CardSource));				
//			//					MagicStack.Pop;
//			//					return;
//			//				}
//			//			}
//		}

		public bool TryToHandleClick (object target){
			MagicStackElement mse = Peek ();
			if (mse.Player != engine.pp)
				Debugger.Break ();

			if (mse is Damage) {				
				Damage d = mse as Damage;
				//TODO:is target valid?
				d.Target = target as IDamagable;
				PopMagicStackElement ();
				d.Deal ();
				return true;
			}

			if (mse is MagicChoice)				
				return false;

			MagicAction ma = mse as MagicAction;

			if (ma.IsComplete)
				Debugger.Break ();
			
			return ma.TryToAddTarget (target);
		}

//		public void UpdateStackLayouting()
//		{
//			SpellStackLayout.Cards.Clear ();
//			foreach (MagicAction ma in this.OfType<MagicAction>()) {
//				if (ma is Spell)
//					SpellStackLayout.Cards.Add ((ma as Spell).CardSource);
//				else if (ma is AbilityActivation)
//					SpellStackLayout.Cards.Add (new CardInstance(ma));
//			}
//			SpellStackLayout.UpdateLayout ();			
//		}
		#endregion
	}
}

