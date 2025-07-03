using UnityEngine;

public class WallBumpDetect : MonoBehaviour
{
    public string AudioName;
    public bool WallBumpSound;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Obstacle") && WallBumpSound)
        {
            AudioManager.Instance.PlaySound(AudioName, transform.position);
        }
    }
}