using UnityEngine;
using System.Collections;

public class Launcher : SelectableBody {
	
	/// <summary>
	/// Velocity of ship at launch.
	/// </summary>
	public float LaunchForce;
	
	/// <summary>
	/// Distance launched ships can travel "faster than light"
	/// </summary>
	public float MaxDistance;
	
	private Ship launchingShip;
	
	
	void Update()
	{
		if(launchingShip == null)
			return;
		if(launchingShip.GetLaunchState() != "Launching")
			return;
		
		Vector3 targetDirection = (transform.position - launchingShip.transform.position).normalized;
		
		//float rotationAngle = Vector2.Angle(new Vector2(targetDirection.x, targetDirection.z), 
		//												new Vector2(transform.up.x, transform.up.z) ); 
		
		Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		
		transform.rotation = targetRotation;
		
	}
	
	void OnTriggerEnter(Collider other)
	{
		launchingShip = other.gameObject.GetComponent<Ship>() as Ship;
		if(launchingShip != null)
		{
			launchingShip.ReadyLaunch(gameObject, LaunchForce, MaxDistance);
		}
	}
	
	
}
