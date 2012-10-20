using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour {
	
	//Constants
	public readonly float TOP_CAMERA_DISTANCE = 50;
	public readonly float ACTION_CAMERA_DISTANCE = 25;
	public readonly float ROTATION_SPEED = 0.5f; 
	public readonly float VIEW_ANGLE_LOW = 15;
	public readonly float VIEW_ANGLE_HIGH = 90;
	
	Quaternion defaultFocusRotation;
	
	public float distanceFromFocus;
	public float currentViewAngle;
	
	
	//used for linear interpolation
	float moveStartTime, moveDuration, startingDistanceFromFocus;
	float rotationRemaining; // non-zero if changing view angle
	Quaternion startingRotationAroundFocus, startingRotation;
	Vector3 startingFocusPosition, transitFocusPosition;
	public Transform focus;
	public GameObject subject;
	
	//Camera locking controls
	[HideInInspector]
	public bool lockMovement = false;
	public bool lockFocus = false;
	public bool matchSubjectRotation = true;

	void Awake()
	{
		matchSubjectRotation = true;
		lockMovement = false;
		lockFocus = false;
		currentViewAngle = 90;
		distanceFromFocus = TOP_CAMERA_DISTANCE;

	}
	
	// Use this for initialization, happens after awake
	void Start() 
	{
		focus = new GameObject("Camera Focus").transform;
		defaultFocusRotation = focus.rotation;
		//start with focus directly beneath camera;
		Vector3 startingPosition = transform.position;
		startingPosition.y = 0;
		StartMoveFocus(startingPosition, 0);
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
		AlignSubjectAndFocus();
		StartMoveFocus(target.transform.position, duration);
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
	/// Changes the distance from focus. Positive distance tracks in, negative tracks out.
	/// </summary>
	public void Track(float deltaDistance)
	{
		if(distanceFromFocus + deltaDistance > 0)
			distanceFromFocus += deltaDistance;
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
				focus.localRotation = Quaternion.AngleAxis(angle, axis);
				focus.localRotation = Quaternion.Euler(subject.transform.localRotation.eulerAngles.x, 
					subject.transform.localRotation.eulerAngles.y, 0);
			}
		}
	}
	
	void HandleMovement()
	{		
		if(focus != null)
		{
			//how far along in the current motion are we?
			float progress = (moveDuration == 0) ? 1 : (Time.time - moveStartTime)/moveDuration;
			
			if(progress >= 1)
				lockMovement = false;

			//Calculate appropriate position this frame
			
			transitFocusPosition = Vector3.Lerp(startingFocusPosition, focus.position, progress);
				
			//Calculate the appropreate view angle this frame
			Quaternion targetRotationAroundFocus = Quaternion.AngleAxis(currentViewAngle, focus.transform.right);
			Quaternion currentRotationAroundFocus = Quaternion.Lerp(startingRotationAroundFocus,
																	targetRotationAroundFocus, progress);
			
			float currentDistanceFromFocus = Mathf.Lerp(startingDistanceFromFocus, distanceFromFocus, progress);
			Vector3 offsetFromFocus = -focus.transform.forward * currentDistanceFromFocus; //apply distance
			offsetFromFocus = currentRotationAroundFocus * offsetFromFocus ; //apply view angle 		
			
			//Set position
			transform.position = transitFocusPosition + offsetFromFocus;
			
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
		
		if(lockMovement || lockFocus)
			return;
		
		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");
		if(inputX != 0 || inputY != 0)
		{
			focus.Translate(inputX, 0, inputY);
		}
		
	}
	
}

public enum CameraViewMode
{
	Top,
	Perspective
}
