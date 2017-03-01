using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using GGL;

namespace MagicCrow.Effects
{
    [Serializable]
    public class Effect
    {
		#region enums
		public enum Types
        {
            OneShot,
            Continuous,
            Replacement,
            Prevention,
        }
		public enum Action
		{
			Add,
			Remove,
			Set
		}
		public enum Mode
		{
			Continuous,

			SetCost,
			RaiseCost,
			ReduceCost,

			CantBeCast,
			CantPlayLand,
			CantTarget,
			CantAttack,
			CantAttackUnless,
			CantBeActivated,
			CantBlockUnless,
			ETBTapped,

			PreventDamage,
		}
		#endregion
			
        public Action Act;
        
		#region CTOR
        public Effect() { }
		#endregion
	}

	[Serializable]
	public class KeywordEffect
	{
		public List<EvasionKeyword> Keywords;
	}
	[Serializable]
	public class PointEffect
	{
		public IntegerValue Power;
		public IntegerValue Toughness;
	}
	[Serializable]
	public class TypeEffect
	{
		public List<CardTypes> Types;
	}
	[Serializable]
	public class ColorEffect
	{
		public List<MagicColors> Colors;
	}
}
