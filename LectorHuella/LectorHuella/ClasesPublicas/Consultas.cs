using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectorHuella.ClasesPublicas
{
    public class Consultas
    {
        public DataTable Huellas()
        {
            String select = "SELECT id,huella from huellas.huellas;";
            DataTable data = new DataTable();
            MySqlDataAdapter adaptador = new MySqlDataAdapter(select, Main.Instancia._ConexionBD.Conexion);
            adaptador.Fill(data);
            return data;
        }
    }
}
