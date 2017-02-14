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
	public class MultiformAttribut<T> : IComparable
    {
        public List<T> Values = new List<T>();
        public AttributeType attributeType;

		public MultiformAttribut(AttributeType at, params T[] list)
        {
            attributeType = at;

			foreach (T i in list)
				AddValue (i);		
        }
		public MultiformAttribut(params T[] list)
		{
			attributeType = AttributeType.Composite;

			foreach (T i in list)
				AddValue (i);
		}
		public MultiformAttribut(){}

		MultiformAttribut<T> Clone
		{
			get { 
				return new MultiformAttribut<T> (this.attributeType, this.Values.ToArray());
			}
		}
		void AddValue(T a)
		{
			if (a != null)
				Values.Add (a);
		}

        public int Count
        {
            get { return Values.Count; }
        }
        public T Value
        {
            get
            {
                if (Values.Count == 0)
                    return default(T);
                if (Values.Count == 1)
                    return Values[0];
                //?
                return Values.FirstOrDefault();
            }
            set
            {
                Values.Add(value);
            }
        }
        public bool Contains(T a)
        {
            return Values.Contains(a) ? true : false;
        }

		#region operators
		public static implicit operator MultiformAttribut<T>(T a)
		{
			return new MultiformAttribut<T> (a);
		}

		public static MultiformAttribut<T> operator +(MultiformAttribut<T> ma, T a)
        {
			if (ma == null)
				return a == null ? null : new MultiformAttribut<T> (a);
			if (ma.attributeType == AttributeType.Composite) {
				MultiformAttribut<T> tmp = ma.Clone;
				tmp.AddValue (a);
				return tmp;
			} else if (a == null)
				return ma.Clone;
			else
				return null;

        }
		public static MultiformAttribut<T> operator +(MultiformAttribut<T> ma1, MultiformAttribut<T> ma2)
		{
			if (ma1 == null)
				return ma2;
			if (ma2 == null)
				return ma1;
			if (ma1.attributeType != ma2.attributeType)
				throw new Exception ("incompatible multiform attribute");
			return new MultiformAttribut<T> (ma1.attributeType, ma1.Values.Concat (ma2.Values).ToArray());
		}

//		public static MultiformAttribut<T> operator +(T a, MultiformAttribut<T> ma)
//		{
//			if (ma == null)
//				return a == null ? null : a;
//			return ma.Clone.AddValue (a);
//		}
		public static MultiformAttribut<T> operator |(MultiformAttribut<T> ma, T a)
		{
//			if (ma == null)
//				return a == null ? null : new MultiformAttribut<T> (a);
			if (ma.attributeType == AttributeType.Choice) {
				MultiformAttribut<T> tmp = ma.Clone;
				tmp.AddValue (a);
				return tmp;
			} else if (a == null)
				return ma.Clone;
			else
				return null;
			
//				new MultiformAttribut<MultiformAttribut<T>>(AttributeType.Choice,
//				ma.Clone, new MultiformAttribut<T> (a));
		}
        public static bool operator ==(MultiformAttribut<T> a1, MultiformAttribut<T> a2)
        {
			try {
				foreach (T i in a1.Values)
				{
					foreach (T j in a2.Values)
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
        public static bool operator !=(MultiformAttribut<T> a1, MultiformAttribut<T> a2)
        {
			try {
	            foreach (T i in a1.Values)
	            {
	                foreach (T j in a2.Values)
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
        public static bool operator ==(MultiformAttribut<T> a, T v)
        {
			if (a == null)
				return v == null ? true : false;
			foreach (T i in a.Values)
            {
                if (EqualityComparer<T>.Default.Equals(i, v))
                    return true;
            }
            return false;
        }
        public static bool operator !=(MultiformAttribut<T> a, T v)
        {
			if (a == null)
				return v == null ? false : true;
            foreach (T i in a.Values)
            {
                if (EqualityComparer<T>.Default.Equals(i, v))
                    return false;
            }
            return true;
        }
		public static bool operator >=(MultiformAttribut<T> a1, MultiformAttribut<T> a2){
			if (a2 == null)
				return true;
			if (a1 == null)
				return false;
			foreach (T j in a2.Values) {
				if (!a1.Contains (j))
					return false;
			}
			return true;
		}
		public static bool operator <=(MultiformAttribut<T> a1, MultiformAttribut<T> a2){
			foreach (T j in a1.Values) {
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

            foreach (T i in Values)
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
