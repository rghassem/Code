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
	/// Converts a screen point to UI scale, without changing the origin. These might 
	/// differ if a manual height is set in UIRoot.
	/// </summary>
	public Vector2 ScreenToUIScale(Vector2 screenPoint)
	{
		return screenPoint * uiRoot.manualHeight/Screen.height;
	}
	
	/// <summary>
	/// Converts a coordinate from Unity Screen coordinates to 2D NGUI Screen coordinates. 
	/// </summary>
	public Vector2 ScreenToUIPoint(Vector2 screenPoint)
	{
		Vector3 uiPointUnscaled = 
			screenPoint 												 //Get screen pos of object
			- new Vector2(Screen.width/2, Screen.height/2);           //compensate for UI 0 at center of screen
		
		return ScreenToUIScale(uiPointUnscaled);
	}
	
	/// <summary>
	/// Converts a coordinate from Unity Screen coordinates to 2D NGUI Screen coordinates relative to a particular GUI object.
	/// This method will iterate up the uiObject's transform heirarchy to adjust for scene graph offsets.
	/// </summary>
	public Vector2 ScreenToUIPoint(Vector2 screenPoint, GameObject uiObject)
	{
		Vector2 uiPoint = ScreenToUIPoint(screenPoint);
		Vector2 absoluteUIWidgetOrigin = GetUIAbsoluteCoordinate(uiObject);
		return uiPoint - absoluteUIWidgetOrigin;
	}
	
	
	/// <summary>
	/// Converts a coordinate from 2D NGUI Screen coordinates to Unity Screen coordinates. These might 
	/// differ if a manual height is set in UIRoot.
	/// </summary>
	/*public Vector2 UIToScreenPoint(Vector2 uiPoint)
	{
		return uiPoint;// / (uiRoot.manualHeight/Screen.height);
	}*/
	
	public Vector2 WorldToUIPoint(Vector3 worldPoint)
	{
		Vector3 screenPoint3D = Game.mainCamera.camera.WorldToScreenPoint(worldPoint);
		Vector3 targetSreenCoord = 
			screenPoint3D 												 //Get screen pos of object
			- new Vector3(Screen.width/2, Screen.height/2, 0);           //compensate for UI 0 at center of screen
		
		Vector2 screenPoint2D = new Vector2(targetSreenCoord.x, targetSreenCoord.y);
		return ScreenToUIScale(screenPoint2D);
	}
	
	/*public Vector3 UIToWorldPoint(Vector3 uiPoint) //relies on useless UIToScreenPoint
	{
		Vector2 screenPoint = UIToScreenPoint(uiPoint);
		return Game.mainCamera.camera.ScreenToWorldPoint(screenPoint);
	}*/
	
	/// <summary>
	/// Gets the coordinates of an NGUI widget in the space of root NGUI objects (by iterating up the NGUI scene graph).
	/// </summary>
	public Vector2 GetUIAbsoluteCoordinate(GameObject uiObject)
	{
		if(uiObject.transform.parent == null || uiObject.transform.parent.GetComponent<UICamera>() != null)
			return uiObject.transform.localPosition;
		else //if there is a parent in the NGUI scene graph, add in its coordinates
			return XYof(uiObject.transform.localPosition) + GetUIAbsoluteCoordinate(uiObject.transform.parent.gameObject);
	}
	
	/// <summary>
	/// Scales a coordinate (presumably a UICoordinate) by the UI Root scale
	/// </summary>
	public Vector2 ScaleToRuntimeUI(Vector2 uiCoord)
	{
		return Vector2.Scale(uiCoord, XYof(uiRoot.transform.localScale));
	}
	
	public Vector2 XYof(Vector3 threeDCoord)
	{
		return new Vector2(threeDCoord.x, threeDCoord.y);
	}
}
