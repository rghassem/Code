using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColonyBuildList : MonoBehaviour {

	/// <summary>
	/// Prefab to use for draggable building names
	/// </summary>
	public GameObject buildingBoxPrefab;
	
	UITable structuresTable;
	int buildingCount;
	GameObject currentPlanet;
	HashSet<BuildListItemData> avaiblableStructures;
		
	// Use this for initialization
	void Awake () {
		structuresTable = GetComponent<UITable>() as UITable;
		buildingCount = 0;
	}
	
	
	public void LoadBuildings(GameObject planet)
	{
		currentPlanet = planet; 
		UpdateBuildList(false);
	}
	
	/// <summary>
	/// Updates the build list taking into account both the player's available buildings and the state of the build menu.
	/// </summary>
	public void UpdateBuildList(bool surfaceMode)
	{
		avaiblableStructures = new HashSet<BuildListItemData>();
		foreach(BuildListItemData building in Game.state.possibleBuildings)
		{
			//only add buildings for the correct mode
			if(building.buildingType == BuildingType.Surface && !surfaceMode || 
			   building.buildingType != BuildingType.Surface && surfaceMode)
				continue;
			
			avaiblableStructures.Add(building);
		}
		RefreshList();
	}
	
	private void RefreshList()
	{
		foreach(Transform child in transform)
		{
			Destroy(child.gameObject);
		}
				
		foreach(BuildListItemData buildable in avaiblableStructures)
		{
			AddBuildable(buildable);
		}
		structuresTable.repositionNow = true;
	}
	
	
	/*public void OnChildMoving(GameObject child)
	{
	}*/
	
	public void OnChildMoved(GameObject child)
	{
		BuildListItemData missingData = child.GetComponent<BuildListItem>().data;
		RestoreOneItem(missingData);
	}
	
	
	private void RestoreOneItem(BuildListItemData item)
	{
		AddBuildable(item);
		structuresTable.Reposition();
	}
	
	private void AddBuildable(BuildListItemData buildingData)
	{
		GameObject newBuilding = Instantiate(buildingBoxPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		BuildListItem.Spawn(newBuilding, gameObject, buildingData);
		buildingCount++;
	}
	
}
