using UnityEngine;

public class Particle : MonoBehaviour
{
    private ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        // 检查被碰撞的游戏物体是否需要被击飞
        if (other.CompareTag("Agent"))
        {
            // 这里可以添加你想要的逻辑，比如将其位置重置或者应用力使其移动
            Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
            if (otherRigidbody != null)
            {
                // 重置位置示例
                other.transform.position = Vector3.zero;

                // 应用力示例
                otherRigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
            }
        }
    }
}
