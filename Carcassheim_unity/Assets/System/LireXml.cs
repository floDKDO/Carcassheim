///  ( proba * 60 ) Puis mélanger  
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.system
{
    public class LireXml
    {
        public static String _file;
        public LireXml(string file)
        {
            _file = file;
        }

        public Dictionary<ulong, Tuile> ReadXml()
        {
            ulong idTu = 0, idTe = 0, idSl = 0;
            int i = 0, j = 0, nbPos = 0;
            List<Slot> slot = new List<Slot>();
            Dictionary<ulong, Tuile> resultat = new Dictionary<ulong, Tuile>();

            int[][] lien = new int[12][];
            for (int n = 0; n < 12; n++)
            {
                lien[n] = new int[12];
            }
            List<int> tab = new List<int>();
            string nomTe = "", tmp = "";

            using (XmlReader reader = XmlReader.Create(Application.streamingAssetsPath + "/" + @_file))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "terrain":
                                reader.ReadToFollowing("id");
                                string readed = reader.ReadString();
                                Debug.Log("terrain" + readed);
                                idTe = Convert.ToUInt64(readed);
                                reader.ReadToFollowing("nom"); //depend de l'écriture dans le fichier xml
                                nomTe = reader.ReadString();
                                //Debug.Log("Terrain : ");
                                //Debug.Log(idTe);
                                //Debug.Log(nomTe);

                                break;

                            case "tuile":

                                reader.ReadToFollowing("idTu");
                                idTu = Convert.ToUInt64(reader.ReadString());

                                reader.ReadToFollowing("nbSlots");
                                j = Int32.Parse(reader.ReadString());

                                for (i = j; i > 0; i--)
                                {
                                    reader.ReadToFollowing("slot");
                                    reader.ReadToFollowing("idSl");
                                    idSl = Convert.ToUInt64(reader.ReadString());//id du slot

                                    //Récupérer le tableau des positions internes (slot[])
                                    reader.ReadToFollowing("positionsSlot");
                                    tmp = reader.ReadString();
                                    string[] subs = tmp.Split(',', '.', '\0');
                                    foreach (string sub in subs)
                                    {
                                        if (sub == "N" || sub == "NNO" || sub == "NNE" || sub == "NEE" || sub == "E" || sub == "SEE" || sub == "S" || sub == "SSO" || sub == "SOO" || sub == "O" || sub == "NOO")
                                        {
                                            //Debug.Log(sub);
                                            tab.Add(Tuile.PointsCardPos[sub]);
                                        }
                                    }

                                    reader.ReadToFollowing("terrain");
                                    idTe = Convert.ToUInt64(reader.ReadString());
                                    slot.Add(new Slot(idTe));
                                }
                                lien[idSl] = tab.ToArray();      //Ajouter le tableau des liens sémantiques
                                Tuile t = new Tuile(idTu, slot.ToArray(), lien);
                                resultat.Add(idTu, t); //Ajouter la tuile à la Dico
                                break;
                        }
                    }
                }
            }
            return resultat;
        }
        public static void afficheDicoTuiles()
        {
            foreach (var element in Tuile.DicoTuiles)
            {
                Debug.Log(element.Key);
            }
        }
        public static void CreateXMLFile()//sert au test unitaire de la méthode LireXml
        {


            //Decalre a new XMLDocument object
            XmlDocument doc = new XmlDocument();

            //xml declaration 
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            //create the root element
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            //string.Empty makes cleaner code
            XmlElement element1 = doc.CreateElement(string.Empty, "carcasheim", string.Empty);
            doc.AppendChild(element1);

            //terrain
            XmlElement element2 = doc.CreateElement(string.Empty, "terrain", string.Empty);


            XmlElement element3 = doc.CreateElement(string.Empty, "idTe", string.Empty);

            XmlText text1 = doc.CreateTextNode("5");

            element1.AppendChild(element2);
            element2.AppendChild(element3);
            element3.AppendChild(text1);


            XmlElement element4 = doc.CreateElement(string.Empty, "nomTe", string.Empty);
            XmlText text2 = doc.CreateTextNode("Chemin");
            element4.AppendChild(text2);
            element2.AppendChild(element4);

            //Tuile
            XmlElement element5 = doc.CreateElement(string.Empty, "tuile", string.Empty);


            XmlElement element6 = doc.CreateElement(string.Empty, "idTu", string.Empty);

            XmlText text3 = doc.CreateTextNode("2");

            element1.AppendChild(element5);
            element5.AppendChild(element6);
            element6.AppendChild(text3);


            XmlElement element7 = doc.CreateElement(string.Empty, "nbSlots", string.Empty);
            XmlText text4 = doc.CreateTextNode("1");
            element7.AppendChild(text4);
            element5.AppendChild(element7);


            //slots
            XmlElement element8 = doc.CreateElement(string.Empty, "slot", string.Empty);


            XmlElement element9 = doc.CreateElement(string.Empty, "idSl", string.Empty);

            XmlText text5 = doc.CreateTextNode("0");

            element5.AppendChild(element8);
            element8.AppendChild(element9);
            element9.AppendChild(text5);


            XmlElement element10 = doc.CreateElement(string.Empty, "positionsSlot", string.Empty);
            XmlText text6 = doc.CreateTextNode("NEE,N.");
            element10.AppendChild(text6);
            element8.AppendChild(element10);

            XmlElement element11 = doc.CreateElement(string.Empty, "terrain", string.Empty);
            XmlText text7 = doc.CreateTextNode("5");
            element11.AppendChild(text7);
            element8.AppendChild(element11);

            doc.Save(Directory.GetCurrentDirectory() + "//" + _file);

        }
        /* 
         void Start()
         {
             CreateXMLFile();
             String file = "Assets/system/infos.xml";
             LireXml l = new LireXml(file);
             afficheDicoTuiles();
             File.Delete(file);

         }
        */
    }
}


