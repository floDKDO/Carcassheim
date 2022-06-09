using System;
using System.Threading;
using System.Collections.Generic;

using Server;

namespace system
{

    public class Serveur_main
    {

        static void Main(string[] args)
        {



            // Initialisation du gestionnaire de thread de com
            GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

            /*
            Thread.Sleep(2000);

            List<int> listIdGerees = _lst_obj_threads_com[0].Get_id_parties_gerees();
            Console.WriteLine("Liste parties gérées par le premier thread com: [");
            for(int i = 0; i< listIdGerees.Count; i++)
            {
                Console.WriteLine(listIdGerees[0] + ", ");

            }
            Console.WriteLine("]\n");
            */


            // Fermeture de tous les threads
            /*
            foreach (Thread thread in _lst_threads_com)
            {
                thread.Join();
            }
            */



            //Ecoute serveur
            Server.Server.StartListening(0);
            // TODO -> récupérer le port sur config.json
            


        }

    }
}