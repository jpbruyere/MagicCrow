//
//  Ability.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2017 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Reflection;

namespace MagicCrow.Abilities
{
	public enum SubClass {
		#region AlterLife
		LoseLife,
		GainLife,
		SetLife,
		ExchangeLife,
		Poison,
		#endregion

		Animate,
		Attach,
		Bond,
		ChangeState,
		ChangeZone,
		Charm,
		Choose,
		Clash,
		CleanUp,

		#region Combat
		Fog,
		#endregion

		#region Copy
		CopyPermanent,
		CopySpell,
		#endregion

		Counter,//CounterMagic

		#region Counters
		PutCounter,
		PutCounterAll,
		RemoveCounter,
		RemoveCounterAll,
		Proliferate,
		MoveCounters,
		#endregion

		DealDamage,
		DamageAll,

		Debuff,
		DebuffAll,

		DelayedTrigger,

		Destroy,
		DestroyAll,

		Effect,
		EndGameCondition,
		GainControl,
		Mana,
		PeekAndReveal,
		#region PermanentState
		Untap,
		UntapAll,
		Tap,
		TapAll,
		TapOrUntap,
		Phases,
		#endregion
		Play,
		PreventDamage,
		Protection,
		ProtectionAll,
		Pump,
		Regenerate,
		RegenerateAll,
		Repeat,
		RestartGame,
		#region Reveal
		Dig,
		DigUntil,
		RevealHand,
		Scry,
		RearrangeTopOfLibrary,
		Reveal,
		#endregion
		ReverseTurnOver,
		Sacrifice,
		StoreSVar,
		Token,
		#region Turns
		AddTurns,
		EndTurns,
		#endregion
		#region ZoneAffecting
		Draw,
		Discard,
		Mill,
		Shuffle
		#endregion
	}
	[Serializable]
	public class Ability
	{
		string targetPrompt;

		#region Public fields
		public AbilityCategory Category = AbilityCategory.Acivated;
		public SubClass SubClass;
		public AttributGroup<Target> ValidTargets = null;
		public AttributGroup<Target> Defined = null;
		public Cost ActivationCost = null;
		public Cost UnlessCost = null;
		public string SpellDescription;
		public string StackDescription;
		//public var Conditions;
		#endregion

		#region CTOR
		public Ability ()
		{
		}
		public Ability (AbilityCategory category, SubClass subClass){
			Category = category;
			SubClass = subClass;
		} 
		#endregion

