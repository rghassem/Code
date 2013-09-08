using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds information on the player's current global game state. Eg, what technologies have been unlocked, how many research
/// points, available buildings and ships, and so on.
/// </summary>
public class PlayerState
{
	public List<BuildListItemData> possibleBuildings;
	
	public PlayerState()
	{
		possibleBuildings = new List<BuildListItemData>();
		
		GameObject powerPlant = Game.buildables.GetBuildableObjectByName("Nuclear Power Plant");
		possibleBuildings.Add(new BuildListItemData("Nuclear Plant", BuildingType.Surface, powerPlant ));
		
		GameObject spaceShip = Game.buildables.GetBuildableObjectByName("BasicShip");
		possibleBuildings.Add(new BuildListItemData("Space Ship", BuildingType.Ship, spaceShip ));
		
		GameObject launcher = Game.buildables.GetBuildableObjectByName("Space Cannon");
		possibleBuildings.Add(new BuildListItemData("Launcher", BuildingType.Orbital, launcher ));

	}
	
}