using UnityEngine;

public class Detection : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private GameObject scanGO;
    [SerializeField] private GameObject rightEye;
    [SerializeField] private GameObject leftEye;

    public void Play()
    {
        scanGO.SetActive(true);
        rightEye.SetActive(false);
        leftEye.SetActive(false);
        anim.Play("Scan");
        particle.Play();
    }

    public void Stop()
    {
        scanGO.SetActive(false);
        rightEye.SetActive(true);
        leftEye.SetActive(true);
        anim.Play("Nothing");
        particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
	
}
