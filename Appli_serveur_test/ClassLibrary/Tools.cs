namespace ClassLibrary;

using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

/// <summary>
///     Class involving methods used by an instance of <see cref="Packet" /> while communicating.
/// </summary>
public static class Tools
{
    /// <summary>
    ///     Indicates which type of error has occurred.
    /// </summary>
    public enum Errors
    {
        None = 0,
        Unknown = -1,
        Socket = 1,
        Format = 2,
        ConfigFile = 3,
        Receive = 4,
        Data = 5,
        Permission = 6,
        Database = 7,
        RoomList = 8,
        RoomJoin = 9,
        RoomLeave = 10,
        RoomCreate = 11,
        RoomSettings = 12,
        PlayerReady = 20,
        BadPort = 21,
        BadData = 22,
        IllegalPlay = 23,
        NbPlayers = 24,
        NotFound = 404,
        ToBeDetermined = 999
    }

    /// <summary>
    ///     Indicates which type of data is used by an instance of the <see cref="Packet" /> class.
    /// </summary>
    public enum IdMessage : byte
    {
        Default = 0,
        
        AccountSignup = 1,
        AccountLogin = 2,
        AccountLogout = 3,
        AccountStatistics = 4,
        
        RoomList = 10,
        RoomCreate = 11,
        RoomSettingsGet = 12,
        RoomSettingsSet = 13,
        RoomAskPort = 14,
        
        PlayerJoin = 20,
        PlayerLeave = 21,
        PlayerKick = 22,
        PlayerReady = 23,
        PlayerCheat = 24,
        PlayerList = 25,
        PlayerCurrent = 26,
        
        StartGame = 30,
        EndGame = 31,
        EndTurn = 32,
        TimerPlayer = 33,
        TimerGame = 34,
        
        TuileDraw = 40,
        TuilePlacement = 41,
        TuileVerification = 42,
        CancelTuilePlacement = 43,
        PionPlacement = 44,
        CancelPionPlacement = 45,
        
        NoAnswerNeeded = 99
    }

    /// <summary>
    ///     Indicates the player status for a room
    /// </summary>
    public enum PlayerStatus
    {
        Default = 0,
        Success = 1,
        Kicked = 2,
        Full = 3,
        Found = 4,
        Permissions = 5,
        LastPlayerReady = 6,
        NotFound = -1
    }
    
    /// <summary>
    ///     Indicates the game status
    /// </summary>
    public enum GameStatus
    {
        Room = 0,
        Running = 1,
        Stopped = 2
    }

    /// <summary>
    ///     Indicates the lenght of the timer
    /// </summary>
    public enum Timer
    {
        DixSecondes = 10,
        DemiMinute = 30,
        Minute = 60,
        DemiHeure = 1800,
        Heure = 3600
    }

    /// <summary>
    ///     Indicates the number of meeple per player
    /// </summary>
    public enum Meeple
    {
        Quatre = 4,
        Huit = 8,
        Dix = 10
    }

    /// <summary>
    ///     Indicates the different endgame types
    /// </summary>
    public enum Mode
    {
        Default = 0,
        TimeAttack = 1,
        Score = 2
    }

    /// <summary>
    ///     Indicates the different extensions
    /// </summary>
    public enum Extensions
    {
        None = 0,
        Riviere = 1,
        Abbaye = 2
    }

    /// <summary>
    ///     Converts an instance of <see cref="Packet" /> to a byte array (serialized).
    /// </summary>
    /// <param name="packet">Instance of <see cref="Packet" /> which is being serialized.</param>
    /// <param name="error">Stores the <see cref="Errors" /> value.</param>
    /// <returns>
    ///     A byte array corresponding to an instance of <see cref="Packet" /> which has been
    ///     serialized.
    /// </returns>
    /// <exception cref="InvalidEnumArgumentException"></exception>
    public static byte[] PacketToByteArray(this Packet packet, ref Errors error)
    {
        // Check if enum "Errors" do exist.
        if (!Enum.IsDefined(typeof(Errors), error))
        {
            throw new InvalidEnumArgumentException(nameof(error), (int)error, typeof(Errors));
        }

        // Initialize a byte array to empty.
        var packetAsBytes = Array.Empty<byte>();

        // Catch : could not serialize the "Packet" instance.
        try
        {
            // Converts an instance of "Packet" to a JSON string.
            var packetAsJsonString = JsonConvert.SerializeObject(packet);
            // Converts a JSON string to byte array.
            packetAsBytes = Encoding.ASCII.GetBytes(packetAsJsonString);

            // No error has occured.
            error = Errors.None;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            // Setting the error value.
            error = Errors.Socket;
        }

        return packetAsBytes;
    }

    /// <summary>
    ///     Converts a byte array to an instance of <see cref="Packet" /> (deserialized).
    /// </summary>
    /// <param name="byteArray">Byte array which is being deserialized.</param>
    /// <param name="error">Stores the <see cref="Errors" /> value.</param>
    /// <returns>
    ///     A list of instances of <see cref="Packet" /> corresponding to byte arrays which has been
    ///     deserialized.
    /// </returns>
    /// <exception cref="InvalidEnumArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<Packet>? ByteArrayToPacket(this byte[] byteArray, ref Errors error)
    {
        // Check if enum "Errors" do exist.
        if (!Enum.IsDefined(typeof(Errors), error))
        {
            throw new InvalidEnumArgumentException(nameof(error), (int)error, typeof(Errors));
        }

        // Initialize an empty list of Packet.
        var packets = new List<Packet>();

        try
        {
            // Converts a byte array to a JSON string.
            var packetAsJson = Encoding.ASCII.GetString(byteArray);
            
            // Splits the different packets received.
            var packetAsJsonList = packetAsJson.Split('}');

            // Puts each packet received in the list.
            for (var i = 0; i < packetAsJsonList.Length - 1; i++)
            {
                // Converts a JSON string to an instance of "Packet".
                packets.Add(JsonConvert.DeserializeObject<Packet>(packetAsJsonList[i] + "}") ??
                            throw new ArgumentNullException(packetAsJson));
            }

            // No error has occured.
            error = Errors.None;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            // Setting the error value.
            error = Errors.Socket;
        }

        return packets;
    }
}
