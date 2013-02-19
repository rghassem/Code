using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetMenuControls : MonoBehaviour
{	
	/// <summary>
	/// the center panel showing all the colony information and ships.
	/// </summary>
	public GameObject mainColonyPanel;
	
	/// <summary>
	/// the left-most panel -- controls building on a given planet
	/// </summary>
	public GameObject buildPanel;
	
	// <summary>
	/// Currently contains only the close button.
	/// </summary>/
	public GameObject rightPanel;
	
	/// <summary>
	/// The UI which displays details for structures
	/// </summary>
	public GameObject structurePopupPanel;
	
	[HideInInspector]
	public GameObject planet;
	
	List<ScalablePanel> childPanels;
	
	ColonyPanel colonySection;
	ColonyBuildPanel buildSection;
	StructurePopupPanel structureInfoSection;

	// Use this for initialization
	void Start ()
	{		
		childPanels = new List<ScalablePanel>();
		childPanels.Add(mainColonyPanel.GetComponent<ScalablePanel>());
		childPanels.Add(buildPanel.GetComponent<ScalablePanel>());
		childPanels.Add(rightPanel.GetComponent<ScalablePanel>());
		
		colonySection = mainColonyPanel.GetComponent<ColonyPanel>();
		buildSection = buildPanel.GetComponent<ColonyBuildPanel>();
		structureInfoSection = structurePopupPanel.GetComponent<StructurePopupPanel>();
	}
	
	/// <summary>
	/// Open the menu, filling it with info passed by planetData.
	/// </summary>
	public void Open(GameObject currentPlanet)
	{
		//Load planet data
		colonySection.LoadColony(currentPlanet);
		buildSection.LoadColony(currentPlanet);
		
		//Show the menu
		foreach(ScalablePanel child in childPanels)
		{
			child.Open();
		}
		
		//Lock the game and camera on this planet
		Game.lockSelection = true;
		Game.mainCamera.SwitchViewAngle(CameraViewMode.Perspective);
		planet = currentPlanet;
	}
	
	public void Close()
	{
		//Hide the menu
		foreach(ScalablePanel child in childPanels)
		{
			child.Close();
		}
		structureInfoSection.Hide();
		
		//Free the selection/camera
		Game.lockSelection = false;
		Game.mainCamera.SwitchViewAngle(CameraViewMode.Top);

	}
	
	/// <summary>
	/// Populates the structue popup with info for the passed in structure, than places and shows it.
	/// </summary>
	/// <param name='structure'>
	/// Structure.
	/// </param>
	public void ShowStructurePopup(Structure structure)
	{
		structureInfoSection.LoadStructure(structure);
	}
		
}


