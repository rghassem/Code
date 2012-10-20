using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script implements a gravitational field that acts on ships in flight
/// </summary>/
public class GravityField : MonoBehaviour {
	
	public float GravitationalRadius;
	public float GravitationStrength;
	public float BaseGravitationalForce;
	public GravityMode GravitationMode;
	
	public enum GravityMode
	{
		Constant,
		Linear,
		LinearWithBase,
		Newtonian
	}
	
	private HashSet<GameObject> objectsVisible;
	private HashSet<GameObject> ignoreList;
	private Dictionary<GameObject, Gravitable> gravityTargetList;
	
	private int layerBitMask;

	void Start()
	{
		ignoreList = new  HashSet<GameObject>();
		gravityTargetList = new Dictionary<GameObject, Gravitable>();
		layerBitMask = 1 << LayerMask.NameToLayer("SmallBodies");
	}
	
	void FixedUpdate ()
	{
		//Find nearby ships
		objectsVisible = new HashSet<GameObject>();
		Collider[] collidersInRange = Physics.OverlapSphere(transform.position, GravitationalRadius, layerBitMask);
		foreach(Collider col in collidersInRange)
		{
			GameObject target = col.gameObject;
			if( gravityTargetList.ContainsKey(target) || ignoreList.Contains(target) )
			{
				objectsVisible.Add(target);
				continue;
			}
			else
			{
				Gravitable targetInterface = target.GetComponent<Gravitable>() as Gravitable;
				if( targetInterface == null )
					ignoreList.Add(target);
				else
					gravityTargetList.Add(target, targetInterface);
			}
				
		}
		
		foreach(KeyValuePair<GameObject, Gravitable> targetPair in gravityTargetList)
		{
			if(objectsVisible.Contains(targetPair.Key))
				applyGravity(targetPair.Key, targetPair.Value);
			else
				targetPair.Value.EscapeGravity(gameObject);
		}
	}
	
	void applyGravity(GameObject targetObject, Gravitable targetInterface)
	{
		
		Vector3 lineFromTarget = transform.position - targetObject.transform.position;
		float distance = lineFromTarget.magnitude;
		Vector3 pullDirection = lineFromTarget.normalized;
		Vector3 pull = pullDirection;
		float gravityStrength;
		
		switch(GravitationMode)
		{
		case GravityMode.Constant:
			pull =  pullDirection * BaseGravitationalForce;
			break;
		
		case GravityMode.Linear:
			gravityStrength = GravitationStrength/distance;
			pull =  pullDirection * gravityStrength;
			break;
			
		case GravityMode.LinearWithBase:
			gravityStrength = GravitationStrength/distance;
			Vector3 basePull = pullDirection * BaseGravitationalForce;
			pull = basePull + pullDirection * gravityStrength;
			break;
			
		case GravityMode.Newtonian:
			gravityStrength = ( 1 / Mathf.Pow(distance, 2) ) * GravitationStrength;
			pull =  pullDirection * gravityStrength;
			break;
		}
		
		if(pull.magnitude <= 1)
			targetInterface.EscapeGravity(gameObject);
		else
			targetInterface.Gravitate(gameObject, pull);
	}
	
	
}


