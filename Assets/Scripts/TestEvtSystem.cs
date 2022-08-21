using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEvtSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventSys.Instance.AddEvt(EventSys.test, CB);
        Debug.Log($"���� {this.name}����� abc�¼���");
    }

    void CB(object obj)
    {
        Debug.Log("���� TestEvtSystem�����˴����� abc�¼���");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Input Key Down [D]");
            EventSys.Instance.RemoveEvt(EventSys.test, CB);
            Debug.Log($"���� {this.name}��ɾ�� abc�¼���");
        }else if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Input Key Down [F]");
            EventSys.Instance.AddEvt(EventSys.test, CB);
            Debug.Log($"���� {this.name}����� abc�¼���");
        }
    }
}
