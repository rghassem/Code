using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {
	
	private const int LIGHTSPEED_PARTICLE_ORIGIN_DISTANCE = 50;
	private const int LIGHTSPEED_PARTICLE_SIMULATE_TIME = 5;
	
	Transform effectTransform;
	ParticleSystem lightSpeedEffect;
	Vector3 lastFramePosition; //for estimating the velocity of the transform (can't assume a rigidbody is present)
	Vector3 defaultEffectPosition;
	float defaultEmissionSpeed;
	
	void Awake()
	{
		effectTransform = transform.Find("Light Speed Effect");
		lightSpeedEffect = effectTransform.particleSystem;
		lightSpeedEffect.Stop();
		lastFramePosition = transform.position;
		defaultEffectPosition = lightSpeedEffect.transform.localPosition;
	}
	
	void Start()
	{
		defaultEmissionSpeed = lightSpeedEffect.startSpeed;
	}
	
	void Update()
	{
		Vector3 velocity = transform.position - lastFramePosition;
		
		//position lightSpeedEffect
		effectTransform.position = transform.position + velocity.normalized * LIGHTSPEED_PARTICLE_ORIGIN_DISTANCE;
		effectTransform.LookAt(Game.mainCamera.transform);
		
		//scale effect based on speed
		lightSpeedEffect.emissionRate = velocity.magnitude * 2;
		lightSpeedEffect.startSpeed = velocity.magnitude;
		
		lastFramePosition = transform.position;
	}
	
	public void ActivateLightSpeed()
	{
		lightSpeedEffect.transform.LookAt(Game.mainCamera.transform.position);
		lightSpeedEffect.Simulate(LIGHTSPEED_PARTICLE_SIMULATE_TIME);
		lightSpeedEffect.Play();
	}
	
	public void DeactivateLightSpeed()
	{
		lightSpeedEffect.Stop();
	}
		
}
