
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Crow;
using System.Diagnostics;
using System.Threading;
using OpenTK.Input;

namespace MagicCrow
{


	public class Player : IDamagable, IValueChange
    {
		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public void NotifyValueChange(string propName, object newValue)
		{
			try {
				ValueChanged.Raise(this, new ValueChangeEventArgs (propName, newValue));
			} catch (Exception ex) {
				Debug.WriteLine (propName + ";" + newValue  + ";" + ex.ToString ());
			}
		}
		#endregion

		/// <summary>
		/// Overall game status
		/// </summary>
		public enum PlayerStates
		{
			Init,
			DeckLoading,
			PlayDrawChoice,		
			InitialDraw,
			WaitHandReady,
			KeepMuliganChoice,
			Ready,
			SelectTarget,
		}

        public static int InitialLifePoints = 20;

        int _lifePoints;
        string _name;
        Deck _deck;
		//MagicAction _currentAction;

        CardInstance _currentBlockingCreature;
        List<Damage> _damages = new List<Damage>();

		public volatile PlayerStates CurrentState;
		public volatile bool DeckLoaded = false;
		public string deckPath = "Lightforce.dck";
		public Cost ManaPool;
		public bool Keep = false;
		public int CardToDraw = 7;
		public bool PhaseDone = false;
		public int LifePointsLooseThisTurn = 0;
		public int LifePointsGainedThisTurn = 0;

		volatile int progressValue=0;
		volatile int progressMax=0;
		public int ProgressValue{
			get { 
				return progressValue; 
			}
			set {
				progressValue = value;
				NotifyValueChange ("ProgressValue", progressValue);
			}
		}
		public int ProgressMax{
			get { 
				return progressMax; 
			}
			set {
				progressMax = value;
				NotifyValueChange ("ProgressMax", progressMax);
			}
		}
		public bool ProgressBarVisible {
			get { return MagicEngine.CurrentEngine.pp == this; }
		}


		public Library Library;
		public CardGroup Hand;
		public CardGroup Graveyard;
		public InPlayGroup InPlay;
		public CardGroup Exhiled;
		public CardGroup[] allGroups = new CardGroup[5];
		public bool[] PhaseStops = new bool[] {
			false,
			false,
			false,
			true,
			false,
			false,
			false,
			false,
			false,
			false,
			true,
			false,
			false};

		public CardGroup GetGroup(CardGroupEnum zone)
		{
			return allGroups.Where (g => g.GroupName == zone).FirstOrDefault ();
		}

		#region CTOR
		public Player()
		{
			initCardGroups ();
			CurrentState = PlayerStates.Init;
		}
		public Player(string _name) : this()
		{
			Name = _name;
		}

		#endregion
		   
		void initCardGroups()
		{
			Library = new Library();

			Hand = new CardGroup(CardGroupEnum.Hand);
			Hand.y = -8.0f;
			Hand.z = 3.5f;
			Hand.xAngle = Vector3.CalculateAngle (Magic.vLook, Vector3.UnitZ);
			//Hand.xAngle = 0f;//MathHelper.Pi - Vector3.CalculateAngle (Magic.vLook, Vector3.UnitZ);
			Hand.HorizontalSpacing = 0.5f;
			Hand.VerticalSpacing = -0.02f;
			Hand.Scale = 0.7f;
			//Hand.DepthTest = false;

			Graveyard = new CardGroup(CardGroupEnum.Graveyard);
			Graveyard.x = -5.5f;
			Graveyard.y = -2.8f;
			Graveyard.VerticalSpacing = 0.02f;

			InPlay = new InPlayGroup(this);

			Exhiled = new CardGroup(CardGroupEnum.Exhiled);
			Exhiled.IsVisible = false;

			allGroups[0] = Hand;
			allGroups[1] = InPlay;
			allGroups[2] = Graveyard;
			allGroups[3] = Exhiled;
			allGroups[4] = Library;
		}
     
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
				NotifyValueChange ("Name", _name);
            }
        }
		public Deck Deck
		{
			get { return _deck; }
			set
			{
				_deck = value;
				NotifyValueChange ("Deck", _deck);
			}
		}
        public int LifePoints
        {
            get { return _lifePoints; }
            set
            {
				if (_lifePoints == value)
					return;

                _lifePoints = value;

				if (_lifePoints < 1)
					MagicEngine.CurrentEngine.RaiseMagicEvent (new MagicEventArg (MagicEventType.PlayerHasLost, this));				

				NotifyValueChange ("LifePoints", _lifePoints);
            }
        }
		public String[] ManaPoolElements
		{
			get{
				if (ManaPool == null)
					return null;
				string tmp = ManaPool.ToString ();
				return tmp.ToCharArray ().Where(cc => cc != ' ').
					Select(c => new string(c,1)).ToArray ();
			}
		}

		public int AllowedLandsToBePlayed {
			get;
			set;
		}
        
