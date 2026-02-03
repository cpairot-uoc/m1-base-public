using UnityEngine;

namespace Complete.UI
{
    /// <summary>
    /// Ensures that world space UI elements (like health bars) maintain a fixed rotation relative to the camera.
    /// </summary>
    public class UIDirectionControl : MonoBehaviour
    {
        [SerializeField] private bool m_UseRelativeRotation = true;       // Should the object maintain a fixed rotation?


        private Quaternion m_RelativeRotation;          // The local rotation at the start of the scene.


        private void Start ()
        {
            m_RelativeRotation = transform.parent.localRotation;
        }


        private void Update ()
        {
            if (m_UseRelativeRotation)
                transform.rotation = m_RelativeRotation;
        }
    }
}