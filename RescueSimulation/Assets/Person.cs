using UnityEngine;

public class Person : MonoBehaviour
{
    Rigidbody rBody;
    private Transform agentTransform;

    private void Start()
    {
        // 找到Agent物體
        GameObject agent = GameObject.FindGameObjectWithTag("Agent");
        agentTransform = agent.transform;
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true; // 禁用刚体的旋转
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 檢查碰撞事件是否發生在Agent物體上
        if (collision.gameObject.CompareTag("Agent"))
        {
            // 找到Agent物體
            GameObject agent = GameObject.FindGameObjectWithTag("Agent");
            agentTransform = agent.transform;
            // 將Person物體的位置設定為Agent物體的正上方
            transform.position = agentTransform.position - Vector3.down * transform.localScale.y;
            // 將Person物體設定為Agent物體的子物體
            transform.parent = agentTransform;
        }
    }
}
