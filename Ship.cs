using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Engine))]
[RequireComponent(typeof(ObstacleSpawner))]
[RequireComponent(typeof(Looter))]
public class Ship : SelectableBody
{	
	#region Member Vars
	readonly float SCREEN_SWIPE_ANGLE = 180; //Total turning angle from swiping accross the entire screen with mouse or touch controls
	readonly float MIN_FTL_SPEED = 5;
	
	//Orbit mouse drag variables
	public float MaxDragDistance;      //Maximum distance draggable from start in launch mode
	public float MaxAcceleration;      //Maximum acceleration attainable
	public float MaxOrbitAngularSpeed; //Rotational speed in degrees per second
	public float Responsiveness;	   //Newton s of force applied per meter distant the mouse gets

	public ShipData shipInfo;

	public Gravitable gravityComponent;
	
	private Engine engine;
	private ObstacleSpawner asteroidSpawner;
	private Looter lootCollector;

	bool allowMouseDragMovement;
	float defaultDrag;
	
	//State management
	StateMachine<ShipState> state;
	private enum ControlMode
	{
		Tactical,
		Flight
	}
	private ControlMode mode;
	
	#endregion
	
	#region Events
	
	// Initialization event
	void Start() 
	{
		//Initialize
		shipInfo = ScriptableObject.CreateInstance<ShipData>();
		defaultDrag = rigidbody.drag;
		engine = GetComponent<Engine>() as Engine;
		engine.enabled = false;
		asteroidSpawner = GetComponent<ObstacleSpawner>();
		lootCollector = GetComponent<Looter>();
		lootCollector.enabled = false;
		if(gravityComponent != null)
			gravityComponent.SetActive(false);
		
		//Set default stuff
		state = new StateMachine<ShipState>(new Orbit(this));
	}
	
	// Update is called once per frame
	void FixedUpdate() 
	{	
		state.current.OnFixedUpdate(Time.fixedDeltaTime);
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
	
	void OnMouseUp()
	{
		if(!allowMouseDragMovement)
			return;
		
		state.current.OnMouseUp();
		allowMouseDragMovement = false;
	}
	
	void OnMouseDrag()
	{
		if(state.current is Orbit && !Game.lockSelection)
		{
			switchMode(ControlMode.Tactical);
		}
	}
	
	void OnMouseDown()
	{
		allowMouseDragMovement = true;
	}
	
	/// <summary>
	/// Called when taking off from a landable
	/// </summary>
	public void OnTakeOff()
	{
		engine.enabled = true; //Hack, ensures the OnDisable event on engine does get called.
		state.ChangeState(new Orbit(this));
	}
	
	public void OnLand(Planet landing)
	{
		//state.ChangeState(new Orbit(this));
	}
	
	#endregion
	
	#region Public interface
	
	public ShipData GetShipData()
	{
		return shipInfo;
	}

	public string GetLaunchState()
	{	
		if (state.current is Launching)
			return "Launching";
		else if (state.current is Transit)
			return "FTL";	
		return "Sublight";
	}
	
	public void ReadyLaunch(GameObject launchBase, float launchForce, float maxDistance)
	{
		if(state.current is Transit || state.current is Launching)
			return;
		state.ChangeState( new Launching(this, launchBase, launchForce, maxDistance) );
	}
	
	
	//Note that these are externally called functions
	public void LeaveFTL()
	{
		state.ChangeState( new Orbit(this) );
	}
	
	/// <summary>
	/// Switch state to interplanetary travel without a launch
	/// </summary>
	public void EnterFTL()
	{
		if(! (state.current is Transit) )
			state.ChangeState( new Transit(this) );
	}
	
	public float GetSpeed()
	{
		return rigidbody.velocity.magnitude;
	}
	
	#endregion
	
	#region Base Class Overrides
	
	public override void SelfSelect()
	{
		Game.SelectObject(gameObject, false);
	}
	
	public override float GetSelectionRadius()
	{
		return collider.bounds.extents.magnitude / 2;
	} 
	
	#endregion
	
	#region Coroutintes
	
	/// <summary>
	/// Coroutine for changing ship angle based on drag. Used only in transit state, but must be
	/// here because its a coroutine.
	/// </summary>
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
	
	#endregion

	#region Private Utility Methods
	
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
	

	private void TurnTowards(Vector3 direction)
	{
		Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
		Quaternion rotationThisFrame = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime*MaxOrbitAngularSpeed);
		transform.rotation = rotationThisFrame;
	}
	
	#endregion
	
	#region State Code
	
	abstract class ShipState : IState
	{
		protected Ship self;
		protected Rigidbody rigidbody;
		protected Transform transform;
		protected Engine engine;
		protected GameObject gameObject;
		
		public ShipState(Ship self)
		{
			this.self = self;
			rigidbody = self.rigidbody;
			transform = self.transform;
			engine = self.engine;
			gameObject = self.gameObject;
		}
		
		//All these are optional for ship states
		public void OnPush() {}
		public void OnPop() {}
		public virtual void OnEnter(){}
		public virtual void OnExit(IState nextState) {}

		public abstract void OnFixedUpdate(float deltaTime);
		
		public abstract void OnMouseUp();
	}
	

	class Orbit : ShipState
	{	
		public Orbit(Ship self) : base(self) {}
		
		public override void OnEnter()
		{
			self.allowMouseDragMovement = false;
			self.switchMode(Ship.ControlMode.Tactical);
			engine.enabled = false;
		}
		
		public override void OnExit(IState nextState)
		{
		}
		
		public override void OnFixedUpdate(float deltaTime)
		{
			Vector3 forceVector;
			forceVector = self.GetMouseDragMovement();
			rigidbody.AddForce(forceVector,ForceMode.Force);
			if(forceVector != Vector3.zero)
				self.TurnTowards(forceVector);
		}
				
