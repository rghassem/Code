using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CameraEffects))]
public class CameraControls : MonoBehaviour {
	
	//Constants
	public readonly float TOP_CAMERA_DISTANCE = 50;
	public readonly float ACTION_CAMERA_DISTANCE = 25;
	public readonly float ROTATION_SPEED = 0.5f; 
	public readonly float VIEW_ANGLE_LOW = 15;
	public readonly float VIEW_ANGLE_HIGH = 90;
	
	public readonly float DRAG_MOVEMENT_THRESHOLD = 5;
	public readonly float DRAG_MOVEMENT_CAP = 100;
	public readonly float DRAG_MOVEMENT_MAX_SPEED = 1000; //meters per second
	public readonly float DRAG_RELEASE_SPEED_FALLOFF = 4; //fraction of speed lost in one second after drag release
	
	public readonly string FOCUS_OBJECT_NAME = "CameraFocus";
	
	Quaternion defaultFocusRotation;
	
	public float distanceFromFocus;
	public float currentViewAngle;
	
	public CameraEffects effects;

	public Transform focus;
	public GameObject subject;
	
	//used for linear interpolation
	float moveStartTime, moveDuration, startingDistanceFromFocus;
	float rotationRemaining; // non-zero if changing view angle
	Quaternion startingRotationAroundFocus, startingRotation;
	Vector3 startingFocusPosition, transitFocusPosition, startingCameraPosition;
	
	//Camera locking controls
	public bool lockInput;
	bool lockMovement;	
	bool lockFocus;
	bool matchSubjectRotation;
	
	void Awake()
	{
		effects = GetComponent<CameraEffects>();
			
		matchSubjectRotation = true;
		lockMovement = false;
		lockFocus = false;
		currentViewAngle = 90;
		distanceFromFocus = TOP_CAMERA_DISTANCE;

	}
	
	// Use this for initialization, happens after awake
	void Start() 
	{
		//Create focus
		focus = new GameObject(FOCUS_OBJECT_NAME).transform;
		focus.gameObject.AddComponent<SphereCollider>();
		focus.gameObject.AddComponent<Rigidbody>();
		focus.collider.isTrigger = true;
		focus.rigidbody.isKinematic = true;
		
		defaultFocusRotation = focus.rotation;
		//start with focus directly beneath camera;
		Vector3 startingPosition = transform.position;
		startingPosition.y = 0;
		StartMoveFocus(startingPosition, 0);
		
		//Start infinite coroutine to handle mouse movement
		StartCoroutine(HandleMouseMovement());
	}
	
	/// <summary>
	/// The scroll wheel sensitivity in meters from camera per tick
	/// </summary>
	int scrollWheelSensitivity = 50;

	
	/// <summary>
	/// The camera centers on the object passed as a parameter. If match target rotation is true, the camera
	/// will always follow "behind" the target.
	/// </summary>
	public void CenterOn(GameObject target, float duration, bool matchTargetRotation)
	{
		if(lockMovement)
			return;
		
		if(lockFocus)
			return;
		
		subject = target;
		matchSubjectRotation = matchTargetRotation;
		StartMoveFocus(target.transform.position, duration);
		AlignSubjectAndFocus(); //must happen second so initial conditions are properly set
	} 
	
	private void StartMoveFocus(Vector3 targetPosition, float moveTime )
	{
		//Set up intitial conditions for move
		#pragma warning disable 
		Vector3 previousFocusPos;
		if(focus != null)
			previousFocusPos = (transitFocusPosition != null) ? transitFocusPosition : focus.position;
		else previousFocusPos = new Vector3(transform.position.x, 0, transform.position.z);
		#pragma warning restore
		startingFocusPosition = previousFocusPos;
		startingDistanceFromFocus = Vector3.Distance(transform.position, previousFocusPos);
		
		//set starting camera direction vector
		startingCameraPosition = transform.position; 
		
		moveStartTime = Time.time;
		moveDuration = moveTime;
		
		startingRotation = transform.rotation;
		/*if(subject != null)
			startingRotationAroundFocus = Quaternion.AngleAxis(currentViewAngle, subject.transform.right); 
		else */
		startingRotationAroundFocus = Quaternion.AngleAxis(currentViewAngle, focus.right); 
		
		focus.position = targetPosition;
	}
	
