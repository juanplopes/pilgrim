﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Pilgrim.Template.Dialog
{
	public class DataSource
	{
		public const string MicrosoftSqlServerFileName = "MicrosoftSqlServerFile";

		private DataSource()
		{
			_displayName = Strings.DataSource_UnspecifiedDisplayName;
			_providers = new DataProviderCollection(this);
		}

		public DataSource(string name, string displayName)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			_name = name;
			_displayName = displayName;
			_providers = new DataProviderCollection(this);
		}

		public static void AddStandardDataSources(DataConnectionDialog dialog)
		{
			dialog.DataSources.Add(DataSource.SqlDataSource);
			dialog.DataSources.Add(DataSource.SqlFileDataSource);
			dialog.DataSources.Add(DataSource.OracleDataSource);
			dialog.DataSources.Add(DataSource.AccessDataSource);
			dialog.DataSources.Add(DataSource.OdbcDataSource);
			dialog.UnspecifiedDataSource.Providers.Add(DataProvider.SqlDataProvider);
			dialog.UnspecifiedDataSource.Providers.Add(DataProvider.OracleDataProvider);
			dialog.UnspecifiedDataSource.Providers.Add(DataProvider.OleDBDataProvider);
			dialog.UnspecifiedDataSource.Providers.Add(DataProvider.OdbcDataProvider);
			dialog.DataSources.Add(dialog.UnspecifiedDataSource);
		}

		public static DataSource SqlDataSource
		{
			get
			{
				if (_sqlDataSource == null)
				{
					_sqlDataSource = new DataSource("MicrosoftSqlServer", Strings.DataSource_MicrosoftSqlServer);
					_sqlDataSource.Providers.Add(DataProvider.SqlDataProvider);
					_sqlDataSource.Providers.Add(DataProvider.OleDBDataProvider);
					_sqlDataSource.DefaultProvider = DataProvider.SqlDataProvider;
				}
				return _sqlDataSource;
			}
		}
		private static DataSource _sqlDataSource;

		public static DataSource SqlFileDataSource
		{
			get
			{
				if (_sqlFileDataSource == null)
				{
					_sqlFileDataSource = new DataSource("MicrosoftSqlServerFile", Strings.DataSource_MicrosoftSqlServerFile);
					_sqlFileDataSource.Providers.Add(DataProvider.SqlDataProvider);
				}
				return _sqlFileDataSource;
			}
		}
		private static DataSource _sqlFileDataSource;

		public static DataSource OracleDataSource
		{
			get
			{
				if (_oracleDataSource == null)
				{
					_oracleDataSource = new DataSource("Oracle", Strings.DataSource_Oracle);
					_oracleDataSource.Providers.Add(DataProvider.OracleDataProvider);
					_oracleDataSource.Providers.Add(DataProvider.OleDBDataProvider);
					_oracleDataSource.DefaultProvider = DataProvider.OracleDataProvider;
				}
				return _oracleDataSource;
			}
		}
		private static DataSource _oracleDataSource;

		public static DataSource AccessDataSource
		{
			get
			{
				if (_accessDataSource == null)
				{
					_accessDataSource = new DataSource("MicrosoftAccess", Strings.DataSource_MicrosoftAccess);
					_accessDataSource.Providers.Add(DataProvider.OleDBDataProvider);
				}
				return _accessDataSource;
			}
		}
		private static DataSource _accessDataSource;

		public static DataSource OdbcDataSource
		{
			get
			{
				if (_odbcDataSource == null)
				{
					_odbcDataSource = new DataSource("OdbcDsn", Strings.DataSource_MicrosoftOdbcDsn);
					_odbcDataSource.Providers.Add(DataProvider.OdbcDataProvider);
				}
				return _odbcDataSource;
			}
		}
		private static DataSource _odbcDataSource;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public string DisplayName
		{
			get
			{
				return (_displayName != null) ? _displayName : _name;
			}
		}

		public DataProvider DefaultProvider
		{
			get
			{
				switch (_providers.Count)
				{
					case 0:
						Debug.Assert(_defaultProvider == null);
						return null;
					case 1:
						// If there is only one data provider, it must be the default
						IEnumerator<DataProvider> e = _providers.GetEnumerator();
						e.MoveNext();
						return e.Current;
					default:
						return (_name != null) ? _defaultProvider : null;
				}
			}
			set
			{
				if (_providers.Count == 1 && _defaultProvider != value)
				{
					throw new InvalidOperationException(Strings.DataSource_CannotChangeSingleDataProvider);
				}
				if (value != null && !_providers.Contains(value))
				{
					throw new InvalidOperationException(Strings.DataSource_DataProviderNotFound);
				}
				_defaultProvider = value;
			}
		}

		public ICollection<DataProvider> Providers
		{
			get
			{
				return _providers;
			}
		}

		internal static DataSource CreateUnspecified()
		{
			return new DataSource();
		}

		private class DataProviderCollection : ICollection<DataProvider>
		{
			public DataProviderCollection(DataSource source)
			{
				Debug.Assert(source != null);

				_list = new List<DataProvider>();
				_source = source;
			}

			public int Count
			{
				get
				{
					return _list.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}

			public void Add(DataProvider item)
			{
				if (item == null)
				{
					throw new ArgumentNullException("item");
				}
				if (!_list.Contains(item))
				{
					_list.Add(item);
				}
			}

			public bool Contains(DataProvider item)
			{
				return _list.Contains(item);
			}

			public bool Remove(DataProvider item)
			{
				bool result = _list.Remove(item);
				if (item == _source._defaultProvider)
				{
					_source._defaultProvider = null;
				}
				return result;
			}

			public void Clear()
			{
				_list.Clear();
				_source._defaultProvider = null;
			}

			public void CopyTo(DataProvider[] array, int arrayIndex)
			{
				_list.CopyTo(array, arrayIndex);
			}

			public IEnumerator<DataProvider> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			private ICollection<DataProvider> _list;
			private DataSource _source;
		}

		private string _name;
		private string _displayName;
		private DataProvider _defaultProvider;
		private ICollection<DataProvider> _providers;
	}
}
