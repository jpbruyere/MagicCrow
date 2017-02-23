using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace MagicCrow.Triggers
{
	public enum Mode {
		Always,
		Attached,
		Detached,
		AttackerBlocked,
		AttackerUnblocked,
		AttackersDeclared,
		Attacks,
		BecomeMonstrous,
		BecomesTarget,
		BlockersDeclared,
		Blocks,
		Championed,
		ChangesController,
		ChangesZone,
		Clashed,
		CombatDamageDoneOnce,
		CounterAdded,
		CounterRemoved,
		Countered,
		Cycled,
		DamageDone,
		DealtCombatDamageOnce,
		Destroyed,
		Devoured,
		Discarded,
		Drawn,
		Evolved,
		FlippedCoin,
		LandPlayed,
		LifeGained,
		LifeLost,
		LosesGame,
		NewGame,
		PayCumulativeUpkeep,
		PayEcho,
		Phase,
		EndPhase,
		PhaseIn,
		PhaseOut,
		PlanarDice,
		PlaneswalkedTo,
		PlaneswalkedFrom,
		Sacrificed,
		Scry,
		SearchedLibrary,
		SetInMotion,
		Shuffled,
		SpellCast,
		AbilityCast,
		SpellAbilityCast,
		Taps,
		Untaps,
		TapsForMana,
		Transformed,
		TurnFaceUp,
		Unequip,
		Vote,
	}
	[Serializable]
	public class Trigger
	{
		#region public field
		public Mode Mode;
		public Abilities.Ability Exec;
		public string Description;
		public CardGroupEnum TriggerZone;
		public AttributGroup<Target> ValidTarget;
		public AttributGroup<Target> ValidSouce;
		public GamePhases TriggerPhases;
		public bool InhibStacking;
		public bool OpponentTurn;
		public bool PlayerTurn;
		public bool Metalcraft;
		public bool Threshold;
		public bool PlayersPoisoned;
		/*
			IsPresent
			PresentCompare,PresentZone & PresentPlayer - These parameters only matter if the IsPresent parameter is used. They can be used to narrow down and how many valid cards must be present and where they must be.
				IsPresent2,PresentCompare2,PresentZone2 & PresentPlayer2 - Second requirement (see above).
				CheckSVar - Calculates the named SVar and compares the result against the accompanying SVarCompare parameter which takes the form of <operator><operand> where <operator> is in LT,LE,EQ,NE,GE,GT. 
				*/
		#endregion

		#region CTOR
		public Trigger ()
		{			
		}
		#endregion

		public static Trigger Parse (string str)
		{
			Trigger t = new Trigger ();

			string[] tmp = str.Trim ().Split (new char[] { '|' });

			for (int i = 0; i < tmp.Length; i++) {
				int dol = tmp [i].IndexOf ('$');
				if (!t.TrySetParameter (tmp [i].Substring (0, dol).Trim (), tmp [i].Substring (dol + 1).Trim ()))
					System.Diagnostics.Debug.WriteLine ("Error parsing: " + tmp [0]);
			}
			foreach (string i in tmp) {
				string[] f = i.Trim ().Split (new char[] { '$' });
				string data = f [1].Trim ();
				switch (f [0]) {
				case "Mode":
					Mode trigMode;
					if (!Enum.TryParse (tmp [0].Substring (3).Trim (), out AbSubclass))
						throw new Exception ("Unknown Ability SubClass: " + tmp [0].Substring (3).Trim ());
					break;
				case "Origin":
					t.Origine = CardGroup.ParseZoneName (data);
					break;
				case "Destination":
					t.Destination = CardGroup.ParseZoneName (data);
					break;
				case "ValidCard":														
					t.ValidTarget = Target.ParseTargets (data);
					break;
				case "Execute":
					SVarToResolve.RegisterSVar(data, t, t.GetType().GetField("Exec"));
					break;
				case "TriggerDescription":
					t.Description = data;
					break;
				case "Phase":
					t.Mode = MagicEventType.BeginPhase;
					switch (data) {
					case "End of Turn":
						t.Phase = GamePhases.EndOfTurn;
						break;
					default:
						Debug.WriteLine ("Trigger parsing: Unknown phase:" + data);
						break;
					}
					break;
				case "TriggerZones":
					t.TriggerZone = CardGroup.ParseZoneName (data);
					break;
				case "CheckSVar":
					break;
				case "SVarCompare":
					break;
				case "OptionalDecider":
					break;
				case "ValidActivatingPlayer":
					break;
				case "Secondary":
					break;
				default:
					Debug.WriteLine ("Unknown trigger var:" + f [0]);
					break;
				}
			}
			return t;
		}
		public virtual bool TrySetParameter(string paramName, string value){
			switch (paramName) {
			case "Mode":				
				if (!Enum.TryParse (value, out Mode))
					throw new Exception ("Unknown Trigger Mode: " + value);
				return true;
			case "Execute":
				SVarToResolve.RegisterSVar (value, this, this.GetType ().GetField ("Exec"));
				return true;
			case "Static":
				return true;
			default:
				System.Diagnostics.Debug.WriteLine ("unknwon parameter: " + paramName);
				return false;
			}
		}
	}
}
