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
		public static MagicEngine CurrentEngine;

		#region EVENT HANDLERS
		public event EventHandler<MagicEventArg> MagicEvent;
		public event EventHandler<MagicEventArg> Attached;
		public event EventHandler<MagicEventArg> AttackerBlocked;
		public event EventHandler<MagicEventArg> AttackerUnblocked;
		public event EventHandler<MagicEventArg> AttackersDeclared;
		public event EventHandler<MagicEventArg> Attacks;
		public event EventHandler<MagicEventArg> BecomeMonstrous;
		public event EventHandler<MagicEventArg> BecomeTarget;
		public event EventHandler<MagicEventArg> BlockersDeclared;
		public event EventHandler<MagicEventArg> Blocks;
		public event EventHandler<MagicEventArg> Championed;
		public event EventHandler<MagicEventArg> ChangesController;
		public event EventHandler<ChangeZoneEventArg> ChangesZone;
		public event EventHandler<MagicEventArg> Clashed;
		public event EventHandler<MagicEventArg> CombatDamageDoneOnce;
		public event EventHandler<MagicEventArg> CounterAdded;
		public event EventHandler<MagicEventArg> CounterRemoved;
		public event EventHandler<MagicEventArg> Countered;
		public event EventHandler<MagicEventArg> Cycled;
		public event EventHandler<MagicEventArg> DamageDone;
		public event EventHandler<MagicEventArg> DealtCombatDamageOnce;
		public event EventHandler<MagicEventArg> Destroyed;
		public event EventHandler<MagicEventArg> Devoured;
		public event EventHandler<MagicEventArg> Discarded;
		public event EventHandler<MagicEventArg> Drawn;
		public event EventHandler<MagicEventArg> Evolved;
		public event EventHandler<MagicEventArg> FlippedCoin;
		public event EventHandler<MagicEventArg> LandPlayed;
		public event EventHandler<MagicEventArg> LifeLost;
		public event EventHandler<MagicEventArg> LifeGained;
		public event EventHandler<MagicEventArg> LoseGame;
		public event EventHandler<MagicEventArg> NewGame;
		public event EventHandler<MagicEventArg> PayCumulativeUpkeep;
		public event EventHandler<MagicEventArg> PayEcho;
		public event EventHandler<PhaseEventArg> Phase;
		public event EventHandler<MagicEventArg> PhaseIn;
		public event EventHandler<MagicEventArg> PhaseOut;
		public event EventHandler<MagicEventArg> PlanarDice;
		public event EventHandler<MagicEventArg> PlaneWalked;
		public event EventHandler<MagicEventArg> PlanesWalkedFrom;
		public event EventHandler<MagicEventArg> Sacrificed;
		public event EventHandler<MagicEventArg> Scry;
		public event EventHandler<MagicEventArg> SearchedLibrary;
		public event EventHandler<MagicEventArg> SetInMotion;
		public event EventHandler<MagicEventArg> Shuffled;
		public event EventHandler<SpellEventArg> SpellCast;
		public event EventHandler<MagicEventArg> AbilityCast;
		public event EventHandler<MagicEventArg> SpellAbilityCast;
		public event EventHandler<MagicEventArg> Tap;
		public event EventHandler<MagicEventArg> Untap;
		public event EventHandler<MagicEventArg> TapsForMana;
		public event EventHandler<MagicEventArg> Transformed;
		public event EventHandler<MagicEventArg> TurnFaceUp;
		public event EventHandler<MagicEventArg> Unequip;
		public event EventHandler<MagicEventArg> Vote;
		#endregion

		public void RaiseMagicEvent (MagicEventArg arg)
		{
			Magic.AddLog("EVENT => " + arg.ToString());

			switch (arg.Type) {
			case MagicCrow.Triggers.Mode.Always:
				break;
			case MagicCrow.Triggers.Mode.Attached:
				Attached.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.AttackerBlocked:
				AttackerBlocked.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.AttackerUnblocked:
				AttackerUnblocked.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.AttackersDeclared:
				AttackersDeclared.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Attacks:
				Attacks.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.BecomeMonstrous:
				BecomeMonstrous.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.BecomesTarget:
				Attached.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.BlockersDeclared:
				BlockersDeclared.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Blocks:
				Blocks.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Championed:
				Championed.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.ChangesController:
				ChangesController.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.ChangesZone:
				ChangesZone.Raise (this, arg as ChangeZoneEventArg);
				break;
			case MagicCrow.Triggers.Mode.Clashed:
				Clashed.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.CombatDamageDoneOnce:
				CombatDamageDoneOnce.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.CounterAdded:
				CounterAdded.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.CounterRemoved:
				CounterRemoved.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Countered:
				Countered.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Cycled:
				Cycled.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.DamageDone:
				DamageDone.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.DealtCombatDamageOnce:
				DealtCombatDamageOnce.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Destroyed:
				Destroyed.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Devoured:
				Devoured.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Discarded:
				Discarded.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Drawn:
				Drawn.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Evolved:
				Evolved.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.FlippedCoin:
				FlippedCoin.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.LandPlayed:
				LandPlayed.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.LifeGained:
				LifeGained.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.LifeLost:
				LifeLost.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.LosesGame:
				LoseGame.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.NewGame:
				NewGame.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.PayCumulativeUpkeep:
				PayCumulativeUpkeep.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.PayEcho:
				PayEcho.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Phase:
				Phase.Raise (this, arg as PhaseEventArg);
				break;
			case MagicCrow.Triggers.Mode.PhaseIn:
				PhaseIn.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.PhaseOut:
				PhaseOut.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.PlanarDice:
				PlanarDice.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.PlaneswalkedTo:
				Attached.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.PlaneswalkedFrom:
				PlanesWalkedFrom.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Sacrificed:
				Sacrificed.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Scry:
				Scry.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.SearchedLibrary:
				SearchedLibrary.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.SetInMotion:
				SetInMotion.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Shuffled:
				Shuffled.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.SpellCast:
				SpellCast.Raise (this, arg as SpellEventArg);
				break;
			case MagicCrow.Triggers.Mode.AbilityCast:
				AbilityCast.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.SpellAbilityCast:
				SpellAbilityCast.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Taps:
				Attached.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Untaps:
				Attached.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.TapsForMana:
				TapsForMana.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Transformed:
				Transformed.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.TurnFaceUp:
				TurnFaceUp.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Unequip:
				Unequip.Raise (this, arg);
				break;
			case MagicCrow.Triggers.Mode.Vote:
				Vote.Raise (this, arg);
				break;
			default:
				break;
			}
			MagicEvent.Raise (this, arg);
		}

		public volatile EngineStates State = EngineStates.Stopped;
		public Player[] Players;
		public MagicStack MagicStack;

		public bool DecksLoaded = false;

		int _curPlayerIdx;
		int _priorityPlayer;
		int _interfacePlayer = 0;//index of player using this interface

		public GamePhases CurrentPhase;

		//public int currentAttackingCreature = 0;    //combat damage resolution
		//public Damage currentDamage;

		/// <summary>
		/// player having his turn running
		/// </summary>
		public Player cp {
			get { return Players [CurPlayerIdx]; }
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
		/// <summary>
		/// Current Player having his turn
		/// </summary>
		public int CurPlayerIdx {
			get { return _curPlayerIdx; }
			set
			{ 
				if (value == _curPlayerIdx)
					return;

				if (value >= Players.Length)
					_curPlayerIdx = 0;
				else
					_curPlayerIdx = value;

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
			}
		}
		/// <summary>
		/// Player controling the graphic interface
		/// </summary>
		public int interfacePlayer {
			get { return _interfacePlayer; }
			set { _interfacePlayer = value; }
		}			

		public void SwitchToNextPhase ()
		{
			MagicEvent.Raise (this, new PhaseEventArg {
				Type = MagicCrow.Triggers.Mode.EndPhase,
				Phase = CurrentPhase,
				source = Players [CurPlayerIdx]
			});
		}
		public void SwitchToNextPlayer ()
		{
			MagicEvent (this, new MagicEventArg {
				Type = MagicEventType.EndTurn
				//Player = Players [currentPlayer]
			});
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

			MagicEvent += MagicEngine_MagicEvent;
			Phase += MagicEngine_Phase;
		}

		#endregion

		void startGame()
		{
			CurrentPhase = GamePhases.Main1;
			cp.AllowedLandsToBePlayed = 1;//it's normaly set in the untap phase...
			State = EngineStates.CurrentPlayer;
			MagicEvent (this, new PhaseEventArg {
				Type = MagicCrow.Triggers.Mode.Phase,
				Phase = CurrentPhase,
				source = cp
			});
		}
		void initGame(){
			CardInstance.Create3DCardsTextureAndVBO();
			Players[0].CurrentState = Player.PlayerStates.InitialDraw;
			Players[1].CurrentState = Player.PlayerStates.InitialDraw;
		}
		public void Process ()
		{
			//animate only if cards are loaded
			if (!DecksLoaded) {
				DecksLoaded = Players.Where (p => !p.DeckLoaded).Count () == 0;
				if (DecksLoaded)
					initGame ();
			}

			foreach (Player p in Players)
				p.Process ();
		}

		/// <summary>
		/// Validate current action on stack or pass, cancel action if not complete
		/// </summary>
		public void Validate(){
			
			if (MagicStack.Count == 0) {
				priorityPlayer++;
				if (pp == cp)
					SwitchToNextPhase ();
				return;
			}

			MagicStackElement mse = MagicStack.Peek ();

			if (mse is Damage)
				return;

			if (mse.Player != pp) {
				priorityPlayer++;
				if (mse.Player == pp)
					MagicStack.Resolve ();
				return;
			}

			if (mse is MagicChoice) {
				if (!mse.IsMandatory)
					MagicStack.PopMagicStackElement ();
				else
					Magic.AddLog("Cannot cancel mandatory choice.");
				return;
			}
			MagicAction ma = mse as MagicAction;
			if (ma.IsComplete) {
				MagicStack.Resolve ();
				return;
			}

			ma.Validate ();

			if (ma.IsComplete) {
				priorityPlayer++;
				return;
			}

			//try to cancel ma
			if (ma.IsMandatory)
				Magic.AddLog ("Unable to cancel mandatory action");
			else
				MagicStack.PopMagicStackElement ();
		}
			
		void MagicEngine_MagicEvent (object sender, MagicEventArg arg)
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
						if (eg.TrigEnd.Mode != arg.Type)
							continue;
						switch (eg.TrigEnd.Mode) {
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
			default:
				break;
			}

			CheckCardInstanceUpdates ();

			foreach (Player p in Players)//TODO:too wide update
				p.InPlay.UpdateLayout ();			
		}

		void MagicEngine_Phase (object sender, PhaseEventArg e)
		{
			if (e.Type == MagicCrow.Triggers.Mode.Phase)
				processPhaseBegin (e);
			else
				processPhaseEnd (e);
		}

		void processPhaseBegin (PhaseEventArg pea)
		{
			priorityPlayer = _curPlayerIdx;

			switch (pea.Phase) {
			case GamePhases.Untap:
				cp.AllowedLandsToBePlayed = 1;
				cp.LifePointsGainedThisTurn = cp.LifePointsLooseThisTurn = 0;
				cp.CardToDraw = 1;
				foreach (CardInstance c in cp.InPlay.Cards) {
					c.HasSummoningSickness = false;
					c.IsTapped = false;
				}
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:				
				while(cp.CardToDraw > 0){
					cp.DrawOneCard ();
					cp.CardToDraw--;
				}
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
					
					Damage d = new Damage (null, ac, ac.Power, true);

					foreach (CardInstance def in ac.BlockingCreatures.Where
						(cpac => cpac.HasAbility(AbilityEnum.FirstStrike) || 
							cpac.HasAbility(AbilityEnum.DoubleStrike)))
						new Damage (ac, def, def.Power, true).Deal();

					if (ac.BlockingCreatures.Count == 0) {
						d.Target = cp.Opponent;
						d.Deal ();
					} else if (ac.BlockingCreatures.Count == 1) {
						d.Target = ac.BlockingCreatures [0];
						d.Deal ();
					} else {
						//push damages one by one for further resolution
						for (int i = 0; i < ac.Power; i++)
							MagicStack.PushOnStack (new Damage (null, d.Source, 1, true));
					}
				}
				break;
			case GamePhases.CombatDamage:
				Chrono.Reset ();
				foreach (CardInstance ac in cp.AttackingCreature) {
					if (ac.Power <= 0)
						continue;
					
					Damage d = new Damage (null, ac, ac.Power, true);

					foreach (CardInstance def in ac.BlockingCreatures.Where
						(cpac => !cpac.HasAbility(AbilityEnum.FirstStrike)))
						new Damage (ac, def, def.Power, true).Deal();

					if (ac.HasAbility (AbilityEnum.FirstStrike)&&!ac.HasAbility (AbilityEnum.DoubleStrike))
						return;
					
					if (ac.BlockingCreatures.Count == 0) {
						d.Target = cp.Opponent;
						d.Deal();
					} else if (ac.BlockingCreatures.Count == 1) {
						d.Target = ac.BlockingCreatures [0];
						d.Deal();
					} else {
						for (int i = 0; i < ac.Power; i++)
							MagicStack.PushOnStack (new Damage (null, d.Source, 1, true));
					}
				}					
				break;
			case GamePhases.EndOfCombat:
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				foreach (CardInstance ac in Players.SelectMany(p => p.InPlay.Cards)) {
					ac.Damages.Clear ();
					ac.HasCombatDamage = false;
					ac.HasDealtCombatDamages = false;
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
				if (!pea.SourcePlayer.HasAttackingCreature) {
					CurrentPhase = GamePhases.EndOfCombat;
					break;
				}
				foreach (CardInstance c in pea.SourcePlayer.AttackingCreature) {					
					if (!c.IsTapped && !c.HasAbility (AbilityEnum.Vigilance)) {
						c.IsTapped = true;
						RaiseMagicEvent (new MagicEventArg (Triggers.Mode.Attacks, c));
					}
				}
				RaiseMagicEvent (new MagicEventArg (Triggers.Mode.AttackersDeclared, pea.SourcePlayer));
				break;
			case GamePhases.DeclareBlocker:
				foreach (CardInstance c in pea.SourcePlayer.AttackingCreature) {
					if (c.BlockingCreatures?.Count > 0) {
						RaiseMagicEvent (new MagicEventArg (Triggers.Mode.AttackerBlocked, c, c.BlockingCreatures));
						foreach (CardInstance bc in c.BlockingCreatures)
							RaiseMagicEvent (new MagicEventArg (Triggers.Mode.Blocks, bc, c));						
					}else
						RaiseMagicEvent (new MagicEventArg (Triggers.Mode.AttackerUnblocked, c));
				}
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
				CurPlayerIdx++;
				priorityPlayer = _curPlayerIdx;
				break;
			}

			foreach (Player p in Players) {
				//p.CurrentAction = null;
				p.ManaPool = null;
				p.NotifyValueChange ("ManaPoolElements", null);
			}

			if (pea.Phase != GamePhases.CleanUp)
				CurrentPhase++;
			else
				CurrentPhase = GamePhases.Untap;

			MagicEvent.Raise (this, new PhaseEventArg {
				Type = MagicCrow.Triggers.Mode.Phase,
				Phase = CurrentPhase,
				source = Players [CurPlayerIdx]
			});
		}			

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
				//TODO: put here fast cancel and switch between spell cast
				if (CurrentPhase == GamePhases.Main1 || CurrentPhase == GamePhases.Main2){
					if (c.HasType(CardTypes.Land)) {
						if (cp.AllowedLandsToBePlayed>0){
							c.ChangeZone (CardGroupEnum.InPlay);
							cp.AllowedLandsToBePlayed--;
							RaiseMagicEvent (new MagicEventArg(Triggers.Mode.LandPlayed, c));
						}
					} else {
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
