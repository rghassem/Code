using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns objects listed in prefabsToSpawn variable
/// </summary>
public class Spawner : MonoBehaviour {
	
	/// <summary>
	/// The spawned objects. One will be randomly selected to spawn each spawn call
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
	
	/// <summary>
	/// If true, the spawner will destroy itself if:
	/// 1.) It has spawned some objects already.
	/// 2.) None of those objects remain
	/// Use to clean up spawners with one time purposes.
	/// </summary>
	public bool destroyWhenEmpty = false;

	
	protected List<GameObject> spawnedObjects; //Contains references to every object spawned by this spawner.
	protected int targetNumberOfObjects;
	protected GameObject container;
	
	bool isInitialized = false;
	
	void Start()
	{
		Initialize();
		if(beginAutomatically)
			Go();

	}
	
	private void Initialize()
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
		
		isInitialized = true;
	}
	
	/// <summary>
	/// Spawn asteroids.
	/// </summary>
	public virtual void Go()
	{
		if(!isInitialized)
			Initialize();
		
		container = new GameObject(prefabsToSpawn[0].name + "s");
		container.transform.position = Vector3.zero;
		while(spawnedObjects.Count < targetNumberOfObjects)
		{
			Spawn();
		}
		
		if(destroyWhenEmpty)
			StartCoroutine(WaitTillEmptyAndCleanUp());
	}
	
	public void SetParmateres(
		int minimumSpawnDistance,
		int maximumSpawnDistance,
		float chanceMoving,
		float minimumSpeed,
		float maximumSpeed,
		int maximumSpawnedObjects,
		int minimumSpawnedObjects = -1,
		float minimumScale = 1,
		float maximumScale = 1
		)
	{
		this.minimumSpawnDistance = minimumSpawnDistance;
		this.maximumSpawnDistance = maximumSpawnDistance;
		this.chanceMoving = chanceMoving;
		this.minimumSpeed = minimumSpeed;
		this.maximumSpeed = maximumSpeed;
		this.maximumSpawnedObjects = maximumSpawnedObjects;
		this.minimumSpawnedObjects = (minimumSpawnedObjects < 0) ? maximumSpawnedObjects : minimumSpawnedObjects;
		this.minimumScale = minimumScale;
		this.maximumScale = maximumScale;
		beginAutomatically = false;
		
		Initialize();
	}
	
	public void SetSpawnObjects(params GameObject[] objects)
	{
		prefabsToSpawn = objects;
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
		spawnedObject.transform.parent = container.transform;
		
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
	
	IEnumerator WaitTillEmptyAndCleanUp()
	{
		yield return new WaitForSeconds(5); //just minor cleanup, no hurry
		
		if(transform.childCount == 0)
			Destroy(gameObject);
	}
	


}
