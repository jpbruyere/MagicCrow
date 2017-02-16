using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicCrow
{
	/// <summary>
	/// items list relation, AND, OR, NOR
	/// </summary>
    public enum AttributeType
    {		
		/// <summary>OR</summary>
		Choice,
		/// <summary>AND</summary>
        Composite,
		/// <summary>NOR</summary>
        Exclude
    }
	[Serializable]
	public class AttributGroup<T> : List<T>, IComparable
    {        
        public AttributeType attributeType = AttributeType.Composite;

		public AttributGroup(AttributeType at, params T[] list):base(list)
        {
            attributeType = at;
        }
		public AttributGroup(params T[] list) : base (list){}
		public AttributGroup(){}

		AttributGroup<T> Clone
		{
			get { 
				return new AttributGroup<T> (this.attributeType, this.ToArray());
			}
		}      

		#region operators
		public static implicit operator AttributGroup<T>(T a)
		{
			return new AttributGroup<T> (a);
		}

		public static AttributGroup<T> operator +(AttributGroup<T> ma, T a)
        {
			if (!(ma?.Count > 0))
				return a == null ? null : new AttributGroup<T> (a);
			if (ma.attributeType == AttributeType.Composite) {
				AttributGroup<T> tmp = ma.Clone;
				tmp.Add (a);
				return tmp;
			} else if (a == null)
				return ma.Clone;
			else
				return null;

        }
		public static AttributGroup<T> operator +(AttributGroup<T> ma1, AttributGroup<T> ma2)
		{
			if (ma1 == null)
				return ma2;
			if (ma2 == null)
				return ma1;
			if (ma1.attributeType != ma2.attributeType)
				throw new Exception ("incompatible multiform attribute");
			return new AttributGroup<T> (ma1.attributeType, ma1.Concat (ma2).ToArray());
		}
		public static AttributGroup<T> operator |(AttributGroup<T> ma, T a)
		{
//			if (ma == null)
//				return a == null ? null : new MultiformAttribut<T> (a);
			if (ma.attributeType == AttributeType.Choice) {
				AttributGroup<T> tmp = ma.Clone;
				tmp.Add (a);
				return tmp;
			} else if (a == null)
				return ma.Clone;
			else
				return null;
			
//				new MultiformAttribut<MultiformAttribut<T>>(AttributeType.Choice,
//				ma.Clone, new MultiformAttribut<T> (a));
		}
        public static bool operator ==(AttributGroup<T> a1, AttributGroup<T> a2)
        {
			try {
				foreach (T i in a1)
				{
					foreach (T j in a2)
					{
						if (EqualityComparer<T>.Default.Equals(i, j))
						{
							//if (a1.attributeType == AttributeType.Choice)
							return a1.attributeType == AttributeType.Exclude ? false : true;
						}
						else if (a2.attributeType == AttributeType.Composite && a2.Count > 1)
							return false;
					}

				}
				return a1.attributeType == AttributeType.Exclude ? true : false;
			} catch (Exception ex) {
				return object.Equals(a1,a2);
			}
        }
        public static bool operator !=(AttributGroup<T> a1, AttributGroup<T> a2)
        {
			try {
	            foreach (T i in a1)
	            {
	                foreach (T j in a2)
	                {
	                    if (EqualityComparer<T>.Default.Equals(i, j))
	                    {
	                        if (a2.attributeType == AttributeType.Choice)
	                            return a1.attributeType == AttributeType.Exclude ? true : false;
	                    }
	                    else if (a2.attributeType == AttributeType.Composite && a2.Count > 1)
	                        return true;
	                }

	            }
	            return a1.attributeType == AttributeType.Exclude ? false : true;
			} catch (Exception ex) {
				return !object.Equals(a1,a2);
			}
		}
        public static bool operator ==(AttributGroup<T> a, T v)
        {
			if (a == null)
				return v == null ? true : false;
			foreach (T i in a)
            {
                if (EqualityComparer<T>.Default.Equals(i, v))
                    return true;
            }
            return false;
        }
        public static bool operator !=(AttributGroup<T> a, T v)
        {
			if (a == null)
				return v == null ? false : true;
            foreach (T i in a)
            {
                if (EqualityComparer<T>.Default.Equals(i, v))
                    return false;
            }
            return true;
        }
		public static bool operator >=(AttributGroup<T> a1, AttributGroup<T> a2){
			if (a2 == null)
				return true;
			if (a1 == null)
				return false;
			foreach (T j in a2) {
				if (!a1.Contains (j))
					return false;
			}
			return true;
		}
		public static bool operator <=(AttributGroup<T> a1, AttributGroup<T> a2){
			foreach (T j in a1) {
				if (!a2.Contains (j))
					return false;
			}
			return true;
		}
		#endregion
        
		public override string ToString()
        {
            if (Count == 0)
                return "empty";

            string tmp = "";
            string separator = " ";
            if (attributeType == AttributeType.Choice)
                separator = ",";

            foreach (T i in this)
            {
                tmp += i.ToString() + separator;
            }
            return tmp.Substring(0, tmp.Length - 1);
        }

		#region IComparable implementation

		public int CompareTo (object obj)
		{
			return string.Compare (this.ToString (), obj.ToString ());
		}

		#endregion
    }
}