//		MagicAction priviousAction;
//		public MagicAction CurrentAction
//        {
//            get { return _currentAction; }
//            set
//            {
//                if (_currentAction == value)
//                    return;
//				
//				if (value is AbilityActivation) {
//					if ((value as AbilityActivation).Source is ManaAbility)
//						priviousAction = _currentAction;
//				}
//
//				_currentAction = value;
//
//				if (_currentAction == null && priviousAction != null) {
//					_currentAction = priviousAction;
//					priviousAction = null;
//					CurrentAction.PayCost (ref ManaPool);
//				}
//            }
//        }
        public CardInstance CurrentBlockingCreature
        {
            get { return _currentBlockingCreature; }
            set
            {
                _currentBlockingCreature = value;

//                if (_currentBlockingCreature == null)
//                {
//                    MagicCrow.pCurrentSpell.Visible = false;
//                    return;
//                }
//                MagicCrow.pCurrentSpell.Promt.Text = "Choose creature to block...";
//                MagicCrow.pCurrentSpell.Visible = true;
            }
        }
        public CardInstance[] AttackingCreature
        {
            get
            {
                return MagicEngine.CurrentEngine.cp == this ?
                    InPlay.Cards.Where(c => c.Combating == true).ToArray() :
                    new CardInstance[0];
            }

        }
        public CardInstance[] BlockingCreature
        {
            get
            {
                return MagicEngine.CurrentEngine.cp != this ?
                    InPlay.Cards.Where(c => c.Combating == true).ToArray() :
                    new CardInstance[0];
            }

        }

        // TODO: implement multiplayer
        public Player Opponent
        {
            get
            {
                return MagicEngine.CurrentEngine.Players[0] == this ?
                    MagicEngine.CurrentEngine.Players[1] :
                    MagicEngine.CurrentEngine.Players[0];
            }
        }

		#region interface
		//public Panel playerPanel;
		public GraphicObject playerPanel;
		MessageBox msgBox;

		Color ActiveColor = new Color (0.5, 0.5, 0.6, 0.7);
		Color InactiveColor = new Color (0.1, 0.1, 0.1, 0.4);

		public virtual void InitInterface()
        {
//			playerPanel = crowin.Load ("#MagicCrow.ui.player.goml");
//			playerPanel.DataSource = this;
//			//playerPanel.Background = InactiveColor;
//			playerPanel.MouseClick += PlayerPanel_MouseClick;
//
//			Magic.CurrentGameWin.CrowInterface.AddWidget (playerPanel);
        }

		protected void PlayerPanel_MouseClick (object sender, Crow.MouseButtonEventArgs e)
		{
			MagicEngine me = MagicEngine.CurrentEngine;
			if (me.pp != me.ip)				
				return;
			me.MagicStack.TryToHandleClick (this);
		}
		public void UpdateUi()
		{
//			if (MagicEngine.CurrentEngine.pp == this)
//				pgBar.Visible = true;
//			else
//				pgBar.Visible = false;
		}
		void createKeepMulliganChoice()
		{
			msgBox = Magic.CurrentGameWin.Load ("#MagicCrow.ui.keepOrMuligan.iml") as MessageBox;
			msgBox.DataSource = this;
		}
		void OnKeep(Object sender, EventArgs _e)
		{
			Magic.CurrentGameWin.DeleteWidget (msgBox);
			msgBox = null;
			CurrentState = PlayerStates.Ready;
			MagicEngine e = MagicEngine.CurrentEngine;
			e.RaiseMagicEvent(new MagicEventArg(MagicEventType.PlayerIsReady,this));
		}
		void OnTakeMulligan(Object sender, EventArgs e)
		{
			Magic.CurrentGameWin.DeleteWidget (msgBox);
			msgBox = null;

			for (int i = 0; i < CardToDraw; i++)
				Library.AddCard (Hand.TakeTopOfStack);

			CardToDraw--;

			CurrentState = PlayerStates.InitialDraw;
		}
		//
		#endregion
		public volatile int LoadedCardsCount = 0;

		#region deck async loading
		public void LoadDeckCards(){
			if (DeckLoaded) {
				Reset (false);
				return;
			}
			CurrentState = PlayerStates.DeckLoading;
			Thread thread = new Thread(() => loadingThread());
			thread.IsBackground = true;
			thread.Start();
		}
		void loadingThread(){
			Deck.LoadCards ();
				
			Reset (false);

			DeckLoaded = true;
		}
		#endregion

        /// <summary>
        /// init life points, put all cards in library
        /// </summary>
		public void Reset(bool anim = true)
        {
			Keep = false;
			CardToDraw = 7;
            LifePoints = InitialLifePoints;

            foreach (CardGroup cg in allGroups)
                cg.Cards.Clear();

            foreach (CardInstance c in Deck.Cards)
            {
                c.Controler = this;
//                foreach (Ability a in c.Model.Abilities)
//                    a.Source = c;

                c.ResetPositionAndRotation();
				c.yAngle = MathHelper.Pi;
                Library.AddCard(c, anim);
            }
        }
		public void AddCardForeignToHand(MagicCard mc){
			CardInstance ci = Deck.AddCard (mc);
			ci.Controler = this;
			ci.ResetPositionAndRotation();
			ci.yAngle = MathHelper.Pi;
			Hand.AddCard(ci,true);			
		}

		public void initialDraw ()
		{
			GGL.Animation.DelayMs = 300;
			Library.ShuffleAndLayoutZ();
			for (int i = 0; i < CardToDraw; i++) {
				GGL.Animation.DelayMs += i * 50;
				DrawOneCard ();
			}
			GGL.Animation.DelayMs = 0;
		}
 		public void DrawOneCard()
        {
			if (Library.Cards.Count == 0) {
				MagicEngine.CurrentEngine.RaiseMagicEvent (new MagicEventArg (MagicEventType.PlayerHasLost, this));
				return;
			}
            CardInstance c = Library.TakeTopOfStack;
            Hand.AddCard(c);
			MagicEngine.CurrentEngine.RaiseMagicEvent (
				new ChangeZoneEventArg (c,CardGroupEnum.Library,CardGroupEnum.Hand));
        }			
		public void AddDamages(Damage d)
		{
			LifePointsLooseThisTurn	+= d.Amount;
			//test here damage prevention effects
			LifePoints -= d.Amount;

			MagicEngine.CurrentEngine.RaiseMagicEvent(new DamageEventArg(d));

			if (LifePoints < 1)
			{
			}
		}
			
		public virtual void Process()
        {
			MagicEngine e = MagicEngine.CurrentEngine;

			switch (CurrentState) {
			#region StartUp
			case PlayerStates.Init:
				return;
			case PlayerStates.DeckLoading:
				//if (!DeckLoaded)
				//	return;
				
				return;
			case PlayerStates.PlayDrawChoice:
				return;
			case PlayerStates.InitialDraw:
				initialDraw ();
				CurrentState = PlayerStates.WaitHandReady;
				return;
			case PlayerStates.WaitHandReady:
				if (Deck.HasAnimatedCards)
					return;
				createKeepMulliganChoice ();
				CurrentState = PlayerStates.KeepMuliganChoice;
				return;
			case PlayerStates.KeepMuliganChoice:
				return;
				#endregion
			}

			if (e.pp != this || e.State < EngineStates.CurrentPlayer)
				return;

			switch (e.CurrentPhase)
			{
			case GamePhases.Untap:
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				break;
			case GamePhases.Main1:
			case GamePhases.Main2:                        
				//e.Chrono.Reset();
				break;
			case GamePhases.BeforeCombat:
				//e.Chrono.Restart();
				break;
			case GamePhases.DeclareAttacker:
				//if (cp != this)

				//if (HaveUntapedCreatureOnTable)
				//    e.Chrono.Stop();
				//if (pp != cp || !cp.HaveUntapedCreatureOnTable)
				//{
				//    SwitchToNextPhase();
				//    GivePriorityToNextPlayer();
				//}

				break;
			case GamePhases.DeclareBlocker:
				break;
			case GamePhases.FirstStrikeDame:
				break;
			case GamePhases.CombatDamage:
				break;
			case GamePhases.EndOfCombat:
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				break;
			}

			//            if (e.Chrono.ElapsedMilliseconds < MagicEngine.timerLength)
			//                return;
			if (e.MagicStack.Count == 0 && !PhaseStops[(int)e.CurrentPhase])
				PhaseDone = true;
			//            e.Chrono.Stop();
        }
 
        public IEnumerable<CardInstance> CreaturesAbleToAttack
        {
            get
            {
				foreach (CardInstance c in InPlay.Cards) {
					if (c.Model.Types == CardTypes.Creature && 
						!c.IsTapped && !c.HasSummoningSickness) {						
							yield return c;
					}
				}
            }
        }
        public bool HasAttackingCreature
        {
            get { return AttackingCreature.Length > 0 ? true : false; }
        }
		public bool HasActionPending
		{
			get {				
				MagicEngine e = MagicEngine.CurrentEngine;
				if (e.pp != this)
					return false;
				if (e.MagicStack.Count == 0)
					return false;
				MagicAction ma = e.MagicStack.Peek () as MagicAction;

				return ma == null ? false : (ma.IsComplete || ma.Player != this) ? false : true;				
			}
		}
