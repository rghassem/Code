using UnityEngine;
using System.Collections;

public class CloseButton : MonoBehaviour {

	void OnClick()
	{
		Game.gui.planetMenu.ExitPlanetView();
	}
}
