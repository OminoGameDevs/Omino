using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ice : MonoBehaviour
{
    private BoxCollider trigger;
    private ParticleSystem particles;

    private void Awake()
    {
        trigger = GetComponent<BoxCollider>();
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (!particles.isPlaying)
            particles.Play();
    }
}
