using UnityEngine;

public class FireExtinguisher : MonoBehaviour
{
    public float gasAmount = 30;
    public float sprayDur = 3;
    public GameObject Projectile;
    public Collider2D FreezeBox;
    public string sprayAudioName;

    private GameObject holdingPlayer;
    private float currentSprayDur;
    private float currentGasAmount;
    private bool isPlayingSound = false;
    private PlayerInputManager playerInputManager;
    private Animator animator;
    private WeatherManager window;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        window = GameObject.FindGameObjectWithTag("Window").GetComponent<WeatherManager>();

        currentSprayDur = sprayDur;
        currentGasAmount = gasAmount;
        Projectile.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (animator == null) return;

        if (window.weather != WeatherManager.Weather.Cold) FreezeBox.enabled = false;
        else FreezeBox.enabled = true;

        //Get top parent, can use if top parent is player
        if (transform.parent != null && transform.root.gameObject.CompareTag("Player"))
        {
            holdingPlayer = transform.root.gameObject;

            animator.SetBool("Usable", true);

            if (currentSprayDur <= 0 || currentGasAmount <= 0)
            {
                Projectile.SetActive(false);
                AudioManager.Instance.StopSound(sprayAudioName);
                isPlayingSound = false;
            }

            playerInputManager = holdingPlayer.GetComponent<PlayerInputManager>();

            //Use fire extinguisher
            if (Input.GetKey(playerInputManager.inputConfig.attackKey) && currentSprayDur > 0 && currentGasAmount >= 0)
            {
                Projectile.SetActive(true);

                if (!isPlayingSound)
                {
                    AudioManager.Instance.PlaySound(sprayAudioName, transform.position);
                    isPlayingSound = true;
                }

                currentSprayDur -= Time.deltaTime;
                currentGasAmount -= Time.deltaTime;
            }

            if (Input.GetKeyUp(playerInputManager.inputConfig.attackKey))
            {
                Projectile.SetActive(false);
                AudioManager.Instance.StopSound(sprayAudioName);
                isPlayingSound = false;
                currentSprayDur = sprayDur;
            }
        }

        if (transform.parent == null)
        {
            animator.SetBool("Usable", false);
            Projectile.SetActive(false);
            AudioManager.Instance.StopSound(sprayAudioName);
            isPlayingSound = false;
        }
    }
}
