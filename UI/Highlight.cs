using UnityEngine;
using System.Collections;

public class Highlight : MonoBehaviour {

	// Use this for initialization
	void Start () {
		hide();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void show()
	{
		this.gameObject.active = true;
	}
	
	public void attach(GameObject selectedObject)
	{
		//lock on to the object we want to highlight
		transform.parent = selectedObject.transform;
		transform.position = selectedObject.transform.position;
		
		//scale to the selected object's size
		light.range = selectedObject.GetComponent<SelectableBody>().GetSelectionRadius();
	}
	
	public void detach()
	{
		transform.parent = null;
	}
	
	public void hide()
	{
		this.gameObject.active = false;
	}
}
