using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;


public class LootReport
{
	Dictionary<Loot.DropType, float> quantityPerType;
	StringBuilder stringBuilder;
	
	public LootReport()
	{
		quantityPerType = new Dictionary<Loot.DropType, float>();
		stringBuilder = new StringBuilder();
	}
	
	public void Add(Loot.DropType type, float quantity)
	{
		if(quantityPerType.ContainsKey(type))
			quantityPerType[type] += quantity;
		else quantityPerType.Add(type, quantity);
	}
	
	public void Add(Loot loot)
	{
		Add(loot.dropType, loot.value);
	}
	
	public float GetQuantity(Loot.DropType type)
	{
		if( quantityPerType.ContainsKey(type) )
			return quantityPerType[type];
		else return 0;
	}
	
	public bool HasType(Loot.DropType type)
	{
		return quantityPerType.ContainsKey(type);
	}
	
	public override string ToString ()
	{
		stringBuilder.Length = 0;
		foreach(Loot.DropType type in quantityPerType.Keys)
		{
			if(stringBuilder.Length > 0)
				stringBuilder.Append("\n");
			stringBuilder.Append( string.Format ("{0}: {1}\n", type, quantityPerType[type]) );
		}
		return stringBuilder.ToString();
	}
}


public class Looter : MonoBehaviour {
	
	public float lootDistance = 500;
	public float collectionSpeed = 1;
	
	LootReport currentBatch;
	bool waitingToReport = false;
	
	DynamicLabel label;
	float runningTotal;
	float resetRunningTotalSeconds = 0; 
	
	void Start()
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
				Vector3 lootMoveDirection = transform.position - loot.transform.position;
				loot.transform.position += lootMoveDirection.normalized * Mathf.Min ( collectionSpeed * Time.deltaTime, lootMoveDirection.magnitude );
				if(thing.bounds.Contains(transform.position))
				{
					Collect (loot);
				}
			}
		}
		
		if(runningTotal > 0)
		{
			if(label == null)
				label = GetLabel();
			string labelText = currentBatch.ToString();
			label.SetLabelText(labelText);
		}
		else
		{
			if(label != null)
			{
				label.Remove();
				label = null;
			}
		}
		
		if(resetRunningTotalSeconds <= 0)
			runningTotal = 0;
		
		resetRunningTotalSeconds -= Time.deltaTime;
		 
	}
	
	void Collect(Loot loot)
	{	
		currentBatch.Add(loot);

		runningTotal += loot.value;
		resetRunningTotalSeconds = 1;
		
		if(!waitingToReport)
			StartCoroutine(ReportLoot(currentBatch));
		
		Destroy(loot.gameObject);
	}
	
	IEnumerator ReportLoot(LootReport report)
	{
		waitingToReport = true;
		yield return new WaitForSeconds(0.5f); //collect for a bit first, to minimize amount of ineffiecient message passing
		BroadcastMessage("OnCollectLoot", report ,SendMessageOptions.DontRequireReceiver);
		currentBatch = new LootReport();
		waitingToReport = false;
	}
	
	private DynamicLabel GetLabel()
	{
		//Special case for ships with renderers on 
		if(transform.GetComponent<Engine>() != null)
			label = Game.gui.labelPool.Label(transform.Find("ShipMesh").gameObject, Vector3.up * collider.bounds.extents.magnitude * 5);
		else //general case
			label = Game.gui.labelPool.Label(gameObject, Vector3.up * 10); //place label a little above object
		
		return label;
	}
	
}
