using UnityEngine;
using System.Collections;

public class StructurePopupPanel : MonoBehaviour {
	
	Structure structure;
	
	UILabel nameField;
	UILabel productionField;
	VectorLine linkToSubject;
	
	float offsetDistance = 150;
	
	private bool clickedThisFrame;
	
	// Use this for initialization
	void Start()
	{
		nameField = transform.Find("Name").GetComponent<UILabel>();
		productionField = transform.Find("ProductionPerMinute").GetComponent<UILabel>();
		
		Hide();
	}
	
	void Update()
	{
		//Keep the line to the subject up to date
		if(linkToSubject != null && linkToSubject.active == true && structure != null)
		{
			Vector3 targetScreenPosition = Game.mainCamera.camera.WorldToScreenPoint(structure.transform.position);
			linkToSubject.points2[1] = new Vector2(targetScreenPosition.x, targetScreenPosition.y);
			linkToSubject.Draw();
		}
	}
	
	public void Hide()
	{
		if(structure != null)
			structure.NotifyClickAway();

		if(linkToSubject != null)
		{
			VectorLine.Destroy(ref linkToSubject);
			linkToSubject = null;
		}
		transform.position += new Vector3( Screen.width + 10000, 0, 0);
	}
	
	public void Show(Vector3 position)
	{
		transform.localPosition = position;
	}
	
	public void LoadStructure (Structure selectedStructure) {
		
		Hide();
		structure = selectedStructure;
		
		//Fill fields based on structure data
		nameField.text = selectedStructure.structureName;
		productionField.text = selectedStructure.productionPerMinute.ToString();
		
		//Place the popup
		Vector3 targetScreenPosition = Game.mainCamera.camera.WorldToScreenPoint(selectedStructure.gameObject.transform.position);
		Vector3 popupPosition = CalculatePopupPosition(targetScreenPosition);
		
		
		//Draw line
		Vector3 targetScreenCoords = targetScreenPosition;
		Vector3 popupScreenPosition = targetScreenPosition + new Vector3(offsetDistance ,0,0);
		linkToSubject = VectorLine.SetLine(Color.gray, 
				new Vector2(popupScreenPosition.x, popupScreenPosition.y),
				new Vector2(targetScreenCoords.x, targetScreenCoords.y));

		
		Show(popupPosition);
	}
	

	Vector3 CalculatePopupPosition(Vector3 targetScreenPosition)
	{	
		Vector3 targetSreenCoord = 
			targetScreenPosition 												 //Get screen pos of object
			- new Vector3(Screen.width/2, Screen.height/2, 0)                    //compensate for UI 0 at center of screen
			+ new Vector3(offsetDistance ,0,0);                                  //add offset   
		
		Vector3 targetUICoord = Game.gui.ScreenToUIScale(targetSreenCoord );     //take into account possible UI scaling
		
		Vector3 result = targetUICoord; 
		result.z = 0;
		return result;
	}
	
	//Use the click event and late update to determine if the mouse is pressed, and if the popup was clicked.
	//If mouse was pressed anywhere else, hide popup.
	
	void OnMouseDown() {
		clickedThisFrame = true;
	}
	
	void LateUpdate()
	{
		if( Input.GetMouseButtonDown(0) )
		{
			if(!clickedThisFrame)
				Hide();
		}
		clickedThisFrame = false;
	}
}
