using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MagicCrow
{   	
	public class Spell : MagicAction
    {
		#region CTOR
		public Spell(CardInstance _cardInstance) : base(_cardInstance)
		{            
			_cardInstance.BindedAction = this;

			List<AbilityActivation> tmpAA = new List<AbilityActivation> ();
			foreach (Abilities.Ability a in CardSource.Model.Abilities.Where (sma => sma.Category == AbilityCategory.Spell)) {
				tmpAA.Add (new AbilityActivation (CardSource, a));
			} 
			spellAbilities = tmpAA.ToArray ();

			if (_cardInstance.Model.Cost != null) {
				remainingCost = _cardInstance.Model.Cost.Clone ();
				remainingCost.OrderFirst(_cardInstance.Model.Cost.GetDominantMana());
			}

			Magic.AddLog ("Trying to cast: " + CardSource.Model.Name);

			if (CardSource.Controler.ManaPool != null) {
				PayCost (ref CardSource.Controler.ManaPool);
				CardSource.Controler.NotifyValueChange ("ManaPoolElements", CardSource.Controler.ManaPoolElements);
			}

			if (IsComplete && GoesOnStack)
				MagicEngine.CurrentEngine.Validate ();
		}
		#endregion
        
		#region implemented abstract members of MagicStackElement
		public override string Title {
			get { return "Trying to cast " + CardSource.Model.Name; }
		}
		public override string Message {
			get {
				return CurrentAbility == null ? "Cast " + CardSource.Model.Name :
					CurrentAbility.NextMessage ();
			}
		}
		public override bool IsMandatory {
			get { return false;	}
		}

		#endregion

		int ptrSpellAbilities;
		AbilityActivation[] spellAbilities;

		public AbilityActivation CurrentAbility {
			get { return ptrSpellAbilities < spellAbilities.Length ?
					spellAbilities [ptrSpellAbilities] : null;
			}
		}			


		#region MagicAction implementation
		public override Cost RemainingCost {
			get { return CurrentAbility == null ? remainingCost : CurrentAbility.RemainingCost; }
			set {
				if (CurrentAbility == null)
					remainingCost = value;
				else
					CurrentAbility.RemainingCost = value;
			}
		}
		public override bool IsComplete
		{			
			get {
				return base.IsComplete & CurrentAbility == null;
			}
		}
		public override int RequiredTargetCount
		{
			get
			{
				return (CurrentAbility == null) ?
					RequiredTargetCount : CurrentAbility.RequiredTargetCount;
			}
		}
		public override AttributGroup<Target> ValidTargets {
			get
			{
				return (CurrentAbility == null) ?
					ValidTargets : CurrentAbility.ValidTargets;
			}	
		}
		public override List<Object> SelectedTargets
		{
			get
			{
				return (CurrentAbility == null) ?
					SelectedTargets : CurrentAbility.SelectedTargets;
			}
		}
        
		public override void Resolve ()
		{
			Magic.AddLog ("Resolve => " + this.ToString());

			CardSource.BindedAction = null;

			if (IsCountered) {
				CardSource.PutIntoGraveyard ();
				return;
			}

			foreach (AbilityActivation aa in spellAbilities)
				aa.Resolve ();			

			//sumoning sickness
			if (CardSource.HasType (CardTypes.Creature) && !CardSource.HasAbility (EvasionKeyword.Haste))
				CardSource.HasSummoningSickness = true;

			CardGroupEnum dest = CardGroupEnum.InPlay;

			if (CardSource.HasType (CardTypes.Instant) || CardSource.HasType (CardTypes.Sorcery))
				dest = CardGroupEnum.Graveyard;

			CardSource.ChangeZone (dest);

			MagicEngine.CurrentEngine.RaiseMagicEvent (new SpellEventArg (this));
		}			
		public override void Validate ()
		{
			Magic.AddLog ("Validate => " + this.ToString());

			if (IsComplete)
				return;

			CurrentAbility.Validate ();

			if (CurrentAbility.IsComplete)
				ptrSpellAbilities++;			
		}
		public override bool TryToAddTarget (object c)
		{
			if (CurrentAbility == null)
				return false;					
			return CurrentAbility.TryToAddTarget (c);
		}

		public override string NextMessage ()
		{
			return CurrentAbility == null ? "" :
				CurrentAbility.NextMessage ();
		}
		#endregion

		public override string ToString ()
		{
			return CardSource == null ? "spell with no card source" : CardSource.Model.Name;
		}	
    }		
}
