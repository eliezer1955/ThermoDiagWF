using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;

namespace ThermoDiagWF
{
    internal class DataBase
    {
        public  SqlConnection db;
       
        public DataBase()
        {
            db = new SqlConnection( "S2Bringup.sqlite3" );
            db.Open();

        }


        public void  Open() 
        {
            db.Open();
        }
        public void Close() 
        {
            db.Close();
        }



    }
}
