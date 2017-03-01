using System;

namespace MagicCrow
{	
	public class SpellEventArg : MagicEventArg
	{
		public Spell Spell;

		public SpellEventArg (Spell _spell)
		{
			Spell = _spell;
			Type = Triggers.Mode.SpellCast;
		}
		public override string ToString ()
		{
			return "Cast " + Spell.ToString();
		}
	}
}

