public class joueur {
    private ulong _idjoueur;
    public int _tricheJoueur;
    public int _noteJoueur;
    public int _pointsJoueur;
    public int _abbe;
    public int _meeples;
    public void initJoueur(ulong id){
        _idjoueur=id;
        _tricheJoueur=0;    //initialisé à 0, incremente si triche, à 2 déconnecter joueur
        _noteJoueur=3;      //3 tours à rater avant déconnecter le joueur
        _abbe=2;
        _meeples=5;
        _pointsJoueur=0;
    }
    public ulong getIdJoueur(){
        return _idjoueur;
    }
    public void ajoutPoints(int pointsAAjouter){     //Ajouter des points au joueur
        _pointsJoueur+=pointsAAjouter;
    }
}