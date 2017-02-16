//
//  MembersView.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2016 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Crow;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;

namespace MagicCrow
{
	public abstract class VariableContainer : IValueChange
	{
		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged(string MemberName, object _value)
		{
			ValueChanged.Raise(this, new ValueChangeEventArgs(MemberName, _value));
		}
		#endregion

		protected object instance;

		public abstract string Name { get; }
		public abstract object Value { get; set;}
		public abstract string Type { get; }
		public abstract string[] Choices { get; }
	}
	public class FieldContainer : VariableContainer
	{
		FieldInfo fi;
		public override string Name { get { return fi.Name; }}
		public override object Value {
			get { return fi.GetValue(instance); }
			set {
				try {
					if (!fi.FieldType.IsAssignableFrom(value.GetType()) && fi.FieldType != typeof(string)){
						if (fi.FieldType.IsEnum) {
							fi.SetValue (instance, value);
						} else {
							MethodInfo me = fi.FieldType.GetMethod
								("Parse", BindingFlags.Static | BindingFlags.Public,
									System.Type.DefaultBinder, new Type [] {typeof (string)},null);
							fi.SetValue (instance, me.Invoke (null, new object[] { value }));
						}
					}else
						fi.SetValue(instance, value);
				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine ("Error setting field:"+ ex.ToString());
				}
				NotifyValueChanged ("Value", value);
			}
		}
		public override string Type { get { return fi.FieldType.IsEnum ?
				"System.Enum"
					: fi.FieldType.IsGenericType ? fi.FieldType.ToString() : fi.FieldType.FullName; }}
		public override string[] Choices {
			get {
				return Enum.GetNames (fi.FieldType);
			}
		}

		public FieldContainer(FieldInfo prop, object _instance){
			fi = prop;
			instance = _instance;
		}

	}
	public class PropertyContainer : VariableContainer
	{
		PropertyInfo pi;

		public override string Name { get { return pi.Name; }}
		public override object Value {
			get { return pi.GetValue(instance); }
			set {
				try {
					if (!pi.PropertyType.IsAssignableFrom(value.GetType()) && pi.PropertyType != typeof(string)){
						if (pi.PropertyType.IsEnum) {
							pi.SetValue (instance, value);
						} else {
							MethodInfo me = pi.PropertyType.GetMethod
								("Parse", BindingFlags.Static | BindingFlags.Public,
									System.Type.DefaultBinder, new Type [] {typeof (string)},null);
							pi.SetValue (instance, me.Invoke (null, new object[] { value }), null);
						}
					}else
						pi.SetValue(instance, value);
				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine ("Error setting property:"+ ex.ToString());
				}
				NotifyValueChanged ("Value", value);
			}
		}
		public override string Type { get { return pi.PropertyType.IsEnum ?
					"System.Enum"
					: pi.PropertyType.FullName; }}
		public override string[] Choices {
			get {
				return Enum.GetNames (pi.PropertyType);
			}
		}

		public PropertyContainer(PropertyInfo prop, object _instance){
			pi = prop;
			instance = _instance;
		}

	}
	public class MembersView : ListBox
	{		
		object instance;

		[XmlAttributeAttribute][DefaultValue(null)]
		public virtual object Instance {
			get { return instance; }
			set {
				if (instance == value)
					return;
				instance = value;
				NotifyValueChanged ("Instance", instance);

				if (instance == null) {
					Data = null;
					return;
				}

				MemberInfo[] members = instance.GetType ().GetMembers (BindingFlags.Public | BindingFlags.Instance);

				List<VariableContainer> props = new List<VariableContainer> ();
				foreach (MemberInfo m in members) {
					if (m.MemberType == MemberTypes.Property) {
						PropertyInfo pi = m as PropertyInfo;
						if (!pi.CanWrite)
							continue;
						if (pi.GetCustomAttribute (typeof(XmlIgnoreAttribute)) != null)
							continue;
						props.Add (new PropertyContainer (pi, instance));
					} else if (m.MemberType == MemberTypes.Field) {
						FieldInfo fi = m as FieldInfo;
						if (fi.GetCustomAttribute (typeof(XmlIgnoreAttribute)) != null)
							continue;
						props.Add (new FieldContainer (fi, instance));
					}
				}
				Data = props.ToArray ();
			}
		}
		public MembersView () : base()
		{
		}
	}
}
