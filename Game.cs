using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	
	//Global static variables
	public static GameObject SelectedObject;
	public static bool lockSelection;
	
	public static readonly float SPEED_OF_LIGHT = 100;
	public static readonly float DOUBLE_CLICK_THRESHHOLD_INTERVAL = 0.25f;
	
	//Persistent reference to the point light used to highlight selected objects
	public static Highlight highlight;
	
	public static CameraControls mainCamera;
			
	public static Planet lastPlanet;
	
	public static UI gui;
	
	public static PlayerState state;
	
	private static Game gameInstance;
		
	void Awake () 
	{
		highlight = GameObject.FindWithTag("Highlight").GetComponent<Highlight>();
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraControls>();
		gameInstance = this;
		gui = new UI();
		gui.Initialize();
		
		state = new PlayerState();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetMouseButtonDown(0))
		{
			DeSelect();
		}
	}
	#region selection
	
	public static void SelectObject(GameObject target, bool centerCamera)
	{
		SelectObject(target, centerCamera, true, true);
	}
	
	public static void SelectObject(GameObject target, bool centerCamera, bool cameraMatchRotation, bool doHighlight)
	{
		if(lockSelection)
			return;
		
		if(centerCamera)
			mainCamera.CenterOn(target, 0.25f, cameraMatchRotation);
		else
			mainCamera.UnCenter();
		
		SelectedObject = target;
		SelectedObject.SendMessage("OnPlayerSelect", SendMessageOptions.DontRequireReceiver);
		if(doHighlight)
		{
			highlight.attach(target);
			highlight.show();
		}
	}
	
	public static void DeSelect()
	{
		if(lockSelection)
			return;
		
		highlight.hide();
		mainCamera.UnCenter();
		if(SelectedObject != null)
			SelectedObject.SendMessage("OnPlayerDeSelect", SendMessageOptions.DontRequireReceiver);
		SelectedObject = null;
	}
	
	#endregion
	/// <summary>
	/// Returns focus to the last selected planet.
	/// </summary>
	/// <param name='delay'>
	/// Delay in seconds before returning
	/// </param>
	public static void ReturnToLastPlanet(float delay)
	{		
		if(lastPlanet == null)
		{
			lastPlanet = GameObject.FindWithTag("StartingPlanet").GetComponent<Planet>();
		}
		Game.mainCamera.lockFocus = false;
		gameInstance.StartCoroutine(lastPlanet.DelayedSelfSelect(delay));
	}
	
	
	public static Vector3 GetMousePositon()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 1;
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
		Vector3 cameraPosition = mainCamera.transform.position;
		Vector3 directionToWorld = (mousePosition - cameraPosition).normalized;
				
		Plane playingField = new  Plane(Vector3.up, Vector3.zero); //the plane y = 0;
		Ray cameraToWorldByMouse = new Ray(cameraPosition, directionToWorld);
		float projectionDistance;
		playingField.Raycast(cameraToWorldByMouse, out projectionDistance);
		
		Vector3 result = cameraPosition + (directionToWorld * projectionDistance);
		GameObject.Find("Marker").transform.position = result;
		return result;
	}
	

	
	
	
}