	public void UnCenter()
	{
		if(lockFocus)
			return;
		
		subject = null;
		focus.rotation = defaultFocusRotation;
		
	}
	
	
	public void SwitchViewAngle(CameraViewMode desiredAngle)
	{
		if(lockMovement)
			return;
		
		if(desiredAngle == CameraViewMode.Perspective)
		{
			SetViewAngle(VIEW_ANGLE_LOW, ROTATION_SPEED);
			distanceFromFocus = ACTION_CAMERA_DISTANCE;
			lockFocus = true;
		}
		
		if(desiredAngle == CameraViewMode.Top)
		{
			SetViewAngle(VIEW_ANGLE_HIGH, ROTATION_SPEED);
			distanceFromFocus = TOP_CAMERA_DISTANCE;
			lockFocus = false;
		}
		
		lockMovement = true;
	}
	
	public void SwitchViewAngle()
	{			
		if(currentViewAngle == VIEW_ANGLE_HIGH)
			SwitchViewAngle(CameraViewMode.Perspective);
		else
			SwitchViewAngle(CameraViewMode.Top);
	}
	
	public void SetViewAngle(float targetViewAngle, float moveDuration)
	{
		if( targetViewAngle > VIEW_ANGLE_HIGH || targetViewAngle < VIEW_ANGLE_LOW)
			return;
		
		StartMoveFocus(focus.position, moveDuration); //set up initial conditions for camera move
		currentViewAngle = targetViewAngle;
	}
	
	public void CloseUp()
	{
		distanceFromFocus = (distanceFromFocus != ACTION_CAMERA_DISTANCE) ? ACTION_CAMERA_DISTANCE : TOP_CAMERA_DISTANCE;
	}
	
	/// <summary>
	/// Restores the default camera settings. Meaning top down view
	/// </summary>
	public void RestoreDefault()
	{
		distanceFromFocus = Game.mainCamera.TOP_CAMERA_DISTANCE;
		SetViewAngle(VIEW_ANGLE_HIGH, ROTATION_SPEED);
		effects.DeactivateLightSpeed();
		Game.mainCamera.lockFocus = false;
	}
	
	/// <summary>
	/// Changes the distance from focus. Positive distance tracks in, negative tracks out.
	/// </summary>
	public void Track(float deltaDistance)
	{
		if(distanceFromFocus + deltaDistance > 0)
			distanceFromFocus += deltaDistance;
	}
	
	public void FreezeMovement(bool freeze)
	{
		if(freeze)
		{
			//Stop movement
		}
		else
		{
			//all
		}
	}
	
	// Update is called once per frame
	void Update() 
	{
		AlignSubjectAndFocus();
		HandleInput();
		HandleMovement();
	}
	
	void AlignSubjectAndFocus()
	{
		if(subject != null)
		{
			focus.position = subject.transform.position;
			
			if(matchSubjectRotation)
			{
				float angle; Vector3 axis;
				subject.transform.localRotation.ToAngleAxis(out angle, out axis);
				axis.y = 1;
				focus.localRotation = Quaternion.Euler(subject.transform.localRotation.eulerAngles.x, 
					subject.transform.localRotation.eulerAngles.y, 0);
			}
		}
	}
	
	//TODO: Rethink this whole transition thing from the ground up. There are several layers of half-understood
	//implementation here. It works, but it is messy and inefficient. All the starting values and the frame logic
	//should be re-figured-out.
	
