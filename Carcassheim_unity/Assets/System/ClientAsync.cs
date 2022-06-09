using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using ClassLibrary;
using UnityEngine;
using Assets.System;
using System.Threading.Tasks;


/// <summary>
///     State object for receiving data from remote device.
/// </summary>
public class StateObject
{
    /// <summary>
    ///    Représente le <see cref="Socket" /> du joueur.
    /// </summary>
    /// <value>Par Défaut = null</value>
    /// <returns> <see cref="Socket" /> </returns>
    public Socket workSocket = null;

    /// <summary>
    ///     Représente la taille maximale du buffeur. 
    /// </summary>
    /// <value>Par Défaut = <see cref="Packet.MaxPacketSize" /></value>
    /// <returns> const <see cref="int" /> </returns>
    public const int BufferSize = Packet.MaxPacketSize;

    /// <summary>
    ///     Représente le buffeur. 
    /// </summary>
    /// <value>Par Défaut = <see cref="byte"/>[<see cref="BufferSize"/>] </value>
    /// <returns> <see cref="byte"/>[] </returns>
    public byte[] buffer = new byte[BufferSize];
    // Received data string.

    /// <summary>
    ///     Représente la chaine de caractère reçu.
    /// </summary>
    /// <value>Par Défaut = <see cref="StringBuilder"/> </value>
    /// <returns> <see cref="StringBuilder"/> </returns>
    public StringBuilder sb = new StringBuilder();

    /// <summary>
    ///     Représente la liste des Packet.
    /// </summary>
    /// <value>Par Défaut = null </value>
    /// <returns> <see cref="List"/> de <see cref="Packet"/> </returns>
    public List<Packet> Packets { get; set; }

    /// <summary>
    ///     Représente la liste des données.
    /// </summary>
    /// <value>Par Défaut = <see cref="Array.Empty"/> </value>
    /// <returns> <see cref="string"/>[] </returns>
    public string[] Data { get; set; } = Array.Empty<string>();
}

/// <summary>
///     Cette class permet de stocker l'adresse ip et le port.
/// </summary>
[Serializable]
public class Parameters
{
    /// <summary>
    ///     Le port du Serveur.
    /// </summary>
    /// <returns> <see cref="int"/> </returns>
    public int ServerPort { get; set; }

    /// <summary>
    ///     L'adresse du Serveur.
    /// </summary>
    /// <returns> <see cref="string"/> </returns>
    public string ServerIP { get; set; } = "";
}

/// <summary>
///     Cette class permet de comminiquer de manière asynchrone.
/// </summary>
/// <remarks>
///     Elle doit être utilisé avec <see cref="Communication"/>.
/// </remarks>
public class ClientAsync
{
    /// <summary>
    ///     Attribut pour mettre en attente lors d'une connexion.
    /// </summary>
    /// <value>Par Défaut = <see cref="ManualResetEvent" />(false)</value>
    /// <returns> <see cref="ManualResetEvent" /> </returns>
    public static ManualResetEvent connectDone = new ManualResetEvent(false);

    /// <summary>
    ///     Attribut pour mettre en attente lors de la lecture loop.
    /// </summary>
    /// <value>Par Défaut = <see cref="ManualResetEvent" />(false)</value>
    /// <returns> <see cref="ManualResetEvent" /> </returns>
    private static ManualResetEvent receiveDone = new ManualResetEvent(false);

    /// <summary>
    ///     Attribut permet d'avoir la forme des pointeurs de fonction 
    ///     qu'il faut utiliser pour la communication.
    /// </summary>
    public delegate void OnPacketReceivedHandler(object sender, Packet packet);

    /// <summary>
    ///     Attribut permet d'avoir les pointeurs de fonction des class qui souhaite communiquer.
    /// </summary>
    /// <returns> <see cref="event" /> </returns>
    public static event OnPacketReceivedHandler OnPacketReceived;

    /// <summary>
    ///     Attribut qui permet de gérer <see cref="ReceiveLoop(Socket)"/> le début et la fin.
    /// </summary>
    /// <value>Par Défaut = false</value>
    /// <returns> <see cref="bool" /> </returns>
    private static bool mustLoop = false;

    /// <summary>
    ///     Permet d'initialiser une connection TCP asynchrone avec le Serveur.
    /// </summary>
    /// <remarks>
    ///     Le socket créé est placé dans <see cref="Communication.lesSockets"/>.
    /// </remarks>
    /// <param name="parameters"> L'objet avec l'adresse ip et le port du Serveur. </param>
    public static void Connection(Parameters parameters)
    {
        connectDone.Reset();

        //Version : Unity
        IPAddress ipAddress = IPAddress.Parse(parameters.ServerIP);
        var remoteEP = new IPEndPoint(ipAddress, parameters.ServerPort);

        // Create a TCP/IP socket.
        Socket clientSocket = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        Communication.Instance.LeSocket = clientSocket;

        // Connect to the remote endpoint.
        clientSocket.BeginConnect(remoteEP,
            new AsyncCallback(ConnectCallback), clientSocket);
    }

