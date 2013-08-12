using UnityEngine;
using System.Collections;

public class Landable : MonoBehaviour {

	public void Launch(Vector3 launchPoint)
	{
		transform.parent = null;
		launchPoint.y = 0;
		transform.position = launchPoint;
		gameObject.SetActiveRecursively(true);
		BroadcastMessage("OnTakeOff", SendMessageOptions.DontRequireReceiver);
		if(collider != null)
			collider.enabled = true;
	}
	
	void OnCollisionEnter(Collision collisionInfo)
	{
		Planet land = collisionInfo.gameObject.GetComponent<Planet>();
		if(land != null)
		{
			if(land.RequestLanding(this))
			{
				//Notify other components of this game object that we have landed, and where.
				BroadcastMessage("OnLand", land, SendMessageOptions.DontRequireReceiver);
				
				if(collider != null)
					collider.enabled = false;
				transform.parent = land.transform;
				transform.position = Vector3.zero;
				gameObject.SetActiveRecursively(false);
				
				//Notify Planet that we have landed
				land.NotifyLanding(this);
			}
		}
		
	}
}
