using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct LootReport
{
	public float fuelQuantity;
	public float healthQuantity;
}


public class Looter : MonoBehaviour {
	
	public float lootDistance = 500;
	public float collectionSpeed = 1;
	
	LootReport currentBatch;
	bool waitingToReport = false;
	
	void Awake()
	{
		currentBatch = new LootReport();
	}
	
	void Update()
	{
		Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, lootDistance);
		foreach(Collider thing in nearbyObjects)
		{
			Loot loot = thing.GetComponent<Loot>();
			if(loot != null)
			{
				if(thing.bounds.Contains(transform.position))
				{
					Collect (loot);
				}
				else
				{
					Vector3 lootMoveDirection = transform.position - loot.transform.position;
					loot.transform.position += lootMoveDirection.normalized * Mathf.Min ( collectionSpeed * Time.deltaTime, lootMoveDirection.magnitude );
				}
			}
		}
	}
	
	void Collect(Loot loot)
	{
		if(loot.dropType == Loot.DropType.Fuel)
			currentBatch.fuelQuantity += loot.value;
		if(loot.dropType == Loot.DropType.Health)
			currentBatch.healthQuantity += loot.value;
		
		if(!waitingToReport)
			StartCoroutine(ReportLoot(currentBatch));
		
		Destroy(loot.gameObject);
	}
	
	IEnumerator ReportLoot(LootReport report)
	{
		yield return new WaitForSeconds(0.5f);
		BroadcastMessage("OnCollectLoot", report ,SendMessageOptions.DontRequireReceiver);
		currentBatch = new LootReport();
		waitingToReport = false;
	}

}
