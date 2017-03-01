using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using MagicCrow.Effects;

namespace MagicCrow
{
	[Serializable]
	public class Ability
	{
		#region CTOR
		public Ability (){}
		public Ability (EvasionKeyword a)
		{
			AbilityType = a;
			Category = AbilityCategory.Spell;
		}
		public Ability (EffectType et)
		{
			AbilityType = EvasionKeyword.Unset;
			Effects.Add (new Effect (et));
		}
		public Ability (Trigger trig)
		{
			AbilityType = EvasionKeyword.Unset;
			Trigger = trig;
		}
		public Ability (Effect effect)
		{
			AbilityType = EvasionKeyword.Unset;
			this.Effects.Add (effect);
		}
		#endregion

		int _requiredTargetCount = -1;
		string targetPrompt = "";
		List<Object> _selectedTargets = new List<object> ();
		public AttributGroup<Target> ValidTargets;
		public Trigger Trigger;
		public EvasionKeyword AbilityType = EvasionKeyword.Unset;
		public EffectGroup Effects = new EffectGroup ();
		public Ability SubAbility;
		public Cost ActivationCost = null;
		public string Description = "";
		public string Message = "";
		public int MinimumTargetCount = -1;
		public int MaximumTargetCount = -1;
		/// <summary>
		/// if true, ability activation can't be cancelled and have to be completed
		/// </summary>
		public bool Mandatory = false;

		public bool IsTriggeredAbility {
			get { return Trigger != null; }
		}
		public bool IsStaticAbility {
			get { return ActivationCost == null && Trigger == null; }
		}
		public bool IsActivatedAbility {
			get { return Trigger == null && Category != AbilityCategory.Spell; }
		}

		public bool HasContinuousMode {
			get { return Effects == null ? false : Effects.Mode == Effect.ModeEnum.Continuous; }
		}
		public bool ContainsEffect(EffectType et)
		{
			return Effects.Where (e => e.TypeOfEffect == et).Count () > 0;
		}
		/// <summary>
		/// return MinimumTargetCount if set or _requiredTargetCount
		/// </summary>
		public int RequiredTargetCount {
			get {
				return MinimumTargetCount > 0 ? MinimumTargetCount : _requiredTargetCount;
			}
			set {
				_requiredTargetCount = value;
			}
		}
		/// <summary>
		/// Used to know if ability could accept target 
		/// for msg prompt.
		/// </summary>
		/// <value>The possible target count.</value>
		public int PossibleTargetCount {
			get { 
				return  
					MaximumTargetCount > 0 ? MaximumTargetCount : _requiredTargetCount; 
			}
		}
		/// <summary>
		/// if minTarget or maxTarget are set return true 
		/// else return true if required target > 0
		/// </summary>
		///
		public bool AcceptTargets {
			get { 
				return MinimumTargetCount > 0 || MaximumTargetCount > 0 ? true : 
					_requiredTargetCount > 0; 
			} 
		}			
		public string TargetPrompt {
			get {
				return string.IsNullOrWhiteSpace (targetPrompt) ? "\tSelect " + ValidTargets?.ToString () : targetPrompt;
			}
			set {
				targetPrompt = value;
			}
		}

		public virtual void Activate (CardInstance _source, List<object> _targets)
		{
			if (Effects == null)
				return;
			if (Effects.Count == 0)
				return;
			if (this.AbilityType == EvasionKeyword.Pump) {
				if (this.AcceptTargets) {
					foreach (CardInstance ci in _targets.OfType<CardInstance>()) {
						//TODO:make single function for pumping effect clonage
						EffectGroup eg = Effects.Clone ();
						eg.AddRange (Effects);
						eg.Affected = new CardTarget (TargetType.Self);
						ci.PumpEffect.Add (eg);
					}
				} else {
					if (Effects.Affected == null) {
						EffectGroup eg = Effects.Clone ();
						eg.AddRange (Effects);
						eg.Affected = new CardTarget (TargetType.Self);
						_source.PumpEffect.Add (eg);
					} else {
						foreach (CardInstance ci in Effects.GetAffectedCardInstances(_source,this)) {
							EffectGroup eg = Effects.Clone ();
							eg.AddRange (Effects);
							eg.Affected = new CardTarget (TargetType.Self);
							ci.PumpEffect.Add (eg);
						}
					}
				}
			}else
				Effects.Apply (_source, this, _targets);

			if (SubAbility == null)
				return;

			MagicEngine.CurrentEngine.MagicStack.PushOnStack (new AbilityActivation (_source, SubAbility));
		}

