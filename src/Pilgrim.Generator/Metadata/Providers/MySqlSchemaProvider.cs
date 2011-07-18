﻿using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace Pilgrim.Generator.Metadata
{
    public class MySqlSchemaProvider : DbSchemaProvider
    {
        public MySqlSchemaProvider(MetaContext context) : base(context) { }

        #region ' IDbProvider Members '

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
                tblRow["TABLE_SCHEMA"] = viewRow["TABLE_SCHEMA"];
                tblRow["TABLE_NAME"] = viewRow["TABLE_NAME"];
                tblRow["TABLE_TYPE"] = "VIEW";

                tbl.Rows.Add(tblRow);
            }

            return ConstructTables(tbl.Rows.OfType<DataRow>());
        }

        public override string QualifiedTableName(DbTableName table)
        {
            if (!string.IsNullOrEmpty(table.Schema))
                return string.Format("`{0}`.`{1}`", table.Schema, table.Name);
            else
                return string.Format("`{0}`", table.Name);
        }

        public override DbType GetDbColumnType(string providerDbType)
        {
            switch (providerDbType)
            {
                case "0":   // MySqlDbType.Decimal
                case "246": // MySqlDbType.NewDecimal
                    return DbType.Decimal;

                case "1":   // MySqlDbType.Byte
                    return DbType.SByte;

                case "2":   // MySqlDbType.Int16
                    return DbType.Int16;

                case "3":   // MySqlDbType.Int32
                case "9":   // MySqlDbType.Int24
                    return DbType.Int32;

                case "4":   // MySqlDbType.Float
                    return DbType.Single;

                case "5":   // MySqlDbType.Double
                    return DbType.Double;

                case "7":   // MySqlDbType.Timestamp
                case "12":  // MySqlDbType.DateTime
                    return DbType.DateTime;

                case "8":   // MySqlDbType.Int64
                    return DbType.Int64;

                case "10":  // MySqlDbType.Date
                case "13":  // MySqlDbType.Year
                case "14":  // MySqlDbType.Newdate
                    return DbType.Date;

                case "11":  // MySqlDbType.Time
                    return DbType.Time;

                case "16":  // MySqlDbType.Bit
                case "508": // MySqlDbType.UInt64
                    return DbType.UInt64;

                case "249": // MySqlDbType.TinyBlob
                case "250": // MySqlDbType.MediumBlob
                case "251": // MySqlDbType.LongBlob
                case "252": // MySqlDbType.Blob
                    return DbType.Binary;

                case "254": // MySqlDbType.String
                    return DbType.StringFixedLength;

                case "247": // MySqlDbType.Enum
                    return DbType.String;

                case "248": // MySqlDbType.Set
                case "253": // MySqlDbType.VarChar
                case "750": // MySqlDbType.MediumText
                case "749": // MySqlDbType.TinyText
                case "751": // MySqlDbType.LongText
                case "752": // MySqlDbType.Text
                    return DbType.String;

                case "501": // MySqlDbType.UByte
                    return DbType.Byte;

                case "502": // MySqlDbType.UInt16
                    return DbType.UInt16;

                case "503": // MySqlDbType.UInt32
                case "509": // MySqlDbType.UInt24
                    return DbType.UInt32;

                default:
                    return DbType.AnsiString;
            }
        }

        #endregion

        #region ' SQL code: database constrains '
        //
        // MySQL Server: find database constrains
        //
        const string sqlConstraints =
            "SELECT" +
            "	KCUUC.TABLE_CATALOG AS PK_TABLE_CATALOG," +
            "	KCUUC.TABLE_SCHEMA AS PK_TABLE_SCHEMA," +
            "	KCUUC.TABLE_NAME AS PK_TABLE_NAME," +
            "	KCUUC.COLUMN_NAME AS PK_COLUMN_NAME," +
            "	KCUUC.ORDINAL_POSITION AS PK_ORDINAL_POSITION," +
            "	KCUUC.CONSTRAINT_NAME AS PK_NAME," +
            "	KCUC.TABLE_CATALOG AS FK_TABLE_CATALOG," +
            "	KCUC.TABLE_SCHEMA AS FK_TABLE_SCHEMA," +
            "	KCUC.TABLE_NAME AS FK_TABLE_NAME," +
            "	KCUC.COLUMN_NAME AS FK_COLUMN_NAME," +
            "	KCUC.ORDINAL_POSITION AS FK_ORDINAL_POSITION," +
            "	KCUC.CONSTRAINT_NAME AS FK_NAME " +
            "FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC " +
            "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCUC" +
            "	ON (KCUC.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA" +
            "  AND KCUC.CONSTRAINT_NAME = RC.CONSTRAINT_NAME" +
            "  AND KCUC.TABLE_NAME = RC.TABLE_NAME)" +
            "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCUUC" +
            "	ON (KCUUC.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA" +
            "  AND KCUUC.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME" +
            "  AND KCUUC.TABLE_NAME = RC.REFERENCED_TABLE_NAME)";

        #endregion


        public override IEnumerable<DbRelation> GetConstraints(System.Collections.Generic.IList<string> includedTables, System.Collections.Generic.IList<string> excludedTables)
        {
            DataTable tbl = GetDTSchemaConstrains();

            var conn = GetConnection();
            if (conn.ServerVersion.StartsWith("5."))
            {
                LoadTableWithCommand(tbl, sqlConstraints);
            }
            else
            {
                DataTable tblConstraints = conn.GetSchema("Foreign Key Columns");
                foreach (DataRow constraintRow in tblConstraints.Rows)
                {
                    DataRow constraint = tbl.NewRow();
                    if (constraintRow["REFERENCED_TABLE_CATALOG"] != DBNull.Value)
                        constraint["PK_TABLE_CATALOG"] = constraintRow["REFERENCED_TABLE_CATALOG"];
                    constraint["PK_TABLE_SCHEMA"] = constraintRow["REFERENCED_TABLE_SCHEMA"];
                    constraint["PK_TABLE_NAME"] = constraintRow["REFERENCED_TABLE_NAME"];
                    constraint["PK_COLUMN_NAME"] = constraintRow["REFERENCED_COLUMN_NAME"];
                    //constraint["PK_ORDINAL_POSITION"] = constraintRow[""];
                    //constraint["PK_NAME"] = constraintRow[""];

                    if (constraintRow["TABLE_CATALOG"] != DBNull.Value)
                        constraint["FK_TABLE_CATALOG"] = constraintRow["TABLE_CATALOG"];
                    constraint["FK_TABLE_SCHEMA"] = constraintRow["TABLE_SCHEMA"];
                    constraint["FK_TABLE_NAME"] = constraintRow["TABLE_NAME"];
                    constraint["FK_COLUMN_NAME"] = constraintRow["COLUMN_NAME"];
                    constraint["FK_ORDINAL_POSITION"] = constraintRow["ORDINAL_POSITION"];
                    constraint["FK_NAME"] = constraintRow["CONSTRAINT_NAME"];

                    tbl.Rows.Add(constraint);
                }
            }

            return ConstructRelations(tbl.Rows.OfType<DataRow>());
        }
    }
}
