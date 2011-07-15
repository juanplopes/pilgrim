//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

namespace Pilgrim.Template.Dialog
{
	public class AdoDotNetConnectionProperties : IDataConnectionProperties, ICustomTypeDescriptor
	{
		public AdoDotNetConnectionProperties(string providerName)
		{
			Debug.Assert(providerName != null);
			_providerName = providerName;

			// Create an underlying connection string builder object
			DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);
			Debug.Assert(factory != null);
			_connectionStringBuilder = factory.CreateConnectionStringBuilder();
			Debug.Assert(_connectionStringBuilder != null);
			_connectionStringBuilder.BrowsableConnectionString = false;
		}

		public virtual void Reset()
		{
			_connectionStringBuilder.Clear();
			OnPropertyChanged(EventArgs.Empty);
		}

		public virtual void Parse(string s)
		{
			_connectionStringBuilder.ConnectionString = s;
			OnPropertyChanged(EventArgs.Empty);
		}

		public virtual bool IsExtensible
		{
			get
			{
				return !_connectionStringBuilder.IsFixedSize;
			}
		}

		public virtual void Add(string propertyName)
		{
			if (!_connectionStringBuilder.ContainsKey(propertyName))
			{
				_connectionStringBuilder.Add(propertyName, String.Empty);
				OnPropertyChanged(EventArgs.Empty);
			}
		}

		public virtual bool Contains(string propertyName)
		{
			return _connectionStringBuilder.ContainsKey(propertyName);
		}

		public virtual object this[string propertyName]
		{
			get
			{
				// Property name must not be null
				if (propertyName == null)
				{
					throw new ArgumentNullException("propertyName");
				}

				// If property doesn't exist, just return null
				object testValue = null;
				if (!_connectionStringBuilder.TryGetValue(propertyName, out testValue))
				{
					return null;
				}

				// If property value has been set, return this value
				if (_connectionStringBuilder.ShouldSerialize(propertyName))
				{
					return _connectionStringBuilder[propertyName];
				}

				// Get the property's default value (if any)
				object val = _connectionStringBuilder[propertyName];

				// If a default value exists, return it
				if (val != null)
				{
					return val;
				}

				// No value has been set and no default value exists, so return DBNull.Value
				return DBNull.Value;
			}
			set
			{
				// Property name must not be null
				if (propertyName == null)
				{
					throw new ArgumentNullException("propertyName");
				}

				// Remove the value
				_connectionStringBuilder.Remove(propertyName);

				// Handle cases where value is DBNull.Value
				if (value == DBNull.Value)
				{
					// Leave the property in the reset state
					OnPropertyChanged(EventArgs.Empty);
					return;
				}

				// Get the property's default value (if any)
				object val = null;
				_connectionStringBuilder.TryGetValue(propertyName, out val);

				// Set the value
				_connectionStringBuilder[propertyName] = value;

				// If the value is equal to the default, remove it again
				if (Object.Equals(val, value))
				{
					_connectionStringBuilder.Remove(propertyName);
				}

				OnPropertyChanged(EventArgs.Empty);
			}
		}

		public virtual void Remove(string propertyName)
		{
			if (_connectionStringBuilder.ContainsKey(propertyName))
			{
				_connectionStringBuilder.Remove(propertyName);
				OnPropertyChanged(EventArgs.Empty);
			}
		}

		public event EventHandler PropertyChanged;

		public virtual void Reset(string propertyName)
		{
			if (_connectionStringBuilder.ContainsKey(propertyName))
			{
				_connectionStringBuilder.Remove(propertyName);
				OnPropertyChanged(EventArgs.Empty);
			}
		}

		public virtual bool IsComplete
		{
			get
			{
				return true;
			}
		}

		public virtual void Test()
		{
			string testString = ToTestString();

			// If the connection string is empty, don't even bother testing
			if (testString == null || testString.Length == 0)
			{
				throw new InvalidOperationException(Strings.AdoDotNetConnectionProperties_NoProperties);
			}

			// Create a connection object
			DbConnection connection = null;
			DbProviderFactory factory = DbProviderFactories.GetFactory(_providerName);
			Debug.Assert(factory != null);
			connection = factory.CreateConnection();
			Debug.Assert(connection != null);

			// Try to open it
			try
			{
				connection.ConnectionString = testString;
				connection.Open();
				Inspect(connection);
			}
			finally
			{
				connection.Dispose();
			}
		}

		public override string ToString()
		{
			return ToFullString();
		}

		public virtual string ToFullString()
		{
			return _connectionStringBuilder.ConnectionString;
		}

		public virtual string ToDisplayString()
		{
			PropertyDescriptorCollection sensitiveProperties = GetProperties(new Attribute[] { PasswordPropertyTextAttribute.Yes });
			List<KeyValuePair<string, object>> savedValues = new List<KeyValuePair<string, object>>();
			foreach (PropertyDescriptor sensitiveProperty in sensitiveProperties)
			{
				string propertyName = sensitiveProperty.DisplayName;
				if (ConnectionStringBuilder.ShouldSerialize(propertyName))
				{
					savedValues.Add(new KeyValuePair<string, object>(propertyName, ConnectionStringBuilder[propertyName]));
					ConnectionStringBuilder.Remove(propertyName);
				}
			}
			try
			{
				return ConnectionStringBuilder.ConnectionString;
			}
			finally
			{
				foreach (KeyValuePair<string, object> savedValue in savedValues)
				{
					if (savedValue.Value != null)
					{
						ConnectionStringBuilder[savedValue.Key] = savedValue.Value;
					}
				}
			}
		}

		public DbConnectionStringBuilder ConnectionStringBuilder
		{
			get
			{
				return _connectionStringBuilder;
			}
		}

		protected virtual PropertyDescriptor DefaultProperty
		{
			get
			{
				return TypeDescriptor.GetDefaultProperty(_connectionStringBuilder, true);
			}
		}

		protected virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(_connectionStringBuilder, attributes);
		}

		protected virtual void OnPropertyChanged(EventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		protected virtual string ToTestString()
		{
			return _connectionStringBuilder.ConnectionString;
		}

		protected virtual void Inspect(DbConnection connection)
		{
		}

		#region ICustomTypeDescriptor implementation

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(_connectionStringBuilder, true);
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(_connectionStringBuilder, true);
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(_connectionStringBuilder, true);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(_connectionStringBuilder, editorBaseType, true);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(_connectionStringBuilder, true);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return DefaultProperty;
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return GetProperties(new Attribute[0]);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return GetProperties(attributes);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(_connectionStringBuilder, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(_connectionStringBuilder, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(_connectionStringBuilder, attributes, true);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return _connectionStringBuilder;
		}

		#endregion

		private string _providerName;
		private DbConnectionStringBuilder _connectionStringBuilder;
	}
}
