using UnityEngine;
using System.Collections;

public class ShipButton : MonoBehaviour {

	Landable landedShip;
	Planet planet;
	
	/// <summary>
	/// Initialize this button.
	/// </summary>
	public void Setup(Landable landable, Planet thePlanet)
	{
		planet = thePlanet;
		landedShip = landable;
	}
		
	void OnClick()
	{
		planet.RequestLaunch(landedShip);
		Game.gui.planetMenu.mainColonyPanel.GetComponent<ColonyPanel>().RemoveShipButton(gameObject.transform.parent.gameObject);
	}
	
}
