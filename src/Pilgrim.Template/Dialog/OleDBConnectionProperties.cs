﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;

namespace Pilgrim.Template.Dialog
{
	public class OleDBConnectionProperties : AdoDotNetConnectionProperties
	{
		public OleDBConnectionProperties()
			: base("System.Data.OleDb")
		{
		}

		public bool DisableProviderSelection
		{
			get
			{
				return _disableProviderSelection;
			}
			set
			{
				_disableProviderSelection = value;
			}
		}

		public override bool IsComplete
		{
			get
			{
				if (!(ConnectionStringBuilder["Provider"] is string) ||
					(ConnectionStringBuilder["Provider"] as string).Length == 0)
				{
					return false;
				}
				return true;
			}
		}

		protected override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection descriptors = base.GetProperties(attributes);
			if (_disableProviderSelection)
			{
				PropertyDescriptor providerDescriptor = descriptors.Find("Provider", true);
				if (providerDescriptor != null)
				{
					int index = descriptors.IndexOf(providerDescriptor);
					PropertyDescriptor[] descriptorArray = new PropertyDescriptor[descriptors.Count];
					descriptors.CopyTo(descriptorArray, 0);
					descriptorArray[index] = new DynamicPropertyDescriptor(providerDescriptor, ReadOnlyAttribute.Yes);
					(descriptorArray[index] as DynamicPropertyDescriptor).CanResetValueHandler = new CanResetValueHandler(CanResetProvider);
					descriptors = new PropertyDescriptorCollection(descriptorArray, true);
				}
			}
			return descriptors;
		}

		/// <summary>
		/// Gets the registered OLE DB providers as an array of ProgIDs.
		/// </summary>
		public static List<string> GetRegisteredProviders()
		{
			// Get the sources rowset for the root OLE DB enumerator
			Type rootEnumeratorType = Type.GetTypeFromCLSID(NativeMethods.CLSID_OLEDB_ENUMERATOR);
			OleDbDataReader dr = OleDbEnumerator.GetEnumerator(rootEnumeratorType);

			// Read the CLSIDs of each data source (not binders or enumerators)
			Dictionary<string, string> sources = new Dictionary<string, string>(); // avoids duplicate entries
			using (dr)
			{
				while (dr.Read())
				{
					int type = (int)dr["SOURCES_TYPE"];
					if (type == NativeMethods.DBSOURCETYPE_DATASOURCE_TDP ||
						type == NativeMethods.DBSOURCETYPE_DATASOURCE_MDP)
					{
						sources[dr["SOURCES_CLSID"] as string] = null;
					}
				}
			} // reader is disposed here

			// Get the ProgID for each data source
			List<string> sourceProgIds = new List<string>(sources.Count);
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("CLSID");
			using (key)
			{
				foreach (KeyValuePair<string, string> source in sources)
				{
					Microsoft.Win32.RegistryKey subKey = key.OpenSubKey(source.Key + "\\ProgID");
					// if this subkey does not exist, ignore it.
					if (subKey != null)
					{
						using (subKey)
						{
							sourceProgIds.Add(subKey.GetValue(null) as string);
						} // subKey is disposed here
					}
				}
			} // key is disposed here

			// Sort the prog ID array by name
			sourceProgIds.Sort();

			// The OLE DB provider for ODBC is not supported by the OLE DB .NET provider, so remove it
			while (sourceProgIds.Contains("MSDASQL.1"))
			{
				sourceProgIds.Remove("MSDASQL.1");
			}

			return sourceProgIds;
		}

		private bool CanResetProvider(object component)
		{
			return false;
		}

