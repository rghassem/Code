using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base Class for planetary structures. Will be dragged onto planet are clickable
/// </summary>
///
[RequireComponent(typeof(ResourceDependencyDrawer))]
public class Structure : SelectableBody {
	
	public static readonly float DEFAULT_OUTPUT_RANGE = 10;
	
	public float outputRange;
	
	public string structureName;
	public ResourceType resourceType;
	public int instantProduction;
	public int productionPerMinute;
	
	public LocalResource[] input;
	public LocalResource output;
		
	Planet planet;
	
	ResourceDependencyDrawer dependencies;
	
	void Awake()
	{
		dependencies = GetComponent<ResourceDependencyDrawer>();
	}
	
	// Use this for initialization
	protected virtual void Start () {
			
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
		
	void OnClick()
	{
		Game.gui.planetMenu.ShowStructurePopup(this);
		if(planet != null)
			dependencies.DrawDependencyInfo(planet, this);
	}
	
	//Called when ShowStructurePopup is removed
	public void NotifyClickAway()
	{
		dependencies.ClearDependencyInfo();
	}
	
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
