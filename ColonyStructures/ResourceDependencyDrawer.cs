using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceDependencyDrawer : MonoBehaviour {
	
	Dictionary<GameObject, VectorLine> dependencyLines; 
	
	#region events
	// Use this for initialization
	void Awake () 
	{
		dependencyLines = new Dictionary<GameObject, VectorLine>();
	}
	
	void OnDestroy()
	{
		DeleteDependencyLines();
	}
	#endregion
	
	#region public interface
	/// <summary>
	/// Draw lines and colors related to dependency, and return whether placement is valid or not
	/// </summary>
	public void DrawDependencyInfo(Planet planet, Structure structure)
	{
		List<Structure> neighbors = planet.GetNeigboringStructures(transform.position);
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
		ClearDependencyLines();
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
