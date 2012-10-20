using UnityEngine;
using System.Collections;

public class BuildListItem : MonoBehaviour {
	
	public BuildListItemData data {get; protected set;}
	
	UILabel label;
	
	// Use this for initialization
	void Awake () {
		label = transform.Find("Label").GetComponent<UILabel>();
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
