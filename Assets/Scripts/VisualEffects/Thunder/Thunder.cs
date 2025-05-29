using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Thunder : MonoBehaviour
{
    public float thunderDamageDelay;
    public float thunderAnimationLength;
    public string thunderAudioName;

    private CapsuleCollider2D capsuleCollider;
    private Light2D light2D;

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        light2D = GetComponent<Light2D>();

        capsuleCollider.enabled = false;
        light2D.enabled = false;
        StartCoroutine(ThunderStrike(thunderDamageDelay));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator ThunderStrike(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance.PlaySound(thunderAudioName, 1.0f, transform.position);
        capsuleCollider.enabled = true;
        light2D.enabled = true;
        Destroy(this.gameObject, thunderAnimationLength - thunderDamageDelay);
    } 
}
