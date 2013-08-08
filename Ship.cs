using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Engine))]
[RequireComponent(typeof(ObstacleSpawner))]
public class Ship : SelectableBody
{	
	readonly float SCREEN_SWIPE_ANGLE = 180; //Total turning angle from swiping accross the entire screen with mouse or touch controls
	
	//Orbit mouse drag variables
	public float MaxDragDistance;    //Maximum distance draggable from start in launch mode
	public float MaxAcceleration;    //Maximum acceleration attainable
	public float MaxOrbitAngularSpeed; //Rotational speed in degrees per second
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
		engine.enabled = false;
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
			engine.enabled = false;
			asteroidSpawner.Stop();
			switchMode(ControlMode.Tactical);
			Game.mainCamera.effects.DeactivateLightSpeed();
			break;
			
		case LaunchState.Launching:
			disableGravity();
			engine.enabled = false;
			asteroidSpawner.Stop();
			switchMode(ControlMode.Flight);
			Game.mainCamera.effects.DeactivateLightSpeed();
			break;
			
		case LaunchState.Orbit:
			rigidbody.isKinematic = false;
			rigidbody.drag = defaultDrag;
			disableGravity();
			engine.enabled = false;
			asteroidSpawner.Stop();
			Game.mainCamera.effects.DeactivateLightSpeed();
			break;
			
		case LaunchState.Transit:
			enableGravity();
			engine.enabled = true;
			asteroidSpawner.Go();
			rigidbody.drag = 0;
			rigidbody.isKinematic = false;
			//Activate light speed particle system
			Game.mainCamera.effects.ActivateLightSpeed();		

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
		Game.mainCamera.RestoreDefault(); //done inside ReturnToLastPlanet
		if(Game.SelectedObject == gameObject)
		{
			Game.lockSelection = false;
			Game.highlight.detach();
			Game.ReturnToLastPlanet(2);
		}
	}

	
	public string GetLaunchState()
	{
		switch(state)
		{
			case LaunchState.Idle:			return "Idle";
			case LaunchState.Launching: 	return "Launching";
			case LaunchState.Orbit:			return "Sublight";
			case LaunchState.Transit: 		return "FTL";
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
		
		//When we are launching, exit the current safe zone early
		Game.lastPlanet.transform.Find("SafeZone").GetComponent<SafeZone>().OnExit();

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
		
		
	}
	
	
	public void LeaveFTL()
	{
		switchState(LaunchState.Idle);
		switchMode(ControlMode.Tactical);
		StopCoroutine("HandleMouseDragRotation");
	}
	
	/// <summary>
	/// Switch state to interplanetary travel without a launch
	/// </summary>
	public void EnterFTL()
	{
		switchState(LaunchState.Transit);
		switchMode(ControlMode.Flight);
		StartCoroutine("HandleMouseDragRotation");
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
				else //ControlMode.Tactical
				{
					forceVector = GetMouseDragMovement();
					rigidbody.AddForce(forceVector,ForceMode.Force);
					if(forceVector != Vector3.zero)
					{
						TurnTowards(forceVector);
					}
				}
				break;	
			
			case LaunchState.Transit:
			
				//Apply force to gradually slow down (ignore mass)
				if(launcher == null)
				{
					//Slow down if moving faster than the speed of light
					if(ftlCounterForce != 0)
					{
						if(rigidbody.velocity.magnitude > Game.SPEED_OF_LIGHT)
							rigidbody.AddForce(rigidbody.velocity.normalized * ftlCounterForce, ForceMode.Acceleration);
					}
					else 
					{
						if(rigidbody.velocity.magnitude > Game.SPEED_OF_LIGHT)
							rigidbody.velocity = rigidbody.velocity.normalized * Game.SPEED_OF_LIGHT * 0.99f;
					}
					//if we've lost all momentum, and its more than one second past launch, then stop
					if(rigidbody.velocity.magnitude < minFTLSpeed && (Time.fixedTime - timeOfLaunch) > 1  
						&& !IsGravitating()) 
					{
						LeaveFTL();
					}
				}
				HandleKeyInput();
				//TurnTowards(rigidbody.velocity.normalized);
				

				break;
		}
	}
	
	IEnumerator HandleMouseDragRotation()
	{
		bool passedThreshold;
		while(true)
		{
			passedThreshold = false;
			if(Input.GetMouseButtonDown(0))
			{
				Vector3 clickPosition = Input.mousePosition;
				Vector3 currentMousePos = clickPosition;
								
				while( Input.GetMouseButton(0) ) //Button held down
				{
					//float mouseMovementDirection = Mathf.Sign( (Input.mousePosition - currentMousePos).x );
				    currentMousePos = Input.mousePosition;
					Vector3 diffVector = currentMousePos - clickPosition;
					
					
					if(diffVector.magnitude > Game.mainCamera.DRAG_MOVEMENT_THRESHOLD || passedThreshold)
					{
						passedThreshold = true; //once past the threshold, stay there this click
						
						//Calculate direction and magnitude of turn
						float diffInScreenX = currentMousePos.x - clickPosition.x;
						float diffInScreenRatio = diffInScreenX / Screen.width;
						float totalTargetRotation = diffInScreenRatio * SCREEN_SWIPE_ANGLE;
		
						//Turn the ship in the appropriate direction, and record degrees of rotation (depends on internal engine constraints)
						float rotation = 0;
						if( Mathf.Abs(totalTargetRotation) > engine.MaxAngularSpeed * Time.deltaTime ) //prevents jittering
							rotation = engine.Turn(Mathf.Sign(diffInScreenRatio));
						
						//Adjust clickPosition to account for the degrees of rotation acheived this frame 
						clickPosition += diffVector * (rotation/totalTargetRotation);
					}
					yield return null;
				}
			}
			yield return null;
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
			
			
			engine.Fly(forceVector);
			
			if( Input.GetKey(KeyCode.Space) )
			{
				engine.Boost(transform.forward);
			}
			
			float orientationInputX = Input.GetAxis("RightHorizontal");
			float orientationInputZ = Input.GetAxis("RightVertical");
			
			if(orientationInputZ != 0)
				engine.TurnTowards(rigidbody.velocity.normalized * Mathf.Sign(orientationInputZ));
			else if(orientationInputX != 0)
				engine.Turn(orientationInputX);
		}
	}
	
	private void TurnTowards(Vector3 direction)
	{
		if(state == LaunchState.Transit)
			engine.TurnTowards(direction);
		else
		{
			Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
			Quaternion rotationThisFrame = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime*MaxOrbitAngularSpeed);
			transform.rotation = rotationThisFrame;
		}
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
	public override void SelfSelect()
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


