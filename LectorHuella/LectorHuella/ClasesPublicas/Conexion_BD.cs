using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasesPublicas
{
    public class Conexion_BD
    {
        public MySqlConnection Conexion;
        
        public string _Usuario = string.Empty;
        public string _Contraseña = string.Empty;


        private string _Area_db = string.Empty;
        private string _Server = string.Empty;
        private string _BaseDatos = string.Empty;
        private string _Puerto = string.Empty;


        #region ConectarDesconectarBD
        public Conexion_BD(string Server, string Usuario, string Contraseña)
        {
            _Server = Server;
            _Usuario = Usuario;
            _Contraseña = Contraseña;
            ConectarLoginDB();
        }

        public Conexion_BD(string Server, string Usuario, string Contraseña, string BaseDatos)
        {
            _Server = Server;
            _Usuario = Usuario;
            _Contraseña = Contraseña;
            _BaseDatos = BaseDatos;
            ConectarDB();
        }

        public Conexion_BD(string Server, string Usuario, string Contraseña, string BaseDatos, string puerto)
        {
            _Server = Server;
            _Usuario = Usuario;
            _Contraseña = Contraseña;
            _BaseDatos = BaseDatos;
            _Puerto = puerto;
            ConectarDB();
        }

        public bool IsConected()
        {
            bool ok = false;
            try
            {
                if (Conexion != null)
                {
                    switch (Conexion.State)
                    {
                        case ConnectionState.Closed:
                        case ConnectionState.Broken:
                        case ConnectionState.Connecting:
                            ok = false;
                            break;
                        case ConnectionState.Open:
                        case ConnectionState.Fetching:
                        case ConnectionState.Executing:
                            ok = true;
                            break;
                    }
                }
                else
                {
                    ok = false;
                }
            }
            catch (Exception)
            {
                ok = false;
            }
            return ok;
        }

        public bool ConectarDB()
        {
            bool bConectar = false;
            int ncount = 0;
        retryConectar:
            try
            {
                Conexion = new MySqlConnection();
                Conexion.ConnectionString = string.Format("server={0};uid={1};password={2};database={3};Convert Zero Datetime=True;Connect Timeout=3", _Server, _Usuario, _Contraseña, _BaseDatos);
                Conexion.Open();
                bConectar = true;
            }
            catch (Exception ex)
            {                            
                if (ncount < 3)
                {
                    ncount++;
                    goto retryConectar;
                }
            }
            return bConectar;
        }

        public bool ConectarLoginDB()
        {
            bool bConectar = false;
            int ncount = 0;
        retryConectar:
            try
            {
                Conexion = new MySqlConnection();
                Conexion.InfoMessage += Conexion_InfoMessage;
                Conexion.ConnectionString = string.Format("server={0};uid={1};password={2};database={3};Convert Zero Datetime=True;Connect Timeout=3;", _Server, _Usuario, _Contraseña, _BaseDatos);
                Conexion.Open();

                bConectar = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                if (ncount < 3)
                {
                    ncount++;
                    goto retryConectar;
                }
            }
            return bConectar;
        }

        private void Conexion_InfoMessage(object sender, MySqlInfoMessageEventArgs args)
        {
            try
            {
                string PathLog = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);               
                foreach (MySqlError X in args.errors)
                {
                   
                }
            }
            catch (Exception)
            {
            }
        }

        public bool DesconectarDB()
        {
            try
            {
                if (Conexion != null)
                {
                    if (Conexion.State != ConnectionState.Closed)
                    {
                        Conexion.Close();
                        Conexion = null;
                    }
                }
            }
            catch (Exception)
            {
                Conexion = null;
            }
            return true;
        }
        #endregion
        // Nos sirve para ejecutar comandos Mysql, Insert, Update, etc
        public bool EjecutarComandoSql(string Sql)
        {
            if (Sql.Length == 0)
            {
                return false;
            }                       
            bool bRespuesta = false;
            int nPeticionesFallidas = 0;
        Reconectar:
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(Sql, Conexion))
                {

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    bRespuesta = true;                  
                }
            }
            catch (Exception ex)
            {               
                if (nPeticionesFallidas < 3)
                {
                    nPeticionesFallidas++;
                    DesconectarDB();
                    ConectarLoginDB();
                    goto Reconectar;
                }
            }
            return bRespuesta;
        }

    }


    

}
