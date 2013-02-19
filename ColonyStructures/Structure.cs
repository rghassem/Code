using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base Class for planetary structures. Will be dragged onto planet are clickable
/// </summary>
public class Structure : SelectableBody {
	
	public static readonly float DEFAULT_OUTPUT_RANGE = 10;
	
	public float outputRange;
	
	public string structureName;
	public ResourceType resourceType;
	public int instantProduction;
	public int productionPerMinute;
	
	public LocalResource[] input;
	public LocalResource output;
	
	Dictionary<GameObject, VectorLine> dependencyLines;
	Color defaultColor;
	
	Planet planet;
	
	// Use this for initialization
	protected virtual void Start () {
		dependencyLines = new Dictionary<GameObject, VectorLine>();
		defaultColor = renderer.material.color;
		
		if(transform.parent != null)
		{
			planet = transform.parent.GetComponent<Planet>();
			if(planet != null)
			{
				//if(!CheckRequirements(planet))
				//	Destroy(gameObject);
			}
		}
		
		//maintain global scale
		if(transform.parent != null)
		{
			Vector3 correctedScale = new Vector3(transform.localScale.x / transform.parent.localScale.x,
												 transform.localScale.y / transform.parent.localScale.y,
												 transform.localScale.z / transform.parent.localScale.z);
			transform.localScale = correctedScale; 
		}
		
		//Initialize
		input = new  LocalResource[0];
		output = new LocalResource();
		output.type = LocalResourceType.Power;
		output.quantity = 5;
		
		outputRange = DEFAULT_OUTPUT_RANGE;
	}
	
	/// <summary>
	/// Takes a list production requisites and returns true if they meet this structure's build requirements
	/// </summary>
	public bool CheckRequirements(params LocalResource[] requirements)
	{
		//TODO: More robust reqs checking
		if(requirements.Length == 0)
			return false;
		return requirements[0].type == LocalResourceType.Power &&
			requirements[0].quantity >= 5;
	}
	
	public LocalResource GetOutput()
	{
		return output;
	}
	
	//TODO: Check ALL reqs, not one, for both this function and DrawDependencies
	
	private bool CheckRequirements(Planet planet)
	{
		List<Structure> neighbors = planet.GetNeigboringStructures(transform.position);
		foreach(Structure neighbor in neighbors)
		{
			if(CheckRequirements(neighbor.GetOutput()))
			{
				return true;
			}
		}
		return false;
	}
	
	/// <summary>
	/// Draw lines and colors related to dependency, and return whether placement is valid or not
	/// </summary>
	public void DrawDependencyInfo(Planet planet)
	{
		List<Structure> neighbors = planet.GetNeigboringStructures(transform.position);
		bool isValid = false;
		ClearDependencyLines();
		foreach(Structure neighbor in neighbors)
		{
			if(CheckRequirements(neighbor.GetOutput()))
			{
				UpdateDependencyLine(neighbor.gameObject);
				isValid = true;
			}
			else
				ClearDependencyLine(neighbor.gameObject);
		}
		if(isValid)
			renderer.material.color = Color.green;
		else
		{
			renderer.material.color = Color.red;
		}
	}
	
	public void ClearDependencyInfo()
	{
		if(dependencyLines == null)
			dependencyLines = new Dictionary<GameObject, VectorLine>();
		ClearDependencyLines();
		renderer.material.color = defaultColor;
	}
	
	void OnClick()
	{
		Game.gui.planetMenu.ShowStructurePopup(this);
		if(planet != null)
			DrawDependencyInfo(planet);
	}
	
	//Called when ShowStructurePopup is removed
	public void NotifyClickAway()
	{
		ClearDependencyInfo();
	}
	
	void OnDestroy()
	{
		DeleteDependencyLines();
	}
	
	
	#region DependencyLines

	void UpdateDependencyLine(GameObject dependencyPosition)
	{
		Vector2 screenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(transform.position);
		Vector2 structureScreenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(dependencyPosition.transform.position);
		if(dependencyLines.ContainsKey(dependencyPosition))
		{
			Vector2[] screenCoordinates = new Vector2[2] {screenCoords, structureScreenCoords};
			dependencyLines[dependencyPosition].points2 = screenCoordinates;
			dependencyLines[dependencyPosition].active = true;
			dependencyLines[dependencyPosition].Draw();
		}
		else
		{
			VectorLine line = VectorLine.SetLine(Color.green, screenCoords, structureScreenCoords);
			dependencyLines.Add(dependencyPosition, line);
		}
	}
	
	void ClearDependencyLine(GameObject dependency)
	{
		if(dependencyLines.ContainsKey(dependency))
		{
			dependencyLines[dependency].active = false;
		}
	}
	
	void ClearDependencyLines()
	{
		foreach(VectorLine line in dependencyLines.Values)
		{
			line.active = false;
		}

	}
	
	void DeleteDependencyLines()
	{
		foreach(GameObject key in dependencyLines.Keys)
		{
			VectorLine line = dependencyLines[key];
			VectorLine.Destroy(ref line);
		}
		dependencyLines = new Dictionary<GameObject, VectorLine>();
	}
	#endregion
	
}
public enum ResourceType
{
	Energy,
	Metals,
	Population,
	Research
}

public enum LocalResourceType
{
	Power,
	Food
}

public class LocalResource
{
	public int quantity;
	public LocalResourceType type;
}
