using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private RectTransform my_rect;
    [SerializeField] private Transform results;
    [SerializeField] private PlayerResultRepre result_model;
    [SerializeField] GameObject loading_screen;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void setEndOfGame(List<PlayerRepre> players)
    {
        gameObject.SetActive(true);
        int index = 0;
        PlayerRepre tmp;
        while (index < players.Count)
        {
            int max_index = index;
            int max_score = players[index].Score;
            for (int i = index + 1; i < players.Count; i++)
            {
                int score = players[i].Score;
                if (score > max_score)
                {
                    max_index = i;
                    max_score = score;
                }
            }
            tmp = players[index];
            players[index] = players[max_index];
            players[max_index] = tmp;

            PlayerResultRepre result = Instantiate<PlayerResultRepre>(result_model, results);
            int rank = index;
            while (rank > 0 && players[rank - 1].Score == max_score)
            {
                rank -= 1;
            }
            result.Rank = rank + 1;
            result.PlayerName = players[index].Name;
            result.Score = players[index].Score;

            Vector2 dim = my_rect.sizeDelta;
            dim.y += result.Height;
            my_rect.sizeDelta = dim;
            index++;
        }
    }
    public void Quit()
    {
        loading_screen.SetActive(true);
        StartCoroutine(LoadLocal());
    }


    IEnumerator LoadLocal()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
