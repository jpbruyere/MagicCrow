using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Crow;
using OpenTK;
using OpenTK.Input;
using GGL;

namespace MagicCrow
{
	public enum EngineStates
	{
		Stopped,
		WaitForPlayersToBeReady,
		CurrentPlayer,
		Opponents,
		Resolve,
	}

	public class MagicEngine 
	{
		public delegate void MagicEventHandler (MagicEventArg arg);

		public static event MagicEventHandler MagicEvent;
		public static MagicEngine CurrentEngine;

		public void RaiseMagicEvent (MagicEventArg arg)
		{
			MagicEvent (arg);
		}

		public volatile EngineStates State = EngineStates.Stopped;
		public Player[] Players;
		public MagicStack MagicStack;

		public bool DecksLoaded = false;

		int _currentPlayer;
		int _priorityPlayer;
		int _interfacePlayer = 0;//index of player using this interface
		GamePhases _currentPhase;

		//public int currentAttackingCreature = 0;    //combat damage resolution
		//public Damage currentDamage;

		/// <summary>
		/// player having his turn running
		/// </summary>
		public Player cp {
			get { return Players [currentPlayerIndex]; }
		}
		/// <summary>
		/// player controling the interface, redirection card click
		/// </summary>
		public Player ip {
			get { return Players [_interfacePlayer]; }
		}
		/// <summary>
		/// player having priority
		/// </summary>
		public Player pp {
			get { return Players [_priorityPlayer]; }
		}

