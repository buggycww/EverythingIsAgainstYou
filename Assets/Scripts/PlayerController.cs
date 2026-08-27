using System.Collections;
using System.Collections.Generic;
using Unity.ProjectAuditor.Editor.Core;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class PlayerController : MonoBehaviour
    {
        public float speed;

        private Animator animator;
        private Rigidbody2D rigidbody;
        private Vector2 lastDirection = Vector2.down;

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

        private bool isDead = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody2D>();
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
            GameManager.instance.PlayerDead();
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