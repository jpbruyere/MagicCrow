using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using GGL;

namespace MagicCrow
{
    [Serializable]
    public class Effect
    {
		static bool ListIsNullOrEmpty(IList list)
		{
			if (list == null)
				return true;
			return (list.Count == 0);
		}

		public enum Types
        {
            OneShot,
            Continuous,
            Replacement,
            Prevention,
        }
		public enum ModeEnum
		{
			NotSet,
			Continuous,
			RaiseCost,
			ReduceCost,
			SetCost,
			CantBeCast,
			CantBeActivated,
			CantPlayLand,
			CantAttack,
			CantBlockUnless,
			CantAttackUnless,
			CantTarget,
			PreventDamage,
			ETBTapped
		}
			
        public EffectType TypeOfEffect;
        
		#region CTOR
        public Effect() { }
        public Effect(EffectType _type)
        {
            TypeOfEffect = _type;
        }
		#endregion

		protected virtual void ApplySingle(CardInstance _source, object _target = null)
		{			
			MagicEngine engine = MagicEngine.CurrentEngine;

			Player player = _target as Player;
			CardInstance ci = _target as CardInstance;
			if (ci == null)
				ci = _source;
			if (player == null && _source != null)
				player = _source.Controler;

			switch (TypeOfEffect) {
			case EffectType.GainLife:				
				player.LifePoints += (this as NumericEffect).Amount.GetValue(_source);
				break;
			case EffectType.LoseLife:
				player.LifePoints -= (this as NumericEffect).Amount.GetValue(_source);
				break;
			case EffectType.Unset:
				break;
			case EffectType.ProduceMana:				
				player.ManaPool += (this as ManaEffect).ProducedMana.Clone();
				player.NotifyValueChange ("ManaPoolElements", player.ManaPoolElements);
				break;
			case EffectType.Loose:
				break;
			case EffectType.LooseAllAbilities:
				break;
			case EffectType.Gain:
				
				break;
			case EffectType.Discard:
				ci.ChangeZone (CardGroupEnum.Graveyard);
				break;
			case EffectType.Pump:
				break;
			case EffectType.Effect:
				break;
			case EffectType.Counter:
				(_target as MagicAction).IsCountered = true;
				break;
			case EffectType.Destroy:
				ci.PutIntoGraveyard ();
				break;
			case EffectType.Tap:
				ci.tappedWithoutEvent = true;
				break;
			case EffectType.DoesNotUntap:
				break;			
			case EffectType.TapAll:
				break;
			case EffectType.PreventDamage:
				break;
			case EffectType.Charm:
				break;
			case EffectType.DealDamage:
				engine.MagicStack.PushOnStack (new Damage (_target as IDamagable, _source, (this as NumericEffect).Amount.GetValue(_source))); 
				break;
			case EffectType.ChangeZone:
				if ((this as ChangeZoneEffect).Destination == CardGroupEnum.Reveal) {
					CardGroup cg = player.allGroups.Where (ag => ag.GroupName == (this as ChangeZoneEffect).Origin).FirstOrDefault();
					for (int i = 0; i < (this as ChangeZoneEffect).NumCards; i++) {
						cg.Cards [cg.Cards.Count - 1].SwitchFocus ();
					}
					break;
				}
				ci.Reset ();
				ci.ChangeZone ((this as ChangeZoneEffect).Destination);
				if ((this as ChangeZoneEffect).Tapped)
					ci.tappedWithoutEvent = true;
				else
					ci.tappedWithoutEvent = false;
				break;
			case EffectType.Draw:
				Animation.DelayMs = 300;
				for (int i = 0; i < (this as NumericEffect).Amount.GetValue (_source); i++) {
					player.DrawOneCard ();
					Animation.DelayMs += i * 100;
				}
				Animation.DelayMs = 0;
				break;
			case EffectType.DestroyAll:
				break;
			case EffectType.RepeatEach:
				break;
			case EffectType.Token:
				TokenEffect tkEff = this as TokenEffect;
				MagicCard tk = new MagicCard () {
					Name = tkEff.Name,
					Power = tkEff.Power.GetValue (_source, _target),
					Toughness = tkEff.Toughness.GetValue (_source, _target),
					Colors = tkEff.Colors,
					Types = tkEff.Types,
				};
				string picPath = System.IO.Path.Combine (MagicData.cardsArtPath, "tokens");
				if (string.IsNullOrEmpty (tkEff.Image))
					picPath = System.IO.Path.Combine (picPath,
						new Mana (tkEff.Colors.Values.FirstOrDefault ()).ToString ().ToLower () + "_" + tk.Power.ToString () + "_" + tk.Toughness.ToString () +
						tk.Types.Values.Where (tkt => tkt != CardTypes.Creature).
						Aggregate<CardTypes,string> (String.Empty, (a, b) => a.ToString ().ToLower () + '_' + b.ToString ().ToLower ()) + ".jpg");
				else 
					picPath = System.IO.Path.Combine (picPath, tkEff.Image + ".jpg").Replace(' ','_').ToLower();				

				tk.SetTexture(picPath);

				Player[] players;

				switch (tkEff.Owner) {
				case ControlerType.All:
					players = engine.Players;
					break;
				case ControlerType.You:
					players = new Player[] { _source.Controler};
					break;
				case ControlerType.Opponent:
					players = new Player[] { _source.Controler.Opponent};
					break;
				case ControlerType.Targeted:
					players = new Player[] { _target as Player };
					break;
				default:
					players = new Player[] { _source.Controler};
					break;
				}

				foreach (Player p in players) {
					for (int i = 0; i < tkEff.Amount.GetValue (_source, _target); i++) {
						p.InPlay.AddCard (new CardInstance (tk) { Controler = p, IsToken = true, HasSummoningSickness = true });
					}
					p.InPlay.UpdateLayout ();
					//engine.UpdateOverlays ();
				}					

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
				CardGroup orig = ci.Controler.allGroups.Where (ag => ag.GroupName == (this as ChangeZoneEffect).Origin).FirstOrDefault ();
				while (orig.Cards.Count > 0) {
					CardInstance cc = orig.Cards.FirstOrDefault ();
					cc.Reset ();
					cc.ChangeZone ((this as ChangeZoneEffect).Destination);
					if ((this as ChangeZoneEffect).Tapped)
						cc.tappedWithoutEvent = true;
					else
						cc.tappedWithoutEvent = false;
				}
				orig.UpdateLayout ();
				ci.Controler.allGroups.Where (ag => ag.GroupName == (this as ChangeZoneEffect).Destination).FirstOrDefault ().UpdateLayout ();
				break;
			case EffectType.DamageAll:
				break;
			case EffectType.UntapAll:
				break;
			case EffectType.PutCounter:
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
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
		public virtual void Apply(CardInstance _source, Ability _ability, object _target)
		{			
			IList targets = _target as IList;
			if (ListIsNullOrEmpty(targets))
				ApplySingle (_source, _target);
			foreach (object o in targets)
				ApplySingle (_source, o);					
		}


		#region operators
		public static implicit operator Effect(EffectType et)
		{
			return new Effect (et);
		}
		#endregion

		public override string ToString ()
		{
			return TypeOfEffect.ToString ();
		}
        
		void Effect_MagicEvent(MagicEventArg arg)
        {
//            if (TrigStart.Type != MagicEventType.Unset)
//            {
//                if (arg.Type == TrigStart.Type)
//                { }
//            }
//            if (TrigEnd.Type != MagicEventType.Unset)
//            {
//                if (arg.Type == TrigEnd.Type)
//                {
////                    if (TrigEnd.Source != null && TrigEnd.Source == arg.Source)
////                    {
////                        MagicEngine.MagicEvent -= Effect_MagicEvent;
////                        ContainingList.RemoveEffect(this);
////                    }
//                }
//            }
        }
    }
	 
    [Serializable]
    public class ControlEffect : Effect
    {
        public Player Controler;
    }

   

//	public class LifeEffect : Effect
//	{		
//		public int Amount;
//		public Cost Cost;
//
//		public override void Apply (CardInstance _source, object _target = null)
//		{
//			Source = _source;
//			Source.Controler.LifePoints += Amount;
//		}
//	}
//    public class EffectList : List<Effect>
//    {
//        public void AddEffect(Effect e)
//        {
//            this.Add(e);
//            e.ContainingList = this;
//        }
//        public void RemoveEffect(Effect e)
//        {
//            this.Remove(e);
//            e.ContainingList = null;
//        }
//    }

}
