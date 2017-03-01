using System;

namespace MagicCrow
{
	public class DamageEventArg : MagicEventArg
	{
		public Damage Damage;

		public DamageEventArg (Damage _d)
		{
			Type = Triggers.Mode.DamageDone;
			Damage = _d;
		}
		public DamageEventArg (Triggers.Mode evtType, Damage _d)
		{
			Type = evtType;
			Damage = _d;
		}
		public override string ToString ()
		{
			return "Damage: " + Damage.ToString ();
		}
	}
}

