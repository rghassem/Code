using UnityEngine;
using System.Collections;

/// <summary>
/// This class merely provides a queryable list of buildable objects. It should be instanced once on a God object (ie, use as a singleton)
/// </summary>
public class BuildableRegistry : MonoBehaviour {
	
	public GameObject[] buildableObjects;
	
	public GameObject GetBuildableObjectByName(string name)
	{
		for(int i = 0; i < buildableObjects.Length; i++)
		{
			if(buildableObjects[i].name == name)
				return buildableObjects[i];
		}
		return null;
	}
}
