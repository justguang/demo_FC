using UnityEngine;

///测试事件系统
public class TestEvtSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventSys.Instance.AddEvt(EventSys.test, CB);
        Debug.Log($"我是 {this.name}，添加 {EventSys.test}事件。");
    }

    void CB(object obj)
    {
        Debug.Log($"我是 TestEvtSystem，有人触发了 {EventSys.test}事件。");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Input Key Down [D]");
            EventSys.Instance.RemoveEvt(EventSys.test, CB);
            Debug.Log($"我是 {this.name}，删除 {EventSys.test}事件。");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Input Key Down [F]");
            EventSys.Instance.AddEvt(EventSys.test, CB);
            Debug.Log($"我是 {this.name}，添加 {EventSys.test}事件。");
        }
    }
}
