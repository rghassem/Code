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
		NGUITools.Destroy(gameObject.transform.parent.gameObject); //Ship button UIImageButton shares common parent with its other assets	
	}
	
}
