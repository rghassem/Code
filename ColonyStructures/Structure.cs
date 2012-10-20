using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base Class for planetary structures. Will be dragged onto planet are clickable
/// </summary>
public class Structure : MonoBehaviour {

	
	public string structureName;
	public ResourceType resourceType;
	public int instantProduction;
	public int productionPerMinute;
	
	public LocalResource[] input;
	public LocalResource output;
	
	
	// Use this for initialization
	protected virtual void Start () {
		//temporary:
		transform.localScale /= 8;
		input = new  LocalResource[0];
		output = new LocalResource();
		output.type = LocalResourceType.Power;
		output.quantity = 5;
	}
	
	/// <summary>
	/// Takes a list production requisites and returns true if they meet this structure's build requirements
	/// </summary>
	public bool checkRequirements(params LocalResource[] requirements)
	{
		return true;
	}
	
	public LocalResource getOutput()
	{
		return output;
	}
	
	void OnClick()
	{
		Game.gui.planetMenu.showStructurePopup(this);
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
