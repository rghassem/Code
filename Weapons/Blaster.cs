using UnityEngine;
using System.Collections;

public class Blaster : MonoBehaviour {
	
	public GameObject ordinence;
	public KeyCode triggerKey;
	public float speed;
	public float cooldown;
	
	private Rigidbody actingRigidbody; //The nearest rigidbody available in the object heirarchy
	private float cooldownCounter;
	
	void Start()
	{
		GetNearestRigidBody(); //set actingRigidBody for better performan
	}
	
	// Update is called once per frame
	void Update () {
		
		if(cooldownCounter > 0)
			cooldownCounter -= Time.deltaTime;
		
		if(Input.GetKeyDown(triggerKey) && Game.SelectedObject == transform.parent.gameObject) //do selection better
		{
			if(cooldownCounter <= 0)
				Fire();
		}
	}
	
	public void Fire()
	{
		Fire(transform.forward, speed);
	}
	
	public void Fire(Vector3 targetDirection, float targetSpeed )
	{
		Vector3 targetForce = targetDirection * targetSpeed;
		
		GameObject bullet = Instantiate(ordinence) as GameObject;
		bullet.transform.position = transform.position;
		
		Projectile projectileController = bullet.GetComponent<Projectile>();
		if(projectileController != null)
		{
			projectileController.NotifyFired(GetNearestRigidBody().gameObject);
		}
		
		bullet.rigidbody.AddForce(GetBaseVelocity() + targetForce, ForceMode.VelocityChange);
		
		cooldownCounter = cooldown;
	}
	
	private Vector3 GetBaseVelocity()
	{		
		if(GetNearestRigidBody() == null)
			return Vector3.zero;
		else return actingRigidbody.velocity;
	}

	/// <summary>
	/// search up object heirarchy for nearest rigidbody if this object doesn't have one
	/// </summary>
	/// <returns>
	/// The nearest rigid body, or null if none found
	/// </returns>
	private Rigidbody GetNearestRigidBody()
	{				
		while(actingRigidbody == null && transform.parent != null)
		{
			actingRigidbody = transform.parent.rigidbody;
		}
		
		return actingRigidbody;
	}
}
