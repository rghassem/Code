using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The "shadow" that indicates where a buildable object will be placed. Holds a reference to the buildable object itself.
/// </summary>
///
[RequireComponent(typeof(ResourceDependencyDrawer))]
public class BuildableShadow : MonoBehaviour {
	
	[HideInInspector]
	public GameObject buildable;
	public bool canBeBuilt = false;
	
	ResourceDependencyDrawer dependencies;
	GameObject center;
	
	public static BuildableShadow Spawn(GameObject buildablePrefab)
	{ return BuildableShadow.Spawn(buildablePrefab, null); }
	
	public static BuildableShadow Spawn(GameObject buildablePrefab, GameObject center)
	{
		GameObject obj = Instantiate(buildablePrefab) as GameObject;
		MonoBehaviour[] components = obj.GetComponentsInChildren<MonoBehaviour>();
		foreach( MonoBehaviour compontent in components)
		{
			compontent.enabled = false;
		}
		if(obj.renderer != null) obj.renderer.enabled = true;
		
		BuildableShadow shadow = obj.AddComponent<BuildableShadow>();
		shadow.buildable = buildablePrefab;
		shadow.center = center;
		
		shadow.InitializeMembers();
		return shadow;
	}
	
	private void InitializeMembers()
	{
		dependencies = GetComponent<ResourceDependencyDrawer>();
	}
	
	void Update()
	{
		if(center != null)
			transform.LookAt( transform.position + (transform.position - center.transform.position).normalized);
	}
	
	public void DrawDependencyInfo(Planet planet)
	{
		dependencies.DrawDependencyInfo(planet, buildable.GetComponent<Structure>());
	}
		
	public void ClearDependencyInfo()
	{
		dependencies.ClearDependencyInfo();
	}
	
	/// <summary>
	/// Do the reverse of the spawn method. Enable all aspects of the component and remove this script, thus rendering it a normal
	/// instance of itself. Used when the drop shadow object is the buildable object itself.
	/// </summary>
	public void Build()
	{
		if(!canBeBuilt)
			return;
		
		MonoBehaviour[] components = gameObject.GetComponentsInChildren<MonoBehaviour>();
		foreach( MonoBehaviour compontent in components)
		{
			compontent.enabled = true;
		}
		
		//Make certain colliders start ok
		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
		foreach( Collider collider in colliders)
		{
			collider.enabled = true;
		}
		
		Destroy(this);
	}
	
	

	
}
