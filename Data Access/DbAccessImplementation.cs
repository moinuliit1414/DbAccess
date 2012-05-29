using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using DbAccessLinQ;

namespace DbAccess
{
    //Create the class that will define the table to query against.
    public class Products : Db
    {
        public Products(string conn)
            : base(conn)
        {
            TableName = "Products";
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
		
            #region My Reader API
            
	   //Configura la conexi�n a la base de datos en este caso Northwind. ["Query"] es el nombre de mi connectionString.
            var db = new Products("Query");
			
			
			
			
			
	   //Crea una lista de par�metros, mantiene la Consulta lo m�s limpia posible y evitar el (Sql Injection)
            var param = new List<SqlParameter> { new SqlParameter("@CategoryID", 5) };
			
			
			
			
			
	    //Consulta la tabla de products filtrando por CategoryID, luego pasa una lista de Sqlparameters al argumento opcional param y luego convierte el resultado del CurrentTable a un dynamic para poder acceder
	    //A las propiedades en tiempo de ejecuci�n y tener un codigo m�s limpio y legible.
            var products = db.CurrentTable().ToDynamic(); // Select * From Products
			
			
	    //Accede a los miembros de la clase en tiempo de ejecucion: OJO(Tienes que saber cuales son las columnas para que no te de un RuntimeBinderException)
            foreach (var item in products)
            {
                Console.WriteLine("{0}", item.ProductName);
            }
			
			
			
	    //Tambien puedes especificar las  columnas que devuelve CurrentTable, usar Where y Order By.
	    var columns = new List<string> { "ProductID", "ProductName" };
            var products = db.CurrentTable(where: "CategoryID =@CategoryID", orderby: "ProductName", columns: columns, args: param).ToDynamic();

            foreach (var item in products)
            {
                Console.WriteLine("{0}|{1}",item.ProductID, item.ProductName);
            }
			
			
            //Data tables
	    //Tambien puedes retornar un DataTable del reader que invocaste junto
            var dataTable = db.CurrentTable(where: "CategoryID =@CategoryID", args: param).ToDataTable();

            foreach (DataRow item in dataTable.Rows)
            {
                Console.WriteLine("{0}", item["ProductName"]);
            }
			
			
			
			
			
	    //Stored Procedures 
            List<SqlParameter> spParam = new List<SqlParameter>
            {
                new SqlParameter("@ProductID",5),
                new SqlParameter("@ProductName", "Mi nuevo Producto")
            };
            var product = db.ExcecuteStoredProcedure("dbo.usp_UpdateProducts", args: spParam);
        }
    }
}