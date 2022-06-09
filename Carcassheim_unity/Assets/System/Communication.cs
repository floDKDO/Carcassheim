using UnityEngine;
using System.Net.Sockets;
using ClassLibrary;
using Newtonsoft.Json;

namespace Assets.System
{
    /// <summary>
    /// Cette class est un Singleton qui s'occupe de la communication 
    /// entre le client et le serveur en utilisant la class <see cref="ClientAsync" />.
    /// </summary>
    public class Communication : MonoBehaviour
    {
        /// <summary>
        ///    Représente d'id du joueur.
        /// </summary>
        /// <value>Par Défaut = 0</value>
        /// <returns><see cref="ulong" /></returns>
        public ulong IdClient 
        { get => _idClient;
            set
            {
                if (value < 0)
                    return;
                else
                    _idClient = value;
            }
        }

        /// <summary>
        ///    Représente le nom du joueur.
        /// </summary>
        /// <value>Par Défaut = ""</value>
        /// <returns><see cref="string" /></returns>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        ///     Attribut utilisé par <see cref="IdClient" />.
        /// </summary>
        /// <value>Par Défaut = 0</value>
        /// <returns><see cref="int" /></returns>
        private ulong _idClient = 0;

        /// <summary>
        ///     Représente d'id de la Room.
        /// </summary>
        /// <value>Par Défaut = -1</value>
        /// <returns><see cref="int" /></returns>
        public int IdRoom
        {
            get => _idRoom;
            set
            {
                if(value < 0)
                    return;
                else
                    _idRoom = value;
            }
        }

        /// <summary>
        ///     Attribut utilisé par <see cref="IdRoom" />.
        /// </summary>
        /// <value>Par Défaut = -1</value>
        /// <returns><see cref="int" /></returns>
        private int _idRoom = -1;

        /// <summary>
        ///     Indique si la communication est entrain d'écouter.
        /// </summary>
        /// <value>Par Défaut = false</value>
        /// <returns><see cref="bool" /></returns>
        public bool IsListening { get; set; } = false;

        /// <summary>
        ///     Indique si la communication doit se faire avec le 
        ///     <see cref="Socket" /> de la Room.
        /// </summary>
        /// <value>Par Défaut = 0</value>
        /// <returns><see cref="int" /></returns>
        public int IsInRoom 
        { get => _isInRoom; set
            {
                if (value != 0 && value != 1)
                    _isInRoom = 0;
                else
                    _isInRoom = value;
            }
        }

        /// <summary>
        ///     Attribut utilisé par <see cref="IsInRoom" />.
        /// </summary>
        /// <value>Par Défaut = 0</value>
        /// <returns><see cref="int" /></returns>
        private int _isInRoom = 0;

        /// <summary>
        ///     Représente le port qu'utilise le <see cref="Socket" />
        ///     une fois dans la Room.
        /// </summary>
        /// <value>Par Défaut = -1</value>
        /// <returns><see cref="int" /></returns>
        public int PortRoom 
        { get => _portRoom;
            set
            {
                if (value < 0)
                    return;
                else 
                    _portRoom = value;
            }
        }

        /// <summary>
        ///     Attribut utilisé par <see cref="PortRoom" />.
        /// </summary>
        /// <value>Par Défaut = -1</value>
        /// <returns><see cref="int" /></returns>
        private int _portRoom = -1;

        /// <summary>
        ///     Représente la liste des <see cref="Socket" /> qu'utilise <see cref="Communication" />.
        /// </summary>
        /// <value>Par Défaut = { null, null }</value>
        /// <returns><see cref="Socket" />[2]</returns>
        private Socket[] lesSockets = { null, null };

        /// <summary>
        ///     Représente le <see cref="Socket" /> qu'utilise <see cref="Communication" />.
        /// </summary>
        /// <value>Par Défaut = null</value>
        /// <returns><see cref="Socket" /></returns>
        public Socket LeSocket 
        { get 
            { 
                return lesSockets[IsInRoom]; 
            }
            set 
            {
                lesSockets[IsInRoom] = value;
            } 
        }
        /// <summary>
        ///     Représente la liste des <see cref="bool" />[2] qui indique si les <see cref="Socket" /> 
        ///     qu'utilise <see cref="Communication" /> sont connectés avec le Serveur.
        /// </summary>
        /// <value>Par Défaut = { false, false }</value>
        /// <returns><see cref="bool" />[2]</returns>
        private bool[] isConnected = { false, false };

        /// <summary>
        ///     Représente un <see cref="bool" /> qui indique si le <see cref="Socket" /> 
        ///     qu'utilise <see cref="Communication" /> est connecté avec le Serveur.
        /// </summary>
        /// <value>Par Défaut = false </value>
        /// <returns> <see cref="bool"/> </returns>
        public bool IsConnected 
        {
            get
            {
                return isConnected[IsInRoom];
            }
            set
            {
                isConnected[IsInRoom] = value;
            }
        }

