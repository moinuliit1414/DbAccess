	/// <summary>
    /// <Author>Fagro Vizcaino Salcedo</Author>
    /// <Year>2012</Year>
	///<Inspirated By>https://github.com/robconery/massive</Inspirated By>
    /// For those who like classic data access with a spice of C# 4.0 using the fastest ADO.NET and 
    /// those who think that ORM tools are sometimes overwhelming, when you just want to access your data in fast way.
    /// </summary>
    public abstract class Db 
    {
        private readonly string _conn;
        public string TableName { get; set; }

        public Db(string connectionStringName)
        {
            _conn = ConfigurationManager.ConnectionStrings["Query"].ConnectionString;
        }

        /// <summary>
        /// Crear un Select y lo ejecuta contra la base de datos teniendo en cuendo los parametros opcionales
        /// </summary>
        /// <param name="where">Especifica el criterio de búsqueda de la consulta</param>
        /// <param name="orderby">Especifica el criterio de order de la consulta</param>
        /// <param name="columns">Especifica la cantidad de columnas que contrendra la consulta</param>
        /// <param name="args">Especifie the argumentos to pass to the query</param>
        /// <returns></returns>
        public IDataReader CurrentTable(string where = "", string orderby = "", List<string> columns = null, List<SqlParameter> args = null)
        {
            string sql = BuildQuery(where, orderby, columns);
            return GetReader(sql, args);
        }

        /// <summary>
        /// Crea un DataReader con la data que devuelve el Stored Procedure que se va a ejecutar.
        /// </summary>
        /// <param name="StoredProcedureName">nombre del Stored Procedure</param>
        /// <param name="args">lista de SqlParameters</param>
        /// <returns></returns>
        public IDataReader ExcecuteStoredProcedure(string StoredProcedureName, List<SqlParameter> args = null)
        {
            return GetReader(StoredProcedureName, args,2);
        }

        private string BuildQuery(string where = "", string orderBy = "", List<string> columns = null)
        {
            var query = "Select ";
            if (columns != null)
                query += string.Join(", ", columns) + " From "+TableName;
            else
                query = "Select * From " + TableName;
            
            if (!string.IsNullOrEmpty(where))
                query += " Where " + where;
            if (!string.IsNullOrEmpty(orderBy))
                query += " Order By " + orderBy; 
            
            return query;
        }


        private IDataReader GetReader(string sql, IEnumerable<SqlParameter> args, int type= 1)
        {
            var conn = new SqlConnection(_conn);
            var cmd = new SqlCommand(sql, conn);
            switch (type)
            {
                case 1: cmd.CommandType = CommandType.Text;
                    break;
                case 2: cmd.CommandType = CommandType.StoredProcedure;
                    break;
            }
            if (args != null)
            {
                foreach (var item in args)
                {
                    cmd.Parameters.AddWithValue(item.ParameterName, item.Value);
                }
            }
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Crea una lista de dynamic con la cual puedes acceder y definir a los miembros
        /// en tiempo de ejecución.
        /// </summary>
        /// <param name="rdr">Un objecto que implemente IDataReader</param>
        /// <returns>Lista de dynamic</returns>
        public static List<dynamic> ToDynamic(this IDataReader rdr)
        {
            var result = new List<dynamic>();
            while (rdr.Read())
            {
                var item = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    item.Add(rdr.GetName(i), rdr[i]);
                }
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Crea un objecto DataTable a partir del objecto que implementa el IDataReader.
        /// </summary>
        /// <param name="rdr">Un objecto que implemente IDataReader</param>
        /// <returns>Un DataTable</returns>
        public static DataTable ToDataTable(this IDataReader rdr)
        {
            DataTable table = new DataTable();
            table.Load(rdr);
            return table;
        }
    }