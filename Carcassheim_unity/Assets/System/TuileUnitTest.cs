using UnityEngine;

namespace Assets.system
{
    public class TuileUnitTest
    {
        Tuile subject;
        public TuileUnitTest()
        {
            int[][] lien = new int[][]
            {
            new int[] {1, 7},
            new int[] {2, 3},
            new int[] {4, 5, 6},
            new int[] {8, 9, 10},
            new int[] {11, 0}
            };
            TypeTerrain[] ter = new TypeTerrain[]
            {
            TypeTerrain.Route,
            TypeTerrain.Ville,
            TypeTerrain.Pre,
            TypeTerrain.Auberge,
            TypeTerrain.Abbaye
            };
            subject = new Tuile(0, 5, lien, ter);
        }

        public void IdSlotFromPositionInterne_Test()
        {
            ulong[] expectedResult = new ulong[]
            {
            4, 0, 1, 1, 2, 2, 2, 0, 3, 3, 3, 4
            };
            ulong[] actualResult = new ulong[12];

            for (int i = 0; i < 12; i++)
            {
                actualResult[i] = subject.IdSlotFromPositionInterne(i);

            }
        }
    }
}

