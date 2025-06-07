using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AutoReturnEffect : MonoBehaviour
{
    public string effectName;
    public float overrideLifeTime = -1f; // Optional manual override

    private void OnEnable()
    {
        float duration = GetAnimationDuration();
        Invoke(nameof(DisableSelf), duration);
    }

    private float GetAnimationDuration()
    {
        if (overrideLifeTime > 0f) return overrideLifeTime;

        Animator animator = GetComponent<Animator>();
        if (animator.runtimeAnimatorController == null) return 2f;

        string clipName = animator.GetCurrentAnimatorStateInfo(0).IsName("Default")
            ? "Default"
            : animator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString();

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 2f; // Fallback
    }

    private void DisableSelf()
    {
        if (EffectPool.Instance != null)
            EffectPool.Instance.ReturnEffect(effectName, gameObject);
        else
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();

        // Safety: ensure return to pool if not already handled
        if (EffectPool.Instance != null && gameObject.activeInHierarchy == false)
        {
            EffectPool.Instance.ReturnEffect(effectName, gameObject);
        }
    }
}