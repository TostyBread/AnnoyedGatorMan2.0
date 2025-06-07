using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AutoReturnEffect : MonoBehaviour
{
    public string effectName;
    public float overrideLifeTime = -1f; // Optional manual override
    private Coroutine autoReturnCoroutine;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Precache Animator for very minor optimization
    }

    private void OnEnable()
    {
        autoReturnCoroutine = StartCoroutine(StartAutoReturn());
    }

    private float GetAnimationDuration()
    {
        if (overrideLifeTime > 0f) return overrideLifeTime;

        Animator animator = GetComponent<Animator>();
        var controller = animator.runtimeAnimatorController;
        if (controller == null || controller.animationClips.Length == 0) return 1f;

        // Useful for playing 1 anim clip, but useless if using for transition
        // AnimatorStateInfo.length with yield return null if multiple state switching needed (most likely wont)
        return controller.animationClips[0].length;
    }


    private System.Collections.IEnumerator StartAutoReturn()
    {
        yield return null; // Wait 1 frame to let Animator update

        float duration = GetAnimationDuration();
        Invoke(nameof(DisableSelf), duration);
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
        if (autoReturnCoroutine != null)
            StopCoroutine(autoReturnCoroutine);

        CancelInvoke();

        if (EffectPool.Instance != null && gameObject.activeInHierarchy == false)
            EffectPool.Instance.ReturnEffect(effectName, gameObject);
    }
}