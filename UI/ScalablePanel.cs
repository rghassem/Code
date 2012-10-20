using UnityEngine;
using System.Collections;

[RequireComponent (typeof(TweenScale))]
public class ScalablePanel : MonoBehaviour {
	
	readonly Vector3 hidingOffset = new Vector3(-100, -100, -100);
	
	[HideInInspector]
	public bool isOpen;
	
	//NGUI scaling script, used to show/hide the menu
	TweenScale tweener;
	Vector3 correctPosition;
	
	// Use this for initialization
	void Start () {
			
		isOpen = false;
		
		correctPosition = transform.position;
		transform.position = hidingOffset; //hide offscreen on level start
		
		tweener = gameObject.GetComponent<TweenScale>();
		tweener.eventReceiver = gameObject;
		
	}
	
	public void Open()
	{
		isOpen = true;		

		//planetMenuTweener.enabled = true;
		if(transform.position != correctPosition)
			transform.position = correctPosition;
		tweener.callWhenFinished = "OnOpened";
		tweener.Play(false);
	}
	
	public void Close()
	{
		isOpen = false;
		tweener.Play(true); //play backwards, shrink planet menu out of sight		
		tweener.callWhenFinished = "OnClosed";
	}
	
	void OnOpened()
	{
	}
	
	void OnClosed()
	{
	}
	
}
