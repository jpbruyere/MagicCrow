using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicCrow
{
    public enum CostTypes
    {
        Unset,
        Mana,
        Tap,
        Untap,
        Discard,
        Subtract,
        Sacrifice,
        Exile,
		SubCounter,
        Composite
    }
	[Serializable]
    public class CardCost : Cost
    {
        public int Count = 0;
		public AttributGroup<Target> validTargets = new AttributGroup<Target>();

        public CardCost(CostTypes _type)
            : base(_type)
        { }

		public static CardCost Parse(CostTypes ct, string strct)
		{
			CardCost cc = new CardCost (ct);

			int start = strct.IndexOf("<") + 1;
			int end = strct.IndexOf(">", start);
			string result = strct.Substring(start, end - start);

			string[] tmp = result.Split('/');
			if (tmp.Length != 2)
				throw new Exception("cost card target parsing error");
			if (tmp [0] == "X") {
				//TODO: unknow what to do with this X
			} else {				
				cc.Count = int.Parse (tmp [0]);
			}
			cc.validTargets = CardTarget.ParseTargets (tmp [1]);
			if (cc.CostType == CostTypes.Discard) {
				foreach (CardTarget c in cc.validTargets.OfType<CardTarget>()) {
					c.ValidGroup += CardGroupEnum.Hand;
					c.Controler = ControlerType.You;
				}
			} else {
				foreach (CardTarget c in cc.validTargets.OfType<CardTarget>()) {
					c.ValidGroup += CardGroupEnum.InPlay;
					c.Controler = ControlerType.You;
				}
			}
			return cc;
		}
		/// <summary>
		/// it's not a deep clone.
		/// </summary>
		public override Cost Clone ()
		{
			return new CardCost (CostType) { Count = Count, validTargets = validTargets };
		}
		public override string ToString ()
		{
			string tmp = CostType.ToString () + Count;
			if (validTargets != null)
				tmp += validTargets.ToString ();
			return tmp;
		}
		public override Cost OtherCosts {
			get { return this; }
		}
		public override int RequiredTargetCount { get { return Count; }}
		public override AttributGroup<Target> ValidTargets {
			get { return validTargets; }
			set { validTargets = value;	}
		}

		public override Cost Pay(ref CardInstance ci, CardInstance source){
			CardCost tmp = this.Clone () as CardCost;
			if (ValidTargets != null) {
				foreach (CardTarget t in ValidTargets.OfType<CardTarget>()) {
					if (t.Accept (ci, source)) {
						tmp.Count--;
						if (this.CostType == CostTypes.Discard || this.CostType == CostTypes.Sacrifice )
							ci.ChangeZone (CardGroupEnum.Graveyard);
						ci = null;
						break;
					}
				}
			}else if (this.CostType == CostTypes.Discard) {
				if (ci.CurrentGroup.GroupName == CardGroupEnum.Hand) {
					ci.ChangeZone (CardGroupEnum.Graveyard);
					tmp.Count--;
					ci = null;
				}
			} else if (this.CostType == CostTypes.Sacrifice) {
				if (ci.CurrentGroup.GroupName == CardGroupEnum.InPlay) {
					ci.ChangeZone (CardGroupEnum.Graveyard);
					tmp.Count--;
					ci = null;
				}
			}
			return tmp.Count == 0 ? null : tmp;
		}
    }
	[Serializable]
    public class Cost
    {		

		CostTypes costType;
		public virtual CostTypes CostType {
			get {return costType;}
			set {costType = value;}
		}

        public Cost()
        { }
        public Cost(CostTypes _type)
        {
            CostType = _type;
        }
        
		#region Operators
		public static implicit operator Cost(CostTypes ct)
        {
            return new Cost(ct);
        }
        public static Cost operator +(Cost c1, Cost c2)
        {
            Mana m = c1 as Mana;
            Mana n = c2 as Mana;
            Costs cst1 = c1 as Costs;
            if (cst1 != null)
            {
                for (int i = 0; i < cst1.CostList.Count; i++)
                {
                    Mana cst1m = cst1.CostList[i] as Mana;
                    if (cst1m != null)
                    {
                        if (cst1m.IsSameType(n))
                        {
                            cst1m.count += n.count;
                            return cst1;
                        }
                    }
                }
            
            }

            if (!(m == null || n == null))
            {
                if (m.IsSameType(n))
                {
                    m.count += n.count;
                    return m;
                }
            }

            if (c1 == null)
                if (c2 == null)
                    return null;
                else
                    return c2;
            else if (c2 == null)
                return c1;

            Costs sum = new Costs();
            sum += c1;
            sum += c2;
            return sum;
        }
		public static Cost operator -(Cost c1, Cost c2)
		{
			Mana m = c1 as Mana;
			Mana n = c2 as Mana;
			Costs cst1 = c1 as Costs;
			if (cst1 != null)
			{
				for (int i = 0; i < cst1.CostList.Count; i++)
				{
					Mana cst1m = cst1.CostList[i] as Mana;
					if (cst1m != null)
					{
						if (cst1m.IsSameType(n))
						{
							cst1m.count -= n.count;
							return cst1;
						}
					}
				}

			}

			if (!(m == null || n == null))
			{
				if (m.IsSameType(n))
				{
					m.count += n.count;
					return m;
				}
			}

			if (c1 == null)
			if (c2 == null)
				return null;
			else
				return c2;
			else if (c2 == null)
				return c1;

			Costs sum = new Costs();
			sum += c1;
			sum += c2;
			return sum;
		}
        
		public static bool operator <(Cost c1, Cost c2)
        {
            if (c2 == null)
                return false;
			if (c1 == null) {
				if (c2.CostType == CostTypes.Mana) {
					if ((c2 as Mana).count == 0)
						return false;
				}
				return true;
			}
			Cost right = c2.Clone();
			ManaTypes dominant = right.GetDominantMana ();
            Cost left = c1.Clone();
			left.OrderFirst (dominant);
			right.OrderFirst (dominant);
            Cost result = right.Pay(ref left);
			return result == null || result == CostTypes.Tap ? false : true;
        }
        public static bool operator >(Cost c1, Cost c2)
        {
            if (c1 == null)
                return false;
            Cost left = c2.Clone();
            Cost right = c1.Clone();
            return left.Pay(ref right) == null ? true : false;
        }
		public static bool operator ==(Cost c, CostTypes ct){
			return c == null ? false : c.CostType == ct;
		} 
		public static bool operator !=(Cost c, CostTypes ct){
			return ct == null ? true : c.CostType != ct;
		} 
		#endregion

//		public virtual ManaTypes DominantColor {
//			get {
//				Dictionary<ManaTypes,int> counter = new Dictionary<ManaTypes, int> ();
//				ManaTypes[] colors = Enum.GetValues (typeof(ManaTypes));
//				for (int i = 0; i < colors.Count(); i++) {
//					counter.Add (colors [i], 0);
//				}
//				Costs csts = this as Costs;
//				if (csts != null) {					
//					foreach (Cost co in csts.CostList) {
//						Mana m = co as Mana;
//						if (co == null)
//							continue;
//						counter [m.TypeOfMana] += m.count;
//					}
//				}
//			}
//		}

		static Ability ParseCardCost(EffectType et, string strct)
		{
			Ability ab = new Ability (et);


			return ab;
		}
		public static Cost Parse(string costString)
        {
            if (costString.ToLower() == "no cost")
                return null;
            if (costString.ToLower() == "any")
                return new Mana(ManaTypes.Any);
            if (costString.ToLower() == "combo any")
                return new Mana(ManaTypes.ComboAny);

            Costs sum = new Costs();

            string[] tmp = costString.Split(new char[] { });

            foreach (string c in tmp)
            {
                if (string.IsNullOrWhiteSpace(c))
                    continue;

                switch (c)
                {
                    case "T":
                        sum += new Cost(CostTypes.Tap);
                        continue;
                }

				if (c.StartsWith ("Discard")) {
					sum += CardCost.Parse (CostTypes.Discard, c);
					continue;
				} else if (c.StartsWith ("Sac")) {
					sum += CardCost.Parse (CostTypes.Sacrifice, c);
					continue;
				} else if (c.StartsWith ("SubCounter")) {
					sum += CardCost.Parse (CostTypes.SubCounter, c);
					continue;
				}

                string number = "";
                ManaChoice choice = new ManaChoice();

                for (int i = 0; i < c.Length; i++)
                {
                    if (char.IsDigit(c[i]))
                    {
                        number += c[i];

                        if (i < c.Length - 1)
                            if (char.IsDigit(c[i + 1]))
                                continue;

                        choice += new Mana(int.Parse(number));
                        number = null;
                    }
                    else
                    {
                        switch (c[i])
                        {
                            case 'W':
                                choice += ManaTypes.White;
                                break;
                            case 'R':
                                choice += ManaTypes.Red;
                                break;
                            case 'G':
                                choice += ManaTypes.Green;
                                break;
                            case 'B':
                                choice += ManaTypes.Black;
                                break;
                            case 'U':
                                choice += ManaTypes.Blue;
                                break;
                            case 'P':
                                choice += ManaTypes.Life;
                                break;
                            case 'X':
                                choice += new Mana(-1);
                                break;
                            case '/':
                                continue;
                            default:
                                break;
                        }
                    }
                }

                if (choice.Manas.Count == 1)
                    sum += choice.Manas[0];
                else
                    sum += choice;
            }

            if (sum.CostList.Count == 0)
                return null;
            if (sum.CostList.Count == 1)
                return sum.CostList[0];
            else
                return sum;
            return sum;
        }

		public static bool IsNullOrCountIsZero(Cost c)
		{
			if (c == null)
				return true;
			Mana m = c as Mana;
			if (m == null)
				return false;
			return m.count == 0;
		}
        public virtual bool Contains(Cost c)
        {
            Costs cst = this as Costs;
            if (cst != null)
            {
                foreach (Cost cc in cst.CostList)
                {
                    if (cc.Contains(c))
                        return true;
                }
                return false;
            }
            ManaChoice mc = this as ManaChoice;
            if (mc != null)
            {
                foreach (Mana i in mc.Manas)
                {
                    if (i.Contains(c))
                        return true;
                }
                return false;
            }
            Mana m = this as Mana;
            if (m != null)
            {
                Mana cm = c as Mana;
                if (cm == null)
                    return false;
                if ((cm.TypeOfMana == m.TypeOfMana || 
                    (cm.TypeOfMana == ManaTypes.Colorless && m.TypeOfMana <= ManaTypes.Colorless) ||
                    (m.TypeOfMana == ManaTypes.Colorless && cm.TypeOfMana <= ManaTypes.Colorless) )&&
                    m.count >= cm.count)
                    return true;
                else
                    return false;                
            }               
            return this == c ? true : false;
        }
        public virtual bool Contains(CostTypes ct)
        {
            return this.CostType == ct ? true : false;
        }
		public virtual Cost Pay(ref CardInstance ci, CardInstance source){			
			return this;
		}
        public virtual Cost Pay(ref Cost c)
        {
            Cost result = null;
            if (c != null)
            {
				c.OrderFirst(this.GetDominantMana ());
                Costs cl = c as Costs;
                if (cl != null)
                {
                    for (int i = 0; i < cl.CostList.Count; i++)
                    {
                        Cost cli = cl.CostList[i];
                        if (Pay(ref cli) == null)
                        {
                            cl.CostList.RemoveAt(i);
                            return null;
                        }
                    }
                }

                if (CostType == c.CostType)
                {
                    c = null;
                    return null;
                }
            }
            return this;
        }
        public virtual Cost Clone()
        {
            return new Cost
            {
                CostType = this.CostType
            };
        }
        public override string ToString()
        {
            switch (CostType)
            {
                case CostTypes.Unset:
                case CostTypes.Mana:
                case CostTypes.Composite:
                    return "erreur => " + CostType.ToString();
                case CostTypes.Tap:
                    return "T";
			case CostTypes.Sacrifice:
				return "Sacrifice " + (this as CardCost).validTargets.ToString();
			case CostTypes.Discard:
				return "Discard "+ (this as CardCost).validTargets.ToString();
            }
            return "erreur";
        }
		public virtual void OrderFirst(ManaTypes mt){			
		}
		public virtual ManaTypes GetDominantMana(){
			return ManaTypes.Any;
		}
		public virtual Cost ManaCost {
			get { return null; }
		}
		public virtual Cost OtherCosts {
			get { return null; }
		}
		public virtual int RequiredTargetCount { get { return 0; }}
		public virtual AttributGroup<Target> ValidTargets {
			get { return null; }
			set { throw new NotImplementedException(); }
		}

//		public virtual CardCost[] GetCardCosts ();
//		public virtual CardCost[] GetCostWithoutCardCosts ();
    }
	[Serializable]
    public class Costs : Cost
    {
        public List<Cost> CostList = new List<Cost>();
		public override CostTypes CostType {
			get {
				return CostList.Count == 1 ?
					CostList[0].CostType :
					base.CostType;
			}
			set {
				base.CostType = value;
			}
		}
        public Costs()
            : base(CostTypes.Composite)
        {
        }

        public bool IsSameType(Cost m)
        {
            return m.CostType == this.CostType ? true : false;//??????
        }
        public override bool Contains(Cost c)
        {
            foreach (Cost ct in CostList)
                if (ct.Contains(c))
                    return true;
            return false;
        }
        public override bool Contains(CostTypes t)
        {
            foreach (Cost ct in CostList)
                if (ct.Contains(t))
                    return true;
            return false;
        }
		public override Cost Pay (ref CardInstance ci, CardInstance source)
		{
			Costs tmp = new Costs ();
			int i = 0;
			while (i < CostList.Count) {
				Cost res = CostList [i].Pay (ref ci, source);
				i++;
				if (res != null)
					tmp.CostList.Add (res);
				if (ci == null) {
					break;
				}
			}
			while (i < CostList.Count) {
				tmp.CostList.Add (CostList [i].Clone());
				i++;
			}
			return tmp.CostList.Count == 0 ? null : tmp.CostList.Count == 1 ? tmp.CostList [0] : tmp;
		}
        public override Cost Pay(ref Cost c)
        {			
            if (c.CostType == CostTypes.Composite)
            {
                Costs composite = c as Costs;
                Cost result = this.Clone();
                for (int i = 0; i < composite.CostList.Count; i++)
                {
                    Cost tmp = composite.CostList[i];                                        

                    result = result.Pay(ref tmp);

                    if (tmp == null)
                    {
                        composite.CostList.RemoveAt(i);
                        i--;
                    }

                    if (result == null)
                        return null;

                }
				if ((result as Costs) != null) {
					if ((result as Costs).CostList.Count == 0)
						return null;
				}
                return result;
            }
            else
            {

                for (int i = 0; i < CostList.Count; i++)
                {
                    Cost result = CostList[i].Pay(ref c);
                    if (result == null)
                    {
                        CostList.RemoveAt(i);
                        i--;
                    }
                    else
                        CostList[i] = result;
                }
            }
            return CostList.Count == 0 ? null : this;
        }
        public override Cost Clone()
        {
            Costs c = new Costs();
            foreach (Cost ct in CostList)
                c.CostList.Add(ct.Clone());
            return c;
        }
        public static Costs operator +(Costs sum, Cost c)
        {
            Mana m = c as Mana;
            if (m != null)
            {
                for (int i = 0; i < sum.CostList.Count; i++)
                {
                    Mana mm = sum.CostList[i] as Mana;

                    if (mm == null)
                        continue;

                    if (mm.IsSameType(m))
                    {
                        sum.CostList[i] += m;
                        return sum;
                    }
                }
            }
            sum.CostList.Add(c);
            return sum;
        }

        //public static ManaSum operator +(ManaSum sum, ManaChoice choice)
        //{
        //    if (choice.Manas.Count == 1)
        //        return sum + choice.Manas[0];

        //    for (int i = 0; i < sum.Manas.Count; i++)
        //    {
        //        if (sum.Manas[i].IsSameType(choice))
        //        {
        //            sum.Manas[i] += choice;
        //            return sum;
        //        }
        //    }
        //    sum.Manas.Add(choice);
        //    return sum;
        //}

        public override string ToString()
        {
            if (CostList.Count == 0)
                return "EmptySum";

            string tmp = CostList[0].ToString();

            for (int i = 1; i < CostList.Count; i++)
                tmp += " " + CostList[i].ToString();

            return tmp;
        }

		public override void OrderFirst (ManaTypes mt)
		{
			List<Cost> manas = new List<Cost> ();
			List<Cost> others = new List<Cost> ();
			foreach (Cost c in CostList) {
				if (c is Mana)
					manas.Add (c);
				else
					others.Add (c);
			}
			CostList = manas.OrderBy (m => (m as Mana).TypeOfMana).Concat(others).ToList();
		}
		public override Cost ManaCost {
			get {
				Costs tmp = new Costs() {CostList = CostList.Where (c => c is Mana).ToList()};
				return tmp.CostList.Count == 0 ? null :
					tmp.CostList.Count == 1 ? tmp.CostList [0] : tmp;
			}
		}
		public override Cost OtherCosts {
			get {
				Costs tmp = new Costs () { CostList = CostList.Where (c => !(c is Mana)).ToList () };
				return tmp.CostList.Count == 0 ? null :
					tmp.CostList.Count == 1 ? tmp.CostList [0] : tmp;
			}
		}
		public override int RequiredTargetCount {
			get {
				int tmp = 0;
				foreach (Cost c in CostList)
					tmp += c.RequiredTargetCount;
				return tmp;
			}
		}
		public override AttributGroup<Target> ValidTargets {
			get {
				AttributGroup<Target> tmp = new AttributGroup<Target> (AttributeType.Composite);
				foreach (CardCost cc in CostList.OfType<CardCost>()) {
					tmp += cc.ValidTargets;
				}
				return tmp;
			}
			set { throw new NotImplementedException (); }
		}
		public override ManaTypes GetDominantMana()
		{
			IList<Mana> manas = CostList.OfType<Mana> ().
				Where (m => m.TypeOfMana != ManaTypes.Colorless && m.TypeOfMana != ManaTypes.Life).
				OrderBy (mm => mm.count).ToList();
			return (manas.Count > 0) ? manas.FirstOrDefault ().TypeOfMana : ManaTypes.Colorless;
		}
    }
}
