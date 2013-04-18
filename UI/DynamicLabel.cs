using UnityEngine;
using System.Collections;

public class DynamicLabel : MonoBehaviour {


	private bool _active;
	new public bool active
	{
		get {return _active;}
		set
		{
			if(value) //active
			{
				_active = true;
				line.active = true;
				NGUITools.SetActive(gameObject, true);
			}
			else //deactivate
			{
				_active = false;
				line.active = false;
				NGUITools.SetActive(gameObject, false);
				Game.gui.labelPool.RecycleLabel(this, false);
				
				anchor = null;
				offset = Vector2.zero;
				label.text = "";
			}
		}
	}
	
	public Transform anchor {get; private set;}
	public Vector2 offset {get; private set;}
	public string text {get{ return label.text; } private set {} }

	private UILabel label;
	private VectorLine line;
	
	void Awake()
	{
		label = gameObject.GetComponent<UILabel>();
		line = VectorLine.SetLine (Color.gray, Vector2.zero, Vector2.one * 100);
		
		active = false;
		label.MakePixelPerfect();
	}
	
	void LateUpdate () 
	{
		if(anchor == null )
			return;
		else if(!anchor.renderer.isVisible) //TODO: get visibility better
		{
			active = false;
		}
		else
		{		
			Vector3 offset3d = new Vector3(offset.x, offset.y, 0);
			Vector3 anchorPosition = anchor.transform.position;
			Vector3 positionInUI = Game.gui.WorldToUIPoint(anchorPosition);
			
			transform.localPosition = positionInUI + offset3d;
			
			Vector2[] points = new Vector2[2];
			points[0] = Game.mainCamera.camera.WorldToScreenPoint(anchorPosition);
			points[1] = Game.mainCamera.camera.WorldToScreenPoint(anchorPosition) + offset3d;				
			line.points2 = points;
			line.Draw();
		}
	}
	
	void OnDestroy()
	{
		Remove();
	}
	
	public void Remove()
	{
		_active = false;
		line.active = false;
		NGUITools.SetActive(gameObject, false);
		Game.gui.labelPool.RecycleLabel(this, true);
		
		anchor = null;
		offset = Vector2.zero;
		label.text = "";
	}
	
	public void BindAnchor(Transform anchorTransform, Vector2 offsetFromAnchor)
	{
		offset = offsetFromAnchor;
		anchor = anchorTransform;
	}

	/// <summary>
	/// Sets the label text to the given strings, one string per line. 
	/// Returns true if succeeded.
	/// </summary>
	/// <param name='labelIndex'>
	/// Label index (provided as return value from SetLabel)
	/// </param>
	public void SetLabelText(params string[] linesOfText)
	{
		SetText(linesOfText);
	}
	
	private void SetText(params string[] linesOfText)
	{
		for(int i = 0; i < linesOfText.Length; i++)
		{
			label.text += (i == linesOfText.Length - 1) ? linesOfText[i] : linesOfText[i] + "\n";
		}
		label.MakePixelPerfect();
	}
		
}
