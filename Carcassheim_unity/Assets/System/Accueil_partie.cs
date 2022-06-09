using System;
using System.Threading;
using System.Collections.Generic;

/// <summary>
///     accueil dans lequel les joueurs se rejoignent et paramètrent la partie
/// </summary>
public class  Accueil_partie
{

    /// <summary>
    ///     Id de la partie
    /// </summary>
    private int _id_partie;

    /// <summary>
    ///     Id de chaque joueur présent 
    /// </summary>
    private List<int> _lst_joueurs;

    /// <summary>
    ///     Identifiant du joueur modérateur
    /// </summary>
    private int _id_moderateur;

    /// <summary>
    ///     Status de la partie
    /// </summary>
    private string _statut_partie;

    /// <summary>
    ///     Indique si la partie est privé
    /// </summary>
    private int _privee; 

    /// <summary>
    ///     Timer séléctionner
    /// </summary>
    private int _timer;

    /// <summary>
    ///     Timer max par joueur séléctionner
    /// </summary>
    private int _timer_max_joueur;

    /// <summary>
    ///     Nombre de meeples par joueur
    /// </summary>
    private int _meeples; // Nombre de meeples par joueur


    /// <summary>
    /// constructeur de l'accueil
    /// </summary>
    /// <param name="id_joueur_createur">id du créteur</param>
    public Accueil_partie(int id_joueur_createur)
    {

        _lst_joueurs = new List<int>();

        // BDD - Parcours de la liste des parties actuelles pour récupérer un ID non utilisé
        //_id_partie = ???

        _lst_joueurs.Add(id_joueur_createur);
        _id_moderateur = id_joueur_createur;

        _statut_partie = "ACCUEIL";

        // Initialisation des valeurs par défaut
        _privee = 1; // Une partie est par défaut privée
        _timer = 3600; // Une heure par défaut
        _timer_max_joueur = 40;
        _meeples = 8;


    }

    // Getters et setters

    // Méthodes


} 