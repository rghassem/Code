using UnityEngine;
using System.Collections;

public class Burstable : Spawner {
	
	public GameObject[] fragmentObjects;
	public float[] fragmentObjectCounts;
	
	void Awake()
	{
		if(fragmentObjects.Length != fragmentObjectCounts.Length)
			throw new UnityException("Fragment type list and list of fragment counts are unequal in length");
	}
	
	void Start()
	{
		//Set all of spawner's (parent class) configuration properties to hard coded values
		//for burstable's specific use case
		
	}
	
	
	/// <summary>
	/// Spawn a bunch of the fragment objects in an explosion pattern, as though they had "burst"
	/// out of this object. Likely to call right before GameObject destruction
	/// </summary>
	public void Burst()
	{
		
	}
}

