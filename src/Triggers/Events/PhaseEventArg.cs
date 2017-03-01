using System;

namespace MagicCrow
{
	public class PhaseEventArg : MagicEventArg
	{
		public GamePhases Phase;
		public override string ToString ()
		{
			string tmp = source?.ToString() + " => " + Type.ToString () + ": " + Phase.ToString();
			return tmp;
		}
	}
}

