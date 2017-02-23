using System;

namespace MagicCrow
{
	public class LifeEventArg : MagicEventArg
	{
		public int OldValue;
		public int NewValue;
		public LifeEventArg(Player _player, CardInstance _source, int _oldValue, int _newValue)
		{
			this.Player = _player;
			this.source = _source;
			this.OldValue = _oldValue;
			this.NewValue = _newValue;
		}
	}
}

