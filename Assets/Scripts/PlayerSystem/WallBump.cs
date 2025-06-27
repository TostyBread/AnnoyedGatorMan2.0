using UnityEngine;

public class WallBumpDetect : MonoBehaviour
{
    public string AudioName;
    public bool WallBumpSound;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Obstacle") && WallBumpSound)
        {
            AudioManager.Instance.PlaySound(AudioName, 1.0f, transform.position);
        }
    }
}