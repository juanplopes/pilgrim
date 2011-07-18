﻿using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace Pilgrim.Generator.Metadata
{
    class PostgreSqlSchemaProvider : DbSchemaProvider
    {
        public PostgreSqlSchemaProvider(MetaContext context) : base(context) { }

        #region ' IDbProvider Members '

        public override IEnumerable<DbTable> GetTables(IList<string> includedTables, IList<string> excludedTables)
        {
            DataTable tbl = GetDTSchemaTables();
            LoadTableWithCommand(tbl, sqlTables);
            return ConstructTables(tbl.Rows.OfType<DataRow>());
        }

        public override IEnumerable<DbRelation> GetConstraints(IList<string> includedTables, IList<string> excludedTables)
        {
            DataTable tbl = new DataTable("Constraints");
            LoadTableWithCommand(tbl, sqlConstraints);
            return ConstructRelations(tbl.Rows.OfType<DataRow>());
        }

        public override DbType GetDbColumnType(string providerDbType)
        {
            switch (providerDbType)
            {
                case "1":	// NpgsqlDbType.Bigint
                    return DbType.Int64;

                case "2":	// NpgsqlDbType.Boolean
                    return DbType.Boolean;

                case "3":	// NpgsqlDbType.Box
                case "5":	// NpgsqlDbType.Circle
                case "10":	// NpgsqlDbType.Line
                case "11":	// NpgsqlDbType.LSeg
                case "14":	// NpgsqlDbType.Path                            
                case "15":	// NpgsqlDbType.Point
                case "16":	// NpgsqlDbType.Polygon
                case "24":	// NpgsqlDbType.Inet
                case "25":	// NpgsqlDbType.Bit
                case "30":	// NpgsqlDbType.Interval
                case "-2147483648":	// NpgsqlDbType.Array
                    return DbType.Object;

                case "4":	// NpgsqlDbType.Bytea
                    return DbType.Binary;

                case "6":	// NpgsqlDbType.Char
                case "29":	// NpgsqlDbType.Oidvector
                    return DbType.String;

                case "7":	// NpgsqlDbType.Date
                    return DbType.Date;

                case "8":	// NpgsqlDbType.Double
                    return DbType.Double;

                case "9":	// NpgsqlDbType.Integer
                    return DbType.Int32;

                case "12":	// NpgsqlDbType.Money
                    return DbType.Currency;

                case "13":	// NpgsqlDbType.Numeric
                    return DbType.Decimal;

                case "17":	// NpgsqlDbType.Real
                    return DbType.Single;

                case "18":	// NpgsqlDbType.Smallint
                    return DbType.Int16;

                case "19":	// NpgsqlDbType.Text
                case "22":	// NpgsqlDbType.Varchar
                case "23":	// NpgsqlDbType.Refcursor
                    return DbType.String;

                case "20":	// NpgsqlDbType.Time
                case "31":	// NpgsqlDbType.TimeTZ
                    return DbType.Time;

                case "21":	// NpgsqlDbType.Timestamp
                case "26":	// NpgsqlDbType.TimestampTZ
                    return DbType.DateTime;

                case "27":	// NpgsqlDbType.Uuid
                    return DbType.Guid;

                case "28":	// NpgsqlDbType.Xml
                    return DbType.Xml;

                default:
                    return DbType.AnsiString;
            }
        }

        public override string QualifiedTableName(DbTableName table)
        {
            if (!string.IsNullOrEmpty(table.Schema))
                return string.Format("{0}.{1}", DoubleQuoteIfNeeded(table.Schema), DoubleQuoteIfNeeded(table.Name));
            else
                return string.Format("{0}", DoubleQuoteIfNeeded(table.Name));
        }

        private string DoubleQuoteIfNeeded(string variable)
        {
            if (variable.IndexOf(' ') > -1)
                return string.Format("\"{0}\"", variable);
            else
                return variable;
        }

        #endregion

        #region ' SQL code: database constrains '
        //
        // PostgreSQL Server: find database constrains
        //
        const string sqlTables =
            "SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE " +
            "FROM INFORMATION_SCHEMA.TABLES " +
            "WHERE (TABLE_SCHEMA <> 'pg_catalog') AND (TABLE_SCHEMA <> 'information_schema') " +
            "ORDER BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME";

        const string sqlProcedures =
            "SELECT SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, ROUTINE_CATALOG, ROUTINE_SCHEMA, ROUTINE_NAME, ROUTINE_TYPE, CREATED, LAST_ALTERED " +
            "FROM INFORMATION_SCHEMA.ROUTINES " +
            "WHERE (SPECIFIC_SCHEMA <> 'pg_catalog') AND (SPECIFIC_SCHEMA <> 'information_schema') " +
            "ORDER BY SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME";

        const string sqlConstraints =
            "SELECT" +
            "	KCUUC.TABLE_CATALOG AS PK_TABLE_CATALOG," +
            "	KCUUC.TABLE_SCHEMA AS PK_TABLE_SCHEMA," +
            "	KCUUC.TABLE_NAME AS PK_TABLE_NAME," +
            "	KCUUC.COLUMN_NAME AS PK_COLUMN_NAME," +
            "	KCUUC.ORDINAL_POSITION AS PK_ORDINAL_POSITION," +
            "	KCUUC.CONSTRAINT_NAME AS PK_NAME, " +
            "	KCUC.TABLE_CATALOG AS FK_TABLE_CATALOG," +
            "	KCUC.TABLE_SCHEMA AS FK_TABLE_SCHEMA," +
            "	KCUC.TABLE_NAME AS FK_TABLE_NAME," +
            "	KCUC.COLUMN_NAME AS FK_COLUMN_NAME," +
            "	KCUC.ORDINAL_POSITION AS FK_ORDINAL_POSITION," +
            "	KCUC.CONSTRAINT_NAME AS FK_NAME " +
            "FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC " +
            "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCUC" +
            "	ON (RC.CONSTRAINT_CATALOG = KCUC.CONSTRAINT_CATALOG" +
            "		AND RC.CONSTRAINT_SCHEMA = KCUC.CONSTRAINT_SCHEMA" +
            "		AND RC.CONSTRAINT_NAME = KCUC.CONSTRAINT_NAME)" +
            "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCUUC" +
            "	ON (RC.UNIQUE_CONSTRAINT_CATALOG = KCUUC.CONSTRAINT_CATALOG" +
            "		AND RC.UNIQUE_CONSTRAINT_SCHEMA = KCUUC.CONSTRAINT_SCHEMA" +
            "		AND RC.UNIQUE_CONSTRAINT_NAME = KCUUC.CONSTRAINT_NAME)";

        #endregion

    }
}
