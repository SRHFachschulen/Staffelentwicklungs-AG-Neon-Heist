using UnityEngine;

public class CameraDetection : MonoBehaviour
{
    [SerializeField] private GameObject camHat;
    [SerializeField] private float minRotateAngle;
    [SerializeField] private float maxRotateAngle;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float rotatResetSpeed;
    [SerializeField] private float detectionProgress;
    [SerializeField] private bool detected;
    [SerializeField] private float tiltAngle;
    [SerializeField] private GameObject detectedPlayer;


    private SphereCollider detectionArea;

    private void Start()
    {
        // Initialize the detection area as a SphereCollider
        detectionArea = gameObject.GetComponent<SphereCollider>();
    }


    // Update is called once per frame
    void Update()
    {
        if (detectionProgress <= 0.05)
        {
            if(rotatResetSpeed < rotateSpeed)
                rotatResetSpeed += Time.deltaTime * 2f;
            else rotatResetSpeed = rotateSpeed;
            // Rotate the camera within the specified angle limits and player enters the detection area the; cameras rotation will follow the player using the skalar product and update the detection progress and detected variables
            float rotationAngle = Mathf.PingPong(Time.time * rotateSpeed, maxRotateAngle - minRotateAngle) + minRotateAngle;
            Quaternion euler = Quaternion.Euler(0, rotationAngle, tiltAngle);
            camHat.transform.rotation =Quaternion.Slerp(camHat.transform.rotation, euler, Time.deltaTime * rotatResetSpeed);
        }

        if (detectedPlayer != null)
        {
            float dot = Vector3.Dot(transform.position, detectedPlayer.transform.position);
            Debug.Log("Dot product: " + dot);

            if (dot > minRotateAngle && dot < maxRotateAngle)
            {
                // If the player is detected, increment the detection progress
                detectionProgress += Time.deltaTime;
                detected = true;
                // rotate the camera towards the player
                Vector3 directionToPlayer = (detectedPlayer.transform.position - camHat.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                camHat.transform.rotation = Quaternion.Slerp(camHat.transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            }
            else
            {
                // If the player is not detected, reset detection progress
                detectionProgress = 0f;
                detected = false;
                rotatResetSpeed = 1f;
            }

        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // When the player enters the detection area, set playerIsInDetectionArea to true
            detectedPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // When the player exits the detection area, reset detectedPlayer and detectionProgress
            detectedPlayer = null;
            detectionProgress = 0f;
            detected = false;
            rotatResetSpeed = 1f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the detection area in the editor for visualization
        if (detectionArea != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detectionArea.transform.position, detectionArea.radius);
        }

    }

    // getter for detectionProgress and detected variables
    public float GetDetectionProgress()
    {
        return detectionProgress;
    }
    public bool IsDetected()
    {
        return detected;
    }


}
