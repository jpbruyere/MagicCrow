using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace MagicCrow
{
	[Serializable]
	public class Trigger
	{
		public Trigger ()
		{			
		}

		public Trigger (MagicEventType _type, 
			AttributGroup<Target> _targets = null,
			Ability _exec = null)
		{			
			Type = _type;
			ValidTarget = _targets;
			Exec = _exec;
		}


		public MagicEventType Type;

		#region changezonearg

		public CardGroupEnum Origine;
		public CardGroupEnum Destination;

		#endregion

		public AttributGroup<Target> ValidTarget;
		//public List<Effect> Effects;
		public GamePhases Phase;
		//public CardInstance Source;
		public Ability Exec;
		public string Description;
		public bool InhibStacking; //prevent triggered ability to goes on the stack (ex card enter tapped)

		bool targetIsValid(object target, CardInstance source)
		{	
			if (ValidTarget == null)
				return false;
			foreach (Target ct in ValidTarget) {
				if (ct.Accept (target, source))
					return true;
			}
			return false;
		}

		public bool ExecuteIfMatch(MagicEventArg arg, CardInstance triggerSource)
		{
			if (Type != arg.Type)
				return false;
													
			switch (Type) {
			case MagicEventType.ChangeZone:
				
				if (!targetIsValid (arg.Source, triggerSource))
					return false;
				
				ChangeZoneEventArg czea = arg as ChangeZoneEventArg;
				if ((czea.Origine == Origine || Origine == CardGroupEnum.Any)
				    && (czea.Destination == Destination || Destination == CardGroupEnum.Any)) {
					if (InhibStacking)
						MagicEngine.CurrentEngine.MagicStack.PushOnStack (new AbilityActivation (arg.Source, Exec) { GoesOnStack = false });
					else
						MagicEngine.CurrentEngine.MagicStack.PushOnStack (new AbilityActivation (arg.Source, Exec));
				}else
					return false;
				return true;
			case MagicEventType.CastSpell:								
				if (targetIsValid ((arg as SpellEventArg).Spell.CardSource, triggerSource))
					MagicEngine.CurrentEngine.MagicStack.PushOnStack 
						(new AbilityActivation ((arg as SpellEventArg).Spell.CardSource, Exec));
				else
					return false;
				return true;						
			default:
				Debug.WriteLine ("default trigger handler: " + this.ToString ());
				if (targetIsValid (arg.Source, triggerSource)) {
					if (InhibStacking)
						MagicEngine.CurrentEngine.MagicStack.PushOnStack (new AbilityActivation (arg.Source, Exec) { GoesOnStack = false });
					else
						MagicEngine.CurrentEngine.MagicStack.PushOnStack (new AbilityActivation (arg.Source, Exec));
				}else
					return false;
				return true;
			}
		}

		public override string ToString ()
		{
			return string.Format("{0}: {1}", Type.ToString(), Description);
		}

		public static Trigger Parse (string str)
		{
			Trigger t = new Trigger (MagicEventType.Unset);
			string[] tmp = str.Trim ().Split (new char[] { '|' });

			foreach (string i in tmp) {
				string[] f = i.Trim ().Split (new char[] { '$' });
				string data = f [1].Trim ();
				switch (f [0]) {
				case "Mode":
					switch (data) {
					case "ChangesZone":
						t.Type = MagicEventType.ChangeZone;
						break;
					case "Phase":
						t.Type = MagicEventType.BeginPhase;
						break;
					case "Attacks":
						t.Type = MagicEventType.Attack;
						break;
					case "SpellCast":
						t.Type = MagicEventType.CastSpell;
						break;
					default:
						Debug.WriteLine ("Unknown trigger " + f [0] + " value:" + data);
						break;
					}
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
					t.Type = MagicEventType.BeginPhase;
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
	}
}
