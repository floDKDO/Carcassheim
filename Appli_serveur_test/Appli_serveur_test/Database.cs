using System.Collections;
using System.Text;
using Npgsql;

namespace Server;

public class Database
{
    private string connString;
    
    public Database()
    {
        connString = "Host=192.168.100.119;Port=5432;Username=pro;Password=PI_Carcassheim;Database=carcassheim";
    }

    public NpgsqlConnection Connect()
    {
        try
        {
            return new NpgsqlConnection(connString);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERREUR: echec de l'ouverture : " + ex);

            return null;
        }
    }
    
    //"INSERT INTO data (some_field) VALUES (@p)"
    public async void ExecuteCommandeModification(string commande,object[] parametres)
    {
        await using NpgsqlConnection connection = this.Connect();
        await connection.OpenAsync();

        await using (var cmd = new NpgsqlCommand(commande, connection))
        {
            int i, taille = parametres.Length;
            for (i = 0; i < taille; i+=2)
            {
                cmd.Parameters.AddWithValue((string)parametres[i], parametres[i+1]);
            }
		    cmd.Prepare();
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.Write("Erreur : commande : " + commande + " " + ex);
                
            }
        }
        connection.Close();
    }
    
    public async Task<object[]> ExecuteCommandeWithResult(string commande,object[] parametres)
    {
        await using NpgsqlConnection connection = this.Connect();
        await connection.OpenAsync();
        ArrayList res = new ArrayList();

        await using (var cmd = new NpgsqlCommand(commande, connection))
        {
            int i, taille = parametres.Length;
            for (i = 0; i < taille; i+=2)
            {
                cmd.Parameters.AddWithValue((string)parametres[i], parametres[i+1]);
            }
            cmd.Prepare();
            
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int  tailleRow = (int)reader.FieldCount;
                    int  j;
                    for ( j = 0; j < tailleRow; j++)
                    {
                        res.Add(reader[j]);
                    }
                    
                }
            }
        }

        connection.Close();
        return (object[])res.ToArray();
    }
    
    /*public void IncrementeNbParties(ulong idu)
    {
        string commande = "update Utilisateur set NbParties =  NbParties + 1 where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        ExecuteCommandeModification(commande,parametres);
    }
    
    public void IncrementeVictoires(ulong idu)
    {
        string commande = "update Utilisateur set Victoires =  Victoires + 1 where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        ExecuteCommandeModification(commande,parametres);
    }
    
    public void IncrementeDefaites(ulong idu)
    {
        string commande = "update Utilisateur set Defaites =  Defaites + 1 where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        ExecuteCommandeModification(commande,parametres);
    }
    
    public int GetXp(ulong idu)
    {
        string commande = "select XP from Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        int XP = Convert.ToInt32(res.Result[0]);
        
        return XP;
    }
    
    public void AddXp(ulong idu, int xp)
    {
        int CurExp = GetXp(idu);
        int lvl = (CurExp + xp) / 100;
        CurExp = (CurExp + xp) % 100;
        string commande = "update Utilisateur set XP =  Defaites + 1 where IDU = @pIDU;";
        string[] parametres = new[] {"pCUREXP",CurExp.ToString(),"pLVL",lvl.ToString(), idu.ToString()};
        ExecuteCommandeModification(commande,parametres);
    }*/
    
    int GetAge(string DateNaiss)
    {
        DateTime toDate = DateTime.Parse(DateNaiss);
        int age = DateTime.Now.Subtract(toDate).Days;
        age = age / 365;
        return age;
    }
    
    /*public int idPartieLibre()
    {
        string commande = "select count(*) from table Partie;";
        string[] parametres = Array.Empty<string>();
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        int nbResult = res.Result.Length;
        if (nbResult > 0)
        {
            return Convert.ToInt32(res.Result[0]);
        }
        
        return 0;
    }*/
    
    public string GetPseudo(int idu)
    {
        string commande = "SELECT Pseudo FROM Utilisateur WHERE IDU = @pIDU;";
        object[] parametres = new object[] {"pIDU", idu};
        Task<object[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        if (res.Result.Length > 0)
        {
            return res.Result[0].ToString();
        }
        
        return "";
    }
    
    /*public string GetPhoto(ulong idu)
    {
        string commande = "select Photo from table Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        int nbResult = res.Result.Length;
        if (nbResult > 0)
        {
            return res.Result[0];
        }
        
        return "";
    }
    
    public int GetNbVictoires(ulong idu)
    {
        string commande = "select Victoires from table Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        int nbResult = res.Result.Length;
        if (nbResult > 0)
        {
            return Convert.ToInt32(res.Result[0]);
        }
        
        return 0;
    }
    
    public int GetNiveau(ulong idu)
    {
        string commande = "select Niveau from table Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        int nbResult = res.Result.Length;
        if (nbResult > 0)
        {
            return Convert.ToInt32(res.Result[0]);
        }
        
        return 0;
    }
    
    public int GetNbefaites(ulong idu)
    {
        string commande = "select Defaites from table Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        int nbResult = res.Result.Length;
        if (nbResult > 0)
        {
            return Convert.ToInt32(res.Result[0]);
        }
        
        return 0;
    }
    
    public int GetNbParties(ulong idu)
    {
        string commande = "select NbParties from table Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        int nbResult = res.Result.Length;
        if (nbResult > 0)
        {
            return Convert.ToInt32(res.Result[0]);
        }
        
        return 0;
    }
    
    /*public void Drop(string tableName)
    {
        string commande = "DROP Table @pTABLENAME;";
        string[] parametres = new[] {"pTABLENAME", tableName};
        ExecuteCommandeModification(commande,parametres);
    }

    public void DropPartie()
    {
        Drop("PartieExt");
        Drop("Partie");
    }*/
    
    public long Identification(string login, string mdp)
    {
        string commande = "SELECT IDU FROM Utilisateur WHERE Pseudo = @pLOGIN AND MDP = @pMDP;";
        object[] parametres = new object[] {"pLOGIN", login, "pMDP", mdp};
        Task<object[]> res = ExecuteCommandeWithResult(commande, parametres);

        if (res.Result.Length == 0)
            return -1;
        return Convert.ToInt64(res.Result[0]);
    }
    
    public void Adduser(string Pseudo, string MDP, string Mail, int Xp, int Niveau, int Victoires, int Defaites, int Nbparties, string DateNaiss)
    {
        int age = GetAge(DateNaiss);

        if (age >= 13)
        {
            string commande = "INSERT INTO Utilisateur (Pseudo,MDP,Mail,XP,Niveau,Victoires,Defaites,Nbparties,DateNaiss) VALUES(@pPSEUDO,@pMDP,@pMAIL,@pXP,@pNIVEAU,@pVICTOIRES,@pDEFAITES,@pNBPARTIES,@pDATENAISS);";
            object[] parametres = new object[] {"pPSEUDO", Pseudo,"pMDP", MDP,"pMAIL", Mail,"pXP", Xp,"pNIVEAU", Niveau,"pVICTOIRES", Victoires,"pDEFAITES", Defaites,"pNBPARTIES", Nbparties,"pDATENAISS", DateNaiss};
            ExecuteCommandeModification(commande,parametres);
        }
        else
        {
            Console.Write("Erreur : Age inferieur à 13 ans : " + age);
        }
    }
    
    /*public void AddExtension(string Nom)
    {
        string commande = "INSERT INTO Extension (Nom) VALUES( @pNOM);";
        string[] parametres = new[] {"pNOM", Nom};
        ExecuteCommandeModification(commande,parametres);
    }
    
    // fonction d'ajout de modèle
    public void AddModele(int Proba, string Nom, int IDE)
    {
        string commande = "INSERT INTO Extension (Proba,Nom,IDE) VALUES( @pPROBA, @pNOM, @pIDE);";
        string[] parametres = new[] {"pPROBA",Proba.ToString(),"pNOM", Nom,"pIDE",IDE.ToString()};
        ExecuteCommandeModification(commande,parametres);
    }
    
    // focntion d'ajout de la tuile  
    public void AddTuile(int IDM, string Image)
    {
        string commande = "INSERT INTO Tuile (IDM,Image) VALUES(@pIDM, @pIMAGE);";
        string[] parametres = new[] {"pIDM",IDM.ToString(),"pIMAGE", Image};
        ExecuteCommandeModification(commande,parametres);
    }
    
    // focntion d'ajout de la partie 
    public long AddPartie(ulong Moderateur, string Statut, string NbMaxJ, string Prive, string Timer, string TMaxJ, string Meeples)
    {
        string commande = "INSERT INTO Partie (Moderateur,Statut,NbMaxJ,Prive,Timer,TMaxJ,Meeples) VALUES(@pMODERATEUR, @pSTATUT, @pNBMAXJ, @pPRIVE, @pTIMER, @pTMAXJ, @pMEEPLES) RETURNING IDP;";
        string[] parametres = new[] {"pMODERATEUR",Moderateur.ToString(),"pSTATUT", Statut,"pNBMAXJ",NbMaxJ,"pPRIVE",Prive,"pTIMER",Timer,"pTMAXJ",TMaxJ,"pMEEPLES",Meeples};
        Task<string[]> res = ExecuteCommandeWithResult(commande, parametres);

        if (res.Result.Length == 0)
            return -1;
        return long.Parse(res.Result[0]);
    }*/
    
    public void RemplirTuiles(Dictionary<ulong, ulong> dico)
    {
        string commande = "SELECT idm,proba FROM Modele WHERE extnom = 'none';";
        string[] parametres = Array.Empty<string>();
        Task<object[]> res = ExecuteCommandeWithResult(commande, parametres);

        int taille = res.Result.Length;
        int i;
        Console.WriteLine(taille);
        for(i = 0; i < taille; i+=2)
        {
            try
            {
                dico.Add(Convert.ToUInt64(res.Result[i]), Convert.ToUInt64(res.Result[i + 1]));
            }
            catch (Exception ex)
            {
                Console.Write("Erreur : Convertion string to int : " + ex);
            }
        }
        
    }

    public void RemplirRivieres(List<ulong> rivieres)
    {
        string commande = "SELECT idm FROM Modele WHERE extnom = 'rivière';";
        string[] parametres = Array.Empty<string>();
        Task<object[]> res = ExecuteCommandeWithResult(commande, parametres);

        int taille = res.Result.Length;
        int i;
        
        for(i = 0; i < taille; i++)
        {
            try
            {
                rivieres.Add(Convert.ToUInt64(res.Result[i]));
            }
            catch (Exception ex)
            {
                Console.Write("Erreur : Convertion string to int : " + ex);
            }
        }
    }
    
    public string[] GetStatistics(ulong idu)
    {
        string commande = "select XP,Niveau,Victoires,Defaites,Nbparties, from Utilisateur where IDU = @pIDU;";
        string[] parametres = new[] {"pIDU", idu.ToString()};
        Task<object[]> res = ExecuteCommandeWithResult(commande, parametres);
        
        return ToStringArray(res.Result);
    }
    
    public static string[] ToStringArray(object[] array, bool includeNulls = false, string nullValue = "")
    {
        IEnumerable<object> enumerable = array;
        if (!includeNulls)
            enumerable = enumerable.Where(e => e != null);
        return enumerable.Select(e => (e ?? nullValue).ToString()).ToArray();
    }
}
