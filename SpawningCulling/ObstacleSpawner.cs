using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns objects in front and out of sight of itself. Meant to be attached to player avatar and spawn 
/// objects in his path.
/// </summary>
public class ObstacleSpawner : ContinuousSpawner {

	protected override void Spawn()
	{
		if(prefabsToSpawn.Length == 0)
			return;
		
		int prefabNumber = Random.Range(0, prefabsToSpawn.Length-1);
		float scale = Random.Range(minimumScale, maximumScale);
		
		Vector2 distance = Random.insideUnitCircle.normalized * Random.Range(minimumSpawnDistance, maximumSpawnDistance);
		Vector3 position = transform.position + new Vector3(distance.x, 0, distance.y);
		if(rigidbody != null)
		{
			position += CalculatePositionVelocityOffset(rigidbody.velocity, distance);
		}
				
		GameObject spawnedObject = Instantiate(prefabsToSpawn[prefabNumber], position, Quaternion.identity) as GameObject;
		spawnedObject.transform.localScale = Vector3.one * scale;
		
		//Maybe give it some velocity
		if(Random.value <= chanceMoving)
		{
			Vector2 asteroidVelocity = Random.insideUnitCircle.normalized * Random.Range(minimumSpeed, maximumSpeed);
			Vector3 asteroidVelocity3D = new Vector3(asteroidVelocity.x, 0, asteroidVelocity.y); 
	
			Rigidbody physics = spawnedObject.rigidbody;
			if(physics != null)
			{
				physics.velocity = asteroidVelocity3D;
			}
		}
		
		spawnedObjects.Add( spawnedObject );
		
	}
	
	/// <summary>
	/// Calculates how far to offset to spawn position from a moving object, so the spawned object will spawn in front of the this object
	/// </summary>
	/// <returns>
	/// The position velocity offset.
	/// </returns>
	private Vector3 CalculatePositionVelocityOffset(Vector3 velocity, Vector2 asteroidDistance)
	{
		return velocity + (velocity.normalized * (asteroidDistance.magnitude/2) );
	}
}
