using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns objects continuosly, until ContinousSpawner.Stop() is called.
/// </summary>
public class ContinuousSpawner : Spawner {
	
	/// <summary>
	/// The minimum time between spawns
	/// </summary>
	public float minimumSpawnTime;
	
	/// <summary>
	/// The maximum time between spawns.
	/// </summary>
	public float maximumSpawnTime;
	
	protected bool shouldSpawn;
	/// <summary>
	/// Start spawning asteroids.
	/// </summary>
	public override void Go()
	{
		shouldSpawn = true;
		StartCoroutine(SpawnObjects());
	}
	
	/// <summary>
	/// Stop spawning asteroids
	/// </summary>
	public void Stop()
	{
		shouldSpawn = false;
	}
	
	
	protected virtual IEnumerator SpawnObjects()
	{
		while(shouldSpawn)
		{
			Spawn();
			
			float timeTillNextSpawn = Random.Range(minimumSpawnTime, maximumSpawnTime);
			yield return new WaitForSeconds(timeTillNextSpawn);
		}
	}
	
}
