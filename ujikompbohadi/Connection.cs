using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ujikompbohadi
{
    internal class Connection
    {
        private const string ConnectionString = 
            "Data Source=DESKTOP-3GB66VS\\SQLEXPRESS;Initial Catalog=ujikompbohadi;Integrated Security=True;TrustServerCertificate=true;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);

        }
    }
}
