using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	TrailRenderer trail;
	public float range;
	
	private float distanceTraveled;
		
	// Use this for initialization
	void Start () {
		trail = gameObject.GetComponent<TrailRenderer>();
		distanceTraveled = 0;
	}
	
	// Update is called once per frame
	void Update () {

			//keep track of time traveled by looking at speed. Cannot trust
			//comparing position to starting point because of floating origin coordinates
			distanceTraveled += rigidbody.velocity.magnitude * Time.deltaTime;
			if(distanceTraveled >= range)
				OnOutOfRange();
	}
	
	/// <summary>
	/// Takes information about firing object, in case any special handling is neccesary.
	/// </summary>
	/// <param name='firingObject'>
	/// Firing object.
	/// </param>
	public void NotifyFired(GameObject firingObject)
	{
		//If we have a trail renderer and were fired from a moving object
		trail = gameObject.GetComponent<TrailRenderer>();
		
		//This code offsets the starting position by a small amount so that the trail doesn not overlap the firing ship
		if(firingObject.rigidbody != null && firingObject.rigidbody.velocity.magnitude > 0 && trail != null)
		{
			float startingPointOffset = trail.time * firingObject.rigidbody.velocity.magnitude;
			transform.position += (transform.position - firingObject.transform.position).normalized * startingPointOffset;
		}
	}
	
	void OnOutOfRange()
	{
		Destroy(gameObject);
	}
	
}
