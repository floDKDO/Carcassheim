///  ( proba * 60 ) Puis mélanger  
using System;
using System.Xml;


class XmlReader {

public static xml_reader(string file){

int idTu = 0, idTe=0, idSl = 0, i=0,j=0,k=0;
int[]slot; 
int[][]lien;
int[]tmp;
string nomTe,tmp;
    using (XmlReader reader = XmlReader.Create(@file)){
        while(reader.read()){
            if(reader.isStartElement()){
                switch (reader.Name.ToString())
                {
                    case "terrain":
                        reader.ReadStartElement("idTe"); 
                        idTe = Int32.Parse(reader.ReadString());
                        reader.ReadStartElement("NomTe"); //depend de l'écriture dans le fichier xml
                        nomTe = reader.ReadString();
                        //Appel constructeur du terrein avec idTe et nomTe  ICI
                        //..
                    case " tuile":
                        
                        reader.ReadStartElement("idTu"); 
                        idTu = Int32.Parse(reader.ReadString());

                        while(reader.ReadStartElement("slot"))//slot
                        { 
                        idSl = Int32.Parse(reader.ReadString());
                         
                        //Récupérer le tableau des positions internes (slot[])
                        while(tmp=reader.ReadStartElement()!="terrain"){
                            tmp[j]= Tuile.PointsCardPos[tmp];
                            j++;
                        }
                        lien[slot[i]]=tmp;      //Ajouter le tableau des liens sémantique
                        //
                        reader.ReadStartElement("terrain"); 
                        idTe = Int32.Parse(reader.ReadString());
                        slot[idSl] = new Slot(idTe);//Slots[idSlot] = new Slot(typeTerrain)
                        }
                        Tuile.Tuile(idTe,slot,lien);

                j=0;
                }

            }
        }
    }
}


}

