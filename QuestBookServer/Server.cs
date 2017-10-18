using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;

class Server
{
    public static int baseMoney = 100;

    static void Main(string[] args)
    {  
        TcpListener serverSocket = new TcpListener(8888);
        TcpClient clientSocket = default(TcpClient);
        int counter = 0;

        serverSocket.Start();

        counter = 0;
        while (true)
        {
            counter += 1;
            clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
            handleClinet client = new handleClinet();
            client.startClient(clientSocket, Convert.ToString(counter));
        }

        clientSocket.Close();
        serverSocket.Stop();
        Console.WriteLine(" >> " + "exit");
        Console.ReadLine();
    }

    //tests
    private static void Login(string userName, string password)
    {
        Byte[] sendBytes = null;

        MySqlConnection connection = handleClinet.ConnectBase();
        connection.Open();

        bool result = false;

        string sql = "SELECT COUNT(Password) AS NmberOfUsers FROM users WHERE Name = " +"'"+userName +"'"+ " AND Password = " +"'"+ password+"'";
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);

        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            result = true;
        }

        sendBytes = Encoding.ASCII.GetBytes(new JSONBool(result).ToString());
    }
    private static void TestBase(string userName)
    {
        MySqlConnection connection = handleClinet.ConnectBase();
        List<GameInfo> infos = new List<GameInfo>();
        connection.Open();
        string sql = "SELECT * FROM books"; // Строка запроса  
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        sqlCom.ExecuteNonQuery();
        MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
        DataTable dt = new DataTable();
        dataAdapter.Fill(dt);
        var myData = dt.Select();
        for (int i = 0; i < myData.Length; i++)
        {
            string sql2 = "SELECT Name FROM authors WHERE idAuthors ="+ (int)myData[i].ItemArray[6]; 
            MySqlCommand sqlCom2 = new MySqlCommand(sql2, connection);
            sqlCom2.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sqlCom2);
            DataTable dt2 = new DataTable();
            dataAdapter2.Fill(dt2);
            var myData2 = dt2.Select();

            string sql4 = "SELECT idUsers FROM users WHERE Name = '"+userName+"'";
            MySqlCommand sqlCom4 = new MySqlCommand(sql4, connection);
            sqlCom4.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter4 = new MySqlDataAdapter(sqlCom4);
            DataTable dt4 = new DataTable();
            dataAdapter4.Fill(dt4);
            var myData4 = dt4.Select();

            string sql3 = "SELECT COUNT(idUsersBooks) AS NmberOfBooks FROM usersbooks WHERE books_idBooks = " + (int)myData[i].ItemArray[0] + " AND Users_idUsers = " + (int)myData4[0].ItemArray[0];
            MySqlCommand sqlCom3 = new MySqlCommand(sql3, connection);

            long time = ((DateTime)myData[i].ItemArray[5]).ToFileTime();
            string author = myData2[0].ItemArray[0].ToString();
            GameInfo gi = new GameInfo(myData[i].ItemArray[1].ToString(), myData[i].ItemArray[2].ToString(), (int)myData[i].ItemArray[3], (float)myData[i].ItemArray[4], time, author);
            string booksDirrectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books");
            string bookDirrectory = Path.Combine(booksDirrectory, gi.name); 
            string imagePath = Directory.GetFiles(bookDirrectory).ToList().Find(s => s.EndsWith(".png")|| s.EndsWith(".jpg"));
            
            //gi.imageBytes = File.ReadAllBytes(imagePath);


            if (Convert.ToInt32(sqlCom3.ExecuteScalar())!=0)
            {
                gi.bought = true;
            }
            infos.Add(gi);
        }

        JSONArray booksArray = new JSONArray();
        foreach (GameInfo gi in infos)
        {
            booksArray.Add(gi.SaveToJSON());
        }
    }
    private static void SignUp(string userName, string password)
    {
        MySqlConnection connection = handleClinet.ConnectBase();
        connection.Open();
        int result = 1;
        string sql = "SELECT COUNT(Password) AS NmberOfUsers FROM users WHERE Name = " + "'" + userName + "'";
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);

        Console.WriteLine(Convert.ToInt32(sqlCom.ExecuteScalar()));

        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            result = 0;
        }
        else
        {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (Name, Password, Money) VALUES (@Name, @Password, @Money)";
            command.Parameters.AddWithValue("@Name", userName);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Money", Server.baseMoney);
            command.ExecuteNonQuery();
        }

        Console.WriteLine(result);
        connection.Close();
    }
}

