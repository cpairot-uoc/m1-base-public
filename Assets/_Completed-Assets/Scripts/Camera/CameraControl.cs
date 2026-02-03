using UnityEngine;

namespace Complete.Cameras
{
    /// <summary>
    /// Controls camera movement and zoom to keep all targets in view.
    /// Supports predictive positioning based on target velocity.
    /// </summary>
    public class CameraControl : MonoBehaviour
    {
        [SerializeField] private float m_DampTime = 0.2f;                 // Approximate time for the camera to refocus.
        [SerializeField] private float m_ScreenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge.
        [SerializeField] private float m_MinSize = 6.5f;                  // The smallest orthographic size the camera can be.
        [SerializeField] private float m_MaxSpeedBuffer = 2f;             // Maximum additional buffer based on speed.
        [SerializeField] private float m_SpeedScale = 0.5f;               // How much the speed affects the buffer.
        [HideInInspector] public Transform[] m_Targets;                   // All the targets the camera needs to encompass.


        private Camera m_Camera;                        // Used for referencing the camera.
        private float m_ZoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
        private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position.
        private Vector3 m_DesiredPosition;              // The position the camera is moving towards.
        private Rigidbody[] m_TargetRigidbodies;        // Cached Rigidbody components for all targets.


        private void Awake ()
        {
            m_Camera = GetComponentInChildren<Camera> ();
        }


        private void FixedUpdate ()
        {
            // Move the camera towards a desired position.
            Move ();

            // Change the size of the camera based.
            Zoom ();
        }


        /// <summary>
        /// Moves the camera to the average position of the targets with smoothing.
        /// </summary>
        private void Move ()
        {
            // Find the average position of the targets.
            FindAveragePosition ();

            // Smoothly transition to that position.
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }


        /// <summary>
        /// Calculates the average position of all active targets and applies a predictive velocity offset.
        /// </summary>
        private void FindAveragePosition ()
        {
            Vector3 averagePos = new Vector3 ();
            int numTargets = 0;
            Vector3 velocityOffset = Vector3.zero;

            // Go through all the targets and add their positions together.
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // If the target isn't active, go on to the next one.
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                // Add to the average and increment the number of targets in the average.
                averagePos += m_Targets[i].position;
                numTargets++;

                // Add predicted offset based on velocity
                Rigidbody targetRigidbody = m_TargetRigidbodies[i];
                if (targetRigidbody != null)
                {
                    velocityOffset += targetRigidbody.linearVelocity;
                }
            }

            // If there are targets divide the sum of the positions by the number of them to find the average.
            if (numTargets > 0)
            {
                averagePos /= numTargets;
                velocityOffset /= numTargets;
            }

            // Apply predicted offset to desired position
            averagePos += velocityOffset * m_DampTime;

            // Keep the same y value.
            averagePos.y = transform.position.y;

            // The desired position is the average position;
            m_DesiredPosition = averagePos;
        }


        /// <summary>
        /// Smoothly transitions the camera's orthographic size based on targets' positions and speeds.
        /// </summary>
        private void Zoom ()
        {
            // Find the required size based on the desired position and smoothly transition to that size.
            float requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp (m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }


        /// <summary>
        /// Determines the orthographic size required to encompass all active targets, including speed-based buffering.
        /// </summary>
        private float FindRequiredSize ()
        {
            // Find the position the camera rig is moving towards in its local space.
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // Start the camera's size calculation at zero.
            float size = 0f;
            float maxSpeed = 0f;

            // Go through all the targets...
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // ... and if they aren't active continue on to the next target.
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                // Otherwise, find the position of the target in the camera's local space.
                Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

                // Find the position of the target from the desired position of the camera's local space.
                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

                // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                // Choose the largest out of the current size and the calculated size based on the tank being to the left or right of the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);

                // Try to get the Rigidbody velocity if it exists
                Rigidbody targetRigidbody = m_TargetRigidbodies[i];
                if (targetRigidbody != null)
                {
                    maxSpeed = Mathf.Max(maxSpeed, targetRigidbody.linearVelocity.magnitude);
                }
            }

            // Add the edge buffer and a dynamic buffer based on speed to the size.
            float dynamicBuffer = m_ScreenEdgeBuffer + Mathf.Min(maxSpeed * m_SpeedScale, m_MaxSpeedBuffer);
            size += dynamicBuffer;

            // Make sure the camera's size isn't below the minimum.
            size = Mathf.Max (size, m_MinSize);

            return size;
        }


        /// <summary>
        /// Immediately sets the camera's position and size to match the targets.
        /// Also caches the Rigidbody components of the targets.
        /// </summary>
        public void SetStartPositionAndSize ()
        {
            // Cache the Rigidbody components.
            m_TargetRigidbodies = new Rigidbody[m_Targets.Length];
            for (int i = 0; i < m_Targets.Length; i++)
            {
                m_TargetRigidbodies[i] = m_Targets[i].GetComponent<Rigidbody>();
            }

            // Find the desired position.
            FindAveragePosition ();

            // Set the camera's position to the desired position without damping.
            transform.position = m_DesiredPosition;

            // Find and set the required size of the camera.
            m_Camera.orthographicSize = FindRequiredSize ();
        }
    }
}