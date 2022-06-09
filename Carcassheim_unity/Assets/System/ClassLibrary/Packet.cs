using System;

namespace ClassLibrary
{
    /// <summary>
    ///     Object used when data is sent or received through the sockets between client and server.
    /// </summary>
    public class Packet
    {
        /// <summary>
        ///     The length of a serialized instance of <see cref="Packet" /> can take up to this exact value.
        /// </summary>
        public const int MaxPacketSize = 1024;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Packet" /> class as default.
        /// </summary>
        public Packet() => this.Data = Array.Empty<string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Packet" /> class with all set values.
        /// </summary>
        /// <param name="idMessage">One of the <see cref="Packet" /> values.</param>
        /// <param name="error">One of the <see cref="Packet" /> values.</param>
        /// <param name="idRoom">One of the <see cref="Packet" /> values.</param>
        /// <param name="idPlayer">One of the <see cref="Packet" /> values.</param>
        /// <param name="data">One of the <see cref="Packet" /> values.</param>
        public Packet(Tools.IdMessage idMessage, Tools.Errors error,
            ulong idPlayer, int idRoom, string[] data)
        {
            this.IdMessage = idMessage;
            this.Error = error;
            this.IdPlayer = idPlayer;
            this.IdRoom = idRoom;
            this.Data = data;
        }

        /// <summary>
        ///     <see cref="Tools.IdMessage" /> indicates the ID for a specific instance object of
        ///     <see cref="Packet" />.
        /// </summary>
        /// <value>Default is "Default" = 0.</value>
        public Tools.IdMessage IdMessage { get; set; }

        /// <summary>
        /// <see cref="Tools.Errors"/> indicates the error for a specific instance object of <see cref="Packet"/>.
        /// </summary>
        /// <value>Default is "None" = 0.</value>
        public Tools.Errors Error { get; set; }

        /// <summary>
        ///     The user's ID within the instance object <see cref="Packet" />.
        /// </summary>
        public ulong IdPlayer { get; set; }

        /// <summary>
        ///     The room's ID within the instance object <see cref="Packet" />.
        /// </summary>
        public int IdRoom { get; set; }

        /// <summary>
        ///     The data within the instance object <see cref="Packet" />.
        /// </summary>
        /// <value>Default is empty.</value>
        public string[] Data { get; set; }

        /// <summary>
        ///     Converts the values of this instance to its equivalent string representation.
        /// </summary>
        /// <param name="this">The packet object to which the method is being applied.</param>
        /// <returns>The string representation of the value of this instance under the JSON format.</returns>
        public override string ToString() => "IdMessage:" + this.IdMessage + "; "
                                             + "Error:" + this.Error + "; "
                                             + "IdPlayer:" + this.IdPlayer + "; "
                                             + "IdRoom:" + this.IdRoom + "; "
                                             + "Data:" + string.Join(" ", this.Data) + ";";

    }
}

