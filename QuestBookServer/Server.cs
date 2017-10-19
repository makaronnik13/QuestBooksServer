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

        BuyBook("lol", "Book2");

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


    public static int Login(string userName, string password)
    {
        MySqlConnection connection = ConnectBase();
        connection.Open();
        string sql = "SELECT COUNT(Password) AS NmberOfUsers FROM users WHERE Name = " + "'" + userName + "'" + " AND Password = " + "'" + password + "'";
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        connection.Close();
        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            return 1;
        }
        return 0;
    }
    public static void TestBase(string userName)
    {
        MySqlConnection connection = hConnectBase();
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
    public static void GetMoney(string userName)
    {
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();
        sendBytes = Encoding.ASCII.GetBytes(GetMoneyValue(userName).ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
    }
    public static int GetBookId(string bookName)
    {
        string sql = "SELECT * FROM books WHERE Name = " + "'" + bookName + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sqlCom);
        DataTable dt2 = new DataTable();
        dataAdapter2.Fill(dt2);
        var myData2 = dt2.Select();
        connection.Close();

        if (myData2.Length == 0)
        {
            return -1;
        }

        return (int)myData2[0].ItemArray[0];
    }
    public static int GetUserId(string username)
    {
        string sql = "SELECT * FROM users WHERE Name = " + "'" + username + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sqlCom);
        DataTable dt2 = new DataTable();
        dataAdapter2.Fill(dt2);
        var myData2 = dt2.Select();
        connection.Close();

        if (myData2.Length==0)
        {
            return -1;
        }

        return (int)myData2[0].ItemArray[0];
    }
    public static int BookPrice(string bookName)
    {
        string sql = "SELECT Price FROM books WHERE Name = " + "'" + bookName + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sqlCom);
        DataTable dt2 = new DataTable();
        dataAdapter2.Fill(dt2);
        var myData2 = dt2.Select();
        connection.Close();

        if (myData2.Length == 0)
        {
            return -1;
        }

        return (int)myData2[0].ItemArray[0];
    }
    public static void AddMoney(string username, int bookPrice)
    {
        string sql = "UPDATE users SET Money=Money+" + bookPrice + " WHERE Name = " + "'" + username + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        sqlCom.ExecuteNonQuery();
        connection.Close();
    }
    public static int GetMoneyValue(string userName)
    {
        string sql = "SELECT Money FROM users WHERE Name = " + "'" + userName + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);

        MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sqlCom);
        DataTable dt2 = new DataTable();
        dataAdapter2.Fill(dt2);
        var myData2 = dt2.Select();
        connection.Close();

        if (myData2.Length != 0)
        {
            return (int)myData2[0].ItemArray[0];
        }
        return 0;
    }
    public static void SetBookBought(string username, string bookName)
    {

        string sql = "INSERT INTO usersbooks (Users_idUsers, Books_idBooks) VALUES(@userId, @bookId)";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        sqlCom.Parameters.AddWithValue("@userId", GetUserId(username));
        sqlCom.Parameters.AddWithValue("@bookId", GetBookId(bookName));
        sqlCom.ExecuteNonQuery();
        connection.Close();
    }
    public static int BuyBook(string username, string bookName)
    {
        int bookPrice = BookPrice(bookName);
        if (bookPrice <= GetMoneyValue(username))
        {
            AddMoney(username, -bookPrice);

            SetBookBought(username, bookName);

            return 1;
        }
        else
        {
            return 0;
        }
    }
    public static byte[] GetImage(string bookName)
    {
        
        string booksDirrectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books");
        string bookDirrectory = Path.Combine(booksDirrectory, bookName);
        string imagePath = Directory.GetFiles(bookDirrectory).ToList().Find(s => s.EndsWith(".png") || s.EndsWith(".jpg"));
        return File.ReadAllBytes(imagePath);
    }
    public static int SignUp(string userName, string password)
    { 
        MySqlConnection connection = ConnectBase();
        connection.Open();
        string sql = "SELECT COUNT(Password) AS NmberOfUsers FROM users WHERE Name = " + "'" + userName + "'";
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            connection.Close();
            return 0;
        }
        else
        {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (Name, Password, Money) VALUES (@Name, @Password, @Money)";
            command.Parameters.AddWithValue("@Name", userName);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Money", Server.baseMoney);
            command.ExecuteNonQuery();
            connection.Close();
        }
        return 1;
    }
    public static byte[] GetBooksList(string userName)
    {
        byte[] sendBytes = null;
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
        connection.Close();

        return Encoding.ASCII.GetBytes(booksArray.ToString());
    }

    //not released
    public static int BuyBonuce(string username, string bonuceId)
    {
        int bonucePrice = BonucePrice(int.Parse(bonuceId));
        if (bonucePrice <= GetMoneyValue(username))
        {
            AddMoney(username, -bonucePrice);

            if (int.Parse(bonuceId)>3)
            {

            }
            SetBonuceBought(username, bonuceId);

            return 1;
        }
        else
        {
            return 0;
        }
    }

    private static int BonucePrice(int v)
    {
        throw new NotImplementedException();
    }

    private static void SetBonuceBought(string username, string bonuceId)
    {
        throw new NotImplementedException();
    }

    public static byte[] DownloadBook(string bookName)
    {
        return new byte[2];
    }

    private static MySqlConnection ConnectBase()
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
                        case "Money":
                            GetMoney(methodAndParams[1]);
                            break;
                        case "BuyBook":
                            BuyBook(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "BuyBonuce":
                            BuyBonuce(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "DownloadBook":
                            DownloadBook(methodAndParams[1]);
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

    private void DownloadBook(string bookName)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] result = Server.DownloadBook(bookName);
        networkStream.Write(result, 0, result.Length);
        networkStream.Flush();
    }

    private void BuyBonuce(string userName, string bonuceId)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        networkStream.Write(Encoding.ASCII.GetBytes(Server.BuyBonuce(userName, bonuceId).ToString()), 0, Encoding.ASCII.GetBytes(1.ToString()).Length);
        networkStream.Flush();
    }

    private void BuyBook(string userName, string bookName)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        networkStream.Write(Encoding.ASCII.GetBytes(Server.BuyBook(userName,bookName).ToString()), 0, Encoding.ASCII.GetBytes(1.ToString()).Length);
        networkStream.Flush();
    }
    private void GetImage(string bookName)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] result = Server.GetImage(bookName);
        networkStream.Write(result, 0, result.Length);
        networkStream.Flush();
    }
    private void Login(string userName, string password)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] sendBytes = Encoding.ASCII.GetBytes(Server.Login(userName, password).ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();   
    }
    private void SignUp(string userName, string password)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] sendBytes = Encoding.ASCII.GetBytes(Server.SignUp(userName, password).ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
    }
    private void GetBooksList(string userName)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] sendBytes = Server.GetBooksList(userName);
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
    }
    private void GetMoney(string userName)
    {
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] sendBytes = Encoding.ASCII.GetBytes(Server.GetMoneyValue(userName).ToString());
        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
    }

}