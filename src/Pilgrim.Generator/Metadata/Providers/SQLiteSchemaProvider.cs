﻿using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace Pilgrim.Generator.Metadata
{
    class SQLiteSchemaProvider : DbSchemaProvider
    {
        public SQLiteSchemaProvider(MetaContext context) : base(context) { }

        #region ' IDbProvider Members '

        public override IEnumerable<DbRelation> GetConstraints(IList<string> includedTables, IList<string> excludedTables)
        {
            DataTable tbl = GetDTSchemaConstrains();
            DataTable Constraints = GetConnection().GetSchema("ForeignKeys");
            foreach (DataRow contrainRow in Constraints.Rows)
            {
                DataRow constrain = tbl.NewRow();
                if (contrainRow["FKEY_TO_CATALOG"] != DBNull.Value)
                    constrain["PK_TABLE_CATALOG"] = contrainRow["FKEY_TO_CATALOG"];
                if (contrainRow["FKEY_TO_SCHEMA"] != DBNull.Value)
                    constrain["PK_TABLE_SCHEMA"] = contrainRow["FKEY_TO_SCHEMA"];
                constrain["PK_TABLE_NAME"] = contrainRow["FKEY_TO_TABLE"];
                constrain["PK_COLUMN_NAME"] = contrainRow["FKEY_TO_COLUMN"];
                if (contrainRow["TABLE_CATALOG"] != DBNull.Value)
                    constrain["FK_TABLE_CATALOG"] = contrainRow["TABLE_CATALOG"];
                if (contrainRow["TABLE_SCHEMA"] != DBNull.Value)
                    constrain["FK_TABLE_SCHEMA"] = contrainRow["TABLE_SCHEMA"];
                constrain["FK_TABLE_NAME"] = contrainRow["TABLE_NAME"];
                constrain["FK_COLUMN_NAME"] = contrainRow["FKEY_FROM_COLUMN"];
                constrain["FK_ORDINAL_POSITION"] = contrainRow["FKEY_FROM_ORDINAL_POSITION"];
                constrain["FK_NAME"] = contrainRow["CONSTRAINT_NAME"];

                tbl.Rows.Add(constrain);
            }

            return ConstructRelations(tbl.Rows.OfType<DataRow>());
        }

        /// <summary>
        /// For a given database type, returns a closest-match DbType.
        /// According SQLite SQLiteConvert :: internal static DbType TypeNameToDbType(string Name)
        /// </summary>
        /// <param name="providerDbType">The name of the type to match</param>
        /// <returns>DbType the text evaluates to</returns>
        public override DbType GetDbColumnType(string providerDbType)
        {
            switch (providerDbType.ToUpper())
            {
                case "1":
                    return DbType.Binary;

                case "2":
                    return DbType.Byte;

                case "3":
                    return DbType.Boolean;

                case "4":
                    return DbType.Guid;

                case "6":
                    return DbType.DateTime;

                case "7":
                    return DbType.Decimal;

                case "8":
                    return DbType.Double;

                case "10":
                    return DbType.Int16;

                case "11":
                    return DbType.Int32;

                case "12":
                    return DbType.Int64;

                case "15":
                    return DbType.Single;

                case "16":
                    return DbType.String;

                default:
                    return DbType.Object;
            }
        }

        public override IEnumerable<DbTable> GetTables(IList<string> includedTables, IList<string> excludedTables)
        {
            var conn = GetConnection();
            var tbl = conn.GetSchema("Tables");
            DataTable tblViews = conn.GetSchema("Views");

            foreach (DataRow viewRow in tblViews.Rows)
            {
                DataRow tblRow = tbl.NewRow();
                if (viewRow["TABLE_CATALOG"] != DBNull.Value)
                    tblRow["TABLE_CATALOG"] = viewRow["TABLE_CATALOG"];
                if (viewRow["TABLE_SCHEMA"] != DBNull.Value)
                    tblRow["TABLE_SCHEMA"] = viewRow["TABLE_SCHEMA"];
                tblRow["TABLE_NAME"] = viewRow["TABLE_NAME"];
                tblRow["TABLE_TYPE"] = "VIEW";

                tbl.Rows.Add(tblRow);
            }

            return ConstructTables(tbl.Rows.OfType<DataRow>());
        }

        #endregion

    }
}
