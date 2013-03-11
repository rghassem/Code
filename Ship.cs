using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Engine))]
[RequireComponent(typeof(ObstacleSpawner))]
public class Ship : SelectableBody
{	
	public float MaxDragDistance;    //Maximum distance draggable from start in launch mode
	public float MaxAcceleration;    //Maximum acceleration attainable
	public float MaxRotationalSpeed; //Rotational speed in degrees per second
	public float Responsiveness;	 //Newton s of force applied per meter distant the mouse gets
	
	public Gravitable gravityComponent;
			
	private float defaultDrag;
	
	private bool allowMouseDragMovement;
		
	
	//Variables for FTL
	private Rigidbody launcher;
	private float ftlDistance, ftlTime, ftlImpulse;
	private float ftlCounterForce;
	private float timeOfLaunch = 0;
	private Vector3 ftlDirection;
	private LineRenderer lineRendererToLauncher;
	float minFTLSpeed = 5;
	private Engine engine;
	private ObstacleSpawner asteroidSpawner;

	
	public ShipData shipInfo;
	
	
	// Use this for initialization
	void Start() 
	{
		//Initialize
		shipInfo = ScriptableObject.CreateInstance<ShipData>();
		defaultDrag = rigidbody.drag;
		engine = GetComponent<Engine>() as Engine;
		lineRendererToLauncher = gameObject.AddComponent<LineRenderer>();
		asteroidSpawner = GetComponent<ObstacleSpawner>();
		
		//Set default stuff
		SetToDefaultState();
	}
	
	/// <summary>
	/// Called when taking off from a landable
	/// </summary>
	public void OnTakeOff()
	{
		SetToDefaultState();
	}
	
	public void OnLand(Planet landing)
	{
		SetToDefaultState();
	}
	
	
	private void SetToDefaultState()
	{
		Game.mainCamera.effects.DeactivateLightSpeed();
		engine.Disable();
		lineRendererToLauncher.enabled = false;
		
		state = LaunchState.Idle;
		mode = ControlMode.Tactical;
		
		allowMouseDragMovement = false;
		
		disableGravity();

	}

	
	//State machine
	private LaunchState state;
	private enum LaunchState
	{
		Idle,
		Orbit,
		Launching,
		Transit
	}
	
	private ControlMode mode;
	private enum ControlMode
	{
		Tactical,
		Flight
	}
	
	void switchState(LaunchState newState)
	{ 
		switch(newState)
		{
		case LaunchState.Idle:
			rigidbody.isKinematic = false;
			rigidbody.drag = defaultDrag;
			disableGravity();
			engine.Disable();
			asteroidSpawner.Stop();
			switchMode(ControlMode.Tactical);
			break;
			
		case LaunchState.Launching:
			disableGravity();
			engine.Disable();
			asteroidSpawner.Stop();
			switchMode(ControlMode.Flight);
			break;
			
		case LaunchState.Orbit:
			rigidbody.isKinematic = false;
			rigidbody.drag = defaultDrag;
			disableGravity();
			engine.Disable();
			asteroidSpawner.Stop();
			break;
			
		case LaunchState.Transit:
			enableGravity();
			engine.Enable();
			asteroidSpawner.Go();
			rigidbody.drag = 0;
			rigidbody.isKinematic = false;
			break;
		}
		
		state = newState;
	}
	
	void switchMode(ControlMode newMode)
	{
		if(newMode == ControlMode.Tactical)
		{
			Game.lockSelection = false;
			Game.SelectObject(gameObject, false);
			if(Game.mainCamera.currentViewAngle == Game.mainCamera.VIEW_ANGLE_LOW)
				Game.mainCamera.SwitchViewAngle();
			Game.gui.shipDisplay.Hide();
		}
		else if( newMode == ControlMode.Flight)
		{
			Game.SelectObject(gameObject, true);
			Game.lockSelection = true;
			if(Game.mainCamera.currentViewAngle == Game.mainCamera.VIEW_ANGLE_HIGH)
				Game.mainCamera.SwitchViewAngle();
			Game.gui.shipDisplay.Show(this);
		}
		
		mode = newMode;
	}

	
	
	
	//Public interface
	
	public ShipData GetShipData()
	{
		return shipInfo;
	}
	
