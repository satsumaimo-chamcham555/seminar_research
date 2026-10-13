using UnityEngine;

public class RoomTriggerHelper : MonoBehaviour
{
    // Inspectorで "SpawnNext" または "CloseDoor" を指定
    public string triggerType;

    private FactoryRoom parentRoom;

    void Start()
    {
        parentRoom = GetComponentInParent<FactoryRoom>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parentRoom != null)
        {
            parentRoom.OnChildTriggerEnter(triggerType, other);
        }
    }
}
