using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;



namespace laucherQange
{
    class dbConnect
    {
        private static MySqlConnection connection;
        private static string server;
        private static string database;
        private static string uid;
        private static string password;

        private dbConnect()
        {

        }
        /*=============================================
                  Initialisation de la connextion avec les mdps ...
                  systeme de jeton sur la connexion, si deja connecter, retourne l'object.
              =======================================
              */
        public static MySqlConnection initialise()
        {
            if (connection == null)
            {
                server = "localhost";
                database = "versionqanga";
                uid = "root";
                password = "";
                string connectionString;
                connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

                connection = new MySqlConnection(connectionString);
            }

            return connection;

        }

        public static int insertVersion(string version, string type, string hash, string chemin)
        {
            initialise();
            string query;
           
            query = "INSERT INTO version (date, type, version, hash, chemin)  VALUES(NOW(), @type, @version,@hash, @chemin);";
            try {
                if (OpenConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd = connection.CreateCommand();
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@version", version);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@chemin", chemin);
                    cmd.Prepare();
                    object temps = cmd.ExecuteScalar();
                }
                else
                {
                    return 0;
                }
            }
            catch(Exception e)
            {
                return 0;
            }
            finally
            {
                CloseConnection();
            }
            return 1;
        }
        public static string[] selectLastVersion(string type, string version = null)
        {
            initialise();
            string query;

           
            if(version!= null && version != "0.0.0.0.0.0") query = "SELECT chemin, version FROM `version` WHERE `type` = @type AND`version` >@VersionActuel ORDER BY `version` ASC LIMIT 1;";
            else query = "SELECT chemin, version FROM version WHERE type = @type ORDER BY `version` DESC LIMIT 1;";
            try
            {
                if (OpenConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@type", type);
                    if (version!=null)
                    {
                        cmd.Parameters.AddWithValue("@VersionActuel", version);
                        
                    }

                    cmd.Prepare();
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    int cpt = 0;
                    string[] text = new string [2];
                    while (dataReader.Read())
                    {
                     text[cpt]= dataReader[cpt].ToString();
                     text[1] = dataReader[1].ToString();
                        cpt++;
                       
                    }
                    CloseConnection();
                    return text;
                }
                else
                {
                    return new string[]{ "-1"};
                }
            }
            catch (Exception e)
            {
                return new string[] { "-1" };
            }
            return new string[] { "-1" };
        }
        /*=============================================
               Vérification de la connection de l'utilisateur
            =======================================
            */
        private static bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }



        private static bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
