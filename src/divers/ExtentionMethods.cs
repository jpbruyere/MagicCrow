using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;
using System.Reflection;

namespace MagicCrow
{
    public static class ExtentionMethods
    {
        static Random rnd = new Random(1);

        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static T GetRandomItem<T>(this IList<T> list)
        {                         
            return list[(int)(rnd.NextDouble() * list.Count)];                        
        }
    }
	/// <summary>
	/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
	/// Provides a method for performing a deep copy of an object.
	/// Binary Serialization is used to perform the copy.
	/// </summary>
	public static class ObjectCopier
	{
		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T source)
		{
			Type t = source.GetType();

			FieldInfo[] fields = t.GetFields(BindingFlags.Public|BindingFlags.Instance);

			T result = (T)Activator.CreateInstance(t);

			foreach (FieldInfo fi in fields)
			{
				fi.SetValue(result, fi.GetValue(source));
			}

			return result;

		}
	}
}
