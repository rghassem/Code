using UnityEngine;
using AnimationOrTween;
using System.Collections;

public class OpenShipsButton : MonoBehaviour {
	
	private TweenScale bodyTweener;
	//private bool isOpen = false;
	//private Direction direction = Direction.Forward;
	
	void Awake()
	{
		Transform shipsBody = transform.Find("Ships").Find("ShipsBody");
		bodyTweener = shipsBody.gameObject.GetComponent<TweenScale>();
	}
	
	void OnClick()
	{
		OpenShipsSection();
	}
	
	private void OpenShipsSection()
	{
		bodyTweener.enabled = true;
		//isOpen = true;
	}
	
	private void CloseShipsSection()
	{
		//bodyTweener.direction = AnimationOrTween.Direction.Reverse;
		bodyTweener.enabled = true;
	}

}