    /// <summary>
    ///     Méthode utilisé par <see cref="Connection"/>.
    /// </summary>
    /// <param name="ar"> L'objet asynchrone. </param>
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.
            client.EndConnect(ar);

            Debug.Log("Client is connected to {0} " + client.RemoteEndPoint);

            // Signal that the connection has been made.
            connectDone.Set();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    /// <summary>
    ///     Permet lancer une déconnexion asynchrone avec le Serveur.
    /// </summary>
    /// <param name="clientSocket"> Le <see cref="Socket" /> a utiliser. </param>
    public static void Disconnection(Socket clientSocket)
    {
        try
        {
            Debug.Log("Client disconnected from {0} " + clientSocket.RemoteEndPoint);

            // Complete the disconnect request.
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    /// <summary>
    ///     Permet d'initialiser une écoute asynchrone.
    /// </summary>
    /// <param name="clientSocket"> Le <see cref="Socket" /> a utiliser. </param>
    public static void Receive(Socket clientSocket)
    {
        try
        {
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = clientSocket;

            Communication.Instance.IsListening = true;

            // Begin receiving the data from the remote device.
            clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            Communication.Instance.IsListening = false;
        }
    }

    /// <summary>
    ///     Méthode utilisé par <see cref="Receive"/>.
    /// </summary>
    /// <param name="ar"> L'objet asynchrone. </param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);
            if (bytesRead <= 0)
            {
                mustLoop = false;
                return;
            }

            // Get the last received bytes.
            var packetAsBytes = new byte[bytesRead];
            Array.Copy(state.buffer, packetAsBytes, bytesRead);
            var error_value = Tools.Errors.None;

            // Deserialize the byte array
            state.Packets = packetAsBytes.ByteArrayToPacket(ref error_value);
            if (error_value != Tools.Errors.None)
            {
                Debug.LogError("[ERREUR] : Received PacketToByteArray not None : " + error_value);
                return;
            }

            var tasks = new List<Task>();
            foreach (var packet in state.Packets)
            {
                var debug = "Reading from : " + client.RemoteEndPoint +
                        "\n\t Read {" + client.RemoteEndPoint + "} bytes =>\t" + packet +
                        "\n\t Data buffer =>\t\t" + string.Join(" ", state.Data);

                Debug.Log(debug);

                tasks.Add(Task.Run(() => OnPacketReceived?.Invoke(typeof(ClientAsync), packet)));
            }

            Task.WhenAll(tasks).Wait();
            state.Packets.Clear();

            Communication.Instance.IsListening = false;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            Communication.Instance.IsListening = false;
        }
    }

    /// <summary>
    ///     Permet d'initialiser une écoute en boucle asynchrone.
    /// </summary>
    /// <param name="clientSocket"> Le <see cref="Socket" /> a utiliser. </param>
    public static void ReceiveLoop(Socket clientSocket)
    {
        try
        {
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = clientSocket;

            mustLoop = true;
            Communication.Instance.IsListening = true;

            while (mustLoop)
            {
                receiveDone.Reset();
                // Begin receiving the data from the remote device.
                clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveLoopCallback), state);
                receiveDone.WaitOne();
            }

            Communication.Instance.IsListening = false;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            Communication.Instance.IsListening = false;
        }
    }

    /// <summary>
    ///     Méthode utilisé par <see cref="ReceiveLoop"/>.
    /// </summary>
    /// <param name="ar"> L'objet asynchrone. </param>
    private static void ReceiveLoopCallback(IAsyncResult ar)
    {
        receiveDone.Set();
        ReceiveCallback(ar);
    }

    /// <summary>
    ///     Permet d'envoyer un <see cref="Packet" /> en asynchrone.
    /// </summary>
    /// <param name="clientSocket"> Le <see cref="Socket" /> a utiliser. </param>
    /// <param name="original"> Le <see cref="Packet" /> a envoyer. </param>
    public static void Send(Socket clientSocket, Packet original)
    {
        byte[]? bytes = null;
        var error_value = Tools.Errors.None;

        // Send the data through the socket.
        bytes = original.PacketToByteArray(ref error_value);
        if (error_value != Tools.Errors.None)
        {
            Debug.LogError("[ERREUR] : Send PacketToByteArray not None : " + error_value);
            return;
        }

        // Begin sending the data to the remote device.
        var size = bytes.Length;

        var debug = "Sent total {" + size + "} bytes to server." +
                        "\n\t bytes =>\t" + original;
        Debug.Log(debug);

        clientSocket.BeginSend(bytes, 0, size, 0,
            SendCallback, clientSocket);
    }

    /// <summary>
    ///     Méthode utilisé par <see cref="Send"/>.
    /// </summary>
    /// <param name="ar"> L'objet asynchrone. </param>
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    /// <summary>
    ///     Permet d'arrêter l'écoute en boucle.
    /// </summary>
    /// <remarks>
    ///     Cela ne stoppe pas l'écoute en cours.
    /// </remarks>
    public static void StopListening()
    {
        if  (Communication.Instance.IsInRoom == 0)
            mustLoop = false;
    }
}
