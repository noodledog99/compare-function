using MySql.Data.MySqlClient;

namespace CompareFunction
{
    public class ConnectDB
    {
        public static readonly string connectionString = @"server=203.151.33.208;port=13306;user=inetsqa;password=jPjo8Su3sJknRDulAxcCMPhxVTdBdW3ZHHM6vJZN;database=custsat;Convert Zero Datetime=true";
        public MySqlConnection con;

        public ConnectDB()
        {
            Initialize();
        }

        public void Initialize()
        {
            con = new MySqlConnection(connectionString);
        }

        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                con.Open();
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
                        System.Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        System.Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                con.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                System.Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}