using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace MagicCrow
{
	[Serializable]
	public class EffectGroup : List<Effect>
	{
		public Effect.ModeEnum Mode;
		public Cost CounterEffectCost;
		public Trigger TrigStart;
		public Trigger TrigEnd;
		public MultiformAttribut<Target> Affected;

		/// <summary>
		/// Check if target is suitable for those types of effect
		/// for example damage on cards may only be affected to cards in play, but valid groups
		/// are not always set in target structure
		/// </summary>
		public virtual bool AcceptTarget(object _target)
		{
			CardInstance ci = _target as CardInstance;

			foreach (Effect e in this) {
				switch (e.TypeOfEffect) {
				case EffectType.Unset:
					break;
				case EffectType.Discard:
					break;
				case EffectType.Effect:
					break;
				case EffectType.Counter:
					break;
					break;
				case EffectType.TapAll:
					break;
				case EffectType.LoseLife:
					break;
				case EffectType.PreventDamage:
					break;
				case EffectType.Charm:
					break;
				case EffectType.Pump:
				case EffectType.AddPower:
				case EffectType.AddTouchness:
				case EffectType.SetPower:
				case EffectType.SetTouchness:
				case EffectType.Destroy:
				case EffectType.Tap:
				case EffectType.DoesNotUntap:
				case EffectType.CantAttack:
				case EffectType.CantBlock:
				case EffectType.Loose:
				case EffectType.LooseAllAbilities:
				case EffectType.Gain:
				case EffectType.DealDamage:
					if (ci != null)
						if (ci.CurrentGroup.GroupName != CardGroupEnum.InPlay)
							return false;
					break;
				case EffectType.ChangeZone:
					break;
				case EffectType.Draw:
					break;
				case EffectType.DestroyAll:
					break;
				case EffectType.RepeatEach:
					break;
				case EffectType.Token:
					break;
				case EffectType.GainControl:
					break;
				case EffectType.Repeat:
					break;
				case EffectType.Debuff:
					break;
				case EffectType.ChooseColor:
					break;
				case EffectType.Dig:
					break;
				case EffectType.PumpAll:
					break;
				case EffectType.RemoveCounterAll:
					break;
				case EffectType.ChangeZoneAll:
					break;
				case EffectType.DamageAll:
					break;
				case EffectType.UntapAll:
					break;
				case EffectType.PutCounter:
					break;
				case EffectType.GainLife:
					break;
				case EffectType.PutCounterAll:
					break;
				case EffectType.StoreSVar:
					break;
				case EffectType.FlipACoin:
					break;
				case EffectType.SacrificeAll:
					break;
				case EffectType.Untap:
					break;
				case EffectType.Mill:
					break;
				case EffectType.Animate:
					break;
				case EffectType.Fog:
					break;
				case EffectType.RemoveCounter:
					break;
				case EffectType.ExchangeZone:
					break;
				case EffectType.AnimateAll:
					break;
				case EffectType.ChooseCard:
					break;
				case EffectType.Reveal:
					break;
				case EffectType.ChooseSource:
					break;
				case EffectType.MustBlock:
					break;
				case EffectType.ExchangeControl:
					break;
				case EffectType.RearrangeTopOfLibrary:
					break;
				case EffectType.CopyPermanent:
					break;
				case EffectType.SetState:
					break;
				case EffectType.Balance:
					break;
				case EffectType.RevealHand:
					break;
				case EffectType.Sacrifice:
					break;
				case EffectType.AddTurn:
					break;
				case EffectType.TwoPiles:
					break;
				case EffectType.ManaReflected:
					break;
				case EffectType.SetLife:
					break;
				case EffectType.DebuffAll:
					break;
				case EffectType.Fight:
					break;
				case EffectType.ChooseType:
					break;
				case EffectType.Shuffle:
					break;
				case EffectType.NameCard:
					break;
				case EffectType.PermanentNoncreature:
					break;
				case EffectType.PermanentCreature:
					break;
				case EffectType.TapOrUntap:
					break;
				case EffectType.GenericChoice:
					break;
				case EffectType.Play:
					break;
				case EffectType.BecomesBlocked:
					break;
				case EffectType.AddOrRemoveCounter:
					break;
				case EffectType.WinsGame:
					break;
				case EffectType.Proliferate:
					break;
				case EffectType.Scry:
					break;
				case EffectType.MoveCounter:
					break;
				case EffectType.GainOwnership:
					break;
				case EffectType.ChangeTargets:
					break;
				case EffectType.UnattachAll:
					break;
				case EffectType.PeekAndReveal:
					break;
				case EffectType.LosesGame:
					break;
				case EffectType.DigUntil:
					break;
				case EffectType.CopySpellAbility:
					break;
				case EffectType.RollPlanarDice:
					break;
				case EffectType.RegenerateAll:
					break;
				case EffectType.DelayedTrigger:
					break;
				case EffectType.MustAttack:
					break;
				case EffectType.ProtectionAll:
					break;
				case EffectType.RemoveFromCombat:
					break;
				case EffectType.RestartGame:
					break;
				case EffectType.PreventDamageAll:
					break;
				case EffectType.ExchangeLife:
					break;
				case EffectType.DeclareCombatants:
					break;
				case EffectType.ControlPlayer:
					break;
				case EffectType.Phases:
					break;
				case EffectType.Clone:
					break;
				case EffectType.Clash:
					break;
				case EffectType.ChooseNumber:
					break;
				case EffectType.EachDamage:
					break;
				case EffectType.ReorderZone:
					break;
				case EffectType.ChoosePlayer:
					break;
				case EffectType.EndTurn:
					break;
				case EffectType.MultiplePiles:
					break;
				case EffectType.ProduceMana:
					break;
				} 
			}
			return true;
		}

		public virtual void Apply(CardInstance _source, Ability _ability = null, object _target = null)
		{
			IList<CardInstance> targets = GetAffectedCardInstances (_source, _ability);							
				
			foreach (Effect e in this) {
				if (_target == null)
					e.Apply (_source, _ability, targets);
				else
					e.Apply (_source, _ability, _target);
			}
		}

		public IList<CardInstance> GetAffectedCardInstances (CardInstance _source, Ability _ability){
			List<CardInstance> temp = new List<CardInstance> ();

			if (Affected == null)
				return null;
			
			MagicEngine engine = MagicEngine.CurrentEngine;
			foreach (CardTarget ct in Affected.Values.OfType<CardTarget>()) {
				foreach (CardInstance ci in ct.GetValidTargetsInPlay (_source))
					temp.Add(ci);
			}	
			return temp.Count > 0 ? temp : null;
		}

		public static EffectGroup Parse(string s)
		{
			string[] tmp = s.Split(new char[] { '|' });

			EffectGroup effects= new EffectGroup ();


			foreach (string t in tmp)
			{
				string[] tmp2 = t.Split(new char[] { '$' });
				string value = tmp2[1].Trim();
				int v;
				NumericEffect numEff = null;

				switch (tmp2[0].Trim())
				{
				case "Mode":
					effects.Mode = (Effect.ModeEnum)Enum.Parse (typeof(Effect.ModeEnum), tmp2 [1]);
					break;
				case "Affected":
					effects.Affected = Target.ParseTargets (value);
					break;
				case "GainControl":
					effects.Add (new Effect(EffectType.GainControl));
					break;
				case "Description":
					break;
				case "AddKeyword":
					AbilityEnum ae = AbilityEnum.Unset;
					if (Enum.TryParse (value, true, out ae)) {
						effects.Add (new AbilityEffect (new Ability (ae)));
						break;
					}
					switch (value) {
					case "Double Strike":
						effects.Add (new AbilityEffect (new Ability(AbilityEnum.DoubleStrike)));
						break;
					default:
						Debug.WriteLine ("unknown AddKeyword in effect: " + value);
						break;
					}
					break;
				case "Condition":
					break;
				case "AddAbility":
					break;
				case "AddPower":
					numEff = new NumericEffect (EffectType.AddPower);
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));
						
					effects.Add (numEff);
					numEff = null;
					break;
				case "AddToughness":
					numEff = new NumericEffect (EffectType.AddTouchness);
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar (value, numEff, numEff.GetType ().GetField ("Amount"));

					effects.Add (numEff);
					numEff = null;
					break;
				case "SetPower":
					if (!int.TryParse(value, out v))
						break;
					effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.SetPower,
							Amount = v,
						});
					break;
				case "SetToughness":
					if (!int.TryParse(value, out v))
						break;
					effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.SetTouchness,
							Amount = v,
						});
					break;
				case "EffectZone":
					break;
				case "CharacteristicDefining":
					break;
				case "AddType":
					break;
				case "References":
					break;
				case "ValidCard":
					break;
				case "AddHiddenKeyword":
					switch (value) {
					case "CARDNAME can't attack or block.":
						effects.Add (EffectType.CantAttack);
						effects.Add (EffectType.CantBlock);
						break;
					default:
						Debug.WriteLine ("Unkwnown HiddenKeyword: " + value);
						break;
					}
					break;
				case "CheckSVar":
					break;
				case "SVarCompare":
					break;
				case "AffectedZone":
					break;
				case "Activator":
					break;
				case "Type":
					break;
				case "Color":
					break;
				case "Amount":
					break;
				case "SetColor":
					break;
				case "Caster":
					break;
				case "OpponentAttackedWithCreatureThisTurn":
					break;
				case "AddColor":
					break;
				case "AddSVar":
					break;
				case "Spell":
					break;
				case "SetMaxHandSize":
					break;
				case "AddTrigger":
					break;
				case "RemoveKeyword":
					break;
				case "GlobalRule":
					break;
				case "Attacker":
					break;
				case "Cost":
					break;
				case "Player":
					break;
				case "Phases":
					break;
				case "Target":
					break;
				case "Optional":
					break;
				case "AILogic":
					break;
				case "CheckSecondSVar":
					break;
				case "SecondSVarCompare":
					break;
				case "RemoveSubTypes":
					break;
				case "RemoveAllAbilities":
					break;
				case "AddStaticAbility":
					break;
				case "SharedKeywordsZone":
					break;
				case "SharedRestrictions":
					break;
				case "MaxDamage":
					break;
				case "Source":
					break;
				case "RemoveCreatureTypes":
					break;
				case "TopCardOfLibraryIs":
					break;
				case "NonMana":
					break;
				case "GainsAbilitiesOf":
					break;
				case "GainsAbilitiesOfZones":
					break;
				case "RemoveCardTypes":
					break;
				case "CombatDamage":
					break;
				case "ValidTarget":
					break;
				case "RemoveType":
					break;
				case "ValidSource":
					break;
				case "RaiseMaxHandSize":
					break;
				case "Origin":
					break;
				case "MinMana":
					break;
				case "ValidSpellTarget":
					break;
				case "TapAbility":
					break;
				case "KeywordMultiplier":
					break;
				case "CheckThirdSVar":
					break;
				case "CheckFourthSVar":
					break;
				case "AddReplacementEffects":
					break;
				case "OnlySorcerySpeed":
					break;
				default:
					break;
				}
			}


			return effects;
		}

	}
}