        /// <summary>
        ///     Représente l'instance du Singleton de la class <see cref="Communication" />.
        /// </summary>
        /// <value>Jamais null</value>
        /// <returns><see cref="Communication" /></returns>
        public static Communication Instance
        {
            get
            {
                
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<Communication>();

                    if (_instance == null)
                    {
                        GameObject container = new GameObject("Communication");
                        _instance = container.AddComponent<Communication>();
                    }
                }
                
                return _instance;
                
            }
        }

        /// <summary>
        ///     Attribut utilisé par <see cref="Instance" />.
        /// </summary>
        /// <value>Par Défaut = null</value>
        /// <returns><see cref="Communication" /></returns>
        private static Communication _instance;

        void Awake()
        {
            if(_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            }
        }

        /// <summary>
        ///     Permet d'initialisé la connexion avec le Serveur.  
        /// </summary>
        /// <remarks>
        ///     Remplit un des <see cref="Socket"/> de la liste <see cref="lesSockets"/>.
        /// </remarks>
        public void LancementConnexion()
        {
            TextAsset contents = Resources.Load<TextAsset>("network/config");
            Parameters parameters = JsonConvert.DeserializeObject<Parameters>(contents.ToString());

            if (PortRoom != -1)
                parameters.ServerPort = PortRoom;

            ClientAsync.Connection(parameters);
            ClientAsync.connectDone.WaitOne();

            IsConnected = true;
        }

        /// <summary>
        ///     Permet de déconnecter un des <see cref="Socket"/> du Serveur.
        /// </summary>
        /// <remarks>
        ///     Modifie les valeurs des attributs <see cref="lesSockets"/> et <see cref="isConnected"/>.
        /// </remarks>
        public void LancementDeconnexion()
        {
            if (!IsConnected) return;
            
            ClientAsync.Disconnection(lesSockets[IsInRoom]);
            ClientAsync.connectDone.WaitOne();

            LeSocket = null;
            IsConnected = false;
        }

        /// <summary>
        ///     Permet de démarrer une écoute asynchrone sur le <see cref="Socket"/> actuel.
        /// </summary>
        /// <remarks>
        ///     Démarre une connexion avec le Serveur si <see cref="isConnected"/>[<see cref="IsInRoom"/>] est égal à false.
        /// </remarks>
        /// <param name="pointeurFonction"> Pointeur de fonction de type <see cref="ClientAsync.OnPacketReceivedHandler"/>. </param>
        public void StartListening(ClientAsync.OnPacketReceivedHandler pointeurFonction)
        {
            if(!IsConnected)
                LancementConnexion();

            ClientAsync.OnPacketReceived += pointeurFonction;
            ClientAsync.Receive(LeSocket);
        }

        /// <summary>
        ///     Permet de démarrer une écoute asynchrone en boucle sur le <see cref="Socket"/> actuel.
        /// </summary>
        /// <param name="pointeurFonction"> Pointeur de fonction de type <see cref="ClientAsync.OnPacketReceivedHandler"/>. </param>
        public void StartLoopListening(ClientAsync.OnPacketReceivedHandler pointeurFonction)
        {
            ClientAsync.OnPacketReceived += pointeurFonction;
            ClientAsync.ReceiveLoop(LeSocket);
        }

        /// <summary>
        ///     Permet de stopper l'écoute asynchrone sur le <see cref="Socket"/> actuel.
        /// </summary>
        /// <param name="pointeurFonction"> Pointeur de fonction de type <see cref="ClientAsync.OnPacketReceivedHandler"/>. </param>
        public void StopListening(ClientAsync.OnPacketReceivedHandler pointeurFonction)
        {
            ClientAsync.OnPacketReceived -= pointeurFonction;
            ClientAsync.StopListening();
        }

        /// <summary>
        ///     Permet de redémarrer une écoute asynchrone sur le <see cref="Socket"/> actuel.
        /// </summary>
        public void NewListening()
        {
            ClientAsync.Receive(LeSocket);
        }

        /// <summary>
        ///     Permet de démarrer une écoute asynchrone sur le <see cref="Socket"/> actuel.
        /// </summary>
        /// <remarks>
        ///     Démarre une connexion avec le Serveur si <see cref="isConnected"/>[<see cref="IsInRoom"/>] est égal à false.
        /// </remarks>
        /// <remarks>
        ///      Redémarre une écoute asynchrone sur le <see cref="Socket"/> actuel si <see cref="IsListening"/> est égal false.
        /// </remarks>
        /// <param name="packet"> Le packet a envoyer. </param>
        public void SendAsync(Packet packet)
        {
            if (!IsConnected)
                LancementConnexion();

            if(!IsListening)
                NewListening();

            if(LeSocket != null)
                ClientAsync.Send(LeSocket, packet);
        }
    }
}