		public override void OnMouseUp() {}
	
	}
	
	
	class Transit : ShipState
	{
		protected ObstacleSpawner asteroidSpawner;
		float ftlCounterForce, timeOfLaunch;
		protected Gravitable gravityComponent;
		protected Looter lootCollector;
		
		public Transit( Ship self, float ftlCounterForce = 0, float timeOfLaunch = 0 ) : base(self) 
		{
			asteroidSpawner = self.asteroidSpawner;
			gravityComponent = self.gravityComponent;
			this.ftlCounterForce = ftlCounterForce;
			this.timeOfLaunch = timeOfLaunch;
			this.lootCollector = self.lootCollector;
		}
		
		public override void OnEnter()
		{
			enableGravity();
			engine.enabled = true;
			lootCollector.enabled = true;
			asteroidSpawner.Go();
			rigidbody.drag = 0;
			//Activate light speed particle system
			Game.mainCamera.effects.ActivateLightSpeed();
			self.switchMode(Ship.ControlMode.Flight);
			self.StartCoroutine("HandleMouseDragRotation");
		}
		
		public override void OnExit(IState nextState)
		{
			disableGravity();
			engine.enabled = false;
			lootCollector.enabled = false;
			asteroidSpawner.Stop();
			rigidbody.drag = self.defaultDrag;
			self.switchMode(Ship.ControlMode.Tactical);
			self.StopCoroutine("HandleMouseDragRotation");
		}
		
		public override void OnFixedUpdate(float deltaTime)
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
			/*if(rigidbody.velocity.magnitude < self.MIN_FTL_SPEED && (deltaTime - timeOfLaunch) > 1  
				&& !self.IsGravitating()) 
			{
				self.LeaveFTL();
			}*/
			
			//Handle Key Input:
			
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
		
		private void enableGravity()
		{
			if(self.gravityComponent != null)
			{
				self.gravityComponent.SetActive(true);
			}
		}
		
		private void disableGravity()
		{
			if(self.gravityComponent != null)
			{
				self.gravityComponent.SetActive(false);
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

		
		public override void OnMouseUp() {}
	
	}
	
	
	class Launching : ShipState
	{
		protected LineRenderer lineRendererToLauncher;		
		protected GameObject launchBase;
		float launchForce, maxDistance;
		float ftlDistance, ftlTime, ftlImpulse;
		Rigidbody launcher;
		Transform meshTransform;
		
		public Launching(Ship self, GameObject launchBase, float launchForce, float maxDistance) : base(self) 
		{
			this.launchBase = launchBase;
			this.launchForce = launchForce;
			this.maxDistance = maxDistance;
			this.launcher = launchBase.rigidbody;
			this.meshTransform = transform.FindChild("ShipMesh");
			
			//Setup line renderer
			lineRendererToLauncher = gameObject.GetComponent<LineRenderer>();
			if(lineRendererToLauncher == null)
					lineRendererToLauncher = gameObject.AddComponent<LineRenderer>();

		}
		
		public override void OnEnter()
		{		
			
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
			self.switchMode(ControlMode.Flight);
			
			//When we are launching, exit the current safe zone early
			Game.lastPlanet.transform.Find("SafeZone").GetComponent<SafeZone>().OnExit();
	
			self.allowMouseDragMovement = false;	
		}
		
		public override void OnExit(IState nextState)
		{		
			lineRendererToLauncher.enabled = false;
			launcher = null;
			meshTransform.localScale = Vector3.one;
		}
		
		public override void OnMouseUp() 
		{
			Launch();
		}
		
		public override void OnFixedUpdate(float deltaTime)
		{
			Vector3 launchLine = launcher.transform.position - transform.position;
			Vector3 directionToLauncher = launchLine.normalized;
			self.TurnTowards(directionToLauncher);
			Vector3 forceVector = self.GetMouseDragMovement();
			//curb the force vector
			forceVector = forceVector/5;
			rigidbody.AddForce(forceVector,ForceMode.Force);
		
			float percentNearDragLimit =  launchLine.magnitude / self.MaxDragDistance;
			meshTransform.localScale = new Vector3(meshTransform.localScale.x, meshTransform.localScale.y, 1 + percentNearDragLimit*3);
			Vector3 dragStopForce = directionToLauncher * forceVector.magnitude * percentNearDragLimit;
			rigidbody.AddForce(dragStopForce, ForceMode.Force);
				
			lineRendererToLauncher.SetPosition(1, transform.position);
		}
		
		public void Launch()
		{
			//Calculate launch parameters
			Vector3 lineToLauncher = (launcher.transform.position - transform.position);
			lineToLauncher.y = 0;
			Vector3 ftlDirection = lineToLauncher.normalized;
			float magnitude = lineToLauncher.magnitude;
			float slingshotScaler = Mathf.Min( magnitude / self.MaxDragDistance, 1);
			ftlImpulse *= slingshotScaler;
			ftlDistance *= slingshotScaler;
			ftlDistance += magnitude;
			//Calculate constant slow-down force
			var aveVelocity = ftlImpulse + Game.SPEED_OF_LIGHT / 2; 
			ftlTime = ftlDistance/aveVelocity;     //time till we slow "light speed"
			float ftlCounterForce = -ftlImpulse/ftlTime; //constant "drag" force to make that happen
			
			//Launch in the direction of the launcher
			//state change used to be here, now moved belowss
			rigidbody.AddForce(ftlDirection * ftlImpulse, ForceMode.VelocityChange);
			float timeOfLaunch = Time.fixedTime;
			
			Transit newState = new Transit(self, ftlCounterForce, timeOfLaunch);
			self.state.ChangeState(newState);
		}
	}
	
	#endregion
	
}


