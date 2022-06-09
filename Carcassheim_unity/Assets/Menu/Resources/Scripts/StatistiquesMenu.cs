/// <summary>
///    Statistiques menu.
/// </summary>
public class StatistiquesMenu : Miscellaneous
{
	/// <summary>
	/// Hide stats menu <see cref = "StatistiquesMenu"/> class.
	/// </summary>
	public void HideStat()
	{
		HidePopUpOptions();
		ChangeMenu("StatMenu", "HomeMenu");
	}
}