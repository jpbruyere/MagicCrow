using System;
using System.Collections.Generic;

namespace MagicCrow
{
	public struct Cost2
	{
		public CostTypes CostType;
		public int Count;

		public Cost2(CostTypes type)
		{
			CostType = type;
			Count = 0;
		}
	}

//	public struct Mana2 : Cost2
//	{
//		public ManaTypes ManaColor;
//
//		public Mana2()
//			: base(CostTypes.Mana)
//		{
//			ManaColor = ManaTypes.Colorless;
//		}
//		public Mana2(ManaTypes type)
//			: base(CostTypes.Mana)
//		{
//			ManaColor = type;
//			Count = 1;
//		}
//		public Mana2(int x, ManaTypes type)
//			: base(CostTypes.Mana)
//		{
//			Count = x;
//			ManaColor = type;
//		}
//		public Mana2(int x)
//			: base(CostTypes.Mana)
//		{
//			Count = x;
//			ManaColor = ManaTypes.Colorless;
//		}
//		public static implicit operator Mana2(ManaTypes t)
//		{
//			return new Mana2(t);
//		}
//	}
//
//	public struct Costs2
//	{
//		public List<Cost2> CostList = new List<Cost2> ();
//
//	}
//	public struct CostAlternatives
//	{
//	}
}