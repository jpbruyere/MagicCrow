using System;

namespace MagicCrow
{
	public class ActivatedAbilityEventArg : MagicEventArg
	{
		public Ability Ability;

		public ActivatedAbilityEventArg(Ability _a, CardInstance _source)
		{
			Type = MagicEventType.ActivateAbility;
			Ability = _a;
			source = _source;
		}

		public override string ToString ()
		{
			return "Ability: " + Ability.ToString();
		}
	}
}

