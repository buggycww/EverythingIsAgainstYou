using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class PlayerController : MonoBehaviour
    {
        public float speed;

        private Animator animator;
        private Rigidbody2D rigidbody;
        private SpriteRenderer spriteRenderer;
        public GameObject explosionVFX;
        private Vector2 lastDirection = Vector2.down;
        private Color originalColor;
        private bool isMovingToLocation;
        private Vector3 destination;
        private bool moveToLocationStarted = false;

        public bool isDead { get; private set; } = false;

        public event System.Action OnMoveToLocationComplete;

        [Header("Walking Sounds")]
        [SerializeField] private AudioClip[] walkingAudioClips;
        [SerializeField] private float walkSoundInterval = 0.3f;
        [SerializeField] private float walkSoundVolume = 0.5f;

        private AudioSource audioSource;
        private float walkSoundTimer = 0f;
        private bool wasMoving = false;

        private void Start()
        {
            if (RespawnManager.Instance != null)
            {
                transform.position = RespawnManager.Instance.RespawnPosition;

                if (Camera.main != null)
                {
                    Camera.main.transform.position = new Vector3(
                        RespawnManager.Instance.RespawnPosition.x,
                        RespawnManager.Instance.RespawnPosition.y,
                        Camera.main.transform.position.z
                    );
                }

                SetLayerAndSortingLayer(RespawnManager.Instance.sortingLayer);
            }

            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void FixedUpdate()
        {
            if (isDead)
            {
                return;
            }

            // Handle move to location in FixedUpdate for physics consistency
            if (isMovingToLocation)
            {
                UpdateMoveToLocation();
                return;
            }

            // Normal movement
            Vector2 dir = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) dir.x = -1;
            if (Input.GetKey(KeyCode.D)) dir.x = 1;
            if (Input.GetKey(KeyCode.W)) dir.y = 1;
            if (Input.GetKey(KeyCode.S)) dir.y = -1;

            dir.Normalize();

            bool isMoving = dir.magnitude > 0;

            if (isMoving)
            {
                lastDirection = dir;
                SetAnimation(dir, true);
                PlayWalkingSound();
            }
            else
            {
                SetAnimation(lastDirection, false);
                wasMoving = false;
                walkSoundTimer = 0f;
            }

            rigidbody.linearVelocity = speed * dir;
        }

        private void Update()
        {
            // Start the move to location coroutine in Update (once)
            if (isMovingToLocation && !moveToLocationStarted)
            {
                moveToLocationStarted = true;
                StartCoroutine(MoveToLocationRoutine(destination));
            }
        }

        private void UpdateMoveToLocation()
        {
            if (rigidbody == null) return;

            Vector2 currentPos = transform.position;
            Vector2 targetPos = destination;
            float distance = Vector2.Distance(currentPos, targetPos);

            if (distance <= 0.05f)
            {
                return;
            }

            Vector2 direction = (targetPos - currentPos).normalized;
            rigidbody.linearVelocity = direction * speed;

            SetAnimation(direction, true);
            PlayWalkingSound();
        }

        #region Walking Sounds
        private void PlayWalkingSound()
        {
            // Reset timer if we weren't moving before
            if (!wasMoving)
            {
                walkSoundTimer = 0f;
                wasMoving = true;
            }

            // Update timer
            walkSoundTimer += Time.deltaTime;

            // Play sound at intervals
            if (walkSoundTimer >= walkSoundInterval)
            {
                walkSoundTimer = 0f;

                if (walkingAudioClips != null && walkingAudioClips.Length > 0)
                {
                    // Pick a random clip
                    AudioClip clip = walkingAudioClips[Random.Range(0, walkingAudioClips.Length)];
                    if (clip != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(clip, walkSoundVolume);
                    }
                }
            }
        }

        // Public method to play a single footstep (for animation events)
        public void PlayFootstep()
        {
            if (walkingAudioClips != null && walkingAudioClips.Length > 0)
            {
                AudioClip clip = walkingAudioClips[Random.Range(0, walkingAudioClips.Length)];
                if (clip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(clip, walkSoundVolume);
                }
            }
        }
        #endregion

        private void SetLayerAndSortingLayer(string sortingLayer)
        {
            if (string.IsNullOrEmpty(sortingLayer)) return;

            SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }

        public void Stop()
        {
            if (rigidbody != null)
                rigidbody.linearVelocity = Vector3.zero;

            enabled = false;

            if (animator != null)
                animator.Play("Idle_Down");

            // Reset walking sound state
            wasMoving = false;
            walkSoundTimer = 0f;
        }

        public void Die()
        {
            if (isDead)
                return;

            isDead = true;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("Die");

            if (rigidbody != null)
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.bodyType = RigidbodyType2D.Kinematic;
            }

            if (animator != null)
                animator.Play("Death_Down");

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            if (GameManager.instance != null)
                GameManager.instance.PlayerDead();

            // Stop walking sounds
            wasMoving = false;
            walkSoundTimer = 0f;
            if (audioSource != null)
                audioSource.Stop();
        }

        public void DamagePlayerAndKill()
        {
            if (isDead) return;
            StartCoroutine(DamageCoroutine());
        }

        private IEnumerator DamageCoroutine()
        {
            int damageCount = 5;
            float damageInterval = 0.2f;
            float flashDuration = 0.1f;

            if (rigidbody != null)
                rigidbody.linearVelocity = Vector3.zero;

            if (animator != null && animator.HasState(0, Animator.StringToHash("Hurt")))
            {
                animator.Play("Hurt");
            }

            for (int i = 0; i < damageCount; i++)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX("TraderCurse");

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.red;
                }

                yield return new WaitForSeconds(flashDuration);

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }

                Vector3 originalPos = transform.position;
                transform.position += new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f),
                    0
                );
                yield return new WaitForSeconds(0.05f);
                transform.position = originalPos;

                if (i < damageCount - 1)
                {
                    yield return new WaitForSeconds(damageInterval);
                }
            }

            Die();
        }

        /// <summary>
        /// Moves the player to a target location automatically.
        /// Player input is disabled during movement.
        /// </summary>
        /// <param name="location">The target position to move to</param>
        public void MoveToLocation(Vector3 location)
        {
            if (isMovingToLocation) return;
            isMovingToLocation = true;
            moveToLocationStarted = false;
            destination = location;

            // Store the direction for animation
            Vector2 direction = (location - transform.position).normalized;
            lastDirection = direction;
            SetAnimation(direction, true);

            // Reset walking sound state for new movement
            wasMoving = false;
            walkSoundTimer = 0f;
        }

        private IEnumerator MoveToLocationRoutine(Vector3 location)
        {
            // Wait until we've reached the destination
            // The actual movement is handled in FixedUpdate
            while (Vector3.Distance(transform.position, location) > 0.05f)
            {
                yield return null;
            }

            // Snap to exact position
            if (rigidbody != null)
            {
                rigidbody.MovePosition(location);
                rigidbody.linearVelocity = Vector2.zero;
            }
            else
            {
                transform.position = location;
            }

            // Stop moving
            isMovingToLocation = false;
            moveToLocationStarted = false;

            // Stop walking sounds
            wasMoving = false;
            walkSoundTimer = 0f;
            if (audioSource != null)
                audioSource.Stop();

            // Set final animation to Idle_Right
            Vector2 idleDirection = Vector2.right;
            SetAnimation(idleDirection, false);
            lastDirection = idleDirection;

            yield return new WaitForSeconds(0.1f);

            // Invoke completion event
            OnMoveToLocationComplete?.Invoke();

            // Deactivate the player
            gameObject.SetActive(false);
        }

        private void SetAnimation(Vector2 dir, bool isMoving)
        {
            if (animator == null) return;

            string stateName = (isMoving ? "Walk_" : "Idle_");

            // Determine direction
            if (dir.y > 0.1f) // Up
            {
                if (dir.x > 0.1f)
                    stateName += "RightUp";
                else if (dir.x < -0.1f)
                    stateName += "LeftUp";
                else
                    stateName += "Up";
            }
            else if (dir.y < -0.1f) // Down
            {
                if (dir.x > 0.1f)
                    stateName += "RightDown";
                else if (dir.x < -0.1f)
                    stateName += "LeftDown";
                else
                    stateName += "Down";
            }
            else // Horizontal only
            {
                if (dir.x > 0.1f)
                    stateName += "RightDown";
                else if (dir.x < -0.1f)
                    stateName += "LeftDown";
                else
                    stateName += "Down";
            }

            animator.Play(stateName);
        }
    }
}