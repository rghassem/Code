using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {
	
	private readonly float MAX_TILT_ANGLE = 30; //maximum angle for the ship to tilt when turning, in degrees
	private readonly float TILT_SPEED = 90; //Degrees the ship can tilt in one second
	private readonly float BOOST_EFFECT_DURATION_SEC = 0.3f;
	private readonly float TILT_CORRECTION_FORCE = 360; //degrees per second
	private readonly int BOOST_FLAME_EFFECT_MULTIPLIER = 5;
	
	public float MaxAngularSpeed; //Rotational speed in degrees per second
	
	//Flight variables
	public float engineForce;	
	public float totalFuel;
	public float fuelConsumptionPerSecond;
	public float currentFuel;
	public float boostForce;		//This should eventually be a limited resource...
	public float boostRecharge;
	public float boostFuelCost;
	
	private float lastBoostTime = -1;
	
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
		
		CorrectTilt();
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
			//float lateralForceComponent = (transform.worldToLocalMatrix * direction).normalized.x;
			//targetTilt = MAX_TILT_ANGLE * -lateralForceComponent;
			
			ConsumeFuel( fuelConsumptionPerSecond * Time.deltaTime );
			
			//play effect
			rocketTrail.Play();
		}
		else
		{ 
			rocketTrail.Stop();
			//targetTilt = 0;
		}
	}
	
	
	public float TurnTowards(Vector3 direction)
	{
		Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
		Quaternion rotationThisFrame = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime*MaxAngularSpeed);
		float angle; Vector3 axis;
		rotationThisFrame.ToAngleAxis(out angle, out axis);
		return Turn (angle/360);
	}
	
	public float Turn(float axis)
	{
		axis = Mathf.Clamp(axis, -1, 1);
		float angle = MaxAngularSpeed * axis * Time.deltaTime;
		transform.Rotate(0, angle, 0, Space.World);
		targetTilt = Mathf.Clamp(targetTilt + MAX_TILT_ANGLE * -axis, -MAX_TILT_ANGLE, MAX_TILT_ANGLE);
		return angle;
	}
	
	public void Boost(Vector3 direction)
	{
		//if we have not boosted yet, or the time since the last boost has been enough, boost.
		if(lastBoostTime == -1 || Time.timeSinceLevelLoad - lastBoostTime > boostRecharge)
		{
			rigidbody.AddForce(direction * boostForce);
			lastBoostTime = Time.timeSinceLevelLoad;
			rocketTrail.PlayBurst(BOOST_EFFECT_DURATION_SEC, BOOST_FLAME_EFFECT_MULTIPLIER);
			ConsumeFuel(boostFuelCost);
		}
	}
	
	void ConsumeFuel(float amount)
	{
		currentFuel -= amount;
		Game.gui.shipDisplay.SetFuelDisplay(currentFuel/totalFuel);
	}
	
	void CorrectTilt()
	{
		if(Mathf.Approximately(targetTilt, 0))
			targetTilt = 0;
		else 
		{
			float tiltSign = Mathf.Sign(targetTilt);
			targetTilt = Mathf.Abs(targetTilt) - (TILT_CORRECTION_FORCE * Time.deltaTime) * tiltSign; //pull tilt toward 0 by TILT_CORRECTION
			if(Mathf.Sign(targetTilt) != tiltSign)
				targetTilt = 0;
		}

	}
	
}
