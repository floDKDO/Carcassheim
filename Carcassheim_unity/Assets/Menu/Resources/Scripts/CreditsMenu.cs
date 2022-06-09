using Assets.System;

/// <summary>
///    Credits menu.
/// </summary>
public class CreditsMenu : Miscellaneous
{
	/// <summary>
	/// Hide credits menu <see cref = "CreditsMenu"/> class.
	/// </summary>
	public void HideCredits()
	{
		ChangeMenu("CreditsMenu", "OptionsMenu");
	}
}