using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;

class Server
    {
    static void Main(string[] args)
    {
        TcpListener serverSocket = new TcpListener(8888);
        TcpClient clientSocket = default(TcpClient);
        int counter = 0;

        serverSocket.Start();
        Console.WriteLine(" >> " + "Server Started");

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
                    commandName = commandName.Replace("@","");

                    Console.Write(commandName.Length);

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
        Console.Write("recieving books");
        Byte[] sendBytes = null;
        NetworkStream networkStream = clientSocket.GetStream();

        List<GameInfo> infos = new List<GameInfo>();

        GameInfo fakeGI = new GameInfo("name", "description", 0.3f, 25, 15, "author", "img.png");

        sendBytes = Encoding.ASCII.GetBytes(fakeGI.SaveToJSON().ToString());

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
}