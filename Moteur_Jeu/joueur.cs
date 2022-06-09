public class joueur {
    private int _idjoueur;
    public int _tricheJoueur;
    public int _noteJoueur;
    public int _pointsJoueur;
    public int _abbé;
    public int _meeples;
    public initJoueur(int id){
        _idjoueur=id;
        _tricheJoueur=0;    //initialisé à 0, incremente si triche, à 2 déconnecter joueur
        _noteJoueur=3;      //3 tours à rater avant déconnecter le joueur
        _abbé=2;
        _meeples=5;
        _pointsJoueur=0;
    }
    public getIdJoueur(){
        return this.idjoueur;
    }
    public ajoutPoints(int pointsAAjouter){     //Ajouter des points au joueur
        _pointsJoueur+=pointsAAjouter;
    }
}