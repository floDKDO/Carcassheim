using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.system
{
    /// <summary>
    /// class static dont les fonctions permettent de l'initialisation des tuiles a partir d'un fichier xml
    /// </summary>
    internal class LireXML2
    {
        /// <summary>
        /// donne l'id d'une position interne en fonction d'un point cardinal
        /// </summary>
        static Dictionary<string, int> Positions;

        /// <summary>
        /// permet de recuperer un type de terrain a partir de son id
        /// </summary>
        public static Dictionary<int, TypeTerrain> IdVersTerrain;

        /// <summary>
        /// donne un terrain en fonction de son nom
        /// </summary>
        static Dictionary<string, TypeTerrain> DictionaireTemp;
        static LireXML2()
        {
            Positions = new Dictionary<string, int>
            {
                {"nnw", 0 },
                {"n", 1 },
                {"nne", 2 },
                {"nee", 3 },
                {"e", 4 },
                {"see", 5 },
                {"sse", 6 },
                {"s", 7 },
                {"ssw", 8 },
                {"sww", 9 },
                {"w", 10 },
                {"nww", 11 }
            };


            DictionaireTemp = new Dictionary<string, TypeTerrain>()
            {
                { "abbaye", TypeTerrain.Abbaye },
                { "ville_simple", TypeTerrain.Ville },
                { "ville_blason", TypeTerrain.VilleBlason },
                { "champs", TypeTerrain.Pre },
                { "chemin", TypeTerrain.Route },
                { "riviere", TypeTerrain.Riviere }
            };
        }

        /// <summary>
        /// initialise les type de terrains et les tuiles a partir d'un fichier xml
        /// </summary>
        /// <param name="file">nom du fichier xml se trouvant dans le dossier StreamingAssets</param>
        /// <returns>un dictionnaire dont les clefs sont les ids des tuiles et les valeurs sont les tuiles</returns>
        public static Dictionary<ulong, Tuile> Read(string file)
        {
            var result = new Dictionary<ulong, Tuile>();
            IdVersTerrain = new Dictionary<int, TypeTerrain>();
            string path;
#if UNITY_ANDROID
            path = Path.Combine(Application.streamingAssetsPath, "config_back.xml");
#else
            path = Path.Combine(Application.streamingAssetsPath, "config_back.xml");
#endif



            StringReader xmlData = null;

            if (path.Contains("://"))
            {
                UnityWebRequest www = UnityWebRequest.Get(path);
                www.SendWebRequest();
                path = www.url;
                xmlData = new StringReader(www.downloadHandler.text);
                Debug.Log(xmlData.ReadToEnd());
                xmlData = new StringReader(www.downloadHandler.text);
            }
            Debug.Log(path);

            using (XmlReader reader = (xmlData == null) ? XmlReader.Create(path) : XmlReader.Create(xmlData))
            {
                ReadTerrain(reader);
                bool readingId = false;
                int currentId = 0;
                List<List<int>> lien = new List<List<int>>();
                List<Slot> slots = new List<Slot>();
                Tuile current;
                int lienPtr = 0;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "tuile":
                                    lien = new List<List<int>>();
                                    slots = new List<Slot>();
                                    lienPtr = 0;
                                    break;
                                case "id":
                                    readingId = true;
                                    break;
                                case "slot":
                                    Slot tempSlot;
                                    var tempLien = ReadSlot(reader, out tempSlot);
                                    lien.Add(tempLien);
                                    slots.Add(tempSlot);
                                    lienPtr++;
                                    break;
                                default:
                                    readingId = false;
                                    break;
                            }
                            break;
                        case XmlNodeType.Text:
                            if (readingId)
                                currentId = int.Parse(reader.Value);
                            break;
                        case XmlNodeType.EndElement:
                            readingId = false;
                            if (reader.Name == "tuile")
                            {
                                var temp = new List<int[]>();

                                foreach (var item in lien)
                                {
                                    temp.Add(item.ToArray());
                                }
                                //Debug.Log("TUILE " + currentId.ToString());
                                current = new Tuile((ulong)currentId, slots.ToArray(), temp.ToArray());
                                result.Add((ulong)currentId, current);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// initialise les terrains
        /// </summary>
        /// <param name="xmlReader">fichier xml</param>
        private static void ReadTerrain(XmlReader xmlReader)
        {
            //// Debug.Log("TERAAIN");
            bool readingId = false;
            bool readingNom = false;
            int currentId = 0;
            string currentNom = "";
            bool finish = false;
            while (!finish && xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (xmlReader.Name)
                        {
                            case "id":
                                readingId = true;
                                break;
                            case "nom":
                                readingNom = true;
                                break;
                            case "tuile":
                                finish = true;
                                break;
                            default:
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        readingId = false;
                        readingNom = false;
                        if (xmlReader.Name == "terrain")
                        {
                            //Debug.Log("END TERRAIN : " + currentId.ToString() + ", " + currentNom);
                            IdVersTerrain.Add(currentId, DictionaireTemp[currentNom]);
                        }
                        break;
                    case XmlNodeType.Text:
                        if (readingId)
                            currentId = int.Parse(xmlReader.Value);
                        if (readingNom)
                            currentNom = xmlReader.Value;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// lit une section slot de l'xml
        /// </summary>
        /// <param name="xmlReader">fichier</param>
        /// <param name="slot">slot lu</param>
        /// <returns>un tableau d'adjacence vers les autres solts</returns>
        private static List<int> ReadSlot(XmlReader xmlReader, out Slot slot)
        {
            // // Debug.Log("SLOT");
            var result = new List<int>();
            bool stop = false;
            int idTerrain = 0;
            string nodeName = "";
            string pos_debug = "";
            bool goNext = false;
            List<ulong> tempSlotLink = new List<ulong>();

            while (!stop && xmlReader.Read())
            {
                if (goNext)
                {
                    goNext = false;
                    continue;
                }
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (xmlReader.Name)
                        {
                            case "id":
                                nodeName = "id";
                                break;
                            case "slot":
                                break;
                            case "terrain":
                                nodeName = "terrain";
                                break;
                            case "link":
                                nodeName = "link";
                                break;
                            default:
                                pos_debug += xmlReader.Name + "; ";
                                result.Add(Positions[xmlReader.Name]);
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        switch (nodeName)
                        {
                            case "id":
                                break;
                            case "terrain":
                                idTerrain = int.Parse(xmlReader.Value);
                                break;
                            case "link":
                                tempSlotLink.Add(ulong.Parse(xmlReader.Value));
                                break;
                            default:
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        nodeName = "";
                        if (xmlReader.Name == "slot")
                            stop = true;
                        break;
                    default:
                        break;
                }
            }

            slot = new Slot(IdVersTerrain[idTerrain], tempSlotLink.ToArray());
            //Debug.Log("END SLOT OF" + IdVersTerrain[idTerrain].ToString() + " of " + pos_debug);
            return result;
        }
    }
}
