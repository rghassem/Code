using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {
	
	private readonly float MAX_TILT_ANGLE = 30; //maximum angle for the ship to tilt when turning, in degrees
	private readonly float TILT_SPEED = 90; //Degrees the ship can tilt in one second
	private readonly float BOOST_EFFECT_DURATION_SEC = 0.3f;
	private readonly float TILT_CORRECTION_FORCE = 360; //degrees per second
	private readonly int BOOST_FLAME_EFFECT_MULTIPLIER = 5;
	
	private float THRUST_PIVOT_TIME = 0.8f; //The time it takes to orient the ship when changing thrust direction. Ignores angular speed.
	
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
	
	//Variables for keeping track of tilt
	private float targetTilt, tiltAngle;
	
	Vector3 thrustDirection, lastFrameThrustDirection;
	private float timeSinceThrustPivot;
	private Transform meshTransform; //The transform on the subobject holding the ship mesh. Transformations have a visual effect only
	private Quaternion roll, yaw;
	
	// Use this for initialization
	void Awake () {
		meshTransform = transform.Find("ShipMesh");
		rocketTrail = meshTransform.Find("RocketTrail").GetComponent<RocketTrail>();
		currentFuel = totalFuel;
		
		tiltAngle = 0;
		targetTilt = 0;
		
		yaw = meshTransform.localRotation;
		timeSinceThrustPivot = 0;
	}
	
	float ClosestFactorOf45(float a)
	{
		int floor = (int)a / 45;
		float ceil = (floor * 45) + 45; 
		if( a % 45 > 45/2)
			return ceil;
		else return floor;
	}
	
	void Update()
	{		
		//Rotate the ship in the direction of thrust. Use shipMesh to move ship body independent of everything else.
		
		if(thrustDirection != lastFrameThrustDirection)
			timeSinceThrustPivot = 0;
		
		float sideAngle = Vector3.Angle (transform.right, thrustDirection);
		float sign = sideAngle > 90 ? -1 : 1;
		
		float faceAngle = Vector3.Angle(transform.forward, thrustDirection) * sign;
		Quaternion targetRot = Quaternion.AngleAxis(faceAngle, Vector3.up);
		
		//Calculate the yaw to point ship toward thrust
		float angleToTarget = Quaternion.Angle(yaw, targetRot );
		//float progressToTargetThisFrame = Mathf.Min( MaxAngularSpeed * Time.deltaTime / angleToTarget, 1);
		Quaternion rotationThisFrame = Quaternion.Slerp(yaw, targetRot, timeSinceThrustPivot/THRUST_PIVOT_TIME);
		yaw = rotationThisFrame; 
		
		//Calculate angle to roll ship based on thrust direction (based on yaw)
		float rollAngle = Quaternion.Angle(yaw, Quaternion.LookRotation(Vector3.forward));
		//Calculate additional roll (tilt) based on changing orienation (direction player wants to look, no thrust)
		float deltaTilt = targetTilt - tiltAngle;
		tiltAngle = tiltAngle + Mathf.Sign(deltaTilt) * 
													Mathf.Min(Mathf.Abs(deltaTilt), (TILT_SPEED*Time.deltaTime));
		Quaternion rollThisFrame = Quaternion.AngleAxis(rollAngle + tiltAngle, Vector3.forward);
		roll = rollThisFrame;
		
		//Set the transform on the mesh to the desired rotation
		meshTransform.localRotation = yaw * roll;
		
		//book keeping
		lastFrameThrustDirection = thrustDirection;
		timeSinceThrustPivot = Mathf.Min(timeSinceThrustPivot + Time.deltaTime, THRUST_PIVOT_TIME);
		CorrectTilt();
		
		thrustDirection = transform.forward; //thrust direction resets instantly

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
			
			//remember force direction for rotation effects
			thrustDirection = direction;
			
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
