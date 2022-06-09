using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.System;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
///    Home Menu.
/// </summary>
public class HomeMenu : Miscellaneous
{
	private Transform HCB; // Home Container Buttons
	private Color btnInactivColor;

	/// <summary>
	/// Start is called before the first frame update <see cref = "HomeMenu"/> class.
	/// </summary>
	void Start()
	{
		// INITIALISATION
		HCB = GameObject.Find("SubMenus").transform.Find("HomeMenu").transform.Find("Buttons").transform;
		HCB.Find("ShowRoomSelection").GetComponent<Button>().interactable = GetState();
		HCB.Find("ShowStat").GetComponent<Button>().interactable = GetState();
		ColorUtility.TryParseHtmlString("#808080", out btnInactivColor);
		HCB.Find("ShowRoomSelection").GetComponent<Button>().GetComponentInChildren<Text>().color = btnInactivColor;
		HCB.Find("ShowStat").GetComponent<Button>().GetComponentInChildren<Text>().color = btnInactivColor;

		if (Communication.Instance.IdClient >= 1)
			Connected();
	}

	/// <summary>
	/// Load local <see cref = "HomeMenu"/> class.
	/// </summary>
	public void ShowSolo()
	{
		StartCoroutine(LoadLocal());
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Change to the connection menu <see cref = "HomeMenu"/> class.
	/// </summary>
	public void ShowConnection()
	{
		ChangeMenu("HomeMenu", "ConnectionMenu");
	}

	/// <summary>
	/// Change to the room selection menu <see cref = "HomeMenu"/> class.
	/// </summary>
	public void ShowRoomSelection()
	{
		if (Communication.Instance.IdClient >= 1)
			ChangeMenu("HomeMenu", "RoomSelectionMenu");
		else
			ChangeMenu("HomeMenu", "ConnectionMenu");
		/* SceneManager.LoadScene("InGame"); */
	}

	/// <summary>
	/// Change to the options menu <see cref = "HomeMenu"/> class.
	/// </summary>
	public void ShowOptions()
	{
			ChangeMenu("HomeMenu", "OptionsMenu");
	}

	/// <summary>
	/// Change to the statistics menu <see cref = "HomeMenu"/> class.
	/// </summary>
	public void ShowStat()
	{
		if (Communication.Instance.IdClient >= 1)
			ChangeMenu("HomeMenu", "StatMenu");
		else
			ChangeMenu("HomeMenu", "ConnectionMenu");
	}

	/// <summary>
	/// Quit the game application <see cref = "HomeMenu"/> class.
	/// </summary>
	public void QuitGame() // A LA FIN : quand tout fonctionnera : RemoveAllListeners(); (bouton -> "free")
	{
		Communication.Instance.LancementDeconnexion();
		Application.Quit();
	}

	/// <summary>
	/// Load the local game <see cref = "HomeMenu"/> class.
	/// </summary>
	IEnumerator LoadLocal()
	{
		// The Application loads the Scene in the background as the current Scene runs.
		// This is particularly good for creating loading screens.
		// You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
		// a sceneBuildIndex of 1 as shown in Build Settings.
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("InGame_LOCAL");
		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}