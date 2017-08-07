using CodeGen.AccessModel.Oracle.Properties;
using CodeGen.AccessModel.Oracle.Utils;
using CodeGen.Plugin.Base;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CodeGen.AccessModel.Oracle
{
    /// <summary>
    /// OracleController
    /// </summary>
    public class OracleController : IAccessModelController
    {
        private String _connectionString;

        /// <summary>
        /// Title
        /// </summary>
        public string Title
        {
            get { return "Oracle Access-Model Controller"; }
        }

        /// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy
        {
            get { return ProgramInfo.AssemblyCompany; }
        }

        /// <summary>
        /// Icon
        /// </summary>
        public Image Icon
        {
            get { return null; }
        }

        /// <summary>
        /// Description
        /// </summary>
        public string Description
        {
            get { return "Oracle Access-Model Controller"; }
        }

        /// <summary>
        /// Version
        /// </summary>
        public string Version
        {
            get { return ProgramInfo.AssemblyVersion; }
        }

        /// <summary>
        /// Release Notes Url
        /// </summary>
        public string ReleaseNotesUrl
        {
            get { return Resources.DefaultReleaseNotesUrl; }
        }

        /// <summary>
        /// Author Website Url
        /// </summary>
        public string AuthorWebsiteUrl
        {
            get { return Resources.DefaultAuthorWebsiteUrl; }
        }

        /// <summary>
        /// Settings
        /// </summary>
        public PluginSettings Settings { get; private set; }

        /// <summary>
        /// UpdateSettings
        /// </summary>
        /// <param name="settings"></param>
        public void UpdateSettings(PluginSettings settings)
        {

        }

        /// <summary>
        /// DatabaseTypeCode
        /// </summary>
        public string DatabaseTypeCode
        {
            get { return "Oracle"; }
        }

        /// <summary>
        /// IsLoaded
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// HaveCustomConnectionStringForm
        /// </summary>
        public bool HaveCustomConnectionStringForm
        {
            get { return true; }
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public bool Load(string connectionString)
        {
            _connectionString = connectionString;

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                connection.Close();
            }

            IsLoaded = true;

            return true;
        }

        /// <summary>
        /// ShowGenerateConnectionStringForm
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public bool ShowGenerateConnectionStringForm(out string connectionString)
        {
            FormGenerateConnectionString form = new FormGenerateConnectionString();
            form.LoadLocalVariables();

            if (form.ShowDialog() == DialogResult.OK)
            {
                connectionString = form.GetConnectionString();
                return true;
            }

            connectionString = string.Empty;
            return false;
        }

        /// <summary>
        /// GetTableList
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException">The Controller isn't loaded</exception>
        public List<string> GetTableList()
        {
            if (!IsLoaded)
            {
                throw new ApplicationException("The Controller isn't loaded");
            }

            List<string> tables = new List<string>();

            OracleConnection connection = new OracleConnection(_connectionString);
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM ALL_TABLES;";
                connection.Open();
                OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string tableOwner = reader.GetString(0);
                    string tableName = reader.GetString(1);

                    tables.Add(!string.IsNullOrWhiteSpace(tableOwner) ? tableOwner + "." + tableName : tableName);
                }
                connection.Close();
            }

            tables.Sort();

            return tables;
        }

        /// <summary>
        /// GetEntityInfo
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException">The Controller isn't loaded</exception>
        public DatabaseEntity GetEntityInfo(string tableName)
        {
            if (!IsLoaded)
            {
                throw new ApplicationException("The Controller isn't loaded");
            }

            DatabaseEntity entity = new DatabaseEntity();

            OracleConnection connection = new OracleConnection(_connectionString);

            connection.Open();

            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM ALL_TABLES WHERE TABLE_NAME = '" + tableName + "';";

                DataTable table = new DataTable();
                OracleDataReader reader = command.ExecuteReader();
                table.Load(reader);

                if (table.Rows.Count == 0)
                {
                    throw new DataException("Table not found in database");
                }

                DataRow row = table.Rows[0];

                entity.Qualifier = row["TABLESPACE_NAME"].ToString();
                entity.Owner = row["OWNER"].ToString();
                entity.Name = row["TABLE_NAME"].ToString();
                //entity.Type = row["TABLE_TYPE"].ToString();
            }

            List<string> typesWithPrecision = new List<string> { "varchar2", "char" };

            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM ALL_TAB_COLS WHERE TABLE_NAME = '" + tableName + "';";

                DataTable table = new DataTable();
                OracleDataReader reader = command.ExecuteReader();
                table.Load(reader);

                foreach (DataRow row in table.Rows)
                {
                    string dataType = Convert.ToString(row["DATA_TYPE"]);

                    string fullDataType = typesWithPrecision.Exists(t => t.Equals(dataType, StringComparison.InvariantCultureIgnoreCase)) ? string.Format("{0} ({1})", dataType, row["DATA_LENGTH"]) : dataType;

                    DatabaseEntityField entityField = new DatabaseEntityField
                    {
                        ColumnName = Convert.ToString(row["COLUMN_NAME"]),
                        IsPrimaryKey = false,
                        DataType = Convert.ToInt16(row["DATA_TYPE"]),
                        TypeName = fullDataType.Trim(),
                        SimpleTypeName = dataType,
                        Precision = row["DATA_PRECISION"] != DBNull.Value ? Convert.ToInt32(row["DATA_PRECISION"]) : 0,
                        Length = Convert.ToInt32(row["DATA_LENGTH"]),
                        Scale = row["DATA_SCALE"] != DBNull.Value ? Convert.ToInt16(row["DATA_SCALE"]) : (Int16?)null,
                        //Radix = row["RADIX"] != DBNull.Value ? Convert.ToInt16(row["RADIX"]) : (Int16?)null,
                        IsNullable = row["NULLABLE"].ToString().Equals("Y"),

                    };
                    entity.Fields.Add(entityField);
                }
            }

            return entity;
        }


        /// <summary>
        /// Checks the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public bool CheckConnection(string connectionString)
        {
            try
            {
                return DatabaseUtils.CheckConnectionString(connectionString);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ProcessException(ex);
            }

            return false;
        }
    }
}
