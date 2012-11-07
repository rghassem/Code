using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base Class for planetary structures. Will be dragged onto planet are clickable
/// </summary>
public class Structure : MonoBehaviour {
	
	public static readonly float DEFAULT_OUTPUT_RANGE = 10;
	
	public float outputRange;
	
	public string structureName;
	public ResourceType resourceType;
	public int instantProduction;
	public int productionPerMinute;
	
	public LocalResource[] input;
	public LocalResource output;
	
	
	// Use this for initialization
	protected virtual void Start () {
		
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
	public bool checkRequirements(params LocalResource[] requirements)
	{
		//TODO: More robust reqs checking
		if(requirements.Length == 0)
			return false;
		return requirements[0].type == LocalResourceType.Power &&
			requirements[0].quantity >= 5;
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
