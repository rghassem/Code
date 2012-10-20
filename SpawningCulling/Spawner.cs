using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns objects listed in prefabsToSpawn variable
/// </summary>
public class Spawner : MonoBehaviour {
	
	/// <summary>
	/// The spawned objects. One will be randomly select to spawn each spawn time
	/// </summary>
	public GameObject[] prefabsToSpawn;
	
	
	/// <summary>
	/// The minimum distance from the spawner to spawn the object
	/// </summary>
	public int minimumSpawnDistance;
	
	/// <summary>
	/// The maximum distance from the spawner to spawn the object.
	/// </summary>
	public int maximumSpawnDistance;
	
	/// <summary>
	/// The chance of a spawned object is moving. Must be between 0 and 1
	/// </summary>
	public float chanceMoving;
	
	/// <summary>
	/// Minimum speed of a moving object
	/// </summary>
	public float minimumSpeed;
	
	/// <summary>
	/// Maximum speed of a moving object
	/// </summary>
	public float maximumSpeed;
	
	/// <summary>
	/// The maximum number of spawned objects. Set to zero for infinite.
	/// </summary>
	public int maximumSpawnedObjects;
	
	/// <summary>
	/// The minimum number of spawned objects.
	/// </summary>
	public int minimumSpawnedObjects;
	
	/// <summary>
	/// The minimum ratio by which to scale a spawned object by transform.
	/// </summary>
	public float minimumScale = 1;
	
	/// <summary>
	/// The maximum ratio by which to scale a spawned object by transform.
	/// </summary>
	public float maximumScale = 10;
	
	/// <summary>
	/// True to spawn objects when spawner is first activated, false to wait for call to Spawner.Go().
	/// </summary>
	public bool beginAutomatically;

	
	protected List<GameObject> spawnedObjects; //Contains references to every object spawned by this spawner.
	protected int targetNumberOfObjects;
	
	void Start()
	{
		if(chanceMoving > 1 || chanceMoving < 0)
		{
			chanceMoving = Mathf.Clamp01(chanceMoving);
		}
		
		if( (maximumSpawnedObjects - minimumSpawnedObjects) > 0 )
		{
			if(maximumSpawnedObjects == 0)
				targetNumberOfObjects = -1;
			else targetNumberOfObjects = maximumSpawnedObjects;
		}
		else
			targetNumberOfObjects = Random.Range(minimumSpawnedObjects, maximumSpawnedObjects);
		
		spawnedObjects = new List<GameObject>();
		
		if(beginAutomatically)
			Go();
	}
	
	/// <summary>
	/// Spawn asteroids.
	/// </summary>
	public virtual void Go()
	{
		while(spawnedObjects.Count < targetNumberOfObjects)
		{
			Spawn();
		}
	}
	
	
	/// <summary>
	/// Creates an instance of one of the spawned objects
	/// </summary>
	protected virtual void Spawn()
	{
		if(prefabsToSpawn.Length == 0)
			return;
		
		int prefabNumber = Random.Range(0, prefabsToSpawn.Length-1);
		float scale = Random.Range(minimumScale, maximumScale);
		
		Vector2 distance = Random.insideUnitCircle.normalized * Random.Range(minimumSpawnDistance, maximumSpawnDistance);
		Vector3 position = transform.position + new Vector3(distance.x, 0, distance.y);
				
		GameObject spawnedObject = Instantiate(prefabsToSpawn[prefabNumber], position, Quaternion.identity) as GameObject;
		spawnedObject.transform.localScale = Vector3.one * scale;
		
		//Maybe give it some velocity
		if(Random.value <= chanceMoving)
		{
			Vector2 velocity = Random.insideUnitCircle.normalized * Random.Range(minimumSpeed, maximumSpeed);
			Vector3 velocity3D = new Vector3(velocity.x, 0, velocity.y); 
	
			Rigidbody physics = spawnedObject.rigidbody;
			if(physics != null)
			{
				physics.velocity = velocity3D;
			}
		}
	
		spawnedObjects.Add( spawnedObject );
		
	}
	

}