	void HandleMovement()
	{		
		if(focus != null)
		{
			//how far along in the current motion are we?
			float progress = (moveDuration == 0) ? 1 : (Time.time - moveStartTime)/moveDuration;
			
			if(progress >= 1)
				lockMovement = false;

			//Calculate appropriate position this frame
			
			progress = Mathf.SmoothStep(0,1, progress);
			transitFocusPosition = Vector3.Lerp(startingFocusPosition, focus.position, progress);
				
			//Calculate the appropreate view angle this frame
			Quaternion targetRotationAroundFocus = Quaternion.AngleAxis(currentViewAngle, focus.transform.right);
			Quaternion currentRotationAroundFocus = Quaternion.Lerp(startingRotationAroundFocus,
																	targetRotationAroundFocus, progress);
			
			float currentDistanceFromFocus = Mathf.Lerp(startingDistanceFromFocus, distanceFromFocus, progress);
			//Vector3 currentCameraDirection = Vector3.Lerp(startingCameraDirection, -focus.transform.forward, progress);
			Vector3 offsetFromFocus = -focus.transform.forward * currentDistanceFromFocus; //apply distance
			offsetFromFocus = currentRotationAroundFocus * offsetFromFocus ; //apply view angle 		
			
			//Set position, use a lerp to ensure smooth motion
			transform.position = Vector3.Lerp(startingCameraPosition, transitFocusPosition + offsetFromFocus, progress);
			
			//Calculate appropriate rotation this frame

			// simlify and normalize: 
			//   transitFocusPosition - (transitFocusPosition + ( (targetRotationAroundFocus * focus.transform.forward) * distanceFromFocus));
			Vector3 finalLookVector =  (targetRotationAroundFocus * focus.transform.forward * distanceFromFocus).normalized;
			
			Quaternion rotationInterpolated = Quaternion.Slerp(startingRotation, Quaternion.LookRotation(finalLookVector, Vector3.up), progress);

			//Set rotation
			transform.rotation = rotationInterpolated;	

		}
	}
	
	void HandleInput()
	{
		Track( scrollWheelSensitivity * -Input.GetAxis("Mouse ScrollWheel") ); 
		
		if(lockMovement || lockFocus || lockInput)
			return;
		
		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");
		if(inputX != 0 || inputY != 0)
		{
			focus.Translate(inputX, 0, inputY);
		}
		
	}
	
	IEnumerator HandleMouseMovement() 	//Coroutine
	{		
		Vector3 clickAndHoldPosition;
		float speed;
		Vector3 direction;
		bool rememberMouseClicked = false;
		
		while(true)
		{	
			speed = 0;
			direction = Vector3.zero;
			
			//Mouse button clicked
			if(Input.GetMouseButtonDown(0) || rememberMouseClicked )
			{
				rememberMouseClicked = false;
				clickAndHoldPosition = Input.mousePosition;
				Vector3 clickAndHoldWorldPosition = Game.ProjectScreenPointToWorldPlane(clickAndHoldPosition);
				yield return null;
				
				//Button held down
				while( Input.GetMouseButton(0) ) //SelectedObject because we might be dragging a ship or something and screen movement innapropriate
				{
					if( lockMovement || lockFocus || lockInput || Game.SelectedObject != null)  break;
					
					Vector3 currentMousePos = Input.mousePosition;
					Vector3 diffVector = currentMousePos - clickAndHoldPosition;
					
					bool passedThreshold = false;
					if(diffVector.magnitude > DRAG_MOVEMENT_THRESHOLD || passedThreshold)
					{
						passedThreshold = true; //once past the threshold, stay there this click
						
						
						Vector3 worldDiffVector = Game.ProjectScreenPointToWorldPlane(currentMousePos) - clickAndHoldWorldPosition;
						direction = -worldDiffVector.normalized;
						
						float potential = Mathf.Min(1, worldDiffVector.magnitude / DRAG_MOVEMENT_CAP );
						speed =  (potential > 0.5f) ? Mathf.SmoothStep(0, DRAG_MOVEMENT_MAX_SPEED, potential)
													: Mathf.Lerp(0, DRAG_MOVEMENT_MAX_SPEED, potential);
						
						focus.Translate(direction * speed * Time.deltaTime);
					}
					
					yield return null;
				}
				
				//Button released, decrease speed till we are stopped
				while(speed > 1)
				{			
					if(Input.GetMouseButtonDown(0))
					{
						rememberMouseClicked = true;
						break;
					}
					focus.Translate(direction * speed * Time.deltaTime);
					speed -= (speed * DRAG_RELEASE_SPEED_FALLOFF * Time.deltaTime); //TODO: Constant, adjust by time
					yield return null;
				}
			}
			yield return null;
		}
	}
	
}

public enum CameraViewMode
{
	Top,
	Perspective
}
