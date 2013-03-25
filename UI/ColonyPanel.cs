using UnityEngine;
using System.Collections;

public class ColonyPanel : MonoBehaviour {
	
	public GameObject shipButtonPrefab;
	
	//Menu Components
	UILabel planetName;
	UILabel descriptionText;
	UILabel populationValue;
	UILabel materialsValue;
	UILabel energyValue;
	
	//Ships
	Transform shipsList;
	Transform ships;
	
	//The planet 
	Planet planet;
	
	// Use this for initialization
	void Awake () {
		
		Transform planetInfoBase = transform.Find("PlanetInfoPane");
		
		planetName = planetInfoBase.Find("PlanetName").GetComponent<UILabel>();
		Transform description = planetInfoBase.Find("DescriptionGroup");
		descriptionText = description.Find("Description").GetComponent<UILabel>();
		
		Transform resources = planetInfoBase.Find("ResourcesGroup");
		populationValue = resources.Find("PopulationValue").GetComponent<UILabel>();
		materialsValue = resources.Find("MaterialsValue").GetComponent<UILabel>();
		energyValue = resources.Find("EnergyValue").GetComponent<UILabel>();
		
		ships = transform.Find("Ships");
		shipsList = ships.transform.Find("ShipsBody");

	}
	
	public void LoadColony(GameObject currentPlanet)
	{
		Planet currentPlanetComponent = currentPlanet.GetComponent<Planet>();
		planet = currentPlanetComponent;
		PlanetData currentPlanetData = currentPlanetComponent.planetInfo;
		
		//Load the menu with data for the current planet
		planetName.text = currentPlanetData.planetName;
		descriptionText.text = currentPlanetData.description;
		populationValue.text = currentPlanetData.population;
		materialsValue.text = currentPlanetData.materials;
		energyValue.text = currentPlanetData.energy;
		
		int shipNumber = 0;
		foreach(Landable ship in currentPlanetComponent.GetLandedShips())
		{
			AddShip(ship, shipNumber);
			shipNumber++;
		}
	}
	
	public void AddShip(Landable ship, int index)
	{
		GameObject buttonObject = NGUITools.AddChild(shipsList.gameObject, shipButtonPrefab);
		//panel.Refresh();
		Vector3 buttonObjectPosition = new Vector3(70 * index, 0 , 1);
		buttonObject.transform.localPosition = buttonObjectPosition;
		//BroadcastMessage("CheckParent", SendMessageOptions.DontRequireReceiver);
		ShipButton button = buttonObject.transform.Find("ShipButtonWidget").GetComponent<ShipButton>();
		button.Setup(ship, planet);
	}
	
}
