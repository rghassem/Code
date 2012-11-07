using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DragDropItem))]
public class BuildListItem : MonoBehaviour {
	
	public BuildListItemData data {get; protected set;}
	
	UILabel label;
	
	GameObject marker;
	bool isBeingDragged;
	DragDropItem dragBehavior; // the NGUI script controlling the dragging
	Planet currentPlanet;
	Dictionary<GameObject, VectorLine> validityLines;
	
	bool destroyMarker; //Flag to destroy the marker object on the next update.
	
	// Use this for initialization
	void Awake () {
		label = transform.Find("Label").GetComponent<UILabel>();
		dragBehavior = GetComponent<DragDropItem>();
		validityLines = new Dictionary<GameObject, VectorLine>();
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
		marker = GameObject.Instantiate(dragBehavior.prefab) as GameObject;
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
		if(marker != null)
		{
			Destroy(marker);
		}
		DeleteDependencyLines();
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
				marker.transform.position = hit.point;
				
				//Check if placement here is legal
				List<Structure> neighbors = currentPlanet.GetNeigboringStructures(hit.point);
				Structure structure = dragBehavior.prefab.GetComponent<Structure>();
				bool isValid = false;
				ClearDependencyLines();
				foreach(Structure neighbor in neighbors)
				{
					if(structure.checkRequirements(neighbor.getOutput()))
					{
						UpdateDependencyLine(hit.point, neighbor.gameObject);
						isValid = true;
					}
					else
						ClearDependencyLine(neighbor.gameObject);
				}
				if(isValid)
					marker.renderer.material.color = Color.green;
				else
				{
					marker.renderer.material.color = Color.red;
				}
				
			}
			else
			{
				marker.transform.position = Vector3.zero;
				DeleteDependencyLines();
			}
		}
			
	}
	
	void UpdateDependencyLine(Vector3 itemPosition, GameObject dependencyPosition)
	{
		Vector2 markerScreenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(itemPosition);
		Vector2 structureScreenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(dependencyPosition.transform.position);
		if(validityLines.ContainsKey(dependencyPosition))
		{
			Vector2[] screenCoordinates = new Vector2[2] {markerScreenCoords, structureScreenCoords};
			validityLines[dependencyPosition].points2 = screenCoordinates;
			validityLines[dependencyPosition].active = true;
			validityLines[dependencyPosition].Draw();
		}
		else
		{
			VectorLine line = VectorLine.SetLine(Color.green, markerScreenCoords, structureScreenCoords);
			validityLines.Add(dependencyPosition, line);
		}
	}
	
	void ClearDependencyLine(GameObject dependency)
	{
		if(validityLines.ContainsKey(dependency))
		{
			validityLines[dependency].active = false;
		}
	}
	
	void ClearDependencyLines()
	{
		foreach(VectorLine line in validityLines.Values)
		{
			line.active = false;
		}
	}
	
	void DeleteDependencyLines()
	{
		foreach(GameObject key in validityLines.Keys)
		{
			VectorLine line = validityLines[key];
			VectorLine.Destroy(ref line);
		}
		validityLines = new Dictionary<GameObject, VectorLine>();
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
