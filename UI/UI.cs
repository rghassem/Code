using UnityEngine;
using System.Collections;

public class UI  {
	
	public PlanetMenuControls planetMenu;
	public ShipHUDControls shipDisplay;
	public LabelPool labelPool;
	private UIRoot uiRoot;

	
	/// <summary>
	/// Get references to the various UI panels
	/// </summary>
	public void Initialize()
	{
		planetMenu = GameObject.FindGameObjectWithTag("PlanetMenu").GetComponent<PlanetMenuControls>();
		shipDisplay = GameObject.FindGameObjectWithTag("ShipHUD").GetComponent<ShipHUDControls>();
		uiRoot = GameObject.FindWithTag("UIRoot").GetComponent<UIRoot>();
		labelPool = GameObject.FindWithTag("LabelPool").GetComponent<LabelPool>();
	}
	
	/// <summary>
	/// Converts a coordinate from Unity Screen coordinates to 2D NGUI Screen coordinates. These might 
	/// differ if a manual height is set in UIRoot.
	/// </summary>
	public Vector2 ScreenToUIPoint(Vector2 screenPoint)
	{
		return screenPoint * uiRoot.manualHeight/Screen.height;
	}
	
	/// <summary>
	/// Converts a coordinate from 2D NGUI Screen coordinates to Unity Screen coordinates. These might 
	/// differ if a manual height is set in UIRoot.
	/// </summary>
	public Vector2 UIToScreenPoint(Vector2 uiPoint)
	{
		return uiPoint;// / (uiRoot.manualHeight/Screen.height);
	}
	
	public Vector2 WorldToUIPoint(Vector3 worldPoint, Vector3 parentPosition)
	{
		Vector3 screenPoint3D = Game.mainCamera.camera.WorldToScreenPoint(worldPoint);
		Vector3 targetSreenCoord = 
			screenPoint3D 												 //Get screen pos of object
			- new Vector3(Screen.width/2, Screen.height/2, 0);           //compensate for UI 0 at center of screen
		
		Vector2 screenPoint2D = new Vector2(targetSreenCoord.x, targetSreenCoord.y);
		return ScreenToUIPoint(screenPoint2D);
	}
	
	public Vector2 WorldToUIPoint(Vector3 worldPoint)
	{
		return WorldToUIPoint(worldPoint, Vector3.zero);
	}
	
	public Vector3 UIToWorldPoint(Vector3 uiPoint)
	{
		Vector2 screenPoint = UIToScreenPoint(uiPoint);
		return Game.mainCamera.camera.ScreenToWorldPoint(screenPoint);
	}
}
