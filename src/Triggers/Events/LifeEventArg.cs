using System;

namespace MagicCrow
{
	public class LifeEventArg : MagicEventArg
	{
		public int OldValue;
		public int NewValue;
		public LifeEventArg(Player _player, CardInstance _source, int _oldValue, int _newValue)
		{
			this.source = _source;
			this.target = _player;
			this.OldValue = _oldValue;
			this.NewValue = _newValue;
		}
	}
}

