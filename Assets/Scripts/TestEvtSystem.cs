using UnityEngine;

public class TestEvtSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventSys.Instance.AddEvt(EventSys.test, CB);
        Debug.Log($"���� {this.name}����� {EventSys.test}�¼���");
    }

    void CB(object obj)
    {
        Debug.Log($"���� TestEvtSystem�����˴����� {EventSys.test}�¼���");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Input Key Down [D]");
            EventSys.Instance.RemoveEvt(EventSys.test, CB);
            Debug.Log($"���� {this.name}��ɾ�� {EventSys.test}�¼���");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Input Key Down [F]");
            EventSys.Instance.AddEvt(EventSys.test, CB);
            Debug.Log($"���� {this.name}����� {EventSys.test}�¼���");
        }
    }
}
