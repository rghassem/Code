using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DragDropItem))]
public class BuildListItem : MonoBehaviour {
	
	public BuildListItemData data {get; protected set;}
	
	UILabel label;
	
	GameObject marker;
	bool isBeingDragged;
	DragDropItem dragBehavior; // the NGUI script controlling the dragging
	
	// Use this for initialization
	void Awake () {
		label = transform.Find("Label").GetComponent<UILabel>();
		dragBehavior = GetComponent<DragDropItem>();
	}
	
	public void Initialize(string name, BuildingType type)
	{
		Initialize(new BuildListItemData(name, type));
	}
	
	public void Initialize(BuildListItemData buildingData)
	{
		data = buildingData;
		label.text = buildingData.buildingName;
	}
	
	//DragDropItem Events
	void OnDragStart()
	{
		isBeingDragged = true; 
	}
	
	void OnDropFail ()
	{
		isBeingDragged = false; 
		if(marker != null)
			Destroy(marker);
	}
	
	void OnDropSuccess()
	{
		isBeingDragged = false; 
		if(marker != null)
			Destroy(marker);
	}
	
	void Update()
	{
		if(isBeingDragged)
		{
			//initialize the marker if it hasn't been already
			if(marker == null)
				marker = Instantiate(GameObject.Find("Marker")) as GameObject;
			//Cast a ray against the planet 
			Ray rayFromMouse = Game.mainCamera.camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if( Game.gui.planetMenu.planet.collider.Raycast(rayFromMouse, out hit, 
				Game.mainCamera.ACTION_CAMERA_DISTANCE * 2) //distance just needs to be a good size
			  )
			{
				marker.transform.position = hit.point;
			}
			else
			{
				marker.transform.position = Vector3.zero;
			}
		}
			
	}
	
}

public class BuildListItemData
{
	public string buildingName;
	public BuildingType buildingType;
	
	public BuildListItemData(string name, BuildingType type)
	{
		buildingName = name;
		buildingType = type;
	}
}

public enum BuildingType
{
	Power,
	Food,
	Housing,
	Mineral,
	Research,
	Special
}
