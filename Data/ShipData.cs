using UnityEngine;
using System.Collections;

public class ShipData : ScriptableObject {

	public string ShipName;
	public int Energy;
	
	public ShipData()
	{
		ShipName = "Enterprise";
		Energy = 9001;
	}
		
}
