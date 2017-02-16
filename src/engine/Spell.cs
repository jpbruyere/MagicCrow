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
		public Spell() : base(null){}
		public Spell(CardInstance _cardInstance) : base(_cardInstance)
		{            
			_cardInstance.BindedAction = this;

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
				MagicEngine.CurrentEngine.GivePriorityToNextPlayer ();
		}
		#endregion
        
		#region implemented abstract members of MagicStackElement
		public override string Title {
			get { return "Trying to cast " + CardSource.Model.Name; }
		}
		public override string Message {
			get {
				return currentAbilityActivation == null ? "" :
					currentAbilityActivation.NextMessage ();
			}
		}
		#endregion

		List<AbilityActivation> spellAbilities = new List<AbilityActivation>();
		AbilityActivation currentAbilityActivation = null;
		public MagicAction CurrentAbility {
			get {
				if (currentAbilityActivation == null)
					currentAbilityActivation = NextAbilityToProcess;
				while (currentAbilityActivation != null) {
					if (currentAbilityActivation.IsComplete) {
						spellAbilities.Add (currentAbilityActivation);
						currentAbilityActivation = null;
					} else
						return currentAbilityActivation;
					currentAbilityActivation = NextAbilityToProcess;
				}		
				return null;
			}
		}

		public AbilityActivation NextAbilityToProcess
		{
			get {
				//already done activation
				IEnumerable<Ability> abs = spellAbilities.Select (saa => saa.Source);

				//add kicker if base cost is not paid to exclude list
				//it will be processed only when base cost is paid
				if (remainingCost != null)
					abs = abs.Concat (CardSource.Model.Abilities.Where (ema => ema.AbilityType == AbilityEnum.Kicker));				
				
				Ability a =
					CardSource.Model.Abilities.Where (
						sma => sma.Category == AbilityCategory.Spell &&
						!abs.Contains (sma)
					).FirstOrDefault();

				return a == null ? null :
					new AbilityActivation (CardSource, a);
			}
		}


		#region MagicAction implementation
		public override Cost RemainingCost {
			get {
				return currentAbilityActivation == null ? remainingCost : currentAbilityActivation.RemainingCost;
			}
			set {
				if (currentAbilityActivation == null)
					remainingCost = value;
				else
					currentAbilityActivation.RemainingCost = value;
			}
		}
		public override bool IsComplete
		{			
			get {
				return (CurrentAbility == null) ?
					base.IsComplete : CurrentAbility.IsComplete;
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
			CardSource.BindedAction = null;

			if (IsCountered) {
				CardSource.PutIntoGraveyard ();
				return;
			}

			foreach (AbilityActivation aa in spellAbilities.Where(sa => sa.IsComplete)) {
				aa.Resolve ();
			}

			//sumoning sickness
			if (CardSource.HasType (CardTypes.Creature) &&
				!CardSource.HasAbility (AbilityEnum.Haste))
				CardSource.HasSummoningSickness = true;

			CardGroupEnum dest = CardGroupEnum.InPlay;

			if (CardSource.HasType (CardTypes.Instant) ||
				CardSource.HasType (CardTypes.Sorcery))
				dest = CardGroupEnum.Graveyard;

			CardSource.ChangeZone (dest);

			MagicEngine.CurrentEngine.RaiseMagicEvent (new SpellEventArg (this));
			//MagicEngine.CurrentEngine.UpdateOverlays ();
		}			
		public override void Validate ()
		{
			if (currentAbilityActivation == null){
				MagicEngine.CurrentEngine.MagicStack.CancelLastActionOnStack ();
				return;//maybe cancel spell if not completed
			}
			//cancel spell if mandatory ability activation is canceled
			if (currentAbilityActivation.IsMandatory){
				MagicEngine.CurrentEngine.MagicStack.CancelLastActionOnStack ();
				return;//maybe cancel spell if not completed
			}

			currentAbilityActivation.Validate ();

			spellAbilities.Add (currentAbilityActivation);
			currentAbilityActivation = null;
		}
		public override bool IsMandatory {
			get {
				return false;
			}
		}

		public override bool TryToAddTarget (object c)
		{
			if (CurrentAbility == null)
				return false;
					
			if (CurrentAbility.TryToAddTarget(c))
			{				
				//trick to force update of current ability
				//should simplify currentAbilityActivation update
				MagicAction tmp = CurrentAbility;
				return true;
			}
			return false;
		}

		public override string NextMessage ()
		{
			return currentAbilityActivation == null ? "" :
				currentAbilityActivation.NextMessage ();
		}
		#endregion

		public override string ToString ()
		{
			return CardSource == null ? "spell with no card source" : CardSource.Model.Name;
		}	
    }		
}
