using UnityEngine;
using System.Collections;

/// <summary>
/// Encapsulates an area of space where ships can move around freely, controlled from top down view.
/// Used to implement colony territories
/// </summary>
public class SafeZone : MonoBehaviour {
	
	float radius;
	ILine ring;
	
	public static SafeZone Spawn(float zoneRadius, GameObject origin)
	{
		GameObject obj = new GameObject("SafeZone", typeof(SafeZone), typeof(LineDrawer), typeof(SphereCollider));
		obj.transform.parent = origin.transform;
		obj.transform.position = Vector3.zero;
		obj.layer = LayerMask.NameToLayer("Ignore Raycast");
		
		SafeZone safeZone = obj.GetComponent<SafeZone>();
		SphereCollider collider = obj.GetComponent<SphereCollider>();
		
		safeZone.radius = zoneRadius;
		collider.radius = zoneRadius;
		collider.isTrigger = true;
		
		
		return safeZone;
	}
	
	
	void OnTriggerExit(Collider other)
	{
		Ship ship = other.GetComponent<Ship>();
		if(ship != null)
		{
			ship.EnterFTL();
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		Ship ship = other.GetComponent<Ship>();
		if(ship != null)
		{
			ship.LeaveFTL();
		}
	}
	
	public void SetRadius(float radius)
	{
		this.radius = radius;
	}
	
	public void ShowRing()
	{
		if(ring == null)
			DrawRing();
		ring.Show();
	}
	
	public void HideRing()
	{
		ring.Hide();
	}
	
	void DrawRing()
	{
		ring = GetComponent<LineDrawer>().CreateCircle(gameObject, radius ,Color.green, 5, radius);
	}
}
