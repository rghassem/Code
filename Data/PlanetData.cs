using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A data object holding the name/description and current state of a planet
/// </summary>
public class PlanetData : ScriptableObject
{
	//Basic stuff
	public string planetName;
	public string description;
	
	//Resources
	public string population;
	public string materials;
	public string energy;
	
	public List<ShipData> shipsInPort;
	
	public PlanetData()
	{
		shipsInPort = new List<ShipData>();
	}
	
}

