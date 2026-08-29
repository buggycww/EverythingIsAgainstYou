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
            }
            else
            {
                SetAnimation(lastDirection, false);
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
        }

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
        }

        public void Die()
        {
            if (isDead)
                return;

            isDead = true;

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
                    stateName += "RightDown"; // Default right
                else if (dir.x < -0.1f)
                    stateName += "LeftDown"; // Default left
                else
                    stateName += "Down"; // Fallback
            }

            animator.Play(stateName);
        }
    }
}