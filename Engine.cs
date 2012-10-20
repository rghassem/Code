using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {
	
	private readonly float MAX_TILT_ANGLE = 30; //maximum angle for the ship to tilt when turning, in degrees
	private readonly float TILT_SPEED = 90; //Degrees the ship can tilt in one second

	
	//Flight variables
	public float engineForce;	
	public float totalFuel;
	public float fuelConsumptionPerSecond;
	public float currentFuel;
	public float boostForce;		//This should eventually be a limited resource...

	
	//Effects
	private RocketTrail rocketTrail;
	
	private bool isEnabled;
	
	//Variables for keeping track of tile
	private float targetTilt;
	
	// Use this for initialization
	void Awake () {
		rocketTrail = transform.Find("RocketTrail").GetComponent<RocketTrail>();
		currentFuel = totalFuel;
		targetTilt = 0;
	}
	
	void Update()
	{
		//tilt the ship toward the direction its being turned
		float currentTilt = transform.eulerAngles.z < 180 ? transform.eulerAngles.z : transform.eulerAngles.z - 360;
		float deltaTilt = targetTilt - currentTilt;
		float newTilt = currentTilt + Mathf.Sign(deltaTilt) * 
													Mathf.Min(Mathf.Abs(deltaTilt), (TILT_SPEED*Time.deltaTime));
		Quaternion tilt = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, newTilt);
		transform.rotation = tilt;
	}
	
	public void Disable()
	{
		rocketTrail.Stop();
		isEnabled = false;
	}
	
	public void Enable()
	{
		isEnabled = true;
		targetTilt = 0; 
	}
	
	public void Fly(Vector3 direction)
	{
		if(!isEnabled)
			return;
		
		Vector3 force = direction * engineForce;
		if(currentFuel > 0 && force.magnitude > 0)
		{
			//Add force
			rigidbody.AddForce(force, ForceMode.Force);
			
			//Tilt ship in force direction
			float lateralForceComponent = (transform.worldToLocalMatrix * direction).normalized.x;
			targetTilt = MAX_TILT_ANGLE * -lateralForceComponent;
			
			//deplete fuel
			currentFuel -= fuelConsumptionPerSecond * Time.deltaTime;
			Game.gui.shipDisplay.SetFuelDisplay(currentFuel/totalFuel);

			
			//play effect
			rocketTrail.Play();
		}
		else
		{
			rocketTrail.Stop();
			targetTilt = 0;
		}
	}
	
	public void Boost(Vector3 direction)
	{
		rigidbody.AddForce(direction * boostForce * Time.deltaTime);
	}
	
}
