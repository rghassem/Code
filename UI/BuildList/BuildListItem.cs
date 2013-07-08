using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DragDropItem))]
public class BuildListItem : MonoBehaviour {
	
	public BuildListItemData data {get; protected set;}
	
	[HideInInspector]
	public bool ignoreDropSurfaces;
	
	UILabel label;
	
	//Structure currentPlacingStructure;
	GameObject placementPrefab;
	bool isBeingDragged;
	DragDropItem dragBehavior; // the NGUI script controlling the dragging
	Planet currentPlanet;
	BuildableShadow dropShadow;
	
	ColonyBuildList buildList;
	
	public static BuildListItem Spawn(GameObject host, GameObject creator, BuildListItemData buildingData)
	{
		host.transform.parent = creator.transform;
		host.transform.localPosition = Vector3.zero;
		host.transform.localScale = Vector3.one;
		
		BuildListItem buildingController = host.GetComponent<BuildListItem>();
		
		buildingController.buildList = creator.GetComponent<ColonyBuildList>();
		buildingController.Initialize(buildingData);
		buildingController.enabled = true;
		
		return buildingController;
	}
	
	public void Initialize(BuildListItemData buildingData)
	{
		data = buildingData;
		label = transform.Find("Label").GetComponent<UILabel>();
		dragBehavior = GetComponent<DragDropItem>();
		
		label.text = buildingData.buildingName;
		this.placementPrefab = buildingData.prefab;
		
		dragBehavior.prefab = placementPrefab;
	}
	
	#region Events
	
	void OnDragStart()
	{
		Game.mainCamera.lockInput = true; //Stop the camera from responding to drag controls...
		
		currentPlanet = Game.gui.planetMenu.planet.GetComponent<Planet>();
		isBeingDragged = true; 
		
		if(data.buildingType != BuildingType.Surface)
		{
			dropShadow = BuildableShadow.Spawn(placementPrefab, currentPlanet.gameObject);
			ignoreDropSurfaces = true;
		}
		else 
		{
			dropShadow = BuildableShadow.Spawn(placementPrefab);
			ignoreDropSurfaces = false;
		}
		
		dropShadow.collider.enabled = false; //collider must be disabled, otherwise it will intercept the drop event before
		//the drag drop surface can pick it up.
	}
	
	void OnDropFail()
	{
		EndDragging();
	}
	
	void OnDropSuccess()
	{
		EndDragging();
		currentPlanet.StartCoroutine("RefreshStructuers");
	}
	
	void Update()
	{
		if(isBeingDragged)
		{
			DrawBuildArea();
		}
	}
	
	void EndDragging()
	{
		isBeingDragged = false;
		Game.mainCamera.lockInput = false;
		
		//Destroy shadow if it goes on the planet surface where NGUI's DragDropSurface will instantiate the real thing
		//also destroy if it is in an invalid state for building, such as if overlapping another object.
		if(data.buildingType == BuildingType.Surface || 
			!dropShadow.canBeBuilt )
			Destroy(dropShadow.gameObject);
		else //turn the shadow into the real object
			dropShadow.Build();
	}
	
	#endregion

	/// <summary>
	/// Performs logic to draw build shadow for planet surface objects
	/// </summary>
	void DrawBuildArea()
	{
		//Cast a ray against the planet 
		Ray rayFromMouse = Game.mainCamera.camera.ScreenPointToRay(Input.mousePosition);
		
		if(data.buildingType == BuildingType.Surface) //check against planet
		{
			RaycastHit hit;
			
			if( currentPlanet.gameObject.collider.Raycast(rayFromMouse, out hit, 
				Game.mainCamera.ACTION_CAMERA_DISTANCE * 2) //distance just needs to be a good size
			  )
			{
				dropShadow.transform.position = hit.point;
				dropShadow.DrawDependencyInfo(currentPlanet);
			}
			else
			{
				dropShadow.transform.position = Vector3.zero;
				dropShadow.ClearDependencyInfo();
			}
		}
		else //check against space
		{
			Plane yEqualsZero = new Plane(Vector3.up, 0);
			float distance;
			yEqualsZero.Raycast(rayFromMouse,  out distance);
			Vector3 point = rayFromMouse.GetPoint(distance);
			
			if(!Physics.CheckSphere(point, dropShadow.collider.bounds.extents.magnitude) 
				&& !buildList.CheckMouseOverlap()
				&& currentPlanet.territory.Contains(point))
			{
				//Nothing overlapping
				dropShadow.transform.position = point;
				dropShadow.canBeBuilt = true;
			}
			else
			{
				//Overlapping something
				dropShadow.transform.position = new Vector3(0, 1000, 0); //put it where it won't be seen
				dropShadow.canBeBuilt = false;
			}
		}
	}
	
}

public class BuildListItemData
{
	public string buildingName;
	public BuildingType buildingType;
	public GameObject prefab;
	
	public BuildListItemData(string name, BuildingType type, GameObject prefab)
	{
		buildingName = name;
		buildingType = type;
		this.prefab = prefab;
	}
}

public enum BuildingType
{
	Surface,
	Orbital,
	Ship
}
