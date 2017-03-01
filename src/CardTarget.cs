using System;
using System.Linq;
using System.Collections.Generic;

namespace MagicCrow
{
	[Serializable]
	public class CardTarget : Target
	{
		public AttributGroup<CardTypes> ValidCardTypes;
		public AttributGroup<CardTypes> HavingAttachedCards;
		public AttributGroup<Abilities.Ability> HavingAbilities;
		public AttributGroup<Abilities.Ability> WithoutAbilities;
		public AttributGroup<ManaTypes> ValidCardColors;
		public ControlerType Controler = ControlerType.All;
		public CombatImplication CombatState = CombatImplication.Unset;
		public NumericConstrain PowerConstrain;
		public NumericConstrain ToughnessConstrain;
		/// <summary>
		/// If false, origin card can't be targeted
		/// </summary>
		public bool CanBeTargetted = true;
		public AttributGroup<CardGroupEnum> ValidGroup;

		public CardTarget(TargetType tt = TargetType.Card)
		{
			TypeOfTarget = tt;
		}

		/// <summary>
		/// True if target is valid
		/// </summary>
		/// <param name="_target">CardTarget to test</param>
		/// <param name="_source">The original card providing this targeting conditions</param>
		public override bool Accept (object _target, CardInstance _source)
		{
			if (TypeOfTarget == TargetType.Player)
			{
				Player p = _target as Player;
				if (p == null)
					return false;
				//TODO: more constrains surely for player
				return true;
			}

			CardInstance c = _target as CardInstance;

			if (c == null) {
				if (_target is MagicAction)
					c = (_target as MagicAction).CardSource;
				else
					return base.Accept (_target, _source);
			}

			if (TypeOfTarget == TargetType.Spell && !(_target is Spell))
				return false;
			if (TypeOfTarget == TargetType.Activated) {
				if (!(_target is AbilityActivation))
					return false;
//				if (!(_target as AbilityActivation).Source.IsActivatedAbility)
//					return false;
			}
			if (TypeOfTarget == TargetType.Triggered) {
				if (!(_target is AbilityActivation))
					return false;
//				if (!(_target as AbilityActivation).Source.IsTriggeredAbility)
//					return false;
			}

			if (TypeOfTarget == TargetType.Self && _target != _source)
				return false;
			if (TypeOfTarget == TargetType.Kicked && !c.Kicked)
				return false;
			if (TypeOfTarget == TargetType.Permanent && c.CurrentGroup.GroupName != CardGroupEnum.InPlay)
				return false;
			
			if (TypeOfTarget == TargetType.EquipedBy || TypeOfTarget == TargetType.EnchantedBy) {
				if (_target is CardInstance)
					return  _source.AttachedTo == _target;
				return false;
			} else if (TypeOfTarget == TargetType.Attached) {
				if (_target is CardInstance)
					return  (_target as CardInstance).AttachedTo == _source;
				return false;
			}
			if (!CanBeTargetted && _target == _source)
				return false;
			if (!(c.Model.Types >= ValidCardTypes))
				return false;

			if (ValidCardColors != null) {
				foreach (ManaTypes mc in ValidCardColors) {
					if (!c.HasColor (mc))
						return false;
				}
			}

			if (CombatState == CombatImplication.Attacking) {
				if (!c.Controler.AttackingCreature.Contains (c))
					return false;
			}else if (CombatState == CombatImplication.Blocking){
				if (!c.Controler.BlockingCreature.Contains (c))
					return false;
			}

			if (ValidGroup != null) {
				if (!ValidGroup.Contains(c.CurrentGroup.GroupName))
					return false;
			}
			//TODO: power constrains
			//TODO: abilities check
			//TODO: having attached cards
			if (HavingAttachedCards != null) {				
				foreach (CardInstance ac in c.AttachedCards) {
					if (ac.Model.Types >= HavingAttachedCards)
						return true;					
				}
				return false;
			}

			//_source could be null when action is triggered by engine, not spell
			if (_source == null) {
				//controler have to be current player
				if (c.Controler != MagicEngine.CurrentEngine.cp)
					return false;
				return true;
			}
			if (Controler == ControlerType.You) {
				if (_source.Controler != c.Controler)
					return false;
			}else if (Controler == ControlerType.Opponent){
				if (_source.Controler == c.Controler)
					return false;
			}


			return true;
		}

		#region Operators
		public static bool operator ==(CardTarget ct, CardTypes t)
		{
			return ct.HavingAbilities.Count > 0 ?
				false :
				ct.ValidCardTypes == t;
		}
		public static bool operator !=(CardTarget ct, CardTypes t)
		{
			return ct.HavingAbilities.Count > 0 ?
				true :
				ct.ValidCardTypes != t;
		}
		#endregion

		public IEnumerable<CardInstance> GetValidTargetsInPlay (CardInstance _source){
			MagicEngine engine = MagicEngine.CurrentEngine;
			IEnumerable<CardInstance> cards;

			switch (TypeOfTarget) {
			case TargetType.Self:
				yield return _source;
				break;
			case TargetType.EnchantedBy:
			case TargetType.EquipedBy:
				if (_source.IsAttached)
					yield return _source.AttachedTo;
				break;
			case TargetType.Card:
			case TargetType.Attached:
			case TargetType.Permanent:
				if (Controler == ControlerType.All)
					cards = engine.Players.SelectMany (p => p.InPlay.Cards);
				else if (Controler == ControlerType.You)
					cards = _source.Controler.InPlay.Cards;
				else
					cards = _source.Controler.Opponent.InPlay.Cards;

				foreach (CardInstance ci in cards) {
					if (Accept (ci, _source))
						yield return ci;	
				}
				break;
			}
		}

		public override string ToString()
		{
			string tmp = "";
			if (ValidCardTypes == null)
				tmp += TypeOfTarget.ToString();
			else
				tmp += ValidCardTypes.ToString();

			if (HavingAbilities != null)
				tmp += " having " + HavingAbilities.ToString();

			if (WithoutAbilities != null)
				tmp += " without " + WithoutAbilities.ToString();

			return tmp;
		}

		//public static bool operator ==(CardTarget ct, EffectType t)
		//{
		//    return ct.ValidCardTypes.Count > 0 ?
		//        false :
		//        ct.HavingAbilities == t;
		//}
		//public static bool operator ==(CardTarget ct, EffectType t)
		//{
		//    return ct.ValidCardTypes.Count > 0 ?
		//        false :
		//        ct.HavingAbilities == t;
		//}
	}

}

