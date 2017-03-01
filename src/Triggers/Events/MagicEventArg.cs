using System;

namespace MagicCrow
{
	public class MagicEventArg : EventArgs
	{
		public Triggers.Mode Type;
		public object source;
		public object target;

		public CardInstance SourceCard {
			get { return source as CardInstance; }
		}
		public CardInstance TargetCard {
			get { return target as CardInstance; }
		}
		public Player SourcePlayer {
			get { return source as Player; }
		}
		public Player TargetPlayer {
			get { return target as Player; }
		}

		public MagicEventArg ()
		{
		}
		public MagicEventArg (Triggers.Mode t, object _source = null, object _target = null)
		{
			Type = t;
			source = _source;
			target = _target;
		}
		public override string ToString ()
		{
			string tmp = "=> " + Type.ToString ();
			if (source != null)
				tmp += " " + source.ToString ();
			if (target != null)
				tmp = target.ToString () + " " + tmp ;
			return tmp;
		}
	}
}

