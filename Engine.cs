using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {
	
	private readonly float MAX_TILT_ANGLE = 30; //maximum angle for the ship to tilt when turning, in degrees
	private readonly float TILT_SPEED = 90; //Degrees the ship can tilt in one second
	private readonly float BOOST_EFFECT_DURATION_SEC = 0.3f;
	private readonly float TILT_CORRECTION_FORCE = 360; //degrees per second
	private readonly int BOOST_FLAME_EFFECT_MULTIPLIER = 5;
	
	private readonly float PIVOT_SPEED = 360; //The the speed at which the ship can reorient in response to directional thrust. Degrees per second.
	
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
	
	//Variables for keeping track of rotation
	private float targetTilt, tiltAngle;
	Vector3 thrustDirection, lastFrameThrustDirection;
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
		thrustDirection = transform.forward;
	}
	
	
	void Update()
	{		
		//Rotate the ship in the direction of thrust. Use shipMesh to move ship body independent of everything else.		
		float sideAngle = Vector3.Angle (transform.right, thrustDirection);
		float sign = sideAngle > 90 ? -1 : 1;
		
		float faceAngle = Vector3.Angle(transform.forward, thrustDirection) * sign;
		Quaternion targetRot = Quaternion.AngleAxis(faceAngle, Vector3.up);
		
		//Calculate the yaw to point ship toward thrust
		float angleToTarget = Quaternion.Angle(yaw, targetRot);
		float fractionOfRotationThisFrame =  Mathf.Approximately( angleToTarget, 0) ? 0 :PIVOT_SPEED * Time.deltaTime / angleToTarget;
		yaw = Quaternion.Slerp(yaw, targetRot, fractionOfRotationThisFrame);

		
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
		
		CorrectTilt(); //pull tilt towards normal
	}
	
	void OnDisable()
	{
		rocketTrail.Stop();
		meshTransform.localRotation = Quaternion.identity;
	}
	
	public void Fly(Vector3 direction) //should be called from FixedUpdate
	{
		Vector3 force = direction * engineForce;
		if(currentFuel > 0 && force.magnitude > 0)
		{
			//Add force
			rigidbody.AddForce(force, ForceMode.Force);
						
			//Tilt ship in force direction
			ConsumeFuel( fuelConsumptionPerSecond * Time.deltaTime );
			
			//play effect
			if(!rocketTrail.isPlaying)
				rocketTrail.Play();
			
			//remember force direction for rotation effects
			SetPivotDirection(direction);

		}
		else
		{ 
			rocketTrail.Stop();
			SetPivotDirection(transform.forward); //default to point ship forward
		}
		
	}
	
	
	public float TurnTowards(Vector3 direction)
	{
		//Calculate the angle of rotation this frame
		Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
		float maxAnglularStep = Time.fixedDeltaTime*MaxAngularSpeed;
		Quaternion rotationThisFrame = Quaternion.RotateTowards(transform.rotation, targetRotation, maxAnglularStep);
		float angle = Quaternion.Angle(transform.rotation, rotationThisFrame);
		
		//Calculate the direction of rotation
		float sideAngle = Vector3.Angle (transform.right, direction); //direction of rotation
		float sign = sideAngle > 90 ? -1 : 1;
		
		if(Mathf.Approximately(angle, 0))
			return 0;
		return Turn(angle/maxAnglularStep * sign); //map the turn to a number between -1 and 1
			
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
	
	public void GainFuel(float amount)
	{
		currentFuel = Mathf.Min(currentFuel + amount, totalFuel);
		Game.gui.shipDisplay.SetFuelDisplay(currentFuel/totalFuel);
	}
	
	void SetPivotDirection(Vector3 newThrustDirection)
	{
		newThrustDirection = newThrustDirection.normalized;
		if( thrustDirection != newThrustDirection )
			thrustDirection = newThrustDirection;
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
	
	public void OnCollectLoot(LootReport newloot)
	{
		if(newloot.HasType(Loot.DropType.Fuel))
			GainFuel(newloot.GetQuantity(Loot.DropType.Fuel));
	}
	
}
