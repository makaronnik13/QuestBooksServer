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
    static void Main(string[] args)
    {
        Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        TcpListener serverSocket = new TcpListener(8888);
        TcpClient clientSocket = default(TcpClient);
        int counter = 0;

        serverSocket.Start();
        Console.WriteLine(" >> " + "Server Started");

        TestBase();

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

    private static void TestBase()
    {
        MySqlConnection connection = handleClinet.ConnectBase();
        List<GameInfo> infos = new List<GameInfo>();

        connection.Open();

        string sql = "SELECT * FROM Books"; // Строка запроса  
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);

        sqlCom.ExecuteNonQuery();


        MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);

        DataTable dt = new DataTable();
        dataAdapter.Fill(dt);


        var myData = dt.Select();

        Console.WriteLine(myData.Length);
        for (int i = 0; i < myData.Length; i++)
        {
            infos.Add(new GameInfo(myData[i].ItemArray[1].ToString(), myData[i].ItemArray[2].ToString(), (float)myData[i].ItemArray[3], (float)myData[i].ItemArray[4], (int)myData[i].ItemArray[5], myData[i].ItemArray[6].ToString()));
        }

        JSONArray booksArray = new JSONArray();
        foreach (GameInfo gi in infos)
        {
            booksArray.Add(gi.SaveToJSON());
        }

        Console.WriteLine(booksArray.ToString());

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
                            GetBooksList();
                            break;
                    }
                    // MethodInfo theMethod = typeof(handleClinet).GetMethod(commandName);

                    // Console.Write(theMethod);
                    // Console.Write(theMethod.Name);

                    // theMethod.Invoke(this, methodAndParamsList.ToArray());
                }

            }
            catch (Exception ex)
            {
                // Console.WriteLine(" >> " + ex.ToString());
            }
        }
    }

    private void GetBooksList()
    {
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();
        List<GameInfo> infos = new List<GameInfo>();
        MySqlConnection connection = ConnectBase();
        
        connection.Open();

       
        string sql = "SELECT * FROM Books"; // Строка запроса  
        MySqlCommand sqlCom = new MySqlCommand(sql, connection);

       
        sqlCom.ExecuteNonQuery();

      
        MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
        
        DataTable dt = new DataTable();
        dataAdapter.Fill(dt);
       

        var myData = dt.Select();

        Console.WriteLine(myData.Length);
        for (int i = 0; i < myData.Length; i++)
        {
            infos.Add(new GameInfo(myData[i].ItemArray[1].ToString(), myData[i].ItemArray[2].ToString(), (float)myData[i].ItemArray[3], (float)myData[i].ItemArray[4], (int)myData[i].ItemArray[5], myData[i].ItemArray[6].ToString()));
        }

        JSONArray booksArray = new JSONArray();
        foreach (GameInfo gi in infos)
        {
            
           // gi.imageBytes = File.ReadAllBytes();
            booksArray.Add(gi.SaveToJSON());
        }

        sendBytes = Encoding.ASCII.GetBytes(booksArray.ToString());

        networkStream.Write(sendBytes, 0, sendBytes.Length);
        networkStream.Flush();
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

        Console.Write(connStr);
        return new MySqlConnection(connStr);
    }
}