using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColonyBuildList : MonoBehaviour {

	/// <summary>
	/// Prefab to use for draggable building names
	/// </summary>
	public GameObject buildingBoxPrefab;
	
	/// <summary>
	/// The size of the collder bounds. Copy here from the collider in inspector
	/// </summary>
	public Vector2 collderBoundsSize;
	
	public bool surfaceMode = false;
	
	UITable structuresTable;
	int buildingCount; //Is this needed?
	//GameObject currentPlanet;
	HashSet<BuildListItemData> avaiblableStructures;
		
	// Use this for initialization
	void Awake () {
		structuresTable = GetComponent<UITable>() as UITable;
		buildingCount = 0;
	}
	
	
	public void LoadBuildings(GameObject planet)
	{
		//currentPlanet = planet; 
		UpdateBuildList(false);
	}
	
	/// <summary>
	/// Updates the build list taking into account both the player's available buildings and the state of the build menu.
	/// </summary>
	public void UpdateBuildList(bool surfaceMode)
	{
		this.surfaceMode = surfaceMode;
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
	
	
	/// <summary>
	/// Returns true if the mouse overlaps the build list. Used to determine when a dragdropitem is being returned
	/// to the list.
	/// </summary>
	public bool CheckMouseOverlap()
	{
		//Get the mouse coordinate relative to this widget's pivot.
		Vector2 mousePositionAsGui = Game.gui.ScreenToUIPoint(Input.mousePosition, gameObject);
		
		//Use the colliderBoundsSize (which must be hand copied from the collder's dimensions in the inspector
		//to determine overlap with the panel.
		Rect bounds = new Rect(0, 0, collderBoundsSize.x, collderBoundsSize.y); 
		return bounds.Contains(new Vector2(mousePositionAsGui.x, -mousePositionAsGui.y));//flip y
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
