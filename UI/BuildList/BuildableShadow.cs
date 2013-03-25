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
	
	ResourceDependencyDrawer dependencies;
	
	public static BuildableShadow Spawn(GameObject buildablePrefab)
	{
		GameObject obj = Instantiate(buildablePrefab) as GameObject;
		MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
		foreach( MonoBehaviour compontent in components)
		{
			compontent.enabled = false;
		}
		if(obj.renderer != null) obj.renderer.enabled = true;
		
		BuildableShadow shadow = obj.AddComponent<BuildableShadow>();
		shadow.buildable = buildablePrefab;
		
		shadow.InitializeMembers();
		return shadow;
	}
	
	private void InitializeMembers()
	{
		dependencies = GetComponent<ResourceDependencyDrawer>();
	}
	
	public void DrawDependencyInfo(Planet planet)
	{
		dependencies.DrawDependencyInfo(planet, buildable.GetComponent<Structure>());
	}
		
	public void ClearDependencyInfo()
	{
		dependencies.ClearDependencyInfo();
	}
	
	

	
}
