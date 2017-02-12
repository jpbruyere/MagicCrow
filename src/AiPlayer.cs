using System;
using System.Diagnostics;
using System.Linq;
using Crow;
using System.Threading;

namespace MagicCrow
{
	public class AiPlayer : Player
	{
		#region CTOR
		public AiPlayer () : base(){}
		public AiPlayer (string _name) : base(_name){}
		#endregion

		public override void InitInterface ()
		{
			base.InitInterface ();
			(playerPanel.FindByName ("pic") as Image).Path = "image2/HAL9000.svg";
		}

		public override void Process ()
		{
			MagicEngine e = MagicEngine.CurrentEngine;

			switch (CurrentState) {
			case PlayerStates.Init:
				return;
			case PlayerStates.PlayDrawChoice:
				//chose to play first
				e.currentPlayerIndex = e.getPlayerIndex (this);
				CurrentState = PlayerStates.InitialDraw;
				return;
			case PlayerStates.InitialDraw:				
				initialDraw ();
				CurrentState = PlayerStates.KeepMuliganChoice;
				return;
			case PlayerStates.KeepMuliganChoice:
				//choose to keep
				CurrentState = PlayerStates.Ready;
				e.RaiseMagicEvent(new MagicEventArg(MagicEventType.PlayerIsReady,this));
				return;
			}


			if (e.pp != this || e.State < EngineStates.CurrentPlayer)
				return;

			if (HasActionPending)
			{
				ActivateAvailableMana(e);
				return;
			}

			if (e.cp == this)
			{
				switch (e.CurrentPhase)
				{
				case GamePhases.Untap:
					PhaseDone = true;
					break;
				case GamePhases.Upkeep:
					PhaseDone = true;
					break;
				case GamePhases.Draw:
					PhaseDone = true;
					break;
				case GamePhases.Main1:
				case GamePhases.Main2:
					if (AllowedLandsToBePlayed > 0)
						if (AITryToPlayLand ())
							break;
					if (!CastAvailableAndAllowedCreature())
						PhaseDone = true;
					break;
				case GamePhases.BeforeCombat:
					PhaseDone = true;
					break;
				case GamePhases.DeclareAttacker:
					AITryToAttack ();
					PhaseDone = true;
					break;
				case GamePhases.DeclareBlocker:
					PhaseDone = true;
					break;
				case GamePhases.FirstStrikeDame:
					PhaseDone = true;
					break;
				case GamePhases.CombatDamage:
					PhaseDone = true;
					break;
				case GamePhases.EndOfCombat:
					PhaseDone = true;
					break;
				case GamePhases.EndOfTurn:
					PhaseDone = true;
					break;
				case GamePhases.CleanUp:
					PhaseDone = true;
					break;
				}
			}
			else
			{
				if (e.pp == this) {
					Magic.AddLog ("AI just had priority");
					e.GivePriorityToNextPlayer ();
				}
//				switch (e.CurrentPhase)
//				{
//				case GamePhases.Untap:
//					PhaseDone = true;
//					break;
//				case GamePhases.Upkeep:
//					PhaseDone = true;
//					break;
//				case GamePhases.Draw:
//					PhaseDone = true;
//					break;
//				case GamePhases.Main1:
//					PhaseDone = true;
//					break;
//				case GamePhases.BeforeCombat:
//					PhaseDone = true;
//					break;
//				case GamePhases.DeclareAttacker:
//					PhaseDone = true;
//					break;
//				case GamePhases.DeclareBlocker:
//					PhaseDone = true;
//					break;
//				case GamePhases.FirstStrikeDame:
//					PhaseDone = true;
//					break;
//				case GamePhases.CombatDamage:
//					PhaseDone = true;
//					break;
//				case GamePhases.EndOfCombat:
//					PhaseDone = true;
//					break;
//				case GamePhases.Main2:
//					PhaseDone = true;
//					break;
//				case GamePhases.EndOfTurn:
//					PhaseDone = true;
//					break;
//				case GamePhases.CleanUp:
//					PhaseDone = true;
//					break;
//				}
			}		
		}

//		public void aiPayManaIfNeedeed()
//		{
//			if (CurrentAction != null)
//			{
//				if (CurrentAction.RemainingCost != null)
//				{
//
//				}
//			}
//		}
		public bool CastAvailableAndAllowedCreature()
		{
			Cost availableMana = AvailableManaOnTable;

			foreach (CardInstance c in Hand.Cards.Where(c=>c.HasType(CardTypes.Creature)))
			{
				if (availableMana < c.Model.Cost)
					continue;
				
				MagicEngine.CurrentEngine.MagicStack.PushOnStack(new Spell(c));
				return true;
			}
			return false;
		}

		public bool AITryToPlayLand()
		{            
			//TODO: take mana as ordered in pay cost
			CardInstance[] lands = Hand.Cards.Where(c => c.Model.Types == CardTypes.Land).ToArray();

			if (lands.Length > 0) {
				lands [0].ChangeZone (CardGroupEnum.InPlay);
				AllowedLandsToBePlayed--;
				MagicEngine.CurrentEngine.RaiseMagicEvent (new MagicEventArg (MagicEventType.PlayLand, lands [0]));
				return true;
			}
			return false;
		}
		public void AITryToAttack()
		{
			foreach (CardInstance c in InPlay.Cards.Where(c => c.Model.Types == CardTypes.Creature))
			{
				if (c.CanAttack)
					c.Combating = true;
			}
			InPlay.UpdateLayout();
		}
	}
}

