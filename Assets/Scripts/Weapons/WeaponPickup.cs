using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Transform weaponHolder;
    private Weapon currentWeapon;
    private Fist fist;
    private Animator animator;

    void Start()
    {
        fist = GetComponentInChildren<Fist>();
        animator = GetComponent<Animator>(); // Character's Animator for throw animation
    }

    void Update()
    {
        CheckForWeaponPickup();
        HandleWeaponUsage();
    }

    private void CheckForWeaponPickup()
    {
        if (Input.GetMouseButtonDown(0)) // Example condition for picking up weapon
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Weapon") && MouseHoveringOver(collider.gameObject))
                {
                    PickupWeapon(collider.GetComponent<Weapon>());
                    break;
                }
            }
        }
    }

    private bool MouseHoveringOver(GameObject obj)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return obj.GetComponent<Collider2D>().bounds.Contains(new Vector3(mousePos.x, mousePos.y, 0));
    }

    private void PickupWeapon(Weapon weapon)
    {
        if (currentWeapon != null)
        {
            DropCurrentWeapon();
        }
        currentWeapon = weapon;
        weapon.transform.SetParent(weaponHolder);
        weapon.transform.localPosition = Vector3.zero;
        weapon.gameObject.SetActive(true);
        fist.gameObject.SetActive(false);
    }

    private void DropCurrentWeapon()
    {
        currentWeapon.transform.SetParent(null);
        currentWeapon = null;
        fist.gameObject.SetActive(true);
    }

    private void HandleWeaponUsage()
    {
        if (currentWeapon == null)
        {
            return;
        }

        if (Input.GetMouseButton(1)) // Right-click
        {
            if (Input.GetMouseButtonDown(0) && currentWeapon.isFirearm) // Hold right + left to shoot
            {
                currentWeapon.Shoot();
            }
        }
        else if (Input.GetMouseButtonDown(0)) // Left-click for melee
        {
            currentWeapon.UseAsMelee();
        }
        else if (Input.GetMouseButton(0)) // Hold left-click to throw
        {
            if (Input.GetMouseButtonUp(0)) // Release to throw
            {
                Vector3 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
                currentWeapon.Throw(direction, 10f); // Example throw force
                PlayThrowAnimation();
                DropCurrentWeapon();
            }
        }
    }

    private void PlayThrowAnimation()
    {
        animator.Play("ThrowAnim");
        Invoke("SwitchToFist", 0.5f); // Adjust timing to match throw animation
    }

    private void SwitchToFist()
    {
        fist.gameObject.SetActive(true);
    }
}