		public IList<CardInstance> CardsInPlayHavingSpellEffects
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.Model.SpellEffects.Count() > 0)).ToList();
			}
		}
		public IList<CardInstance> CardsInPlayHavingEffects
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.Effects.Count() > 0)).ToList();
			}
		}
		public IList<CardInstance> CardsInPlayHavingEffect(EffectType et)
		{
			return Players.SelectMany (p => p.InPlay.Cards.Where 
				(c => c.Effects.SelectMany (e => e.Where (ee => ee.TypeOfEffect == et)).Count () > 0)).ToList ();
		}
		public IList<CardInstance> CardsInPlayHavingTriggers
		{
			get {
				return Players.SelectMany (p => p.Deck.Cards.Where (c => c.Model.Triggers.Count()>0)).ToList();
			}
		}
		public IList<CardInstance> CardsInPlayHavingPumpEffects
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.PumpEffect.Count() > 0)).ToList();
			}
		}
		//public List<ActiveEffect> SpellEffectsInPlay = new List<ActiveEffect> ();

		public int currentPlayerIndex {
			get { return _currentPlayer; }
			set
			{ 
				if (value == _currentPlayer)
					return;

				if (value >= Players.Length)
					_currentPlayer = 0;
				else
					_currentPlayer = value;

			}
		}
		public int getPlayerIndex(Player _player)
		{
			for (int i = 0; i < Players.Count(); i++) {
				if (Players [i] == _player)
					return i;
			}
			return -1;
		}
		public int priorityPlayer {
			get { return _priorityPlayer; }
			set {
				if (value == _priorityPlayer)
					return;
				int oldPp = _priorityPlayer;
				int newPp = value;
				if (newPp >= Players.Length)
					newPp = 0;

				_priorityPlayer = newPp;
				Players [oldPp].UpdateUi ();
				Players [_priorityPlayer].UpdateUi ();
			}
		}
		/// <summary>
		/// Player controling the graphic interface
		/// </summary>
		public int interfacePlayer {
			get { return _interfacePlayer; }
			set { _interfacePlayer = value; }
		}

		public GamePhases CurrentPhase {
			get { return _currentPhase; }
			set { _currentPhase = value; }
		}

		public void SwitchToNextPhase ()
		{
			MagicEvent (new PhaseEventArg {
				Type = MagicEventType.EndPhase,
				Phase = _currentPhase,
				Player = Players [currentPlayerIndex]
			});
		}
		public void SwitchToNextPlayer ()
		{
			MagicEvent (new MagicEventArg {
				Type = MagicEventType.EndTurn
				//Player = Players [currentPlayer]
			});
		}
		public void GivePriorityToNextPlayer ()
		{
			//first cancel incomplete action of priority player
			if (MagicStack.NextActionOnStack != null) {
				//cardsource could be null for action request by engine (ex: discard at cleanup)
				if (MagicStack.NextActionOnStack.CardSource == null){
					pp.PhaseDone = false;
					return;
				}
				if (MagicStack.NextActionOnStack.CardSource.Controler == pp){
					if (!MagicStack.NextActionOnStack.IsComplete) {
						if (!MagicStack.CancelLastActionOnStack ()) {
							pp.PhaseDone = false;
							return;
						}
					}
				}
			}

			priorityPlayer++;

			if (!(pp is AiPlayer) && CurrentPhase != GamePhases.DeclareBlocker)
				startChrono ();
			else
				stopChrono ();

			if (MagicStack.NextActionOnStack == null) {
				if (priorityPlayer == _currentPlayer && cp.PhaseDone)
					SwitchToNextPhase ();
			} else if (MagicStack.NextActionOnStack.CardSource.Controler == pp)
				MagicStack.ResolveStack ();
		}

		public void startChrono ()
		{
			//if (pp.Type == Player.PlayerType.ai)
			//    Debugger.Break();

			//Chrono.Restart();
			//pp.pbTimer.Visible = true;

		}
		public void stopChrono ()
		{
			//    //if (pp.Type == Player.PlayerType.ai)
			//    //    Debugger.Break();
			//    Chrono.Reset();
			//    pp.pbTimer.Visible = false;
		}

		public Stopwatch Chrono = new Stopwatch ();
		public static int timerLength = 1500;



		#region CTOR
		public MagicEngine (Player[] _players)
		{
			CurrentEngine = this;

			MagicStack = new MagicStack (this);

			Players = _players;
			MagicEvent += new MagicEventHandler (MagicEngine_MagicEvent);
		}
		#endregion

		void startGame()
		{
			_currentPhase = GamePhases.Main1;
			cp.AllowedLandsToBePlayed = 1;//it's normaly set in the untap phase...
			cp.UpdateUi ();
			State = EngineStates.CurrentPlayer;
			MagicEvent (new PhaseEventArg {
				Type = MagicEventType.BeginPhase,
				Phase = _currentPhase,
				Player = cp
			});
		}

		public void Process ()
		{
			//temp fix to have begin not handle before end event in Magic
			//but those kind of sync problem will surely rise 
			if (raiseBeginPhase) {
				raiseBeginPhase = false;
				MagicEvent (new PhaseEventArg {
					Type = MagicEventType.BeginPhase,
					Phase = _currentPhase,
					Player = Players [currentPlayerIndex]
				});
			}
				
			//animate only if cards are loaded
			if (!DecksLoaded) {
				DecksLoaded = Players.Where (p => !p.DeckLoaded).Count () == 0;
				if (DecksLoaded) {
					CardInstance.Create3DCardsTextureAndVBO();
					Players[0].CurrentState = Player.PlayerStates.InitialDraw;
					Players[1].CurrentState = Player.PlayerStates.InitialDraw;
				}
			}

			MagicStack.CheckLastActionOnStack();

			foreach (Player p in Players)
				p.Process ();

			if (pp.PhaseDone)
				GivePriorityToNextPlayer();
		}
			
		void MagicEngine_MagicEvent (MagicEventArg arg)
		{
			Magic.AddLog("MAGIC EVENT: " + arg.ToString());

			#region check triggers

			//check cards in play having trigger effects
			foreach (CardInstance ci in CardsInPlayHavingTriggers) {
				foreach (Trigger t in ci.Model.Triggers){					
					if (t.ExecuteIfMatch(arg, ci)){
						Magic.AddLog("=> " + t.ToString());
					}
				}
			}
			//check pump effect
			//todo should simplify trigger checking with a single function
			foreach (CardInstance ci in CardsInPlayHavingPumpEffects) {
				List<EffectGroup> egToRemove = new List<EffectGroup>();
				foreach (EffectGroup eg in ci.PumpEffect) {
					if (eg.TrigEnd != null){
						if (eg.TrigEnd.Type != arg.Type)
							continue;
						switch (eg.TrigEnd.Type) {
						case MagicEventType.EndTurn:
							egToRemove.Add(eg);
							break;
						}
					}
				}
				foreach (EffectGroup egtr in egToRemove) {
					ci.PumpEffect.Remove(egtr);
				}
			}
			#endregion

			switch (arg.Type) {
			case MagicEventType.PlayerIsReady:
				//check if all players are ready
				foreach (Player p in Players)
					if (p.CurrentState != Player.PlayerStates.Ready)
						return;
				startGame ();
				break;
			case MagicEventType.BeginPhase:
				processPhaseBegin (arg as PhaseEventArg);
				break;
			case MagicEventType.EndPhase:
				processPhaseEnd (arg as PhaseEventArg);
				break;
			case MagicEventType.PlayLand:
				break;
			case MagicEventType.ActivateAbility:
				break;
			case MagicEventType.CastSpell:
				break;
			case MagicEventType.TapCard:
				break;
			case MagicEventType.ChangeZone:
				if (arg.Source.IsToken)
					arg.Source.CurrentGroup.Cards.Remove (arg.Source);
				break;
			case MagicEventType.Unset:
				break;
			default:
				break;
			}

			CheckCardInstanceUpdates ();

			foreach (Player p in Players)//TODO:too wide update
				p.InPlay.UpdateLayout ();			
		}

		void processPhaseBegin (PhaseEventArg pea)
		{
			foreach (Player p in Players)
				p.PhaseDone = false;
			
			priorityPlayer = _currentPlayer;

			switch (pea.Phase) {
			case GamePhases.Untap:
				cp.AllowedLandsToBePlayed = 1;
				cp.LifePointsGainedThisTurn = cp.LifePointsLooseThisTurn = 0;
				cp.CardToDraw = 1;
				foreach (CardInstance c in cp.InPlay.Cards) {
					c.HasSummoningSickness = false;
					c.TryToUntap ();
				}
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				//if (pea.Player == cp) {
					while(cp.CardToDraw > 0){
						cp.DrawOneCard ();
						cp.CardToDraw--;
					}
				//}
				break;
			case GamePhases.Main1:
			case GamePhases.Main2:
				break;
			case GamePhases.BeforeCombat:				
				break;
			case GamePhases.DeclareAttacker:
//				if (cp.Type == Player.PlayerType.human)
//					stopChrono ();
				break;
			case GamePhases.DeclareBlocker:
				break;
			case GamePhases.FirstStrikeDame:
				Chrono.Reset ();
				foreach (CardInstance ac in cp.AttackingCreature.Where
					(cpac => cpac.HasAbility(AbilityEnum.FirstStrike) || 
						cpac.HasAbility(AbilityEnum.DoubleStrike))) {
					Damage d = new Damage (null, ac, ac.Power);

					foreach (CardInstance def in ac.BlockingCreatures.Where
						(cpac => cpac.HasAbility(AbilityEnum.FirstStrike) || 
							cpac.HasAbility(AbilityEnum.DoubleStrike)))
						MagicStack.PushOnStack (new Damage (ac, def, def.Power));

					if (ac.BlockingCreatures.Count == 0) {
						d.Target = cp.Opponent;
						MagicStack.PushOnStack (d);
					} else if (ac.BlockingCreatures.Count == 1) {
						d.Target = ac.BlockingCreatures [0];
						MagicStack.PushOnStack (d);
					} else {
						//push damages one by one for further resolution
						for (int i = 0; i < ac.Power; i++)
							MagicStack.PushOnStack (new Damage (null, d.Source, 1));
					}
				}

				MagicStack.CheckStackForUnasignedDamage ();
				break;
			case GamePhases.CombatDamage:
				Chrono.Reset ();
				foreach (CardInstance ac in cp.AttackingCreature) {
					Damage d = new Damage (null, ac, ac.Power);

					foreach (CardInstance def in ac.BlockingCreatures.Where
						(cpac => !cpac.HasAbility(AbilityEnum.FirstStrike)))
						MagicStack.PushOnStack (new Damage (ac, def, def.Power));

					if (ac.HasAbility (AbilityEnum.FirstStrike)&&!ac.HasAbility (AbilityEnum.DoubleStrike))
						return;
					
					if (ac.BlockingCreatures.Count == 0) {
						d.Target = cp.Opponent;
						MagicStack.PushOnStack (d);
					} else if (ac.BlockingCreatures.Count == 1) {
						d.Target = ac.BlockingCreatures [0];
						MagicStack.PushOnStack (d);
					} else {
						for (int i = 0; i < ac.Power; i++)
							MagicStack.PushOnStack (new Damage (null, d.Source, 1));
					}
				}

				MagicStack.CheckStackForUnasignedDamage ();
				break;
			case GamePhases.EndOfCombat:
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				foreach (Player p in Players) {
					foreach (CardInstance ac in p.InPlay.Cards) {
						if (ac.Damages.Count > 0) {
							ac.Damages.Clear ();
						}
					}
				}
				int cardDiff = cp.Hand.Cards.Count - 7;
				Ability discard = new Ability (EffectType.Discard);
				discard.ValidTargets += new CardTarget () { ValidGroup = CardGroupEnum.Hand, Controler = ControlerType.You };
				discard.Mandatory = true;
				discard.RequiredTargetCount = 1;
				discard.TargetPrompt = "Select card to discard";

				for (int i = 0; i < cardDiff; i++) {
					MagicStack.PushOnStack (new AbilityActivation (null, discard) { 
						GoesOnStack = false,
						Player = cp
					});
				}
				break;
			}
		}
		void processPhaseEnd (PhaseEventArg pea)
		{
			MagicStack.ClearIncompleteActions ();

			MagicStack.ResolveStack ();

			switch (pea.Phase) {
			case GamePhases.Untap:
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				break;
			case GamePhases.Main2:
			case GamePhases.Main1:
				if (CurrentPhase == GamePhases.Main1 && cp.CreaturesAbleToAttack.Count() == 0)
					CurrentPhase = GamePhases.EndOfCombat;
				break;
			case GamePhases.BeforeCombat:
				break;
			case GamePhases.DeclareAttacker:
				if (!pea.Player.HasAttackingCreature) {
					CurrentPhase = GamePhases.EndOfCombat;
					break;
				}
				foreach (CardInstance c in pea.Player.AttackingCreature) {					
					if (!c.IsTapped && !c.HasAbility (AbilityEnum.Vigilance)) {
						c.Tap ();
						RaiseMagicEvent(new MagicEventArg(MagicEventType.Attack,c));
					}
				}
				break;
			case GamePhases.DeclareBlocker:				
				break;
			case GamePhases.FirstStrikeDame:
				break;
			case GamePhases.CombatDamage:
				break;
			case GamePhases.EndOfCombat:
				foreach (Player p in Players) {
					bool updateLayout = false;
					foreach (CardInstance c in p.InPlay.Cards) {
						if (c.Combating) {
							updateLayout = true;
							c.Combating = false;
						}
					}
					if (updateLayout)
						p.InPlay.UpdateLayout ();
				}
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				MagicEvent (new MagicEventArg {
					Type = MagicEventType.EndTurn
						//Player = Players [currentPlayer]
				});
				currentPlayerIndex++;

				priorityPlayer = _currentPlayer;
				CurrentPhase = GamePhases.Untap;
				//Players[_priorityPlayer].pbTimer.Visible = true;					
				break;
			}

			foreach (Player p in Players) {
				//p.CurrentAction = null;
				p.ManaPool = null;
				p.NotifyValueChange ("ManaPoolElements", null);
			}

			if (pea.Phase != GamePhases.CleanUp)
				CurrentPhase++;

			raiseBeginPhase = true;
		}

		bool raiseBeginPhase = false;

		public void ClickOnCard (CardInstance c)
		{
			//Magic.CurrentGameWin.CursorVisible = true;

			if (pp != ip)				
				return;
			
			if (MagicStack.TryToHandleClick (c))
				return;

			switch (c.CurrentGroup.GroupName) {
			case CardGroupEnum.Library:
				break;
			case CardGroupEnum.Hand:
				#region hand
				//player controling interface may only click in his own hand
				if (c.Controler != ip)
					return;
				if (!MagicStack.CancelLastActionOnStack())
					return;
				if (CurrentPhase == GamePhases.Main1 || CurrentPhase == GamePhases.Main2){
					if (c.HasType(CardTypes.Land)) {
						if (cp.AllowedLandsToBePlayed>0){
							c.ChangeZone (CardGroupEnum.InPlay);
							cp.AllowedLandsToBePlayed--;
							MagicEvent (new MagicEventArg (MagicEventType.PlayLand, c));
						}
					} else {
						//Magic.CurrentGameWin.CursorVisible = true;
						MagicStack.PushOnStack(new Spell (c));
					}
				}else if (CurrentPhase != GamePhases.CleanUp && CurrentPhase != GamePhases.Untap){
					//play instant and abilities
					if (c.HasType(CardTypes.Instant))
						MagicStack.PushOnStack(new Spell (c));
				}
				break;
				#endregion
			case CardGroupEnum.InPlay:
				#region inPlay
				if (CurrentPhase == GamePhases.DeclareAttacker) {
					if (c.CanAttack && ip == cp && c.Controler == ip){
						c.Combating = !c.Combating;
						c.CurrentGroup.UpdateLayout ();
						return;
					}
				} else if (CurrentPhase == GamePhases.DeclareBlocker) {
					if (ip != cp){//ip may declare blockers if it's not the current player
						if (c.Controler == ip) {
							if (c.CanBlock()){
								if (c.Combating) {
									c.Combating = false;
									c.BlockedCreature.BlockingCreatures.Remove (c);
									c.BlockedCreature = null;
									c.Controler.InPlay.UpdateLayout ();
								}
								c.Controler.CurrentBlockingCreature = c;
								return;
							}
						} else if (ip.CurrentBlockingCreature != null && c.Combating) {
							//TODO:there's a redundant test here
							if (ip.CurrentBlockingCreature.Combating) {
								//remove blocker
								ip.CurrentBlockingCreature.Combating = false;
								ip.CurrentBlockingCreature.BlockedCreature.BlockingCreatures.Remove (ip.CurrentBlockingCreature);
								ip.CurrentBlockingCreature.BlockedCreature = null;
							} else if (ip.CurrentBlockingCreature.CanBlock (c)) {
								//try to add blocker
								c.BlockingCreatures.Add (ip.CurrentBlockingCreature);
								ip.CurrentBlockingCreature.BlockedCreature = c;
								ip.CurrentBlockingCreature.Combating = true;
								ip.CurrentBlockingCreature = null;
							}
							ip.InPlay.UpdateLayout ();
							return;
						}
					}
				} else if (CurrentPhase == GamePhases.CombatDamage) {
					
				} 
				if (c.Controler == ip) {					
					#region activable abilities
					if (!(c.IsTapped || c.HasSummoningSickness)) {
						Ability[] activableAbs = c.Model.Abilities.Where (
							                         sma => sma.IsActivatedAbility).ToArray ();

						if (activableAbs.Count() == 1)
							MagicStack.PushOnStack (new AbilityActivation (c, activableAbs[0]));
						else if (activableAbs.Count() > 1){
							MagicChoice aachoice = new MagicChoice() { Player = ip };
							foreach (Ability aa in activableAbs)
								aachoice.Choices.Add(new AbilityActivation (c, aa, true));
						
							MagicStack.PushOnStack (aachoice);
						}
					}
					#endregion					
				}
				#endregion
				break;
			case CardGroupEnum.Graveyard:
				c.CurrentGroup.toogleShowAll ();
				break;
			case CardGroupEnum.Exhiled:
				c.CurrentGroup.toogleShowAll ();
				break;
			default:
				break;
			}
		}

		#region Mouse handling
		public void processMouseDown (OpenTK.Input.MouseButtonEventArgs e)
		{
			if (CardInstance.selectedCard == null)
				return;

			switch (e.Button) {
			case OpenTK.Input.MouseButton.Left:
				ClickOnCard (CardInstance.selectedCard);
				break;
			case OpenTK.Input.MouseButton.Right:
				CardInstance.selectedCard.SwitchFocus ();
				break;
			}
		}
		#endregion

//		public void UpdateOverlays()
//		{
//			foreach (CardInstance ci in CardsInPlayHavingEffects) {
//				foreach (EffectGroup eg in ci.Effects) {
//					foreach (CardTarget ct in eg.Affected.Values.OfType<CardTarget>()) {
//						foreach (CardInstance c in ct.GetValidTargetsInPlay (ci))
//							c.UpdatePointsOverlaySurface ();
//					}	
//				}
//			}			
//		}
		/// <summary>
		/// Checks controler, power, toughness and ability changes with effects, damages and so on
		/// </summary>
		public void CheckCardInstanceUpdates()
		{			
			foreach (CardInstance ci in Players.SelectMany(p => p.InPlay.Cards)) {
				ci.UpdateControler ();
				if (!ci.HasType (CardTypes.Creature))
					continue;			
 				ci.UpdatePowerAndToughness ();
				ci.UpdatePointsOverlaySurface ();
				ci.CheckAbilityChanges ();
				if (ci.AbilityChangesDetected)					
					ci.UpdateInfoOverlaySurface ();				
			}			
		}
	}
}