	void OnDeath(DamageType causeOfDeath)
	{
		Game.gui.shipDisplay.Hide();
		Game.mainCamera.distanceFromFocus = Game.mainCamera.TOP_CAMERA_DISTANCE;
		if(Game.SelectedObject == gameObject)
		{
			Game.lockSelection = false;
			Game.ReturnToLastPlanet(2);
		}
		if(Game.SelectedObject = gameObject)
		{
			Game.highlight.detach();
		}
	}

	
	public string GetLaunchState()
	{
		switch(state)
		{
			case LaunchState.Idle:			return "Idle";
			case LaunchState.Launching: 	return "Launching";
			case LaunchState.Orbit:		return "Sublight";
			case LaunchState.Transit: 			return "FTL";
			default: 			  			return "Error";
		}
	}
	
	public void ReadyLaunch(GameObject launchBase, float launchForce, float maxDistance)
	{
		if(state == LaunchState.Transit || state == LaunchState.Launching)
			return;
		
		launcher = launchBase.rigidbody;
		
		lineRendererToLauncher.enabled = true;
		lineRendererToLauncher.material = new Material (Shader.Find("Particles/Additive"));
		lineRendererToLauncher.SetPosition(0, launchBase.transform.position);
		lineRendererToLauncher.SetPosition(1, transform.position);
		Color lineColor = new Color( 0, 153, 30, 0.5f);
		lineRendererToLauncher.SetColors(lineColor, lineColor);
		lineRendererToLauncher.SetWidth(0.2f, 0.2f);
		lineRendererToLauncher.useWorldSpace = true;
		
		//save data from launcher
		ftlImpulse = launchForce;
		ftlDistance = maxDistance;
		
		rigidbody.velocity = Vector3.zero;
		switchState(LaunchState.Launching);
		allowMouseDragMovement = false;
	}
	
	public void Launch()
	{
		//Kill the constraint to the launcher
		Vector3 lineToLauncher = (launcher.transform.position - transform.position);
		lineToLauncher.y = 0;
		lineRendererToLauncher.enabled = false;
		launcher = null;
		
		//Get the right camera angle
		//Game.SelectObject(gameObject, true);
		//Game.mainCamera.SwitchViewAngle();
		
		//Calculate launch parameters
		ftlDirection = lineToLauncher.normalized;
		float magnitude = lineToLauncher.magnitude;
		float slingshotScaler = Mathf.Min( magnitude / MaxDragDistance, 1);
		ftlImpulse *= slingshotScaler;
		ftlDistance *= slingshotScaler;
		ftlDistance += magnitude;
		//Calculate constant slow-down force
		var aveVelocity = ftlImpulse + Game.SPEED_OF_LIGHT / 2; 
		ftlTime = ftlDistance/aveVelocity;     //time till we slow "light speed"
		ftlCounterForce = -ftlImpulse/ftlTime; //constant "drag" force to make that happen
		
		//Launch in the direction of the launcher
		switchState(LaunchState.Transit);
		rigidbody.AddForce(ftlDirection * ftlImpulse, ForceMode.VelocityChange);
		timeOfLaunch = Time.fixedTime;
		
		//Activate light speed particle system
		Game.mainCamera.effects.ActivateLightSpeed();		
		
	}
	
	
	public void LeaveFTL()
	{
		Game.mainCamera.effects.DeactivateLightSpeed();
		switchState(LaunchState.Idle);
		switchMode(ControlMode.Tactical);
	}
	
