using UnityEngine;
using Cainos.PixelArtTopDown_Basic;
using System.Collections;

public class WickedWind : BaseInteractable
{
    [Header("Wind Settings")]
    [SerializeField] private float windForce = 10f;
    [SerializeField] private float activateWindDelay = 2f;

    [Header("Wind Visuals")]
    [SerializeField] private GameObject topWind;
    [SerializeField] private GameObject leftWind;
    [SerializeField] private GameObject rightWind;
    [SerializeField] private GameObject bottomWind;

    [Header("Dialogue")]
    [SerializeField] private int killedPlayerIndex;
    [SerializeField] private int clearedWindIndex;

    private bool canActivateWind = false;
    public bool hasPlayerTouchedWind = false;
    private bool isPlayerInRange = false;
    private Vector2 currentWindDirection = Vector2.zero;
    private PlayerController playerController;
    private Rigidbody2D playerRb;
    private bool wasPlayerControllerEnabled = false;
    private bool wasWindCleared = false;

    private AudioSource audioSource;

    private void Start()
    {
        SetAllWindVisuals(false);
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isPlayerInRange || playerRb == null || (playerController != null && playerController.isDead))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            return;
        }


        // Get player input
        Vector2 input = GetPlayerInput();

        if (input != Vector2.zero)
        {
            // Blow wind in the OPPOSITE direction of player input
            Vector2 windDirection = -input.normalized;

            // Update wind visual and apply force
            UpdateWind(windDirection);
        }
        else
        {
            // No input, stop wind
            StopWind();
        }
    }

    private void UpdateWind(Vector2 direction)
    {
        if (!audioSource.isPlaying)
            audioSource.Play();

        // If direction changed, update visuals
        if (direction != currentWindDirection)
        {
            currentWindDirection = direction;
            ActivateWindVisual(direction);
        }

        // Disable PlayerController to prevent it from overriding velocity
        if (playerController != null && playerController.enabled)
        {
            wasPlayerControllerEnabled = true;
            playerController.enabled = false;
            Debug.Log("PlayerController disabled");
        }

        // Override velocity directly
        playerRb.linearVelocity = direction * windForce;
        Debug.Log($"Wind applying force: {direction * windForce}");
    }

    private void StopWind()
    {
        if (currentWindDirection != Vector2.zero)
        {
            currentWindDirection = Vector2.zero;
            SetAllWindVisuals(false);

            // Re-enable PlayerController if it was enabled before
            if (playerController != null && wasPlayerControllerEnabled)
            {
                audioSource.Stop();
                playerController.enabled = true;
                wasPlayerControllerEnabled = false;
            }
        }
    }

    #region Trigger Detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isPlayerInRange)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                canActivateWind = true;
                playerController = player;
                playerRb = other.GetComponent<Rigidbody2D>();
                StartCoroutine(ActivateWindAfterDelay(activateWindDelay));
            }
        }
    }

    private IEnumerator ActivateWindAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (canActivateWind)
        {
            isPlayerInRange = true;
            hasPlayerTouchedWind = true;
            wasPlayerControllerEnabled = playerController.enabled;
            audioSource.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            canActivateWind = false;
            isPlayerInRange = false;
            StopWind();
        }
    }
    #endregion

    #region Wind Logic
    private Vector2 GetPlayerInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x = 1;

        return input.normalized;
    }
    #endregion

    #region Visual Management
    private void ActivateWindVisual(Vector2 direction)
    {
        SetAllWindVisuals(false);

        if (direction.y > 0.5f) // Blowing UP
        {
            if (topWind != null) topWind.SetActive(true);
        }
        else if (direction.y < -0.5f) // Blowing DOWN
        {
            if (bottomWind != null) bottomWind.SetActive(true);
        }
        else if (direction.x > 0.5f) // Blowing RIGHT
        {
            if (rightWind != null) rightWind.SetActive(true);
        }
        else if (direction.x < -0.5f) // Blowing LEFT
        {
            if (leftWind != null) leftWind.SetActive(true);
        }
    }

    private void SetAllWindVisuals(bool active)
    {
        if (topWind != null) topWind.SetActive(active);
        if (leftWind != null) leftWind.SetActive(active);
        if (rightWind != null) rightWind.SetActive(active);
        if (bottomWind != null) bottomWind.SetActive(active);
    }
    #endregion

    #region Public Methods
    public void KilledPlayer()
    {
        if (!hasPlayerTouchedWind) return;

        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.StartDialogue(dialogues[killedPlayerIndex]);
        }
        else
        {
            Debug.LogWarning("DialogueSystem.Instance is null - cannot start dialogue!");
        }
    }

    public void PlayerClearedWind()
    {
        if (!wasWindCleared)
        {
            wasWindCleared = true;
            DialogueSystem.StartDialogue(dialogues[clearedWindIndex]);
        }
    }

    public void ForceWind(Vector2 direction)
    {
        if (isPlayerInRange)
        {
            UpdateWind(direction);
        }
    }

    public void StopWindExternal()
    {
        StopWind();
    }
    #endregion

    #region Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
    #endregion
}