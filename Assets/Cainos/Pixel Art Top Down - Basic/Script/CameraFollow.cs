using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float catchUpSpeed = 30f;
        [SerializeField] private float maxFollowDistance = 3f;

        private Vector3 targetPosition;
        private bool isCatchingUp = false;

        private void LateUpdate()
        {
            if (target == null) return;

            // The camera should be at the player's position (with fixed Z)
            targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            // Calculate 2D distance
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(targetPosition.x, targetPosition.y);
            float distance = Vector2.Distance(currentPos, targetPos);

            // Check if we need to catch up
            if (distance > maxFollowDistance)
            {
                isCatchingUp = true;
            }

            if (isCatchingUp)
            {
                // Fast movement to catch up
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    catchUpSpeed * Time.deltaTime
                );

                // Check if we've caught up
                float newDistance = Vector2.Distance(
                    new Vector2(transform.position.x, transform.position.y),
                    new Vector2(targetPosition.x, targetPosition.y)
                );

                if (newDistance <= maxFollowDistance * 0.8f) // Slightly smaller to prevent jitter
                {
                    isCatchingUp = false;
                }
            }
            else
            {
                // Smooth follow
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    smoothSpeed * Time.deltaTime
                );
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;

            Gizmos.color = isCatchingUp ? Color.red : Color.green;
            Gizmos.DrawWireSphere(target.position, maxFollowDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
#endif
    }
}