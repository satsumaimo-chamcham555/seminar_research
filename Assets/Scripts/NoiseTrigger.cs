using UnityEngine;

public class NoiseTrigger : MonoBehaviour
{
    public NoisePuzzleManager noiseManager;

    private void Start()
    {
        if (noiseManager == null)
        {
            noiseManager = FindObjectOfType<NoisePuzzleManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (noiseManager != null)
            {
                noiseManager.StartNoiseSequence();
            }
        }
    }
}