		private bool _disableProviderSelection;
	}

	public class OleDBSpecializedConnectionProperties : OleDBConnectionProperties
	{
		public OleDBSpecializedConnectionProperties(string provider)
		{
			Debug.Assert(provider != null);
			_provider = provider;
			LocalReset();
		}

		public override void Reset()
		{
			base.Reset();
			LocalReset();
		}

		protected override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			bool disableProviderSelection = DisableProviderSelection;
			try
			{
				DisableProviderSelection = true;
				return base.GetProperties(attributes);
			}
			finally
			{
				DisableProviderSelection = disableProviderSelection;
			}
		}

		private void LocalReset()
		{
			// Initialize with the selected provider
			this["Provider"] = _provider;
		}

		private string _provider;
	}

	public class OleDBSqlConnectionProperties : OleDBSpecializedConnectionProperties
	{

		public OleDBSqlConnectionProperties()
			: base("SQLOLEDB")
		{
			LocalReset();
		}

		public override void Reset()
		{
			base.Reset();
			LocalReset();
		}

		public override bool IsComplete
		{
			get
			{
				if (!base.IsComplete)
				{
					return false;
				}
				if (!(ConnectionStringBuilder["Data Source"] is string) ||
					(ConnectionStringBuilder["Data Source"] as string).Length == 0)
				{
					return false;
				}
				if ((ConnectionStringBuilder["Integrated Security"] == null ||
					!ConnectionStringBuilder["Integrated Security"].ToString().Equals("SSPI", StringComparison.OrdinalIgnoreCase)) &&
					(!(ConnectionStringBuilder["User ID"] is string) ||
					(ConnectionStringBuilder["User ID"] as string).Length == 0))
				{
					return false;
				}
				return true;
			}
		}

		protected override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection descriptors = base.GetProperties(attributes);
			if (SqlNativeClientRegistered)
			{
				DynamicPropertyDescriptor providerDescriptor = descriptors.Find("Provider", true) as DynamicPropertyDescriptor;
				if (providerDescriptor != null)
				{
					if (!DisableProviderSelection)
					{
						providerDescriptor.SetIsReadOnly(false);
					}
					providerDescriptor.SetConverterType(typeof(SqlProviderConverter));
				}
			}
			return descriptors;
		}

		private void LocalReset()
		{
			// We always start with integrated security turned on
			this["Integrated Security"] = "SSPI";
		}

		public static List<string> SqlNativeClientProviders
		{
			get
			{
				if (_sqlNativeClientProviders == null)
				{
					_sqlNativeClientProviders = new List<string>();

					List<string> providers = GetRegisteredProviders();
					Debug.Assert(providers != null, "provider list is null");
					foreach (string provider in providers)
					{
						if (provider.StartsWith("SQLNCLI"))
						{
							int idx = provider.IndexOf(".");
							if (idx > 0)
							{
								_sqlNativeClientProviders.Add(provider.Substring(0, idx).ToUpperInvariant());
							}
						}
					}

					_sqlNativeClientProviders.Sort();
				}

				Debug.Assert(_sqlNativeClientProviders != null, "Native Client list is null");
				return _sqlNativeClientProviders;
			}
		}

		private static bool SqlNativeClientRegistered
		{
			get
			{
				if (!_gotSqlNativeClientRegistered)
				{
					Microsoft.Win32.RegistryKey key = null;
					try
					{
						_sqlNativeClientRegistered = SqlNativeClientProviders.Count > 0;
					}
					finally
					{
						if (key != null)
						{
							key.Close();
						}
					}
					_gotSqlNativeClientRegistered = true;
				}
				return _sqlNativeClientRegistered;
			}
		}

		private static bool _sqlNativeClientRegistered;
		private static List<string> _sqlNativeClientProviders = null;
		private static bool _gotSqlNativeClientRegistered;

		private class SqlProviderConverter : StringConverter
		{
			public SqlProviderConverter()
			{
			}

			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				return true;
			}

			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
				return true;
			}

			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				List<string> stdCollection = new List<string>();
				stdCollection.Add("SQLOLEDB");

				foreach (string provider in SqlNativeClientProviders)
				{
					stdCollection.Add(provider);
				}

				return new StandardValuesCollection(stdCollection);
			}
		}
	}

	public class OleDBOracleConnectionProperties : OleDBSpecializedConnectionProperties
	{
		public OleDBOracleConnectionProperties()
			: base("MSDAORA")
		{
		}

		public override bool IsComplete
		{
			get
			{
				if (!base.IsComplete)
				{
					return false;
				}
				if (!(ConnectionStringBuilder["Data Source"] is string) ||
					(ConnectionStringBuilder["Data Source"] as string).Length == 0)
				{
					return false;
				}
				if ((!(ConnectionStringBuilder["User ID"] is string) ||
					(ConnectionStringBuilder["User ID"] as string).Length == 0))
				{
					return false;
				}
				return true;
			}
		}
	}

	public class OleDBAccessConnectionProperties : OleDBSpecializedConnectionProperties
	{
		public OleDBAccessConnectionProperties()
			: base("Microsoft.Jet.OLEDB.4.0")
		{
			_userChangedProvider = false;
		}

		public override void Reset()
		{
			base.Reset();
			_userChangedProvider = false;
		}

		public override object this[string propertyName]
		{
			set
			{
				base[propertyName] = value;
				if (String.Equals(propertyName, "Provider", StringComparison.OrdinalIgnoreCase))
				{
					if (value != null && value != DBNull.Value)
					{
						OnProviderChanged(ConnectionStringBuilder, EventArgs.Empty);
					}
					else
					{
						_userChangedProvider = false;
					}
				}
				if (String.Equals(propertyName, "Data Source", StringComparison.Ordinal))
				{
					OnDataSourceChanged(ConnectionStringBuilder, EventArgs.Empty);
				}
			}
		}

		public override void Remove(string propertyName)
		{
			base.Remove(propertyName);
			if (String.Equals(propertyName, "Provider", StringComparison.OrdinalIgnoreCase))
			{
				_userChangedProvider = false;
			}
			if (String.Equals(propertyName, "Data Source", StringComparison.Ordinal))
			{
				OnDataSourceChanged(ConnectionStringBuilder, EventArgs.Empty);
			}
		}

		public override void Reset(string propertyName)
		{
			base.Reset(propertyName);
			if (String.Equals(propertyName, "Provider", StringComparison.OrdinalIgnoreCase))
			{
				_userChangedProvider = false;
			}
			if (String.Equals(propertyName, "Data Source", StringComparison.Ordinal))
			{
				OnDataSourceChanged(ConnectionStringBuilder, EventArgs.Empty);
			}
		}

		public override bool IsComplete
		{
			get
			{
				if (!base.IsComplete)
				{
					return false;
				}
				if (!(ConnectionStringBuilder["Data Source"] is string) ||
					(ConnectionStringBuilder["Data Source"] as string).Length == 0)
				{
					return false;
				}
				return true;
			}
		}

		public override void Test()
		{
			string dataSource = ConnectionStringBuilder["Data Source"] as string;
			if (dataSource == null || dataSource.Length == 0)
			{
				throw new InvalidOperationException(Strings.OleDBAccessConnectionProperties_MustSpecifyDataSource);
			}
			base.Test();
		}

		public override string ToDisplayString()
		{
			string savedPassword = null;
			if (ConnectionStringBuilder.ContainsKey("Jet OLEDB:Database Password") &&
				ConnectionStringBuilder.ShouldSerialize("Jet OLEDB:Database Password"))
			{
				savedPassword = ConnectionStringBuilder["Jet OLEDB:Database Password"] as string;
				ConnectionStringBuilder.Remove("Jet OLEDB:Database Password");
			}
			string displayString = base.ToDisplayString();
			if (savedPassword != null)
			{
				ConnectionStringBuilder["Jet OLEDB:Database Password"] = savedPassword;
			}
			return displayString;
		}

		protected override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection descriptors = base.GetProperties(attributes);
			if (Access12ProviderRegistered)
			{
				DynamicPropertyDescriptor providerDescriptor = descriptors.Find("Provider", true) as DynamicPropertyDescriptor;
				if (providerDescriptor != null)
				{
					if (!DisableProviderSelection)
					{
						providerDescriptor.SetIsReadOnly(false);
					}
					providerDescriptor.SetConverterType(typeof(JetProviderConverter));
					providerDescriptor.AddValueChanged(ConnectionStringBuilder, new EventHandler(OnProviderChanged));
				}
				PropertyDescriptor dataSourceDescriptor = descriptors.Find("DataSource", true);
				if (dataSourceDescriptor != null)
				{
					int index = descriptors.IndexOf(dataSourceDescriptor);
					PropertyDescriptor[] descriptorArray = new PropertyDescriptor[descriptors.Count];
					descriptors.CopyTo(descriptorArray, 0);
					descriptorArray[index] = new DynamicPropertyDescriptor(dataSourceDescriptor);
					descriptorArray[index].AddValueChanged(ConnectionStringBuilder, new EventHandler(OnDataSourceChanged));
					descriptors = new PropertyDescriptorCollection(descriptorArray, true);
				}
			}
			PropertyDescriptor passwordDescriptor = descriptors.Find("Jet OLEDB:Database Password", true);
			if (passwordDescriptor != null)
			{
				int index = descriptors.IndexOf(passwordDescriptor);
				PropertyDescriptor[] descriptorArray = new PropertyDescriptor[descriptors.Count];
				descriptors.CopyTo(descriptorArray, 0);
				descriptorArray[index] = new DynamicPropertyDescriptor(passwordDescriptor, PasswordPropertyTextAttribute.Yes);
				descriptors = new PropertyDescriptorCollection(descriptorArray, true);
			}
			return descriptors;
		}

		private static bool Access12ProviderRegistered
		{
			get
			{
				if (!_gotAccess12ProviderRegistered)
				{
					Microsoft.Win32.RegistryKey key = null;
					try
					{
						key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("Microsoft.ACE.OLEDB.12.0");
						_access12ProviderRegistered = (key != null);
					}
					finally
					{
						if (key != null)
						{
							key.Close();
						}
					}
					_gotAccess12ProviderRegistered = true;
				}
				return _access12ProviderRegistered;
			}
		}
		private static bool _access12ProviderRegistered;
		private static bool _gotAccess12ProviderRegistered;

		private class JetProviderConverter : StringConverter
		{
			public JetProviderConverter()
			{
			}

			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				return true;
			}

			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
				return true;
			}

			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection(new string[] {
					"Microsoft.Jet.OLEDB.4.0", "Microsoft.ACE.OLEDB.12.0"
				});
			}
		}

		private void OnProviderChanged(object sender, EventArgs e)
		{
			if (Access12ProviderRegistered)
			{
				_userChangedProvider = true;
			}
		}

		private void OnDataSourceChanged(object sender, EventArgs e)
		{
			if (Access12ProviderRegistered && !_userChangedProvider)
			{
				string dataSource = this["Data Source"] as string;
				if (dataSource != null)
				{
					dataSource = dataSource.Trim().ToUpperInvariant();
					if (dataSource.EndsWith(".ACCDB", StringComparison.Ordinal))
					{
						base["Provider"] = "Microsoft.ACE.OLEDB.12.0";
					}
					else
					{
						base["Provider"] = "Microsoft.Jet.OLEDB.4.0";
					}
				}
			}
		}

		private bool _userChangedProvider;
	}
}
