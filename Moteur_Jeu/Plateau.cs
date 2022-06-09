using Tuile;
using System.Collections.Generic;
using System;

public class Plateau
{
    public static readonly int[,] PositionAdjacentes = new int[,]
        { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
    
    private List<Tuile> _tuiles;
    public List<Tuile> Tuiles
    {
        get { return _tuiles; }
        set { _tuiles = value; }
    }
    
    public Plateau()
    {
        _tuiles = new List<Tuile>();
    }

    public Tuile GetTuile(int x, int y)
    {
        foreach (var item in _tuiles)
        {
            if (item.X == x && item.Y == y)
                return item;
        }
        return null;
    }

    public Tuile[] GetTuiles()
    {
        return _tuiles.ToArray();
    }

    public void Poser1ereTuile(Tuile tuile)
    {
        PoserTuile(tuile, 0, 0, 0);
    }

    public void PoserTuile(Tuile tuile, int x, int y, Rotation rot)
    {
        tuile.X = x;
        tuile.Y = y;
        tuile.Rotation = rot;
        _tuiles.Add(tuile);
    }

    public Position[] PositionsPlacementPossible(Tuile tuile)
    {
        List<Position> resultat = new();

        int x, y, rot;
        List<int> checkedX = new(), checkedY = new();
        foreach (var t in _tuiles)
        {
            for (int i = 0; i < 4; i++)
            {
                x = t.X + PositionAdjacentes[i, 0];
                y = t.Y + PositionAdjacentes[i, 1];

                if (checkedX.Contains(x) && checkedY.Contains(y))
                    break;
                
                checkedX.Add(x);
                checkedY.Add(y);

                for (rot = 0; rot < 4; rot++)
                {
                    if (PlacementLegal(tuile, x, y, rot))
                        resultat.Add(new Position(x, y, rot));
                }
            }
        }

        return resultat.ToArray();
    }

    public bool PlacementLegal(Tuile tuile, int x, int y, int rotation)
    {
        if (GetTuile(x, y) != null)
            return false;
        
        Tuile[] tuilesAdjacentes = TuilesAdjacentes(x, y);

        for (int i = 0; i < 4; i++)
        {
            Tuile t = tuilesAdjacentes[i];

            if (t == null)
                break;

            TypeTerrain[] faceTuile1 = tuile.TerrainSurFace((rotation + i) % 4);
            TypeTerrain[] faceTuile2 = t.TerrainSurFace((t.Rotation + i + 2) % 4);

            if (!CorrespondanceTerrains(faceTuile1, faceTuile2))
                return false;
        }

        return true;
    }

    private bool CorrespondanceTerrains(TypeTerrain[] t1, TypeTerrain[] t2)
    {
        for (int i = 0; i < 3; i++)
        {
            if (t1[i] != t2[2 - i])
                return false;
        }
        return true;
    }

    private Tuile[] TuilesAdjacentes(int x, int y)
    {
        Tuile[] resultat = new Tuile[4];

        var tab = PositionAdjacentes;

        for (int i = 0; i < 4; i++)
        {
            resultat[i] = GetTuile(x + tab[i, 0], y + tab[i, 1]);
        }
        return resultat;
    }

    private Tuile[] TuilesAdjacentes(Tuile t)
    {
        return TuilesAdjacentes(t.X, t.Y);
    }

    public bool ZoneFermee(Tuile tuile, int idSlot)
    {
        if (!_tuiles.Contains(tuile)) // ERROR
            return false;

        List<Tuile> tuilesFormantZone = new();
        tuilesFormantZone.Add(tuile);

        return ZoneFermeeAux(tuile, idSlot, tuilesFormantZone);
    }

    private bool ZoneFermeeAux(Tuile tuile, int idSlot, List<Tuile> tuilesFormantZone)
    {
        bool ferme = true, emplacementVide;
        int[] positionsInternes = new int[4];
        Tuile[] tuilesAdjSlot = TuilesAdjacentesAuSlot(tuile, idSlot, out emplacementVide, out positionsInternes);

        if (emplacementVide)
            return false;
        
        int c = 0;
        foreach (var item in tuilesAdjSlot)
        {
            if (!tuilesFormantZone.Contains(item))
            {
                tuilesFormantZone.Add(item);

                int idSlotProchaineTuile = item.IdSlotFromPositionInterne(positionsInternes[c++]);
                ferme = ferme && ZoneFermeeAux(item, idSlotProchaineTuile, tuilesFormantZone);
            }
        }

        return ferme;
    }

    private Tuile[] TuilesAdjacentesAuSlot(Tuile tuile, int idSlot,
        out bool emplacementVide, out int[] positionsInternesProchainesTuiles)
    {
        emplacementVide = false;

        int[] positionsInternes = tuile.LienSlotPosition[idSlot];
        List<Tuile> resultat = new();
        int x = tuile.X, y = tuile.Y;

        int direction, c = 0;
        foreach (int position in positionsInternes)
        {
            //direction = (position + (3 * tuile.Rotation)) / 3;
            direction = (position / 3 + tuile.Rotation) % 3;

            Tuile elem = GetTuile(x + PositionAdjacentes[direction, 0],
                                  y + PositionAdjacentes[direction, 1]);

            if (elem == null)
                emplacementVide = true;
            
            else if (!resultat.Contains(elem))
            {
                resultat.Add(elem);
                positionsInternesProchainesTuiles[c++] = 
                    (position + 6 + (elem.Rotation - tuile.Rotation)) % 3;
            }
        }

        return resultat.ToArray();
    }

    public void PoserPion(int idJoueur, Tuile tuile, int idSlot)
    {
        tuile.Slots[idSlot].IdJoueur = idJoueur;
    }

    public int[] EmplacementPionPossible(Tuile tuile, int idJoueur)
    {
        List<int> resultat = new();
        List<Tuile> parcourues = new();

        for (int i = 0; i < tuile.NombreSlot; i++)
        {
            if (ZoneAppartientAutreJoueur(tuile, i, idJoueur, parcourues))
                resultat.Add(i);
        }

        return result.ToArray();
    }

    public bool ZoneAppartientAutreJoueur(Tuile tuile, int idSlot, int idJoueur, List<Tuile> parcourues)
    {
        bool vide, resultat = true;
        int[] positionsInternesProchainesTuiles;
        Tuile[] adj = TuilesAdjacentesAuSlot(tuile, idSlot, out vide, positionsInternesProchainesTuiles);

        if (adj.length == 0)
            return false;

        int c = 0;
        foreach (var t in adj)
        {
            if (parcourues.Contains(t))
                break;
            parcourues.Add(t);

            int pos = positionsInternesProchainesTuiles[c++];
            int nextSlot = t.IdSlotFromPositionInterne(pos);
            int idJ = t.Slots[nextSlot].idJoueur;
            if (idJ != 0 && idJ != idJoueur)
                return false;
            resultat = resultat && ZoneAppartientAutreJoueur(t, nextSlot, idJoueur, parcourues);
        }

        return resultat;
    }
}

public struct Position
{
    int _x, _y, _rot;

    public int X => _x;
    public int Y => _y;
    public int ROT => _rot;

    public Position(int x, int y, int rot)
    {
        _x = x;
        _y = y;
        _rot = rot;
    }
}