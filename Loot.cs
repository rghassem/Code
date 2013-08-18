using UnityEngine;
using System.Collections;

public class Loot : MonoBehaviour {
	
	public enum DropType
	{
		Fuel,
		Health
	}
	
	//TODO: Hold data about loot and maybe set a timer for expiration
	public float expirationTime;
	public float value;
	public DropType dropType;
	
	// Use this for initialization
	void Start () 
	{
		StartCoroutine(Expire());	
	}
	
	IEnumerator Expire()
	{
		yield return new WaitForSeconds(expirationTime);
		Destroy(gameObject);
	}
	

}
