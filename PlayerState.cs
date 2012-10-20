using System.Collections;
using System.Collections.Generic;

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
		possibleBuildings.Add(new BuildListItemData("Nuclear Plant", BuildingType.Power));
	}
	
}