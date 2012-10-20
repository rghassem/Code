// FloatingOrigin.cs
// Written by Peter Stirling
// 11 November 2010
// Uploaded to Unify Community Wiki on 11 November 2010
// URL: http://www.unifycommunity.com/wiki/index.php?title=Floating_Origin
using UnityEngine;
using System.Collections;

/// <summary>
/// This script translates all objects in the world to keep the camera near the origin preventing spatial jittering 
/// due to floating-point inaccuracies. It detects when the camera is further than 'threshold' units from the origin, 
/// at which point it moves everything so that the camera is back at the origin. There is also an option to disable 
/// physics beyond a certain point (which is currently commented out). 
/// </summary>/
[RequireComponent(typeof(Camera))]
public class FloatingOrigin : MonoBehaviour
{
    public float threshold = 100.0f;
    //public float physicsThreshold = 1000.0f; // Set to zero to disable
    //public float defaultSleepVelocity = 0.14f;
    //public float defaultAngularVelocity = 0.14f;
    
    void LateUpdate()
    {
        Vector3 cameraPosition = gameObject.transform.position;
        cameraPosition.y = 0f;
        if (cameraPosition.magnitude > threshold)
        {
            Object[] objects = FindObjectsOfType(typeof(Transform));
            foreach(Object o in objects)
            {
                Transform t = (Transform)o;
                if (t.parent == null && t.gameObject.layer != 12 ) //layer 12 is Vectrocity stuff. Can't interfere with that
                {
                    t.position -= cameraPosition;
                }
            }

            objects = FindObjectsOfType(typeof(ParticleEmitter));
            foreach (Object o in objects)
            {
                ParticleEmitter pe = (ParticleEmitter)o;
                Particle[] emitterParticles = pe.particles;
                for(int i = 0; i < emitterParticles.Length; ++i)
                {
                    emitterParticles[i].position -= cameraPosition;
                }
                pe.particles = emitterParticles;
            }
			// Commented out by Reza Ghassemi
            /*if (physicsThreshold >= 0f)
            {
                objects = FindObjectsOfType(typeof(Rigidbody));
                foreach (Object o in objects)
                {
                    Rigidbody r = (Rigidbody)o;
                    if (r.gameObject.transform.position.magnitude > physicsThreshold)
                    {
                        r.sleepAngularVelocity = float.MaxValue;
                        r.sleepVelocity = float.MaxValue;
                    }
                    else
                    {
                        r.sleepAngularVelocity = defaultSleepVelocity;
                        r.sleepVelocity = defaultAngularVelocity;
                    }
                }
            } */
        }
    }
}