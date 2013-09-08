using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Loot : MonoBehaviour {
	
	public enum DropType
	{
		Fuel,
		Health
	}
	
	public static readonly Dictionary<DropType, string> dropTypeNames = new Dictionary<DropType, string>()
    {
        {DropType.Fuel, "Fuel"},
        {DropType.Health, "Health"},
    };

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
