using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LabelPool : MonoBehaviour {
	
	/// <summary>
	/// The number of labels in the label pool by default
	/// </summary>
	public int startingLabelCount;
	/// <summary>
	/// The number of labels added to the label pool when the pool needs to expand
	/// </summary>
	public int poolExpansionCount;
	/// <summary>
	/// The template for each spawned label
	/// </summary>
	public GameObject labelTemplate;
	
	public Material lineMaterial;

	//UIPanel panel;
	private List<DynamicLabel> inactiveLabels; //labels in the pool that are not used
	private List<LabelRecord> unlabeledAnchors; //objects that claim they should have labels
	
	
	// Use this for initialization
	void Awake () 
	{
		inactiveLabels = new List<DynamicLabel>();
		unlabeledAnchors = new List<LabelRecord>();
		
		for(int i = 0; i< startingLabelCount; i++)
		{
			inactiveLabels.Add(CreateLabel());
		}
	}
	
	void Start()
	{
		InvokeRepeating("UpdateLabelVisibility", 0, 0.1f);
	}
	
	private void UpdateLabelVisibility()
	{
		for(int i = 0; i < unlabeledAnchors.Count; i ++)
		{
			if(checkVisible(unlabeledAnchors[i].anchor))
			{
				Label(unlabeledAnchors[i].anchor.gameObject,
					  unlabeledAnchors[i].offset, 
					  unlabeledAnchors[i].labelText);	
				unlabeledAnchors.RemoveAt(i);
			}
		}
	}
	
	private bool checkVisible(Transform trans)
	{
		return trans.renderer.isVisible;
	}

	
	/// <summary>
	/// Glues a text label to the given anchor object, with an offset.
	/// Its position will be updated to keep up with the anchor. Label will be recycled
	/// if object is made inactive or destroyed
	/// Label text is given by lines of text, one line per parameter
	/// </summary>
	/// <returns>
	/// An integer that identifies this label. Can be used to access the label
	/// via other labelPool functions
	/// </returns>
	public DynamicLabel Label(GameObject anchor, Vector3 offset, params string[] linesOfText)
	{
		DynamicLabel label = AllocateLabel();
		label.SetLabelText(linesOfText);
		
		label.BindAnchor(anchor.transform, offset);
		
		return label;
	}
	
	/// <summary>
	/// Glues a text label to the given anchor object.
	/// Its position will be updated to keep up with the anchor. Label will be recycled
	/// if object is made inactive or destroyed
	/// Label text is given by lines of text, one line per parameter
	/// </summary>
	/// <returns>
	/// An integer that identifies this label. Can be used to access the label
	/// via other labelPool functions
	/// </returns>
	public DynamicLabel Label(GameObject anchor, params string[] linesOfText)
	{
		return Label(anchor, Vector3.zero, linesOfText);
	}
	
	
	/// <summary>
	/// Called by DynamicLabel to recylce its
	/// </summary>
	public void RecycleLabel(DynamicLabel label, bool unlabel)
	{
		if(label.active)
			label.active = false;
		inactiveLabels.Add(label);
		if(label.anchor != null && !unlabel)
		{
			unlabeledAnchors.Add(new LabelRecord(label.anchor, label.offset, label.text));
		}
	}
	
	private DynamicLabel AllocateLabel()
	{
		foreach(DynamicLabel label in inactiveLabels)
		{
			if(!label.active)
			{
				label.active = true;
				inactiveLabels.Remove(label);
				return label;
			}
		}
		//if we didn't find any active labels
		for(int i = 0; i < poolExpansionCount; i++)
		{
			inactiveLabels.Add(CreateLabel());
		}
		DynamicLabel firstNewLabel = inactiveLabels[inactiveLabels.Count - poolExpansionCount];
		inactiveLabels.Remove(firstNewLabel);
		firstNewLabel.active = true;
		return firstNewLabel;
	}
	
	private DynamicLabel CreateLabel()
	{
		GameObject labelObject = NGUITools.AddChild(gameObject, labelTemplate);
		return labelObject.GetComponent<DynamicLabel>();
	}
	
	private struct LabelRecord
	{
		public Transform anchor;
		public string labelText;
		public Vector2 offset;
		
		public LabelRecord(Transform anchorObject, Vector2 offsetFromAnchor, string text)
		{
			anchor = anchorObject;
			labelText = text;
			offset = offsetFromAnchor;
		}
	}
	

}
