using System.Collections;
using System.Collections.Generic;
using Unity.ProjectAuditor.Editor.Core;
using UnityEngine;

// test

namespace Cainos.PixelArtTopDown_Basic
{
    public class PlayerController : MonoBehaviour
    {
        public float speed;

        private Animator animator;
        private Rigidbody2D rigidbody;
        private SpriteRenderer spriteRenderer;
        private Vector2 lastDirection = Vector2.down;
        private Color originalColor;

        // Map directions to animation states
        private enum DirectionState
        {
            Idle_Down,
            Idle_Up,
            Idle_LeftDown,
            Idle_LeftUp,
            Idle_RightDown,
            Idle_RightUp,
            Walk_Down,
            Walk_Up,
            Walk_LeftDown,
            Walk_LeftUp,
            Walk_RightDown,
            Walk_RightUp
        }

        public bool isDead { get; private set; } = false;

        private void Start()
        {
            transform.position = RespawnManager.Instance.RespawnPosition;
            Camera.main.transform.position = new Vector3(RespawnManager.Instance.RespawnPosition.x, 
                RespawnManager.Instance.RespawnPosition.y,
                    Camera.main.transform.position.z);
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (isDead)
            {
                return;
            }

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

        public void Stop()
        {
            rigidbody.linearVelocity = Vector3.zero;
            enabled = false;
            animator.Play("Idle_Down");
        }

        public void Die()
        {
            if (isDead)
                return;

            isDead = true;
            rigidbody.linearVelocity = Vector3.zero;
            animator.Play("Death_Down");

            // Reset color to original before death
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            GameManager.instance.PlayerDead();
        }

        /// <summary>
        /// Deals damage to the player 5 times with visual feedback, then kills the player.
        /// </summary>
        public void DamagePlayerAndKill()
        {
            if (isDead) return;
            StartCoroutine(DamageCoroutine());
        }

        private IEnumerator DamageCoroutine()
        {
            int damageCount = 5;
            float damageInterval = 0.2f; // Time between each damage instance
            float flashDuration = 0.1f; // How long each flash lasts

            rigidbody.linearVelocity = Vector3.zero;

            // Store the current facing direction for animation
            string currentAnim = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

            // Play hurt animation if available, otherwise use current
            if (animator.HasState(0, Animator.StringToHash("Hurt")))
            {
                animator.Play("Hurt");
            }

            for (int i = 0; i < damageCount; i++)
            {
                // Flash red
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.red;
                }

                // Play hurt sound/effect (optional)
                // AudioManager.Play("PlayerHurt");

                // Wait for flash duration
                yield return new WaitForSeconds(flashDuration);

                // Return to original color
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }

                // Shake effect (optional) - small random displacement
                Vector3 originalPos = transform.position;
                transform.position += new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f),
                    0
                );
                yield return new WaitForSeconds(0.05f);
                transform.position = originalPos;

                // Wait before next damage instance
                if (i < damageCount - 1)
                {
                    yield return new WaitForSeconds(damageInterval);
                }
            }

            // After all damage instances, kill the player
            Die();
        }

        private void SetAnimation(Vector2 dir, bool isMoving)
        {
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