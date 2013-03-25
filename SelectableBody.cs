using UnityEngine;
using System.Collections;

public class SelectableBody : MonoBehaviour {
	
	[HideInInspector]
	protected bool cameraMatchRotationWhenSelected;
	
	void Awake()
	{		
		cameraMatchRotationWhenSelected = true;
	}
	
	// Use this for initialization
	void Start () {
		gameObject.active = true;
	}
	
	void OnMouseUpAsButton()
	{
		SelfSelect();
	}
	
	public virtual void SelfSelect()
	{
		if(Game.SelectedObject == gameObject)
			return;
		Game.SelectObject(gameObject, true, cameraMatchRotationWhenSelected, true);
	}
	
	public virtual IEnumerator DelayedSelfSelect(float delay)
	{
		yield return new WaitForSeconds(delay);
		SelfSelect();
	}
	
	public virtual float GetSelectionRadius()
	{
		return collider.bounds.extents.magnitude * 0.85f;
	}
	
}
