using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gravitable : MonoBehaviour {
	
	private readonly float MAX_LINE_WIDTH = 5;
	//private readonly float GRAVITY_LINE_LABEL_POSITION = 5;

	
	private bool enableGravity = true;
	
	public Material lineMaterial;
	
	/// <summary>
	/// The color of a basic line, before it is "redened" with force of gravity
	/// </summary>
	public Color baseColor;
		
	/// <summary>
	/// How much gravitational force to increase the line redness by 1 RGB.
	/// </summary>
	public float forceAtMaxRed;
	
	
	/// <summary>
	/// The base width of the line, before it is widened with force of gravity.
	/// </summary>
	public float baseWidth;

	/// <summary>
	/// How much gravitational force to increase the line redness by 1 RGB.
	/// </summary>
	public float forceToWidthRatio;
	

	
	//Gravity variabless
	private Dictionary<GameObject, Vector3> currentGravitySources;
	private List<VectorLine> gravityLines;
	private List<LabelAnchor> lineLabels;
	
	public void SetActive(bool isActive)
	{
		if(isActive)
			enableGravity = true;
		else
		{
			enableGravity = false;
			DisableAllLines();	
		}
	}
	
	// Use this for initialization
	void Start () {
		//gravity aid lines
		currentGravitySources =new Dictionary<GameObject, Vector3>();
		gravityLines = new List<VectorLine>();
		lineLabels = new List<LabelAnchor>();
	}
	
	void Update()
	{
		//Draw lines to active gravity sources if user has shift key down
		if(ShowGravityInfo())
		{
			int index = 0;
			foreach(KeyValuePair<GameObject, Vector3> sourcePair in currentGravitySources)
			{
				GameObject source = sourcePair.Key;
				if(index < gravityLines.Count && gravityLines[index] != null)
				{ 
					//update gravity line
					
					//calculate width
					float width = baseWidth + currentGravitySources[source].magnitude / forceToWidthRatio; 
					width = Mathf.Min(width, MAX_LINE_WIDTH);
					float[] widthArray = new float[1];
					widthArray[0] = width; 
					
					//calculate color
					float redPercent = Mathf.Min( 1, currentGravitySources[source].magnitude / forceAtMaxRed);
					Color color =Color.Lerp( baseColor, Color.red, redPercent); 
					
					//set width and color
					gravityLines[index].SetColor(color);
					gravityLines[index].SetWidths(widthArray);
					
					//set line current position
					Vector3[] points = new Vector3[2];
					points[0] = transform.position;
					points[1] = source.transform.position;
					gravityLines[index].points3 = points;
										
					if(!gravityLines[index].active)
						gravityLines[index].active = true;
					
					//UpdateGravityLineLabel(lineLabels[index], Mathf.Round(currentGravitySources[source].magnitude), gravityLines[index]);
				}
				else
				{ 
					//create gravity line
					gravityLines.Add( VectorLine.SetLine3D(Color.yellow, transform.position, source.transform.position) );
					if(lineMaterial != null)
						gravityLines[gravityLines.Count -1].material = lineMaterial;
					gravityLines[gravityLines.Count -1].layer = 12; //layer 12 is Vectrocity
					gravityLines[gravityLines.Count -1].Draw3DAuto(); 
					
					//create label for line
					//lineLabels.Add(CreateGravityLineLabel(currentGravitySources[source].magnitude, gravityLines[index]));
				}
				index++;
					
			}
			//disable any extra lines
			while(index < gravityLines.Count)
			{
				if(gravityLines[index].active)
				{
					gravityLines[index].active = false;
					//UpdateGravityLineLabel(lineLabels[index], 0, gravityLines[index]); 
				}
				index ++;
			}
		}
		else
		{
			DisableAllLines();
		}
	}
	
	void OnDestroy()
	{
		//Clean up all the line objects
		if(gravityLines != null)
		{
			for(int i = 0; i < gravityLines.Count; i ++)
			{
				VectorLine line = gravityLines[i];
					VectorLine.Destroy(ref line);
			}
			for(int i = 0; i < lineLabels.Count; i ++)
			{
				GameObject.Destroy(lineLabels[i].anchor);
			}
		}
	}

	
	private void DisableAllLines()
	{
		if(gravityLines != null)
		{
			foreach(VectorLine line in gravityLines)
			{
				line.active = false;
			}
			foreach(LabelAnchor label in lineLabels)
			{
				label.anchor.active = false;
			}
		}
	}
	
	public void Gravitate(GameObject gravitySource, Vector3 gravitationalPull)
	{	
		if(enableGravity)
		{
			rigidbody.AddForce(gravitationalPull, ForceMode.Force);
			if(!currentGravitySources.ContainsKey(gravitySource))
				currentGravitySources.Add(gravitySource, gravitationalPull);
			currentGravitySources[gravitySource] = gravitationalPull;
		}
	}
	
	public void EscapeGravity(GameObject gravitySource)
	{
		currentGravitySources.Remove(gravitySource);
	}
	
	public Dictionary<GameObject, Vector3> GetGravitySources()
	{
		return currentGravitySources;
	}
	
	private bool ShowGravityInfo()
	{
		return ( Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift) );// && state == LaunchState.FTL;
	}
	
	/*void OnGUI()
	{		
		if( ShowGravityInfo() )
		{
			int index = 60;
			foreach(KeyValuePair<GameObject, Vector3> gravitySource in currentGravitySources)
			{
				GUI.Label(new Rect(0,index,300,30), "Gravitating Towards: " + gravitySource.Key.name);
				GUI.Label(new Rect(0,index + 30,300,30), "Force: " + gravitySource.Value.magnitude);
				index += 60;
			}
			
			foreach(VectorLine line in gravityLines)
			{
				if(line.active)
				{
					line.SetDistances();
					Vector3 point = line.GetPoint3D(20);
					Vector3 screenPoint = Camera.main.WorldToViewportPoint(point);
					GUI.Label(new Rect(screenPoint.x, screenPoint.y,300,30), "Reza is great");
				}
			}
		}
	}*/
	
	/*private LabelAnchor CreateGravityLineLabel(float gravityStrength, VectorLine gravityLine)
	{
		gravityLine.SetDistances();
		Vector3 point = gravityLine.GetPoint3D(GRAVITY_LINE_LABEL_POSITION);
		GameObject anchor = new GameObject();
		anchor.transform.position = point;
		int index = Game.gui.labelPool.SetLabel(anchor, ""+gravityStrength);
		return new LabelAnchor(anchor, index);
	}
	
	private void UpdateGravityLineLabel(LabelAnchor labelAnchor, float gravityStrength, VectorLine gravityLine)
	{
		if(gravityLine.active)
		{
			gravityLine.SetDistances();
			Vector3 point = gravityLine.GetPoint3D(GRAVITY_LINE_LABEL_POSITION);
			labelAnchor.anchor.active = true;
			labelAnchor.anchorTransform.position = point;
			//Game.gui.labelPool.SetLabelText(labelAnchor.index, "" + gravityStrength);
		}
		else
		{
			//labelAnchor.anchor.active = false;
		}
	}*/
		
	private struct LabelAnchor
	{
		public LabelAnchor(GameObject theAnchor, int theIndex)
		{
			anchor = theAnchor;
			anchorTransform = theAnchor.transform;
			index = theIndex;
		}
			
		public GameObject anchor;
		public Transform anchorTransform;
		public int index;
	}
}
