using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicCrow
{
    public enum ManaTypes
    {
        White,
        Blue,
        Black,
        Red,
        Green,
        Colorless,
        Life,
        Composite,
        Any,
        ComboAny,
    }

    public class ManaPool : List<Mana>
    {
        public static ManaPool operator +(ManaPool mp, Mana m)
        {
            foreach (Mana i in mp)
            {
                if (i.IsSameType(m))
                {
                    i.count += m.count;
                    return mp;
                }

            }
            mp.Add(m);
            return mp;
        }
        public static ManaPool operator -(ManaPool mp, Mana m)
        {
            for (int i = 0; i < mp.Count; i++)
            {
                if (mp[i].IsSameType(m))
                {
                    if (mp[i].count < m.count)
                        throw new Exception("Not enough mana");
                    mp[i].count -= m.count;
                    
                    if (mp[i].count == 0)
                        mp.RemoveAt(i);

                    return mp;
                }

            }
            mp.Add(m);
            return mp;
        }

    }
	[Serializable]
    public class ManaChoice : Mana
    {
        public ManaChoice()
            : base()
        {
            TypeOfMana = ManaTypes.Composite;
            count = 1;
        }
        public List<Mana> Manas = new List<Mana>();
        public static ManaChoice operator +(ManaChoice choice, Mana m)
        {
            choice.Manas.Add(m);
            return choice;
        }
        public override bool IsSameType(Mana m)
        {
            ManaChoice choice = m as ManaChoice;
            if (choice == null)
                return false;
            if (choice.Manas.Count != Manas.Count)
                return false;

            for (int i = 0; i < Manas.Count; i++)
                if (!choice.Manas[i].IsSameType(Manas[i]))
                    return false;

            return true;
        }
        public override bool Contains(Cost c)
        {
            return base.Contains(c);
        }
        public override string ToString()
        {
            if (Manas.Count == 0)
                return "EmptyChoice";

            string tmp = Manas[0].ToString();

            for (int i = 1; i < Manas.Count; i++)
                tmp += "" + Manas[i].ToString();

            return string.Concat(Enumerable.Repeat(tmp + " ", count)).Trim();
        }
        public override Cost Clone()
        {
            ManaChoice c = new ManaChoice();
            foreach (Cost ct in Manas)
                c.Manas.Add(ct.Clone() as Mana);
            return c;
        }
        public override Cost Pay(ref Cost c)
        {
            for (int i = 0; i < Manas.Count; i++)
            {
                Cost result = Manas[i].Pay(ref c);
                if (result == null)
                    return null;
            }
            return this;
        }
    }
	[Serializable]
    public class Mana : Cost
    {
        public ManaTypes TypeOfMana = ManaTypes.Colorless;
        public int count = 0;

        public Mana()
            : base(CostTypes.Mana)
        {
            TypeOfMana = ManaTypes.Colorless;
            count = 0;
        }
        public Mana(ManaTypes type)
            : base(CostTypes.Mana)
        {
            TypeOfMana = type;
            count = 1;
        }
        public Mana(int x, ManaTypes type)
            : base(CostTypes.Mana)
        {
            count = x;
            TypeOfMana = type;
        }
        public Mana(int x)
            : base(CostTypes.Mana)
        {
            count = x;
            TypeOfMana = ManaTypes.Colorless;
        }
        
		public override Cost Clone()
        {            
            return new Mana(count,TypeOfMana);
        }
        public override Cost Pay(ref Cost c)
        {
			if (c == null)
				return this;
			
			c.OrderFirst(this.GetDominantMana ());
			Costs cl = c as Costs;
			if (cl != null) {
				Cost tmp = this.Clone ();
				for (int i = 0; i < cl.CostList.Count; i++) {
					Cost cli = cl.CostList [i];
					tmp = tmp.Pay (ref cli);
					if (tmp == null)
						break;
				}
				return tmp;
			}

            Mana m = c as Mana;
            
            if (m == null)
                return this;

            if (m is ManaChoice)
            {
                ManaChoice mc = m.Clone() as ManaChoice;                
                
                for (int i = 0; i < mc.Manas.Count; i++)
                {
                    Cost result = this.Clone();
                    Cost tmp = mc.Manas[i];

                    result = result.Pay(ref tmp);

                    if (tmp == null)
                        c = null;

                    if (result == null)
                        return null;

                }
            }
            else
            {

                if (TypeOfMana == m.TypeOfMana || TypeOfMana == ManaTypes.Colorless)
                {
                    count -= m.count;

                    if (count == 0)
                    {
                        c = null;
                        return null;
                    }
                    else if (count < 0)
                    {
                        m.count = -count;
                        return null;
                    }
                    else
                    {
                        c = null;
                        return this;
                    }
                }
            }
            return this;
        }
        public static implicit operator Mana(ManaTypes t)
        {
            return new Mana(t);
        }


        public static bool operator ==(Mana m, ManaTypes mt)
        {
            return m.TypeOfMana == mt && m.count == 1 ? true : false;
        }
        public static bool operator !=(Mana m, ManaTypes mt)
        {
            return m.TypeOfMana == mt && m.count == 1 ? false : true;
        }
        public virtual bool IsSameType(Mana m)
        {
			return m==null ? false : m.TypeOfMana == TypeOfMana && 
				TypeOfMana != ManaTypes.Composite && count > 0 && m.count > 0 ? true : false;
        }
        public override string ToString()
        {
            if (count < 0)
                return "X";

            switch (TypeOfMana)
            {
                case ManaTypes.Colorless:
                    return count.ToString();
                case ManaTypes.White:
                    return string.Concat(Enumerable.Repeat("W ", count)).Trim();
                case ManaTypes.Blue:
                    return string.Concat(Enumerable.Repeat("U ", count)).Trim();
                case ManaTypes.Black:
                    return string.Concat(Enumerable.Repeat("B ", count)).Trim();
                case ManaTypes.Red:
                    return string.Concat(Enumerable.Repeat("R ", count)).Trim();
                case ManaTypes.Green:
                    return string.Concat(Enumerable.Repeat("G ", count)).Trim();
                case ManaTypes.Life:
                    return string.Concat(Enumerable.Repeat("P ", count)).Trim();
            }
            return "error";
        }

		public override ManaTypes GetDominantMana ()
		{
			return TypeOfMana;
		}
    		
	}
}