//Class to handle each client request separatly
public class handleClinet
{
    TcpClient clientSocket;
    string clNo;
    public void startClient(TcpClient inClientSocket, string clineNo)
    {
        this.clientSocket = inClientSocket;
        this.clNo = clineNo;

        doChat();

        //return thread after finish debug!!!
        //Thread ctThread = new Thread(doChat);
        //ctThread.Start();
    }
    private void doChat()
    {
        int requestCount = 0;
        byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
        string dataFromClient = null;
        requestCount = 0;
        
        while ((clientSocket.Connected))
        {
            try
            {
                requestCount = requestCount + 1;
                NetworkStream networkStream = clientSocket.GetStream();
                if (!clientSocket.Connected)
                {
                    break;
                }

                networkStream.Read(bytesFrom, 0, bytesFrom.Length);

                

                dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                Console.WriteLine(dataFromClient);

                if (dataFromClient.StartsWith("@"))
                {
                    string[] methodAndParams = dataFromClient.Split(',');
                    string commandName = methodAndParams[0];
                    List<string> methodAndParamsList = methodAndParams.ToList();
                    methodAndParamsList.RemoveAt(0);
                    commandName = commandName.Replace("@", "");
                    Console.WriteLine(commandName);
                    switch (commandName)
                    {
                        case "GetBooksList":
                            GetBooksList(methodAndParams[1]);
                            break;
                        case "Login":
                            Login(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "SignUp":
                            SignUp(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "GetImage":
                            GetImage(methodAndParams[1]);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                 //Console.WriteLine(" >> " + ex.ToString());
            }
        }
    }

    private void GetImage(string bookName)
    {
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();
        MySqlConnection connection = ConnectBase();
        connection.Open();

        string booksDirrectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books");
        string bookDirrectory = Path.Combine(booksDirrectory, bookName);
        string imagePath = Directory.GetFiles(bookDirrectory).ToList().Find(s => s.EndsWith(".png") || s.EndsWith(".jpg"));

        byte[] result = File.ReadAllBytes(imagePath);

        sendBytes = Encoding.ASCII.GetBytes(result.ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
        connection.Close();
    }

    private void Login(string userName, string password)
    {
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();
        MySqlConnection connection = ConnectBase();
        connection.Open();

        int result = 0;

        string sql = "SELECT COUNT(Password) AS NmberOfUsers FROM users WHERE Name = " + "'" + userName + "'" + " AND Password = " + "'" + password + "'";
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);

        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            result = 1;
        }
    
        sendBytes = Encoding.ASCII.GetBytes(result.ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
        connection.Close();
    }

    private void SignUp(string userName, string password)
    {
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();
        MySqlConnection connection = ConnectBase();
        connection.Open();
        int result = 1;
        string sql = "SELECT COUNT(Password) AS NmberOfUsers FROM users WHERE Name = " + "'" + userName + "'";
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);


        Console.WriteLine(sqlCom.ExecuteScalar());

        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            result = 0;
        }
        else
        {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (Name, Password, Money) VALUES (@Name, @Password, @Money)";
            command.Parameters.AddWithValue("@Name", userName);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Money", Server.baseMoney);
            command.ExecuteNonQuery();

            Console.WriteLine("user created");
        }

        sendBytes = Encoding.ASCII.GetBytes(result.ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
        connection.Close();
    }
    private void GetBooksList(string userName)
    {
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();
        List<GameInfo> infos = new List<GameInfo>();
        MySqlConnection connection = ConnectBase();
        
        connection.Open();

        string sql = "SELECT * FROM books"; // Строка запроса  
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        sqlCom.ExecuteNonQuery();
        MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
        DataTable dt = new DataTable();
        dataAdapter.Fill(dt);
        var myData = dt.Select();
        for (int i = 0; i < myData.Length; i++)
        {
            string sql2 = "SELECT Name FROM authors WHERE idAuthors =" + (int)myData[i].ItemArray[6];
            MySqlCommand sqlCom2 = new MySqlCommand(sql2, connection);
            sqlCom2.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sqlCom2);
            DataTable dt2 = new DataTable();
            dataAdapter2.Fill(dt2);
            var myData2 = dt2.Select();

            string sql4 = "SELECT idUsers FROM users WHERE Name = '" + userName + "'";
            MySqlCommand sqlCom4 = new MySqlCommand(sql4, connection);
            sqlCom4.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter4 = new MySqlDataAdapter(sqlCom4);
            DataTable dt4 = new DataTable();
            dataAdapter4.Fill(dt4);
            var myData4 = dt4.Select();

            string sql3 = "SELECT COUNT(idUsersBooks) AS NmberOfBooks FROM usersbooks WHERE books_idBooks = " + (int)myData[i].ItemArray[0] + " AND Users_idUsers = " + (int)myData4[0].ItemArray[0];
            MySqlCommand sqlCom3 = new MySqlCommand(sql3, connection);

            long time = ((DateTime)myData[i].ItemArray[5]).ToFileTime();
            string author = myData2[0].ItemArray[0].ToString();
            GameInfo gi = new GameInfo(myData[i].ItemArray[1].ToString(), myData[i].ItemArray[2].ToString(), (int)myData[i].ItemArray[3], (float)myData[i].ItemArray[4], time, author);
           
            if (Convert.ToInt32(sqlCom3.ExecuteScalar()) != 0)
            {
                gi.bought = true;
            }
            infos.Add(gi);
        }

        JSONArray booksArray = new JSONArray();
        foreach (GameInfo gi in infos)
        {
            booksArray.Add(gi.SaveToJSON());
        }


        sendBytes = Encoding.ASCII.GetBytes(booksArray.ToString());

        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
        connection.Close();
    }

    private void BuyBook(string bookName)
    {

    }

    private void BuyBonus(string bonusId)
    {

    }

    private void RecieveMoney(string value)
    {

    }

    public static MySqlConnection ConnectBase()
    {
        string serverName = "127.0.0.1"; // Адрес сервера (для локальной базы пишите "localhost")
        string userName = "root"; // Имя пользователя
        string dbName = "questbooksdatabase"; //Имя базы данных
        string port = "3306"; // Порт для подключения
        string password = "131094Nik"; // Пароль для подключения
        string connStr = "server=" + serverName +
            ";user=" + userName +
            ";database=" + dbName +
            ";port=" + port +
            ";password=" + password +
            ";SslMode=none;";
        return new MySqlConnection(connStr);
    }
}