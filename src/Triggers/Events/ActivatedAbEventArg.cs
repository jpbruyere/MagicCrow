using System;

namespace MagicCrow
{
	public class ActivatedAbilityEventArg : MagicEventArg
	{
		public Abilities.Ability Ability;

		public ActivatedAbilityEventArg(Abilities.Ability _a, CardInstance _source)
		{
			Type = Triggers.Mode.AbilityCast;
			Ability = _a;
			source = _source;
		}

		public override string ToString ()
		{
			return "Ability: " + Ability.ToString();
		}
	}
}

