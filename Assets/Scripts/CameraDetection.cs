using UnityEngine;

public class CameraDetection : MonoBehaviour
{
    [SerializeField] private GameObject camHat;
    [SerializeField] private float minRotateAngle = 0;
    [SerializeField] private float maxRotateAngle = 180;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float rotatResetSpeed;
    [SerializeField] private float detectionProgress;
    [SerializeField] private float fieldOfView = 60f;
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
        if (!detected)
        {
            if(rotatResetSpeed < rotateSpeed)
                rotatResetSpeed += Time.deltaTime * 2f;
            else rotatResetSpeed = rotateSpeed;
            // Rotate the camera within the specified angle limits and player enters the detection area the; cameras rotation will follow the player using the skalar product and update the detection progress and detected variables
            float rotationAngle = Mathf.PingPong(Time.time * rotateSpeed, maxRotateAngle - minRotateAngle) + minRotateAngle;
            Quaternion euler = Quaternion.Euler(tiltAngle, rotationAngle, 0);
            camHat.transform.rotation =Quaternion.Slerp(camHat.transform.rotation, euler, Time.deltaTime * rotatResetSpeed);
        }

        if (detectedPlayer != null)
        {
            if (IsPlayerInView())
            {
                detected = true;
                // rotate the camera towards the player
                Vector3 directionToPlayer = (detectedPlayer.transform.position - camHat.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                Vector3 euler = targetRotation.eulerAngles;
                euler.y = ClampAngle(euler.y, minRotateAngle, maxRotateAngle);
                targetRotation = Quaternion.Euler(euler);
                //camHat.transform.rotation = Quaternion.Slerp(camHat.transform.rotation, targetRotation, detectionProgress );
                camHat.transform.rotation = Quaternion.RotateTowards(camHat.transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            }
            else
            {
                // If the player is not detected, reset detection progress
                detectionProgress = 0f;
                detected = false;
                rotatResetSpeed = 1f;
            }

        }

        if(Physics.Raycast(camHat.transform.position, camHat.transform.forward, out RaycastHit hit, detectionArea.radius))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player Spotted!");
                detectionProgress += Time.deltaTime;
                Debug.Log("Player Spotted!");
                if (detectionProgress >= 3f)
                {
                    Debug.Log("Player Captured!");
                    // Here you can add code to handle what happens when the player is fully detected (e.g., trigger an alarm, end the game, etc.)
                }

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

    bool IsPlayerInView()
    {
        Vector3 dirToPlayer = (detectedPlayer.transform.position - camHat.transform.position).normalized;
        Vector3 camForward = camHat.transform.forward;

        float dot = Vector3.Dot(camForward, dirToPlayer);

        float threshold = Mathf.Cos(fieldOfView * 0.5f * Mathf.Deg2Rad);

        return dot >= threshold;
    }

    float ClampAngle(float angle, float min, float max)
    {
        angle = Mathf.Repeat(angle + 360f, 360f); // Normalisieren auf 0–360
        min = Mathf.Repeat(min + 360f, 360f);
        max = Mathf.Repeat(max + 360f, 360f);

        // Normalfall: min < max
        if (min < max)
            return Mathf.Clamp(angle, min, max);

        // Bereich geht über 0° (z.B. 350°–20°)
        if (angle > min || angle < max)
            return angle;

        // Wenn außerhalb näheren Rand wählen
        float distToMin = Mathf.Abs(Mathf.DeltaAngle(angle, min));
        float distToMax = Mathf.Abs(Mathf.DeltaAngle(angle, max));
        return distToMin < distToMax ? min : max;
    }


}
