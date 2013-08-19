using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceDependencyDrawer : MonoBehaviour {
	
	Dictionary<GameObject, VectorLine> dependencyLines;
	
	Planet planet;
	Structure structure;
	List<Structure> neighbors;
	
	#region events
	// Use this for initialization
	void Awake () 
	{
		dependencyLines = new Dictionary<GameObject, VectorLine>();
		planet = null;
		structure = null;
		neighbors = new List<Structure>();
	}
	
	void OnDestroy()
	{
		DeleteDependencyLines();
	}
	
	void Update()
	{
		foreach(KeyValuePair<GameObject, VectorLine> pair in dependencyLines)
		{
			//This code is somewhat redundent with the code in UpdateDependencyLine
			
			Vector2 screenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(transform.position);
		
			Vector2 structureScreenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(pair.Key.transform.position);
			
			Vector2[] screenCoordinates = new Vector2[2] {screenCoords, structureScreenCoords};
			pair.Value.points2 = screenCoordinates;
			pair.Value.Draw();
		}
	}
	#endregion
	
	#region public interface
	
	
	
	/// <summary>
	/// Draw lines and colors related to dependency.
	/// </summary>
	public void DrawDependencyInfo(Planet planet, Structure structure)
	{
		this.planet = planet;
		this.structure = structure;
		
		List<Structure> neighbors = planet.GetNeigboringStructures(transform.position);
		this.neighbors = neighbors;
		
		bool isValid = false;
		ClearDependencyLines();
		
		foreach(Structure neighbor in neighbors)
		{
			if(structure.CheckRequirements(neighbor.GetOutput()))
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
		//ClearDependencyLines();
		DeleteDependencyLines();
		renderer.material.color = Color.white;
	}
	
	private void DeleteDependencyLines()
	{
		foreach(GameObject key in dependencyLines.Keys)
		{
			VectorLine line = dependencyLines[key];
			VectorLine.Destroy(ref line);
		}
		dependencyLines = new Dictionary<GameObject, VectorLine>();
	}
	#endregion
	
	
	void UpdateDependencyLine(GameObject dependency)
	{
		//Somewhat redundent with the update event
		Vector2 screenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(transform.position);
		
		Vector2 structureScreenCoords = 
			Game.mainCamera.camera.WorldToScreenPoint(dependency.transform.position);
		
		if(dependencyLines.ContainsKey(dependency))
		{
			Vector2[] screenCoordinates = new Vector2[2] {screenCoords, structureScreenCoords};
			dependencyLines[dependency].points2 = screenCoordinates;
			dependencyLines[dependency].active = true;
			dependencyLines[dependency].Draw();
		}
		else
		{
			VectorLine line = VectorLine.SetLine(Color.green, screenCoords, structureScreenCoords);
			dependencyLines.Add(dependency, line);
		}
	}
	
	void ClearDependencyLine(GameObject dependency)
	{
		if(dependencyLines == null)
			dependencyLines = new Dictionary<GameObject, VectorLine>();
		
		if(dependencyLines.ContainsKey(dependency))
		{
			dependencyLines[dependency].active = false;
		}
	}
	
	void ClearDependencyLines()
	{
		if(dependencyLines == null)
			dependencyLines = new Dictionary<GameObject, VectorLine>();
		
		foreach(VectorLine line in dependencyLines.Values)
		{
			line.active = false;
		}
	}
	
	
}