		public static Ability Parse (string strAbility)
		{
			Ability a = new Ability();
			Parse (strAbility, ref a);
			return a;
		}
		public static Ability Parse (string strAbility, Trigger _trig)
		{
			Ability ta = new Ability();
			Parse (strAbility, ref ta);
			ta.Trigger = _trig;
			return ta;
		}

//		static List<string> stringList = new List<string> ();
//		if (stringList.Contains (value))
//			break;
//		stringList.Add (value);
//		using(Stream s = new FileStream(@"/mnt/data2/tokenOwner.txt",FileMode.Append)){
//			using (StreamWriter sw = new StreamWriter(s)){
//				sw.WriteLine ("case \"" + value + "\":");
//				sw.WriteLine ("\tbreak;");
//			}
//		}
//
		public static void Parse (string strAbility, ref Ability a)
		{			
			string[] tmp = strAbility.Split (new char[] { '|' });

			AbilityCategory Category = AbilityCategory.Acivated;
			bool mandatory = false;
			TargetType stackTargets = TargetType.Card;
			NumericEffect numEff = null;
			TokenEffect tokEff = null;

			//example: for destroy, only inplay cards can be targeted
			CardGroupEnum validCardTargetZoneFix = CardGroupEnum.Any;

			foreach (string ab in tmp) {
				int v;

				if (string.IsNullOrWhiteSpace (ab))
					continue;

				int dollarPos = ab.IndexOf ('$');

				if (dollarPos < 0)
					continue;

				AbilityFieldsEnum varName;
				Enum.TryParse (ab.Substring (0, dollarPos), true, out varName);
				string value = ab.Substring (dollarPos + 1).Trim ();


				Effect e = null;


				switch (varName) {
				case AbilityFieldsEnum.SP://Spell ability
				case AbilityFieldsEnum.AB://Triggered or activated					
					if (varName == AbilityFieldsEnum.AB)
						Category = AbilityCategory.Acivated;
					else if (varName == AbilityFieldsEnum.SP)
						Category = AbilityCategory.Spell;					
					#region ability type
					switch (value) {
					case "Discard":
						a.Effects.Add(new NumericEffect(EffectType.Discard));
						break;
					case "Mana":
						a.Effects.Add(new ManaEffect());
						break;
					case "Pump":						
						a.AbilityType = EvasionKeyword.Pump;
						a.Effects.TrigEnd = new Trigger(MagicEventType.EndTurn);
						break;
					case "Attach":
						a.AbilityType = EvasionKeyword.Attach;
						a.RequiredTargetCount = 1;
						break;
					case "Animate":
						a.Effects.Add(new TokenEffect());
						break;
					case "Effect":
						break;
					case "Counter":
						a.Effects.Add(new CounterEffect());
						break;
					case "Destroy":
						a.Effects.Add(new Effect(EffectType.Destroy));
						validCardTargetZoneFix = CardGroupEnum.InPlay;
						break;
					case "Tap":
						a.Effects.Add(new Effect(EffectType.Tap));
						break;
					case "TapAll":
						
						break;
					case "LoseLife":
						a.Effects.Add (new NumericEffect(EffectType.LoseLife));
						break;
					case "GainLife":						
					case "Gainlife":
						a.Effects.Add (new NumericEffect(EffectType.GainLife));
						break;
					case "PreventDamage":
						break;
					case "Charm":
						break;
					case "DealDamage":
						a.Effects.Add(new NumericEffect(EffectType.DealDamage));
						break;
					case "ChangeZone":
						a.Effects.Add (new ChangeZoneEffect());
						break;
					case "ChangeZoneAll":
						a.Effects.Add (new ChangeZoneEffect() { TypeOfEffect = EffectType.ChangeZoneAll });
						break;
					case "Draw":
						a.Effects.Add(new NumericEffect(EffectType.Draw));
						break;
					case "DestroyAll":
						break;
					case "RepeatEach":
						break;
					case "Token":
						a.Effects.Add(new TokenEffect());
						break;
					case "GainControl":
						break;
					case "Repeat":
						break;
					case "Debuff":
						break;
					case "ChooseColor":
						break;
					case "Dig":
						a.Effects.Add (new ChangeZoneEffect() { Origin = CardGroupEnum.Library });
						break;
					case "PumpAll":
						break;
					case "RemoveCounterAll":
						break;
					case "DamageAll":
						break;
					case "UntapAll":
						break;
					case "PutCounter":						
						numEff = new AddOrRemoveCounter(EffectType.AddOrRemoveCounter);
						a.Effects.Add(numEff);
						break;
					case "PutCounterAll":
						break;
					case "StoreSVar":
						break;
					case "Regenerate":
						break;
					case "FlipACoin":
						break;
					case "SacrificeAll":
						break;
					case "Untap":
						break;
					case "Mill":
						a.Effects.Add (new ChangeZoneEffect());
						break;
					case "Fog":
						break;
					case "RemoveCounter":
						break;
					case "Protection":
						break;
					case "ExchangeZone":
						break;
					case "AnimateAll":
						break;
					case "ChooseCard":
						break;
					case "Reveal":
						break;
					case "ChooseSource":
						break;
					case "MustBlock":
						break;
					case "ExchangeControl":
						break;
					case "RearrangeTopOfLibrary":
						a.Effects.Add(new NumericEffect(EffectType.RearrangeTopOfLibrary));
						break;
					case "CopyPermanent":
						break;
					case "SetState":
						break;
					case "Balance":
						break;
					case "RevealHand":
						break;
					case "Sacrifice":
						break;
					case "AddTurn":
						break;
					case "TwoPiles":
						break;
					case "ManaReflected":
						break;
					case "SetLife":
						a.Effects.Add (new NumericEffect(EffectType.SetLife));
						break;
					case "DebuffAll":
						break;
					case "Fight":
						break;
					case "ChooseType":
						break;
					case "Shuffle":
						a.Effects.Add(new Effect(EffectType.Shuffle));
						break;
					case "NameCard":
						break;
					case "PermanentNoncreature":
						break;
					case "PermanentCreature":
						break;
					case "TapOrUntap":
						break;
					case "GenericChoice":
						break;
					case "Play":
						break;
					case "BecomesBlocked":
						break;
					case "AddOrRemoveCounter":
						break;
					case "WinsGame":
						break;
					case "Proliferate":
						break;
					case "Scry":
						break;
					case "MoveCounter":
						break;
					case "GainOwnership":
						break;
					case "ChangeTargets":
						break;
					case "UnattachAll":
						break;
					case "PeekAndReveal":
						break;
					case "LosesGame":
						break;
					case "DigUntil":
						break;
					case "CopySpellAbility":
						break;
					case "RollPlanarDice":
						break;
					case "RegenerateAll":
						break;
					case "DelayedTrigger":
						break;
					case "MustAttack":
						break;
					case "ProtectionAll":
						break;
					case "RemoveFromCombat":
						break;
					case "RestartGame":
						break;
					case "PreventDamageAll":
						break;
					case "ExchangeLife":
						break;
					case "DeclareCombatants":
						break;
					case "ControlPlayer":
						break;
					case "Phases":
						break;
					case "Clone":
						break;
					case "Clash":
						break;
					case "ChooseNumber":
						break;
					case "EachDamage":
						break;
					case "ReorderZone":
						break;
					case "ChoosePlayer":
						break;
					case "EndTurn":
						break;
					case "MultiplePiles":
						break;

					default:
						break;
					}
					break;
					#endregion
				case AbilityFieldsEnum.Cost:	
					if (Category == AbilityCategory.Spell)
						break;//dont parse cost, it's in card cost
					a.ActivationCost = Cost.Parse (value);
					break;
				case AbilityFieldsEnum.ValidTgts:
					a.ValidTargets = Target.ParseTargets (value);
					break;
				case AbilityFieldsEnum.Mode:
					switch (value.Trim ().ToLower ()) {
					case "continuous":

						break;
					default:
						Debug.WriteLine ("unknow ability mode: " + value.Trim ().ToLower ());
						break;
					}
					break;
				case AbilityFieldsEnum.NumCards:
					numEff = a.Effects.OfType<NumericEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));					
					break;
				case AbilityFieldsEnum.References:
					break;
				case AbilityFieldsEnum.SpellDescription:
					a.Description = value.Trim ();
					break;
				case AbilityFieldsEnum.Produced:
					a.Effects.OfType<ManaEffect>().LastOrDefault().ProducedMana = Mana.Parse (value);
					break;
				case AbilityFieldsEnum.AILogic:
					#region AIlogic 
					switch (value.Trim ()) {
					case "GainControl":
						break;
					case "BeginningOfOppTurn":
						break;
					case "Pump":
						e = new Effect { TypeOfEffect = EffectType.Pump };
						break;
					case "Never":
						break;
					case "MostProminentInHumanDeck":
						break;
					case "RedirectSpellDamageFromPlayer":
						break;
					case "Curse":
						break;
					case "MostProminentAttackers":
						break;
					case "Fog":
						break;
					case "Animate":
						break;
					case "Reanimate":
						break;
					case "MostProminentInComputerGraveyard":
						break;
					case "BeforeCombat":
						break;
					case "MostProminentInComputerDeck":
						break;
					case "NeedsPrevention":
						break;
					case "Evasion":
						break;
					case "BalanceCreaturesAndLands":
						break;
					case "BalancePermanents":
						break;
					case "AtLeast1":
						break;
					case "KeepTapped":
						a.Effects.Add (new Effect { TypeOfEffect = EffectType.DoesNotUntap });
						break;
					case "DontCast":
						break;
					case "MostProminentOnBattlefield":
						break;
					case "ChangeType":
						break;
					case "Clone":
						break;
					case "Worst":
						break;
					case "MostProminentComputerControls":
						break;
					case "Always":
						break;
					case "BestBlocker":
						break;
					case "MostProminentHumanControls":
						break;
					case "ZeroToughness":
						break;
					case "EndOfOppTurn":
						break;
					case "DoubleCounters":
						break;
					case "SpecificCard":
						break;
					case "HighestEvaluation":
						break;
					case "Main2":
						break;
					case "EndOfOpponentsTurn":
						break;
					case "WorstCard":
						break;
					case "DiscardLands":
						break;
					case "MostProminentInComputerHand":
						break;
					case "WildHunt":
						break;
					case "MomirAvatar":
						break;
					case "AtLeast2":
						break;
					case "BalanceLands":
						break;
					case "CloneAllTokens":
						break;
					case "CloneMyTokens":
						break;
					case "Color":
						break;
					case "MostProminentHumanCreatures":
						break;
					case "RemoveAllCounters":
						break;
					case "EOT":
						break;
					case "RestrictBlocking":
						break;
					default:
						break;
					}
					#endregion
					break;
				case AbilityFieldsEnum.Name:
					break;
				case AbilityFieldsEnum.Power:                        
					if (!int.TryParse (value, out v))
						break;
					a.Effects.Add (new NumericEffect {
						TypeOfEffect = EffectType.SetPower,
						Amount = v
					});
					break;
				case AbilityFieldsEnum.Toughness:
					if (!int.TryParse (value, out v))
						break;
					a.Effects.Add (new NumericEffect {
						TypeOfEffect = EffectType.SetTouchness,
						Amount = v
					});
					break;
				case AbilityFieldsEnum.StaticAbilities:
					break;
				case AbilityFieldsEnum.RememberObjects:
					break;
				case AbilityFieldsEnum.SubAbility:
					SVarToResolve.RegisterSVar(value, a, a.GetType().GetField("SubAbility"));
					break;
				case AbilityFieldsEnum.TargetType:
					switch (value) {
					case "Spell":
						stackTargets = TargetType.Spell;
						break;
					case "Activated":
						stackTargets = TargetType.Activated;
						break;
					case "Triggered":
						stackTargets = TargetType.Triggered;
						break;		
					}
					break;
				case AbilityFieldsEnum.TgtPrompt:
					a.targetPrompt = value;
					break;
				case AbilityFieldsEnum.ValidCards:
					break;
				case AbilityFieldsEnum.KW:
					EvasionKeyword ae = EvasionKeyword.Unset;
					if (Enum.TryParse (value, true, out ae)) {
						a.Effects.Add (new AbilityEffect (new Ability (ae)));
						break;
					}
					switch (value) {
					case "Double Strike":
						a.Effects.Add (new AbilityEffect (new Ability(EvasionKeyword.DoubleStrike)));
						break;
					default:
						Debug.WriteLine ("unknown KW in pumping: " + value);
						break;
					}
					break;
				case AbilityFieldsEnum.LifeAmount:
					numEff = a.Effects.OfType<NumericEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));
					
