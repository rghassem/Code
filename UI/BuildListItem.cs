using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DragDropItem))]
public class BuildListItem : MonoBehaviour {
	
	public BuildListItemData data {get; protected set;}
	
	UILabel label;
	
	Structure currentPlacingStructure;
	bool isBeingDragged;
	DragDropItem dragBehavior; // the NGUI script controlling the dragging
	Planet currentPlanet;
		
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
		currentPlanet = Game.gui.planetMenu.planet.GetComponent<Planet>();
		isBeingDragged = true; 
		GameObject marker = GameObject.Instantiate(dragBehavior.prefab) as GameObject;
		currentPlacingStructure = marker.GetComponent<Structure>();
		//We need to turn off the collider during dragging, otherwise it can intercept NGUI drop events
		if(marker.collider != null)
			marker.collider.enabled = false;
	}
	
	void OnDropFail ()
	{
		EndDragging();
	}
	
	void OnDropSuccess()
	{
		EndDragging();
		currentPlanet.StartCoroutine("RefreshStructuers");
	}
	
	void EndDragging()
	{
		isBeingDragged = false; 
		if(currentPlacingStructure != null)
		{
			Destroy(currentPlacingStructure.gameObject);
			currentPlacingStructure.ClearDependencyInfo();
		}
	}
	
	void Update()
	{
		if(isBeingDragged)
		{
			//Cast a ray against the planet 
			Ray rayFromMouse = Game.mainCamera.camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if( currentPlanet.gameObject.collider.Raycast(rayFromMouse, out hit, 
				Game.mainCamera.ACTION_CAMERA_DISTANCE * 2) //distance just needs to be a good size
			  )
			{
				currentPlacingStructure.transform.position = hit.point;
				currentPlacingStructure.DrawDependencyInfo(currentPlanet);
			}
			else
			{
				currentPlacingStructure.transform.position = Vector3.zero;
				currentPlacingStructure.ClearDependencyInfo();
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
