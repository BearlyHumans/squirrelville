using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{

    //THIS IS A VERY SIMPLE/INELEGANT IMPLEMENTATION
    //YOU ARE WELCOME TO CHANGE IT


    public List<ParticleSystem> particles = new List<ParticleSystem>();

    public void PlayParticle(string particleName)
    {
        foreach (ParticleSystem P in particles)
        {
            if (P.name == particleName)
            {
                P.gameObject.SetActive(true);
                P.Play();
                return;
            }
        }
    }

    public void PauseParticle(string particleName)
    {
        foreach (ParticleSystem P in particles)
        {
            if (P.name == particleName)
            {
                P.Pause();
                return;
            }
        }
    }

    public void EnableParticle(string particleName)
    {
        foreach (ParticleSystem P in particles)
        {
            if (P.name == particleName)
            {
                P.gameObject.SetActive(true);
                return;
            }
        }
    }

    public void DisableParticle(string particleName)
    {
        foreach (ParticleSystem P in particles)
        {
            if (P.name == particleName)
            {
                P.gameObject.SetActive(false);
                return;
            }
        }
    }

    public void PlayOrContinueParticle(string particleName)
    {
        foreach (ParticleSystem P in particles)
        {
            if (P.name == particleName)
            {
                P.gameObject.SetActive(true);
                if (!P.isPlaying)
                {
                    P.Play();
                    return;
                }
            }
        }
    }

    public void StopParticle(string particleName)
    {
        foreach (ParticleSystem P in particles)
        {
            if (P.name == particleName)
            {
                if (P.isPlaying)
                {
                    P.Stop();
                    return;
                }
            }
        }
    }

    private void Awake()
    {
        ParticleSystem[] childParticles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem child in childParticles)
        {
            if (!particles.Contains(child))
                particles.Add(child);
        }
    }

}