	/// <summary>
	/// Switch state to interplanetary travel without a launch
	/// </summary>
	public void EnterFTL()
	{
		switchState(LaunchState.Transit);
		switchMode(ControlMode.Flight);
	}
	
	
	// Update is called once per frame
	void FixedUpdate() 
	{
		if(Input.GetKey(KeyCode.Tab))
		{
			switchState(LaunchState.Orbit);
			switchMode(ControlMode.Flight);
		}
		
		Vector3 forceVector;
		switch(state)
		{
			case LaunchState.Launching:
				Vector3 launchLine = launcher.transform.position - transform.position;
				Vector3 directionToLauncher = launchLine.normalized;
				TurnTowards(directionToLauncher);
				forceVector = GetMouseDragMovement();
				//curb the force vector
				forceVector = forceVector/5;
				rigidbody.AddForce(forceVector,ForceMode.Force);
			
				float percentNearDragLimit =  launchLine.magnitude / MaxDragDistance;
				Vector3 dragStopForce = directionToLauncher * forceVector.magnitude * percentNearDragLimit;
				rigidbody.AddForce(dragStopForce, ForceMode.Force);
					
				lineRendererToLauncher.SetPosition(1, transform.position);
				
				break;
			
			
			case LaunchState.Orbit:
				if( mode == ControlMode.Flight)
				{
					HandleKeyInput();
					TurnTowards(rigidbody.velocity.normalized);
				}
				else
				{
					forceVector = GetMouseDragMovement();
					rigidbody.AddForce(forceVector,ForceMode.Force);
					if(forceVector != Vector3.zero)
						TurnTowards(forceVector);
				}
				break;	
			
			case LaunchState.Transit:
			
				//Apply force to gradually slow down (ignore mass)
				if(launcher == null)
				{
					//Slow down if moving faster than the speed of light
					if(rigidbody.velocity.magnitude > Game.SPEED_OF_LIGHT)
						rigidbody.AddForce(transform.forward * ftlCounterForce, ForceMode.Acceleration);
					//if we've lost all momentum, and its more than one second past launch, then stop
					//TODO: also need a rule for gravity
					if(rigidbody.velocity.magnitude < minFTLSpeed && (Time.fixedTime - timeOfLaunch) > 1  
						&& !IsGravitating()) 
					{
						LeaveFTL();
					}
				}
				HandleKeyInput();
				TurnTowards(rigidbody.velocity.normalized);
				break;
		}
	}
	
	
	
	/// <summary>
	/// Gets the force by which to move the ship toward the mouse drag, or vector3.zero if no movement
	/// </summary>
	/// <param name='directionVector'>
	/// Direction vector.If zero, the direction of movement is used.
	/// </param>
	private Vector3 GetMouseDragMovement()
	{
		if(!allowMouseDragMovement)
			return Vector3.zero;
		
		Vector3 mouseWorldPoint = Game.GetMousePositon();
		Vector3 forceVector = mouseWorldPoint - transform.position;
		forceVector.y = 0;
				
		//movement
		float mouseDistance = forceVector.magnitude;
		forceVector *= Responsiveness;
		if(forceVector.magnitude > MaxAcceleration)
		{
			forceVector *= MaxAcceleration/forceVector.magnitude;
		}
		if(mouseDistance < 1) //if mouse press is very close to position, stop moving.
		{
			rigidbody.velocity = new Vector3(0,0,0);
			return Vector3.zero;
		}
		else
		{
			return forceVector;			
		}
	}
	
	private void HandleKeyInput()
	{
		if( mode == ControlMode.Flight )
		{
			float inputZ = Input.GetAxis("Vertical");
			float inputX = Input.GetAxis("Horizontal");
			Vector3 forceVector = transform.right * inputX + transform.forward * inputZ;
			
			engine.Fly(forceVector.normalized);
			
			if( Input.GetKey(KeyCode.Space) )
			{
				engine.Boost(transform.forward);
			}
		}
	}
	
	private void TurnTowards(Vector3 direction)
	{
		Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
		Quaternion rotationThisFrame = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime*MaxRotationalSpeed);
		transform.rotation = rotationThisFrame;
	}
	
	private void enableGravity()
	{
		if(gravityComponent != null)
		{
			gravityComponent.SetActive(true);
		}
	}
	
	private void disableGravity()
	{
		if(gravityComponent != null)
		{
			gravityComponent.SetActive(false);
		}
	}
	
	private bool IsGravitating()
	{
		if(gravityComponent != null && gravityComponent.enabled == true)
		{
			return gravityComponent.GetGravitySources().Count > 0;
		}
		else return false;
	}
	
	void OnMouseUp()
	{
		if(!allowMouseDragMovement)
			return;
		
		switch(state)
		{
		case LaunchState.Orbit:
			switchState(LaunchState.Idle);
			break;
		case LaunchState.Launching:
			Launch();
			break;
		}
		allowMouseDragMovement = false;
	}
	
	
	void OnMouseDrag()
	{
		if(state == LaunchState.Idle && !Game.lockSelection)
		{
			switchState(LaunchState.Orbit);
			switchMode(ControlMode.Tactical);
		}
		
	}
	
	void OnMouseDown()
	{
		allowMouseDragMovement = true;
	}
	
	//Overrides
	protected override void SelfSelect()
	{
		Game.SelectObject(gameObject, false);
	}
	
	public override float GetSelectionRadius()
	{
		return collider.bounds.extents.magnitude / 2;
	} 

	public float GetSpeed()
	{
		return rigidbody.velocity.magnitude;
	}
	
}