					break;
				case AbilityFieldsEnum.Amount:
					break;
				case AbilityFieldsEnum.Choices:
					break;
				case AbilityFieldsEnum.NumDmg:
					numEff = a.Effects.OfType<NumericEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));
					break;
				case AbilityFieldsEnum.Origin:
					a.Effects.OfType<ChangeZoneEffect>().LastOrDefault().Origin = CardGroup.ParseZoneName (value);
					break;
				case AbilityFieldsEnum.Destination:
					a.Effects.OfType<ChangeZoneEffect>().LastOrDefault().Destination = CardGroup.ParseZoneName (value);
					break;
				//TODO:make single function to parse numeric fields that could be a counter
				case AbilityFieldsEnum.NumAtt:
					numEff = new NumericEffect (EffectType.AddPower);
					if (value [0] == '+')
						value = value.Substring (1);
					else if (value [0] == '-') {
						numEff.Multiplier = -1;
						value = value.Substring (1);
					}						
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));

					a.Effects.Add (numEff);
					numEff = null;
					break;
				case AbilityFieldsEnum.NumDef:                        
					numEff = new NumericEffect (EffectType.AddTouchness);
					if (value [0] == '+')
						value = value.Substring (1);
					else if (value [0] == '-') {
						numEff.Multiplier = -1;
						value = value.Substring (1);
					}						
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));

					a.Effects.Add (numEff);
					numEff = null;
					break;
				case AbilityFieldsEnum.Defined:
					
					break;
				case AbilityFieldsEnum.Triggers:
					break;
				case AbilityFieldsEnum.SVars:
					break;
				case AbilityFieldsEnum.ImprintCards:
					break;
				case AbilityFieldsEnum.ActivationPhases:
					break;
				case AbilityFieldsEnum.RepeatPlayers:
					break;
				case AbilityFieldsEnum.RepeatSubAbility:
					break;
				case AbilityFieldsEnum.IsCurse:
					break;
				case AbilityFieldsEnum.StackDescription:
					break;
				case AbilityFieldsEnum.TokenAmount:
					tokEff = a.Effects.OfType<TokenEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						tokEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, tokEff, tokEff.GetType().GetField("Amount"));					
					break;
				case AbilityFieldsEnum.TokenName:
					a.Effects.OfType<TokenEffect> ().LastOrDefault ().Name = value;
					break;
				case AbilityFieldsEnum.TokenTypes:
					tokEff = a.Effects.OfType<TokenEffect> ().LastOrDefault ();
					string[] types = value.Split(new char[] { ',' });
					foreach (string t in types)
					{
						if (string.IsNullOrEmpty(t))
							continue;
						string tt = t.Replace('\'', '_');
						tt = tt.Replace('-', '_');
						try {
							tokEff.Types += (CardTypes)Enum.Parse(typeof(CardTypes), tt, true);
						} catch (Exception ex) {
							Debug.WriteLine (ex.Message);
						}
					}
					break;
				case AbilityFieldsEnum.TokenOwner:					
					a.Effects.OfType<TokenEffect> ().LastOrDefault ().Owner = TokenEffect.ParseTokenOWnew (value);
					break;
				case AbilityFieldsEnum.TokenColors:
					if (value != "ChosenColor")
						a.Effects.OfType<TokenEffect> ().LastOrDefault ().Colors = MagicData.ParseMultipleColors(value);
					//else chose color
					break;
				case AbilityFieldsEnum.TokenPower:
					tokEff = a.Effects.OfType<TokenEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						tokEff.Power = v;
					else
						SVarToResolve.RegisterSVar(value, tokEff, a.GetType().GetField("Power"));					
					break;
				case AbilityFieldsEnum.TokenToughness:
					tokEff = a.Effects.OfType<TokenEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						tokEff.Toughness = v;
					else
						SVarToResolve.RegisterSVar(value, tokEff, a.GetType().GetField("Toughness"));					
					break;
				case AbilityFieldsEnum.TokenImage:
					a.Effects.OfType<TokenEffect> ().LastOrDefault ().Image = value;
					break;
				case AbilityFieldsEnum.TokenKeywords:
					break;
				case AbilityFieldsEnum.ChangeType:
					a.ValidTargets = Target.ParseTargets (value);
					foreach (CardTarget ct in a.ValidTargets.OfType<CardTarget>()) {
						ct.ValidGroup += a.Effects.OfType<ChangeZoneEffect>().LastOrDefault().Origin;
					}
					break;
				case AbilityFieldsEnum.ChangeNum:
					if (int.TryParse (value, out v))
						a.RequiredTargetCount = v;
					else
						SVarToResolve.RegisterSVar(value, a, a.GetType().GetProperty("RequiredTargetCount"));
					break;
				case AbilityFieldsEnum.GainControl:
					break;
				case AbilityFieldsEnum.LoseControl:
					break;
				case AbilityFieldsEnum.Untap:
					break;
				case AbilityFieldsEnum.AddKWs:
					break;
				case AbilityFieldsEnum.RepeatOptional:
					break;
				case AbilityFieldsEnum.Keywords:
					break;
				case AbilityFieldsEnum.DigNum:
					ChangeZoneEffect cze = a.Effects.OfType<ChangeZoneEffect> ().LastOrDefault ();
					if (int.TryParse (value, out v))
						cze.NumCards = v;
					else
						SVarToResolve.RegisterSVar(value, cze, cze.GetType().GetProperty("NumCards"));
					break;
				case AbilityFieldsEnum.ConditionCheckSVar:
					break;
				case AbilityFieldsEnum.ConditionSVarCompare:
					break;
				case AbilityFieldsEnum.ReplacementEffects:
					break;
				case AbilityFieldsEnum.Stackable:
					break;
				case AbilityFieldsEnum.ChangeValid:
					break;
				case AbilityFieldsEnum.DestinationZone:
					a.Effects.OfType<ChangeZoneEffect>().LastOrDefault().Destination = CardGroup.ParseZoneName (value);
					break;
				case AbilityFieldsEnum.RememberChanged:
					break;
				case AbilityFieldsEnum.TargetMin:
					if (!int.TryParse (value, out v)) {
						Debug.WriteLine ("target min amount: " + value);
						break;
					}
					a.MinimumTargetCount = v;
					break;
				case AbilityFieldsEnum.TargetMax:
					if (!int.TryParse (value, out v)) {
						Debug.WriteLine ("target min amount: " + value);
						break;
					}
					a.MaximumTargetCount = v;					
					break;
				case AbilityFieldsEnum.AllCounterTypes:
					break;
				case AbilityFieldsEnum.NoReg:
					break;
				case AbilityFieldsEnum.AnyPlayer:
					break;
				case AbilityFieldsEnum.Optional:
					break;
				case AbilityFieldsEnum.Reveal:
					a.Effects.OfType<ChangeZoneEffect>().LastOrDefault().Destination = CardGroupEnum.Reveal;
					break;
				case AbilityFieldsEnum.LibraryPosition:
					break;
				case AbilityFieldsEnum.NoRegen:
					break;
				case AbilityFieldsEnum.RememberDamaged:
					break;
				case AbilityFieldsEnum.ValidDescription:
					break;
				case AbilityFieldsEnum.SorcerySpeed:
					break;
				case AbilityFieldsEnum.Mandatory:
					mandatory = true;
					break;
				case AbilityFieldsEnum.Planeswalker:
					break;
				case AbilityFieldsEnum.CounterNum:
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						Debugger.Break ();
						//SVarToResolve.RegisterSVar(value, a, a.GetType().GetProperty("RequiredTargetCount"));
					break;
				case AbilityFieldsEnum.CounterType:
					(numEff as AddOrRemoveCounter).Type =
						(AddOrRemoveCounter.CounterType)Enum.Parse(typeof(AddOrRemoveCounter.CounterType), value, true);					
					break;
				case AbilityFieldsEnum.Ultimate:
					break;
				case AbilityFieldsEnum.TokenStaticAbilities:
					break;
				case AbilityFieldsEnum.TokenSVars:
					break;
				case AbilityFieldsEnum.Permanent:
					break;
				case AbilityFieldsEnum.ActivationLimit:
					break;
				case AbilityFieldsEnum.PlayerTurn:
					break;
				case AbilityFieldsEnum.SVar:
					break;
				case AbilityFieldsEnum.Type:
					break;
				case AbilityFieldsEnum.Expression:
					break;
				case AbilityFieldsEnum.WinSubAbility:
					break;
				case AbilityFieldsEnum.LoseSubAbility:
					break;
				case AbilityFieldsEnum.CostDesc:
					break;
				case AbilityFieldsEnum.RememberTargets:
					break;
				case AbilityFieldsEnum.RestrictValid:
					break;
				case AbilityFieldsEnum.DiscardValid:
					break;
				case AbilityFieldsEnum.Types:
					break;
				case AbilityFieldsEnum.RemoveCreatureTypes:
					break;
				case AbilityFieldsEnum.RememberMilled:
					break;
				case AbilityFieldsEnum.DestinationZone2:
					break;
				case AbilityFieldsEnum.IsPresent:
					break;
				case AbilityFieldsEnum.Colors:
					break;
				case AbilityFieldsEnum.OverwriteColors:
					break;
				case AbilityFieldsEnum.TokenHiddenKeywords:
					break;
				case AbilityFieldsEnum.Choser:
					break;
				case AbilityFieldsEnum.AITgts:
					break;
				case AbilityFieldsEnum.TgtZone:
					break;
				case AbilityFieldsEnum.AnnounceType:
					break;
				case AbilityFieldsEnum.Gains:
					break;
				case AbilityFieldsEnum.DefinedPlayer:
					break;
				case AbilityFieldsEnum.Chooser:
					break;
				case AbilityFieldsEnum.Unique:
					break;
				case AbilityFieldsEnum.Duration:
					break;
				case AbilityFieldsEnum.Hidden:
					break;
				case AbilityFieldsEnum.OriginChoice:
					break;
				case AbilityFieldsEnum.OriginAlternative:
					break;
				case AbilityFieldsEnum.AlternativeMessage:
					break;
				case AbilityFieldsEnum.PrecostDesc:
					break;
				case AbilityFieldsEnum.ActivationZone:
					break;
				case AbilityFieldsEnum.DividedAsYouChoose:
					break;
				case AbilityFieldsEnum.RememberCountered:
					break;
				case AbilityFieldsEnum.Announce:
					break;
				case AbilityFieldsEnum.Zone2:
					break;
				case AbilityFieldsEnum.CheckSVar:
					break;
				case AbilityFieldsEnum.Sacrifice:
					break;
				case AbilityFieldsEnum.RemoveTypes:
					break;
				case AbilityFieldsEnum.Activation:
					break;
				case AbilityFieldsEnum.sVars:
					break;
				case AbilityFieldsEnum.TokenTapped:
					break;
				case AbilityFieldsEnum.PresentCompare:
					break;
				case AbilityFieldsEnum.PresentZone:
					break;
				case AbilityFieldsEnum.ChoiceZone:
					break;
				case AbilityFieldsEnum.GameActivationLimit:
					break;
				case AbilityFieldsEnum.ValidPlayers:
					break;
				case AbilityFieldsEnum.UntilUntaps:
					break;
				case AbilityFieldsEnum.OpponentTurn:
					break;
				case AbilityFieldsEnum.RevealValid:
					break;
				case AbilityFieldsEnum.RememberRevealed:
					break;
				case AbilityFieldsEnum.XColor:
					break;
				case AbilityFieldsEnum.CharmNum:
					break;
				case AbilityFieldsEnum.RepeatCards:
					break;
				case AbilityFieldsEnum.Zone:
					break;
				case AbilityFieldsEnum.RememberChosen:
					break;
				case AbilityFieldsEnum.NoMove:
					break;
				case AbilityFieldsEnum.RememberLKI:
					break;
				case AbilityFieldsEnum.TargetValidTargeting:
					break;
				case AbilityFieldsEnum.UnlessCost:
					break;
				case AbilityFieldsEnum.Valid:
					break;
				case AbilityFieldsEnum.LibraryPosition2:
					break;
				case AbilityFieldsEnum.ChoiceTitle:
					break;
				case AbilityFieldsEnum.Abilities:
					break;
				case AbilityFieldsEnum.Tapped:
					a.Effects.OfType<ChangeZoneEffect>().LastOrDefault().Tapped = bool.Parse (value);
					break;
				case AbilityFieldsEnum.UnlessPayer:
					break;
				case AbilityFieldsEnum.UnlessAI:
					break;
				case AbilityFieldsEnum.TargetUnique:
					break;
				case AbilityFieldsEnum.TargetsFromSingleZone:
					break;
				case AbilityFieldsEnum.SacValid:
					break;
				case AbilityFieldsEnum.Radiance:
					break;
				case AbilityFieldsEnum.ConditionManaSpent:
					break;
				case AbilityFieldsEnum.NumTurns:
					break;
				case AbilityFieldsEnum.Separator:
					break;
				case AbilityFieldsEnum.ChosenPile:
					break;
				case AbilityFieldsEnum.UnchosenPile:
					break;
				case AbilityFieldsEnum.ColorOrType:
					break;
				case AbilityFieldsEnum.ReflectProperty:
					break;
				case AbilityFieldsEnum.Static:
					break;
				case AbilityFieldsEnum.RevealNumber:
					break;
				case AbilityFieldsEnum.TargetsWithDifferentControllers:
					break;
				case AbilityFieldsEnum.ConditionDefined:
					break;
				case AbilityFieldsEnum.ConditionPresent:
					break;
				case AbilityFieldsEnum.ConditionCompare:
					break;
				case AbilityFieldsEnum.SVarCompare:
					break;
				case AbilityFieldsEnum.NonBasicSpell:
					break;
				case AbilityFieldsEnum.staticAbilities:
					break;
				case AbilityFieldsEnum.AddsNoCounter:
					break;
				case AbilityFieldsEnum.RememberSacrificed:
					break;
				case AbilityFieldsEnum.RememberCounters:
					break;
				case AbilityFieldsEnum.ValidDesc:
					break;
				case AbilityFieldsEnum.MinCharmNum:
					break;
				case AbilityFieldsEnum.AllValid:
					break;
				case AbilityFieldsEnum.RememberUntapped:
					break;
				case AbilityFieldsEnum.AnyNumber:
					break;
				case AbilityFieldsEnum.TokenAbilities:
					break;
				case AbilityFieldsEnum.OptionalDecider:
					break;
				case AbilityFieldsEnum.Destroy:
					break;
				case AbilityFieldsEnum.NewState:
					break;
				case AbilityFieldsEnum.PreventionSubAbility:
					break;
				case AbilityFieldsEnum.ShieldEffectTarget:
					break;
				case AbilityFieldsEnum.OptionQuestion:
					break;
				case AbilityFieldsEnum.ImprintTargets:
					break;
				case AbilityFieldsEnum.SacMessage:
					break;
				case AbilityFieldsEnum.FromBottom:
					break;
				case AbilityFieldsEnum.Shuffle:
					break;
				case AbilityFieldsEnum.RememberDiscarded:
					break;
				case AbilityFieldsEnum.ValidZone:
					break;
				case AbilityFieldsEnum.WithoutManaCost:
					break;
				case AbilityFieldsEnum.Controller:
					break;
				case AbilityFieldsEnum.Image:
					break;
				case AbilityFieldsEnum.InstantSpeed:
					break;
				case AbilityFieldsEnum.RemoveKeywords:
					break;
				case AbilityFieldsEnum.RememberDestroyed:
					break;
				case AbilityFieldsEnum.ST:
					break;
				case AbilityFieldsEnum.DefinedCards:
					break;
				case AbilityFieldsEnum.DiscardMessage:
					break;
				case AbilityFieldsEnum.Equip:
					break;
				case AbilityFieldsEnum.AnyOpponent:
					break;
				case AbilityFieldsEnum.UpTo:
					break;
				case AbilityFieldsEnum.MaxFromEffect:
					break;
				case AbilityFieldsEnum.ValidCard:
					break;
				case AbilityFieldsEnum.ForgetOtherTargets:
					break;
				case AbilityFieldsEnum.NoPrevention:
					break;
				case AbilityFieldsEnum.AllType:
					break;
				case AbilityFieldsEnum.InvalidTypes:
					break;
				case AbilityFieldsEnum.HiddenKeywords:
					break;
				case AbilityFieldsEnum.ForgetOtherRemembered:
					break;
				case AbilityFieldsEnum.RememberCostCards:
					break;
				case AbilityFieldsEnum.ScryNum:
					break;
				case AbilityFieldsEnum.UntilYourNextUpkeep:
					break;
				case AbilityFieldsEnum.Imprint:
					break;
				case AbilityFieldsEnum.RegenerationTrigger:
					break;
				case AbilityFieldsEnum.ReplaceCardUID:
					break;
				case AbilityFieldsEnum.OverwriteTypes:
					break;
				case AbilityFieldsEnum.KeepSupertypes:
					break;
				case AbilityFieldsEnum.KeepCardTypes:
					break;
				case AbilityFieldsEnum.RemoveAllAbilities:
					a.Effects.Add (new Effect (EffectType.LooseAllAbilities));                        
					break;
				case AbilityFieldsEnum.TargetsSingleTarget:
					break;
				case AbilityFieldsEnum.UseImprinted:
					break;
				case AbilityFieldsEnum.ChooseOrder:
					break;
				case AbilityFieldsEnum.UseAllOriginZones:
					break;
				case AbilityFieldsEnum.UnattachValid:
					break;
				case AbilityFieldsEnum.DamageSource:
					break;
				case AbilityFieldsEnum.ConditionDescription:
					break;
				case AbilityFieldsEnum.PeekAmount:
					break;
				case AbilityFieldsEnum.RevealOptional:
					break;
				case AbilityFieldsEnum.ActivationNumberSacrifice:
					break;
				case AbilityFieldsEnum.PumpZone:
					break;
				case AbilityFieldsEnum.OrColors:
					break;
				case AbilityFieldsEnum.RandomKeyword:
					break;
				case AbilityFieldsEnum.NoRepetition:
					break;
				case AbilityFieldsEnum.RememberTokens:
					break;
				case AbilityFieldsEnum.CopyOnce:
					break;
				case AbilityFieldsEnum.CopyCard:
					break;
				case AbilityFieldsEnum.FoundDestination:
					break;
				case AbilityFieldsEnum.RevealedDestination:
					break;
				case AbilityFieldsEnum.FoundLibraryPosition:
					break;
				case AbilityFieldsEnum.RevealedLibraryPosition:
					break;
				case AbilityFieldsEnum.RememberFound:
					break;
				case AbilityFieldsEnum.RepeatCheckSVar:
					break;
				case AbilityFieldsEnum.RepeatSVarCompare:
					break;
				case AbilityFieldsEnum.DefinedLandwalk:
					break;
				case AbilityFieldsEnum.RememberDrawn:
					break;
				case AbilityFieldsEnum.LoseAtEndStep:
					break;
				case AbilityFieldsEnum.DivideEvenly:
					break;
				case AbilityFieldsEnum.Flashback:
					break;
				case AbilityFieldsEnum.ActivationCardsInHand:
					break;
				case AbilityFieldsEnum.CopyIsColor:
					break;
				case AbilityFieldsEnum.AtRandom:
					break;
				case AbilityFieldsEnum.UntilHostLeavesPlay:
					break;
				case AbilityFieldsEnum.Deplete:
					break;
				case AbilityFieldsEnum.Phase:
					break;
				case AbilityFieldsEnum.Execute:
					break;
				case AbilityFieldsEnum.Defender:
					break;
				case AbilityFieldsEnum.AndOrValid:
					break;
				case AbilityFieldsEnum.DifferentNames:
					break;
				case AbilityFieldsEnum.RepeatCounters:
					break;
				case AbilityFieldsEnum.ControlledByTarget:
					break;
				case AbilityFieldsEnum.NewController:
					break;
				case AbilityFieldsEnum.RememberRemoved:
					break;
				case AbilityFieldsEnum.EachBasicType:
					break;
				case AbilityFieldsEnum.TargetsAtRandom:
					break;
				case AbilityFieldsEnum.Condition:
					break;
				case AbilityFieldsEnum.MaxRepeat:
					break;
				case AbilityFieldsEnum.ExileFaceDown:
					break;
				case AbilityFieldsEnum.AttachedTo:
					break;
				case AbilityFieldsEnum.AddsKeywords:
					break;
				case AbilityFieldsEnum.AtEOT:
					break;
				case AbilityFieldsEnum.RestRandomOrder:
					break;
				case AbilityFieldsEnum.Changetype:
					break;
				case AbilityFieldsEnum.Ninjutsu:
					break;
				case AbilityFieldsEnum.DestinationChoice:
					break;
				case AbilityFieldsEnum.ForgetRemembered:
					break;
				case AbilityFieldsEnum.Random:
					break;
				case AbilityFieldsEnum.RememberRemovedFromCombat:
					break;
				case AbilityFieldsEnum.RememberTargetedCard:
					break;
				case AbilityFieldsEnum.UntilYourNextTurn:
					break;
				case AbilityFieldsEnum.UntilEndOfCombat:
					break;
				case AbilityFieldsEnum.AnySupportedCard:
					break;
				case AbilityFieldsEnum.RandomCopied:
					break;
				case AbilityFieldsEnum.RandomNum:
					break;
				case AbilityFieldsEnum.ChoiceNum:
					break;
				case AbilityFieldsEnum.TargetControls:
					break;
				case AbilityFieldsEnum.RestrictFromZone:
					break;
				case AbilityFieldsEnum.RestrictFromValid:
					break;
				case AbilityFieldsEnum.Min:
					break;
				case AbilityFieldsEnum.Max:
					break;
				case AbilityFieldsEnum.TapOnLose:
					break;
				case AbilityFieldsEnum.DeclareAttackers:
					break;
				case AbilityFieldsEnum.DeclareBlockers:
					break;
				case AbilityFieldsEnum.Until:
					break;
				case AbilityFieldsEnum.DestroyTgt:
					break;
				case AbilityFieldsEnum.RememberSplicedOntoCounteredSpell:
					break;
				case AbilityFieldsEnum.Mitosis:
					break;
				case AbilityFieldsEnum.AddAbilities:
					break;
				case AbilityFieldsEnum.AddSVars:
					break;
				case AbilityFieldsEnum.ValidAttribute:
					break;
				case AbilityFieldsEnum.EffectOwner:
					break;
				case AbilityFieldsEnum.MayShuffle:
					break;
				case AbilityFieldsEnum.Monstrosity:
					break;
				case AbilityFieldsEnum.NoCall:
					break;
				case AbilityFieldsEnum.HeadsSubAbility:
					break;
				case AbilityFieldsEnum.TailsSubAbility:
					break;
				case AbilityFieldsEnum.StaticCommandCheckSVar:
					break;
				case AbilityFieldsEnum.StaticCommandSVarCompare:
					break;
				case AbilityFieldsEnum.AddsCounters:
					break;
				case AbilityFieldsEnum.UntilControllerNextUntap:
					break;
				case AbilityFieldsEnum.RandomChange:
					break;
				case AbilityFieldsEnum.TokenController:
					break;
				case AbilityFieldsEnum.NumCopies:
					break;
				case AbilityFieldsEnum.OwnerOnly:
					break;
				case AbilityFieldsEnum.RememberAllPumped:
					break;
				case AbilityFieldsEnum.TargetsFromDifferentZone:
					break;
				case AbilityFieldsEnum.TgtPromp:
					break;
				case AbilityFieldsEnum.UnlessResolveSubs:
					break;
				case AbilityFieldsEnum.RepeatDefined:
					break;
				case AbilityFieldsEnum.RepeatPresent:
					break;
				case AbilityFieldsEnum.RepeatCompare:
					break;
				case AbilityFieldsEnum.OtherwiseSubAbility:
					break;
				case AbilityFieldsEnum.ConditionTargetValidTargeting:
					break;
				case AbilityFieldsEnum.ConditionTargetsSingleTarget:
					break;
				case AbilityFieldsEnum.TargetRestriction:
					break;
				case AbilityFieldsEnum.CopyForEachCanTarget:
					break;
				case AbilityFieldsEnum.CanTargetPlayer:
					break;
				case AbilityFieldsEnum.Redistribute:
					break;
				case AbilityFieldsEnum.ListTitle:
					break;
				case AbilityFieldsEnum.TargetsWithoutSameCreatureType:
					break;
				case AbilityFieldsEnum.ContionSVarCompare:
					break;
				case AbilityFieldsEnum.isPresent:
					break;
				case AbilityFieldsEnum.ImprintTokens:
					break;
				case AbilityFieldsEnum.DamageDesc:
					break;
				case AbilityFieldsEnum.ConditionChosenColor:
					break;
				case AbilityFieldsEnum.SkipUntap:
					break;
				case AbilityFieldsEnum.RecordChoice:
					break;
				case AbilityFieldsEnum.ChoosePlayer:
					break;
				case AbilityFieldsEnum.TokenTriggers:
					break;
				case AbilityFieldsEnum.Source:
					break;
				case AbilityFieldsEnum.XCantBe0:
					break;
				case AbilityFieldsEnum.ReplacePlayerName:
					break;
				case AbilityFieldsEnum.ChangeSingleTarget:
					break;
				case AbilityFieldsEnum.NoReveal:
					break;
				case AbilityFieldsEnum.RememberTapped:
					break;
				case AbilityFieldsEnum.DelayedTrigger:
					break;
				case AbilityFieldsEnum.DestroyPermanent:
					break;
				case AbilityFieldsEnum.SkipReorder:
					break;
				case AbilityFieldsEnum.NoShuffle:
					break;
				case AbilityFieldsEnum.MaxRevealed:
					break;
				case AbilityFieldsEnum.Reference:
					break;
				case AbilityFieldsEnum.PhaseInOrOutAll:
					break;
				case AbilityFieldsEnum.ConditionLifeTotal:
					break;
				case AbilityFieldsEnum.ConditionLifeAmount:
					break;
				case AbilityFieldsEnum.NoPeek:
					break;
				case AbilityFieldsEnum.ImprintRevealed:
					break;
				case AbilityFieldsEnum.ValidTypes:
					break;
				case AbilityFieldsEnum.RememberControlled:
					break;
				case AbilityFieldsEnum.Bonus:
					break;
				case AbilityFieldsEnum.BonusProduced:
					break;
				case AbilityFieldsEnum.IsCursed:
					break;
				case AbilityFieldsEnum.RandomChosen:
					break;
				case AbilityFieldsEnum.Piles:
					break;
				case AbilityFieldsEnum.PreCostDesc:
					break;
				case AbilityFieldsEnum.UnlessType:
					break;
				default:
					break;
				}
			}
			if (!a.AcceptTargets && a.ValidTargets != null) {
				Debug.WriteLine ("required target forced to 1.");
				a.RequiredTargetCount = 1;
			}
			//
			if (validCardTargetZoneFix != CardGroupEnum.Any) {
				if (a.ValidTargets == null)
					a.ValidTargets = new CardTarget (TargetType.Card) { ValidGroup = validCardTargetZoneFix };
				else {
					foreach (CardTarget ct in a.ValidTargets.OfType<CardTarget>()) {
						ct.ValidGroup += validCardTargetZoneFix;
					}
				}
			}
			a.Category = Category;
			a.Mandatory = mandatory;

			//fix for stack targeting
			if (stackTargets != TargetType.Card) {
				if (a.ValidTargets == null)
					a.ValidTargets += stackTargets;
				else {
					foreach (CardTarget trg in a.ValidTargets.OfType<CardTarget>()) {
						trg.TypeOfTarget = stackTargets;
					}
				}
			}
		}


		public static Ability ParseKeyword (string str, MagicCard mc = null)
		{
			Ability a = null;

			if (str == "CARDNAME enters the battlefield tapped.") {
				Trigger trig = new Trigger (
					               MagicEventType.ChangeZone, 
					               new CardTarget (TargetType.Self),
					new Ability (EffectType.Tap) { });
				trig.Origine = CardGroupEnum.Any;
				trig.Destination = CardGroupEnum.InPlay;
				trig.InhibStacking = true;
				mc.Triggers.Add (trig);
				return null;
			}

			int ptrFirstSpace = str.IndexOf (" ");
			string data = str.Substring (ptrFirstSpace + 1, str.Length - ptrFirstSpace - 1);

			//string[] tmp = str.Split (new char[] { ' ' });
			if (ptrFirstSpace < 0)
				ptrFirstSpace = str.Length;
            
			switch (str.Substring(0,ptrFirstSpace)) {
			case "CARDNAME":
				
				break;
			case "Swampwalk":
				a = new Ability (EvasionKeyword.Swampwalk);
				break;
			case "Islandwalk":
				a = new Ability (EvasionKeyword.Islandwalk);
				break;
			case "Plainswalk":
				a = new Ability (EvasionKeyword.Plainswalk);
				break;
			case "Forestwalk":
				a = new Ability (EvasionKeyword.Forestwalk);
				break;
			case "Mountainwalk":
				a = new Ability (EvasionKeyword.Mountainwalk);
				break;
			case "First":
				if (data != "Strike")
					Debugger.Break ();
				return new Ability (EvasionKeyword.FirstStrike);
			case "Flying":
				a = new Ability (EvasionKeyword.Flying);
				break;
			case "Protection":
				a = new Ability (EvasionKeyword.Protection);
				break;
			case "Vigilance":
				a = new Ability (EvasionKeyword.Vigilance);
				break;
			case "Trample":
				a = new Ability (EvasionKeyword.Trample);
				break;
			case "Intimidate":
				a = new Ability (EvasionKeyword.Intimidate);
				break;
			case "Deathtouch":
				a = new Ability (EvasionKeyword.Deathtouch);
				break;
			case "Unblockable":
				a = new Ability (EvasionKeyword.Unblockable);
				break;
			case "Defender":
				a = new Ability (EvasionKeyword.Defender);
				break;
			case "Haste":
				a = new Ability (EvasionKeyword.Haste);
				break;
			case "Banding":
				a = new Ability (EvasionKeyword.Banding);
				break;
			case "Bushido":
				a = new Ability (EvasionKeyword.Bushido);
				break;
			case "Horsemanship":
				a = new Ability (EvasionKeyword.Horsemanship);
				break;
			case "Lifelink":
				a = new Ability (EvasionKeyword.Lifelink);
				break;
			case "Haunt":
				a = new Ability (EvasionKeyword.Haunt);
				break;
			case "Reach":
				a = new Ability (EvasionKeyword.Reach);
				break;
			case "Shroud":
				a = new Ability (EvasionKeyword.Shroud);
				break;

			case "Enchant":
                    //AttachAbility aa = new AttachAbility();
                    //aa.ValidTargets.Value = (CardTypes)Enum.Parse(typeof(CardTypes),tmp[1],true);
				return null;
			case "Cumulative":
				break;
			case "TypeCycling":
				break;
			case "Entwine":
				break;
			case "Equip":
				a = new Ability () { AbilityType = EvasionKeyword.Attach };
				CardTarget ct = new CardTarget ();
				ct.Controler = ControlerType.You;
				ct.ValidCardTypes += CardTypes.Creature;
				a.ValidTargets += ct;
				a.ActivationCost = Cost.Parse (data);
				a.RequiredTargetCount = 1;
				break;
			case "Flashback":
				break;
			case "Echo":
				break;
			case "ETBReplacement":
				break;
			case "Evolve":
				break;
			case "Suspend":
				break;
			case "Rampage":
				break;
			case "Persist":
				break;
			case "Kicker":
				a = new Ability (EvasionKeyword.Kicker) { Category = AbilityCategory.Spell };
				a.ActivationCost = Cost.Parse (data);
				a.Message = "Kick ?";
				break;
			case "etbCounter":
				break;
			case "Flash":
				break;
			case "Evoke":
				break;
			case "Conspire":
				break;
			case "Exalted":
				break;
			case "Morph":
				break;
			case "Cycling":
				break;
			case "Fuse":
				break;
			case "Buyback":
				break;
			case "You":
				break;
			case "CantBeBlockedBy":
				break;
			case "Remove":
				break;
			case "Unearth":
				break;
			case "Fading":
				break;
			case "Convoke":
				break;
			case "Split":
				break;
			case "Multikicker":
				break;
			case "Graft":
				break;
			case "Monstrosity":
				break;
			case "At":
				break;
			case "Modular":
				break;
			case "Fear":
				
				break;
			case "Sunburst":
				break;
			case "Cascade":
				break;
			case "Permanents":
				break;
			case "PreventAllDamageBy":
				break;
			case "Madness":
				Trigger trig = new Trigger (
					MagicEventType.ChangeZone, 
					new CardTarget (TargetType.Self),
					new Ability (EffectType.Tap) { });
				trig.Origine = CardGroupEnum.Hand;
				trig.Destination = CardGroupEnum.Graveyard;
				trig.InhibStacking = false;
				mc.Triggers.Add (trig);
				break;
			case "Annihilator":
				break;
			case "Hexproof":
				a = new Ability (EvasionKeyword.Hexproof);
				break;
			case "Storm":
				break;
			case "Shadow":
				break;
			case "Creatures":
				break;
			case "Indestructible":
				a = new Ability (EvasionKeyword.Indestructible);
				break;
			case "Vanishing":
				break;
			case "Amplify":
				break;
			case "Legendary":
				break;
			case "Bestow":
				break;
			case "Miracle":
				break;
			case "Extort":
				break;
			case "Bloodthirst":
				break;
			case "Living":
				break;
			case "Totem":
				break;
			case "Level":
				break;
			case "maxLevel":
				break;
			case "Flanking":
				break;
			case "Infect":
				break;
			case "Splice":
				break;
			case "If":
				break;
			case "Soulshift":
				break;
			case "Champion":
				break;
			case "Wither":
				break;
			case "Double":
				break;
			case "Transmute":
				break;
			case "CantBlock":
				break;
			case "Phasing":
				break;
			case "Provoke":
				break;
			case "Undying":
				break;
			case "Devour":
				break;
			case "No":
				break;
			case "MayEffectFromOpeningHand":
				break;
			case "CantBeBlockedByAmount":
				break;
			case "Prevent":
				break;
			case "As":
				break;
			case "Rebound":
				break;
			case "Recover":
				break;
			case "Play":
				break;
			case "Dredge":
				break;
			case "Fortify":
				break;
			case "Soulbond":
				break;
			case "Delve":
				break;
			case "Desertwalk":
				break;
			case "Each":
				break;
			case "SpellCantTarget":
				break;
			case "Nonbasic":
				break;
			case "All":
				break;
			case "Epic":
				break;
			case "Damage":
				break;
			case "Tribute":
				break;
			case "Players":
				break;
			case "AlternateAdditionalCost":
				break;
			case "Replicate":
				break;
			case "Hideaway":
				break;
			case "CantEquip":
				break;
			case "Snow":
				break;
			case "Goblin":
				break;
			case "Fox":
				break;
			case "Moonfolk":
				break;
			case "Rat":
				break;
			case "Snake":
				break;
			case "Reveal":
				break;
			case "etbCounters":
				break;
			case "Whenever":
				break;
			case "Ripple":
				break;
			default:
				break;
			}
			return a;
		}       

		public override string ToString ()
		{
			string tmp = Description;
			if (IsTriggeredAbility)
				tmp = "Triggered " + tmp;
			else if (IsActivatedAbility)
				tmp = "Activated " + tmp;
			if (this.AbilityType != EvasionKeyword.Unset) {
				tmp += this.AbilityType.ToString ();
				return tmp;
			}
			foreach (Effect e in Effects) {
				tmp += e.ToString() + ", ";
			}
			return string.IsNullOrEmpty(tmp) ? AbilityType.ToString() : tmp.Substring (0, tmp.Length - 2);
		}
		public String[] CostElements
		{
			get{
				if (ActivationCost == null)
					return null;
				string tmp = ActivationCost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length < 3).ToArray();
			}
		}
		public String[] OtherCostElements
		{
			get{
				if (ActivationCost == null)
					return null;
				string tmp = ActivationCost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length > 3).ToArray();
			}
		}
	}

}
