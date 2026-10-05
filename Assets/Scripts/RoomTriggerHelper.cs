using UnityEngine;

public class RoomTriggerHelper : MonoBehaviour
{
    // Inspectorで "SpawnNext" または "CloseDoor" を指定
    public string triggerType; 
    
    private FactoryRoom parentRoom;

    void Start()
    {
        // 親にある FactoryRoom を取得
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
