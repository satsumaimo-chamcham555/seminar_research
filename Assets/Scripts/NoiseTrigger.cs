using UnityEngine;

public class NoiseTrigger : MonoBehaviour
{
    public NoisePuzzleManager noiseManager;

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
