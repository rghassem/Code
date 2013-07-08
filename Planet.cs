using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Planet : SelectableBody {
		
	public PlanetData planetInfo;
	private float lastClickTime;
	public SafeZone territory;

	float territoryRadius = 50; //should be determined by colony size

	
	const float MAX_SPIN_VELOCITY = 10;
	const float SPIN_DRAG = 2f;
	float spinVelocity;
		
	
	//Structures
	List<Structure> builtStructures;
		
	void Awake()
	{
		planetInfo = ScriptableObject.CreateInstance<PlanetData>();
		planetInfo.planetName = "Earth";
		planetInfo.description = "Homeworld of humanity, and the starting point of this game.";
		planetInfo.population = "7 billion";
		planetInfo.materials = "1500";
		planetInfo.energy = "500";
		
		lastClickTime = 0;	
	}
	
	void Start()
	{		
		//Create floating label
		cameraMatchRotationWhenSelected = false;
		Game.gui.labelPool.Label(gameObject,
									new Vector3(0, gameObject.transform.lossyScale.magnitude + 10, 0), 
									planetInfo.planetName);
		builtStructures = new List<Structure>();
		territory = SafeZone.Spawn(territoryRadius, gameObject);
		territory.ShowRing();
	}
	
	
	public override float GetSelectionRadius()
	{
		return renderer.bounds.extents.magnitude;
	}
	
	/// <summary>
	/// Refreshs the structuers from the gameObject's children on the next frame
	/// </summary>
	public IEnumerator RefreshStructuers()
	{
		yield return new WaitForSeconds(1); //wait till last minute for parenting process to finish
		builtStructures = new List<Structure>();
		foreach(Transform child in transform)
		{
			Structure structure = child.GetComponent<Structure>();
			if(structure != null)
				builtStructures.Add(structure);
		}
	}
	
	/// <summary>
	/// Returns structures for which the given surface coordinates are within resource-sharing range
	/// </summary>
	public List<Structure> GetNeigboringStructures(Vector3 surfaceCoords)
	{
		List<Structure> neigboringStructures = new List<Structure>();
		
		Vector3 centerOfPlanet = gameObject.transform.position;
		Vector3 centerToGivenCoords = centerOfPlanet - surfaceCoords;
		float radius = (gameObject.collider as SphereCollider).radius * transform.localScale.magnitude;
		float circumference = 2 * Mathf.PI * radius;
		
		foreach(Structure structure in builtStructures)
		{
			Vector3 centerToStructure = centerOfPlanet - structure.gameObject.transform.position;
			float acceptenceAngle = structure.outputRange * 360 / circumference;
			float angleToGivenCoords = Vector3.Angle(centerToStructure, centerToGivenCoords);
			if(angleToGivenCoords <= acceptenceAngle)
				neigboringStructures.Add(structure);
		}
		
		return neigboringStructures;
	}
	
	/// <summary>
	/// Check whether given Landable can land here.
	/// </summary>
	/// <returns>
	/// True if landing permission granted
	/// </returns>
	public bool RequestLanding(Landable landingObject)
	{
		Ship landingShip = landingObject.GetComponent<Ship>();
		if(landingShip == null)
			return false;
		
		ShipData data = landingShip.GetShipData();
		if(data == null)
			return false;
		
		return true;
	}
	/// <summary>
	/// Signal to launch the given landable
	/// </summary>
	/// <returns>
	/// True if allowed to launch
	/// </returns>
	public bool RequestLaunch(Landable landedCraft)
	{
		landedCraft.Launch( transform.position + transform.localScale + landedCraft.transform.lossyScale);
		return true;
	}
	
	/// <summary>
	/// Return a list of all landed landables. Will return an empty list if there aren't any.
	/// </summary>
	public List<Landable> GetLandedShips()
	{
		List<Landable> results = new List<Landable>();
		GameObject child;
		for(int i = 0; i < transform.childCount; i++)
		{
			child = transform.GetChild(i).gameObject;
			Landable landable = child.GetComponent<Landable>();
			if(landable != null)
				results.Add(landable);
		}
		return results;
	}
	
	
	public void OnPlayerSelect()
	{
		Game.lastPlanet = this;
		
		if(Time.time - lastClickTime < Game.DOUBLE_CLICK_THRESHHOLD_INTERVAL 
			&& Time.time > Game.DOUBLE_CLICK_THRESHHOLD_INTERVAL) //prevent single automatic selects in first seconds from passing
		{
			//Lock the game and camera on this planet
			Game.lockSelection = true;
			Game.mainCamera.SwitchViewAngle(CameraViewMode.Perspective);
			Game.gui.planetMenu.EnterPlanetView();
		}
		else lastClickTime = Time.time;
	}
	
	public void OnDrag(Vector2 delta)
	{
		if(Game.SelectedObject == gameObject)
		{
			spinVelocity -= MAX_SPIN_VELOCITY * Time.deltaTime * Mathf.Sign(delta.x);
		}
	}
	
	void Update()
	{
		if(Game.SelectedObject == gameObject)
		{
			if(spinVelocity != 0)
			{
				gameObject.transform.RotateAroundLocal( Vector3.up, spinVelocity * Time.deltaTime);
				//pull back toward 0
				float newSpinVelocity = Mathf.Abs(spinVelocity) - SPIN_DRAG * Time.deltaTime;
				if(newSpinVelocity < 0)
					newSpinVelocity = 0;
				spinVelocity = newSpinVelocity * Mathf.Sign(spinVelocity);
			}
		}
	}
}
