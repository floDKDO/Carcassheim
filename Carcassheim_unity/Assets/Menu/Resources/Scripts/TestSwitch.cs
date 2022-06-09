using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///    Test Switch.
/// </summary>
public class TestSwitch : MonoBehaviour
{
	/// <summary>
	/// Update is called once per frame <see cref = "TestSwitch"/> class.
	/// </summary>
	void Update()
	{
		// Press the space key to start coroutine
		if (Input.GetKeyDown(KeyCode.Space))
		{
		// Use a coroutine to load the Scene in the background
		//StartCoroutine(LoadYourAsyncScene());
		}
	}

	/// <summary>
	///    Load the scene asynchronously in the background as the current Scene runs.
	/// </summary>
	IEnumerator LoadYourAsyncScene()
	{
		// The Application loads the Scene in the background as the current Scene runs.
		// This is particularly good for creating loading screens.
		// You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
		// a sceneBuildIndex of 1 as shown in Build Settings.
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("InGame_VG");
		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}