		public static Ability Parse(string abString){
			Ability ability = null;
			AbilityCategory Category = AbilityCategory.Acivated;

			string[] tmp = abString.Split (new char[] { '|' });

			if (tmp [0].StartsWith ("AB"))
				Category = AbilityCategory.Acivated;
			else if (tmp [0].StartsWith ("SP"))
				Category = AbilityCategory.Spell;
			else if (tmp [0].StartsWith ("DB"))
				Category = AbilityCategory.DrawBack;
			else
				throw new Exception ("Unknown Ability Category: " + tmp [0].Substring (0, 2));

			SubClass AbSubclass;
			if (!Enum.TryParse (tmp [0].Substring (3).Trim (), out AbSubclass))
				throw new Exception ("Unknown Ability SubClass: " + tmp [0].Substring (3).Trim ());

			switch (AbSubclass) {
			case SubClass.LoseLife:
			case SubClass.GainLife:
			case SubClass.SetLife:
			case SubClass.ExchangeLife:
			case SubClass.Poison:
				ability = new AlterLife ();
				break;
			case SubClass.Animate:
				break;
			case SubClass.Attach:
				break;
			case SubClass.Bond:
				break;
			case SubClass.ChangeState:
				break;
			case SubClass.ChangeZone:
				break;
			case SubClass.Charm:
				break;
			case SubClass.Choose:
				break;
			case SubClass.Clash:
				break;
			case SubClass.CleanUp:
				break;
			case SubClass.Fog:
				break;
			case SubClass.CopyPermanent:
			case SubClass.CopySpell:
				break;
			case SubClass.Counter:
				break;
			case SubClass.PutCounter:
			case SubClass.PutCounterAll:
			case SubClass.RemoveCounter:
			case SubClass.RemoveCounterAll:
			case SubClass.Proliferate:
			case SubClass.MoveCounters:
				break;
			case SubClass.DealDamage:
			case SubClass.DamageAll:
				break;
			case SubClass.Debuff:
			case SubClass.DebuffAll:
				break;
			case SubClass.DelayedTrigger:
				break;
			case SubClass.Destroy:
			case SubClass.DestroyAll:
				break;
			case SubClass.Effect:
				break;
			case SubClass.EndGameCondition:
				break;
			case SubClass.GainControl:
				break;
			case SubClass.Mana:
				break;
			case SubClass.PeekAndReveal:
				break;
			case SubClass.Untap:
			case SubClass.UntapAll:
			case SubClass.Tap:
			case SubClass.TapAll:
			case SubClass.TapOrUntap:
			case SubClass.Phases:
				break;
			case SubClass.Play:
				break;
			case SubClass.PreventDamage:
				break;
			case SubClass.Protection:
			case SubClass.ProtectionAll:
				break;
			case SubClass.Pump:
				break;
			case SubClass.Regenerate:
			case SubClass.RegenerateAll:
				break;
			case SubClass.Repeat:
				break;
			case SubClass.RestartGame:
				break;
			case SubClass.Dig:
			case SubClass.DigUntil:
			case SubClass.RevealHand:
			case SubClass.Scry:
			case SubClass.RearrangeTopOfLibrary:
			case SubClass.Reveal:
				break;
			case SubClass.ReverseTurnOver:
				break;
			case SubClass.Sacrifice:
				break;
			case SubClass.StoreSVar:
				break;
			case SubClass.Token:
				break;
			case SubClass.AddTurns:
			case SubClass.EndTurns:
				break;
			case SubClass.Draw:
			case SubClass.Discard:
			case SubClass.Mill:
			case SubClass.Shuffle:
				break;
			default:
				break;
			}

			ability.Category = Category;
			ability.SubClass = AbSubclass;

			for (int i = 1; i < tmp.Length; i++) {
				int dol = tmp [i].IndexOf ('$');
				if (!ability.TrySetParameter (tmp [i].Substring (0, dol).Trim (), tmp [i].Substring (dol + 1).Trim ()))
					System.Diagnostics.Debug.WriteLine ("Error parsing: " + tmp [0]);
			}

			return ability;
		}

		public virtual bool TrySetParameter(string paramName, string value){
			switch (paramName) {
			case "Cost":
				if (Category != AbilityCategory.Spell)
					ActivationCost = Cost.Parse (value);
				return true;
			case "UnlessCost":
				UnlessCost = Cost.Parse (value);
				return true;
			case "ValidTgts":
				ValidTargets = Target.ParseTargets (value);
				return true;
			case "TgtPrompt":
				targetPrompt = value;
				return true;
			case "Defined":
				Defined = Target.ParseTargets (value);
				return true;
			case "SpellDescription":
				SpellDescription = value;
				return true;
			case "StackDescription":
				StackDescription = value;
				return true;
			case "Conditions":
				System.Diagnostics.Debug.WriteLine ("Unhandled condition: " + value);
				return false;
			default:
				System.Diagnostics.Debug.WriteLine ("unknwon parameter: " + paramName);
				return false;
			}
		}

		protected void SetInteger (string fieldName, string value){
			FieldInfo fi = this.GetType().GetField(fieldName);
			int v = 0;
			if (int.TryParse (value, out v))
				fi.SetValue(this, v);
			else
				SVarToResolve.RegisterSVar(value, this, fi);
		}

		public virtual void Resolve (AbilityActivation source){
			
		}

		#region old ability props
		/// <summary>
		/// return MinimumTargetCount if set or _requiredTargetCount
		/// </summary>
		public virtual int RequiredTargetCount { get { return 0;}}
		/// <summary>
		/// Used to know if ability could accept target 
		/// for msg prompt.
		/// </summary>
		public virtual int PossibleTargetCount { get { return 0;} }
		/// <summary>
		/// if minTarget or maxTarget are set return true 
		/// else return true if required target > 0
		/// </summary>
		public virtual bool AcceptTargets { get { return false; } }			
		public string TargetPrompt {
			get {
				return string.IsNullOrWhiteSpace (targetPrompt) ? "\tSelect " + ValidTargets?.ToString () : targetPrompt;
			}
			set { targetPrompt = value;	}
		}
		#endregion
	}
}

