using System;

namespace MagicCrow
{
	public class MagicEventArg
	{
		public MagicEventType Type;
		public CardInstance Source;
		public Player Player;

		public MagicEventArg ()
		{
		}

		public MagicEventArg (MagicEventType t, CardInstance _card = null)
		{
			Type = t;
			Source = _card;
		}
		public MagicEventArg (MagicEventType t, Player _player)
		{
			Type = t;
			Player = _player;
		}
		public override string ToString ()
		{
			string tmp = "=> " + Type.ToString ();
			if (Source != null)
				tmp += " " + Source.ToString ();
			if (Player != null)
				tmp = Player.ToString () + " " + tmp ;
			return tmp;
		}
	}

	public class PhaseEventArg : MagicEventArg
	{
		public GamePhases Phase;
		public override string ToString ()
		{
			string tmp = Player.ToString() + " => " + Type.ToString () + ": " + Phase.ToString();
			return tmp;
		}
	}
	public class ChangeZoneEventArg : MagicEventArg
	{
		public CardGroupEnum Origine = CardGroupEnum.Any;
		public CardGroupEnum Destination = CardGroupEnum.Any;

		public ChangeZoneEventArg(CardInstance _source, CardGroupEnum _origine, CardGroupEnum _destination)
		{
			Type = MagicEventType.ChangeZone;
			Source = _source;
			Origine = _origine;
			Destination = _destination;
		}
	}
	public class SpellEventArg : MagicEventArg
	{
		public Spell Spell;

		public SpellEventArg (Spell _spell)
		{
			Spell = _spell;
			Type = MagicEventType.CastSpell;
		}
		public override string ToString ()
		{
			return "Cast " + Spell.ToString();
		}
	}

	public class ActivatedAbilityEventArg : MagicEventArg
	{
		public Ability Ability;

		public ActivatedAbilityEventArg(Ability _a, CardInstance _source)
		{
			Type = MagicEventType.ActivateAbility;
			Ability = _a;
			Source = _source;
		}

		public override string ToString ()
		{
			return "Ability: " + Ability.ToString();
		}
	}

	public class DamageEventArg : MagicEventArg
	{
		public Damage Damage;

		public DamageEventArg (Damage _d)
		{
			Type = MagicEventType.Damage;
			Damage = _d;
		}
		public override string ToString ()
		{
			return "Damage: " + Damage.ToString ();
		}
	}

	public class LifeEventArg : MagicEventArg
	{
		public int OldValue;
		public int NewValue;
		public LifeEventArg(Player _player, CardInstance _source, int _oldValue, int _newValue)
		{
			this.Player = _player;
			this.Source = _source;
			this.OldValue = _oldValue;
			this.NewValue = _newValue;
		}
	}
}

