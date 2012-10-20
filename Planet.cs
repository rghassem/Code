using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Planet : SelectableBody {

	public PlanetData planetInfo;
	private float lastClickTime;
	
	void Awake()
	{
		planetInfo = ScriptableObject.CreateInstance<PlanetData>();
		planetInfo.planetName = "Earth";
		planetInfo.description = "Homeworld of humanity, and the starting point of this game.";
		planetInfo.population = "7 billion";
		planetInfo.materials = "1500";
		planetInfo.energy = "500";
		
		lastClickTime = 0;	
		rigidbody.isKinematic = true;
		rigidbody.centerOfMass = Vector3.zero;
	}
	
	void Start()
	{
		//Create floating label
		cameraMatchRotationWhenSelected = false;
		Game.gui.labelPool.Label(gameObject,
									new Vector3(0, gameObject.transform.lossyScale.magnitude + 10, 0), 
									planetInfo.planetName);
	}
	
	// Update is called once per frame
	void Update () 
	{
		//transform.RotateAround(Vector3.zero, new Vector3(0,1,0),Time.deltaTime*20);
	}
	
	public override float GetSelectionRadius()
	{
		return renderer.bounds.extents.magnitude;
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
		
		if(Time.time - lastClickTime < Game.DOUBLE_CLICK_THRESHHOLD_INTERVAL)
		{
			Game.gui.planetMenu.Open(gameObject);			
		}
		else lastClickTime = Time.time;
	}
	
	public void OnDrag(Vector2 delta)
	{
		Vector3 test = rigidbody.centerOfMass;
		if(Game.SelectedObject == gameObject)
		{
			Vector3 torque = new Vector3(0, -delta.x, 0);
			//rigidbody.AddRelativeTorque( torque, ForceMode.VelocityChange);
			rigidbody.angularVelocity = torque;
			
		}
	}
	
}