//		public bool PlayableSpell
//		{
//			get{
//				MagicEngine e = MagicEngine.CurrentEngine;
//
//				if (e.cp == this) {
//					if (e.CurrentPhase == GamePhases.Main1 || e.CurrentPhase == GamePhases.Main2) {
//						
//					}
//				}
//
//			}
//		}
		public IEnumerable<CardInstance> PlayableInstants{
			get {
				Cost availableMana = AvailableManaOnTable;

				foreach (CardInstance c in Hand.Cards) {
					if (c.Model.Types == CardTypes.Instant) {
						if (c.Model.Cost < availableMana)
							yield return c;
					}
				}
			}
		}
        public void ActivateAvailableMana(MagicEngine engine)
        {
			MagicAction ma = engine.MagicStack.Peek () as MagicAction;
			if (ma == null) {
				Debug.WriteLine ("no action to activate mana for.");
				return;
			}
			if (ma.remainingCost == null) {
				Debug.WriteLine ("Action has no cost");
				return;
			}

            foreach (CardInstance c in InPlay.Cards.Where(crd => !crd.IsTapped))
            {
				foreach (Ability a in c.Model.Abilities.Where(a => a.ContainsEffect(EffectType.ProduceMana)))
                {
					ManaEffect me = a.Effects.OfType<ManaEffect> ().FirstOrDefault ();
                    if (a.ActivationCost.Contains(CostTypes.Tap))
                    {
                        if (ma.remainingCost.Contains(me.ProducedMana))
                        {
							MagicEngine.CurrentEngine.MagicStack.PushOnStack(new AbilityActivation(c,a));
                            return;
                        }
                    }
                }
            }
        }

        public Cost AvailableManaOnTable
        {
            get
            {
                Cost availableMana = null;
                foreach (CardInstance c in InPlay.Cards.Where(crd => !crd.IsTapped))
                {
                    ManaChoice mc = new ManaChoice();
					foreach (Ability a in c.Model.Abilities.Where(a => a.ContainsEffect(EffectType.ProduceMana)))
                    {
						ManaEffect me = a.Effects.OfType<ManaEffect> ().FirstOrDefault ();
                        if (a.ActivationCost.Contains(CostTypes.Tap))
                            mc += me.ProducedMana.Clone() as Mana;
                    }
                    if (mc.Manas.Count == 0)
                        continue;
                    if (mc.Manas.Count == 1)
                        availableMana += mc.Manas[0];
                    else
                        availableMana += mc;
                }
                return availableMana;
            }
        }
			
		public bool HasThreshold
		{ get { return Graveyard.Cards.Count > 7; } }

		#region Rendering
		public float zAngle = 0.0f;
		public Matrix4 Transformations
		{
			get { return Matrix4.CreateRotationZ(zAngle); }
		}

		public void Render()
		{
//			Matrix4 mSave = Magic.texturedShader.ModelMatrix;            
//			Magic.texturedShader.ModelMatrix *= Transformations;
//			Magic.texturedShader.UpdateUniforms ();
//			foreach (CardGroup cg in allGroups)
//			{
//				if (cg.IsVisible)
//					cg.Render();
//			}
//			Magic.texturedShader.ModelMatrix = mSave;
//			Magic.texturedShader.UpdateUniforms ();
		}
		#endregion

		public override string ToString ()
		{
			return string.Format (Name);
		}

    }
}
