using UnityEngine;

public class P2ThrowController : MonoBehaviour
{
    public P2PickSystem p2;
    public GameObject ThrowDirection;
    private GameObject Player;
    private CharacterFlip characterFlip;
    public Vector3 offset;

    [Header("Input")]
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;

    // Start is called before the first frame update
    void Start()
    {
        if (p2 == null)
        {
            Debug.Log("P2 is null, so P2 will get P2PickSystem from its parent");
            p2 = GetComponentInParent<P2PickSystem>();
        }

        Player = GameObject.FindGameObjectWithTag("Player");
        characterFlip = Player.GetComponent<CharacterFlip>();

        gameObject.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        HandleThrowFlip();
        ShowAndHideThrowDirection();
        ThrowContoller();
    }

    private void ShowAndHideThrowDirection()
    {
        if (ThrowDirection != null)
        {
            if (p2.heldItem == null)
            {
                ThrowDirection.SetActive(false);
            }
            else
            {
                ThrowDirection.SetActive(true);
            }
        }
    }

    private void HandleThrowFlip()
    {
        if (characterFlip.isFacingRight)
        {
            transform.position = Player.transform.position + offset;
        }
        else
        {
            transform.position = Player.transform.position - offset;
        }
    }

    private void ThrowContoller()
    {
        if (Input.GetKey(up) && Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, 45);
        }
        else if (Input.GetKey(up) && Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, 135);
        }
        else if (Input.GetKey(down) && Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, -135);
        }
        else if (Input.GetKey(down) && Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, -45);
        }
        else if (Input.GetKeyDown(up))
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetKeyDown(down))
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (Input.GetKeyDown(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (Input.GetKeyDown(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
