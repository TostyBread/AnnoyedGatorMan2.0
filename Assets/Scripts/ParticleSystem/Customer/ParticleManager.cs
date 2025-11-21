using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject particlePrefab;

    public void SpawnParticleOnce()
    {
        bool particleSpawned = false;

        if (particlePrefab != null && !particleSpawned)
        {
             GameObject particle = Instantiate(particlePrefab, transform.position, Quaternion.identity);
             particleSpawned = true;
        }
    }
}
