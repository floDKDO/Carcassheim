using Assets.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    Room is created menu.
/// </summary>
public class RoomIsCreated : Miscellaneous
{
	/// <summary>
	///    Change to Public Room menu <see cref = "RoomIsCreated"/> class.
	/// </summary>
	public void HideRoomIsCreated()
	{
		HidePopUpOptions();
		ChangeMenu("RoomIsCreatedMenu", "PublicRoomMenu");
	}

	/// <summary>
	///    Go to Public Room menu <see cref = "RoomIsCreated"/> class.
	/// </summary>
	public void ShowRoom()
	{
		HidePopUpOptions();
		ChangeMenu("RoomIsCreatedMenu", "PublicRoomMenu");
	}
}