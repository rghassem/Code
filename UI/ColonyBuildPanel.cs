using UnityEngine;
using System.Collections;

public class ColonyBuildPanel : MonoBehaviour {
	
	GameObject structures;
	ColonyBuildList structuresList;

	// Use this for initialization
	void Awake () {
		structures = transform.Find("Structures").gameObject;
		structuresList = structures.GetComponent<ColonyBuildList>() as ColonyBuildList;
	}
	
	public void LoadColony(GameObject currentPlanet)
	{
		structuresList.LoadBuildings(currentPlanet);
	}
}
