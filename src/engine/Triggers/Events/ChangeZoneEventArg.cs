using System;

namespace MagicCrow
{	
	public class ChangeZoneEventArg : MagicEventArg
	{
		public CardGroupEnum Origine = CardGroupEnum.Any;
		public CardGroupEnum Destination = CardGroupEnum.Any;

		public ChangeZoneEventArg(CardInstance _source, CardGroupEnum _origine, CardGroupEnum _destination)
		{
			Type = MagicCrow.Triggers.Mode.ChangesZone;
			source = _source;
			Origine = _origine;
			Destination = _destination;
		}
	}
}

