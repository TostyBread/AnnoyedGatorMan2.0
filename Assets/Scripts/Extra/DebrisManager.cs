using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DebrisManager : MonoBehaviour
{
    public static DebrisManager Instance { get; private set; }
    public List<GameObject> debrisPrefabsList; // Assign in inspector
    private Dictionary<string, GameObject> debrisPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        PreloadDebris();
    }

    private void PreloadDebris()
    {
        foreach (var prefab in debrisPrefabsList)
        {
            if (prefab != null)
            {
                debrisPrefabs[prefab.name] = prefab;
            }
        }
    }

    public void PlayDebrisEffect(string name, Vector3 position, ItemSystem.DamageType damageType)
    {
        string animationState = damageType switch
        {
            ItemSystem.DamageType.Bash => "SmokePuff",
            ItemSystem.DamageType.Cut => "SmokePuff",
            ItemSystem.DamageType.Shot => "SmokePuff",
            _ => "DebrisNeutral"
        };
        PlayDebrisEffect(name, position, animationState);
    }

    public void PlayDebrisEffect(string name, Vector3 position, string animationState = "DebrisNeutral")
    {
        if (debrisPrefabs.TryGetValue(name, out GameObject prefab))
        {
            GameObject debris = Instantiate(prefab, position, Quaternion.identity);
            Animator animator = debris.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.Play(animationState);
                StartCoroutine(PlayAndDestroy(debris, animator));
            }
            else
            {
                Destroy(debris, 1f); // Fallback if no animator exists
            }
        }
    }

    private IEnumerator PlayAndDestroy(GameObject debris, Animator animator)
    {
        yield return null; // Ensure the Animator has updated

        float clipLength = 1f;
        if (animator.GetCurrentAnimatorClipInfo(0).Length > 0)
        {
            clipLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        }
        yield return new WaitForSeconds(clipLength);
        Destroy(debris);
    }
}