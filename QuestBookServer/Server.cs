using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
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
        int result = 0;
        if (Convert.ToInt32(sqlCom.ExecuteScalar()) != 0)
        {
            result = 1;
        }
        connection.Close();
        return result;
    }
    public static void TestBase(string userName)
    {
        MySqlConnection connection = ConnectBase();
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

            int r = Convert.ToInt32(sqlCom3.ExecuteScalar());

            if (r != 0)
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
    public static void SetBonuceBought(string username, string bonuceId)
    {
        switch (int.Parse(bonuceId))
        {
            case 0:
                AddMoney(username, 100);
                break;
            case 1:
                AddMoney(username, 250);
                break;
            case 2:
                AddMoney(username, 500);
                break;
            case 3:
                AddMoney(username, 1000);
                break;
            case 4:
                RemoveAdds(username);
                break;
            case 5:
                MakePremium(username);
                break;
        }
    }
    private static void MakePremium(string username)
    {
        string sql = "UPDATE users SET Premium = 1" + " WHERE Name = " + "'" + username + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        sqlCom.ExecuteNonQuery();
        connection.Close();
    }
    private static void RemoveAdds(string username)
    {
        string sql = "UPDATE users SET NoAdds = 1" + " WHERE Name = " + "'" + username + "'";
        MySqlConnection connection = ConnectBase();
        connection.Open();
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);
        sqlCom.ExecuteNonQuery();
        connection.Close();
    }
    public static int IsPremium(string username)
    {
        string sql = "SELECT Premium FROM users WHERE Name = " + "'" + username + "'";
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
    public static int HasAddBlock(string username)
    {
        string sql = "SELECT NoAdds FROM users WHERE Name = " + "'" + username + "'";
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
    public static byte[] DownloadBook(string bookName)
    {
        string booksDirrectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books");
        string bookDirrectory = Path.Combine(booksDirrectory, bookName);
        string bundlePath = Directory.GetFiles(bookDirrectory).ToList().Find(s => s.EndsWith(".pgb"));
        return File.ReadAllBytes(bundlePath);
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
        Thread ctThread = new Thread(doChat);
        ctThread.Start();
    }

    private void doChat()
    {
        Console.WriteLine("__________________________________");
        int requestCount = 0;
        byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
        string dataFromClient = null;
        requestCount = 0;

        if (!clientSocket.Connected)
        {
            return;
        }
        NetworkStream outStream = clientSocket.GetStream();
        NetworkStream inStream = clientSocket.GetStream();
        byte[] result = new byte[0];

        bool finished = false;

        while (!finished)
        {
            try
            {
                requestCount = requestCount + 1;
                inStream.Read(bytesFrom, 0, bytesFrom.Length);            
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
                    switch (commandName)
                    {
                        case "GetBooksList":
                            result = GetBooksList(methodAndParams[1]);
                            break;
                        case "Login":
                            result = Login(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "SignUp":
                            result = SignUp(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "GetImage":
                            result = GetImage(methodAndParams[1]);
                            break;
                        case "Money":
                            result = GetMoney(methodAndParams[1]);
                            break;
                        case "BuyBook":
                            result = BuyBook(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "BuyBonuce":
                            BuyBonuce(methodAndParams[1], methodAndParams[2]);
                            break;
                        case "DownloadBook":
                            result = DownloadBook(methodAndParams[1]);
                            break;
                        case "IsPremium":
                            result = IsPremium(methodAndParams[1]);
                            break;
                        case "HasAddBlock":
                            result = HasAddBlock(methodAndParams[1]);
                            break;
                    }      
                }

                outStream.Write(result, 0, result.Length);
                outStream.Flush();
                finished = true;
            }
            catch (Exception ex)
            {
                finished = true;
                 //Console.WriteLine(" >> " + ex.ToString());
            }
        }
        clientSocket.Close();
    }

    private byte[] HasAddBlock(string username)
    {
        return Encoding.ASCII.GetBytes(Server.HasAddBlock(username).ToString());
    }

    private byte[] IsPremium(string username)
    {       
        return Encoding.ASCII.GetBytes(Server.IsPremium(username).ToString());
    }

    private byte[] DownloadBook(string bookName)
    {
        return Server.DownloadBook(bookName);
    }

    private void BuyBonuce(string userName, string bonuceId)
    {      
        Server.SetBonuceBought(userName, bonuceId);
    }

    private byte[] BuyBook(string userName, string bookName)
    {    
        return Encoding.ASCII.GetBytes(Server.BuyBook(userName, bookName).ToString());
    }
    private byte[] GetImage(string bookName)
    {
        return Server.GetImage(bookName);
    }
    private byte[] Login(string userName, string password)
    {
        return Encoding.ASCII.GetBytes(Server.Login(userName, password).ToString());
    }
    private byte[] SignUp(string userName, string password)
    {
        return Encoding.ASCII.GetBytes(Server.SignUp(userName, password).ToString());  
    }
    private byte[] GetBooksList(string userName)
    {
        return Server.GetBooksList(userName);
    }
    private byte[] GetMoney(string userName)
    {
        return Encoding.ASCII.GetBytes(Server.GetMoneyValue(userName).ToString());
    }

}