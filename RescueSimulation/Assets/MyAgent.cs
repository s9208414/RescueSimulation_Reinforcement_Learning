using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class MyAgent : Agent
{
    Rigidbody rBody;
    Rigidbody crBody;
    Transform agentTransform;
    Transform houseTransform;
    public GameObject person;
    public GameObject house;
    public GameObject fire;
    public GameObject door1;
    public GameObject door2;
    public GameObject door3;
    public GameObject door4;
    public GameObject door5;
    public GameObject door6;
    public GameObject safe_area;
    public GameObject wall;
    public float spawnRadius = 1f;
    public GameObject[] persons;
    public GameObject[] fires;
    public int saved;
    public float reward;
    private bool[] fireCollisionCooldown; // 火源碰撞冷却计时器
    private float fireCollisionCooldownDuration = 2.0f; // 火源碰撞冷却持续时间
    public GameObject[] copiedPersons;
    private bool isNearHouse = false; // 用于记录Agent是否靠近房子的门口
    private float distanceToHouse; // 记录Agent与房子门口的距离
    private float thresholdDistance = 2.0f; // 设定一个阈值，表示Agent与房子门口的距离达到该值时视为靠近门口

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true; // 禁用刚体的旋转
        agentTransform = GetComponent<Transform>();
        GameObject house = GameObject.Find("House");
        GameObject safe_area = GameObject.Find("SafeArea");
        GameObject door1 = GameObject.Find("Door1");
        GameObject door2 = GameObject.Find("Door2");
        GameObject door3 = GameObject.Find("Door3");
        GameObject door4 = GameObject.Find("Door4");
        GameObject door5 = GameObject.Find("Door5");
        GameObject door6 = GameObject.Find("Door6");
        GameObject wall = GameObject.FindWithTag("Wall");
        houseTransform = house.transform;
        saved = 0;
        reward = 0;
        persons = new GameObject[] { GameObject.Find("Person1"), GameObject.Find("Person2"), GameObject.Find("Person3") };
        fires = new GameObject[] { GameObject.Find("Fire1"), GameObject.Find("Fire2"), GameObject.Find("Fire3"), GameObject.Find("Fire4") };
        fireCollisionCooldown = new bool[fires.Length];
        for (int i = 0; i < fires.Length; i++)
        {
            fireCollisionCooldown[i] = false;
        }
    }


    public override void OnEpisodeBegin()
    {
        saved = 0;
        reward = 0;
        for (int i = 0; i < copiedPersons.Length; i++)
        {
            if (copiedPersons[i] != null) {
                Destroy(copiedPersons[i]);
                copiedPersons[i] = null;
            }
        }
        copiedPersons = new GameObject[persons.Length];
        for (int i = 0; i < persons.Length; i++)
        {
            // 复制人物对象
            copiedPersons[i] = Instantiate(persons[i], persons[i].transform.position, persons[i].transform.rotation);
            copiedPersons[i].name = "Person" + (i + 4);


            // 将复制的人物对象设置为 "TrainingArea" 的子对象
            copiedPersons[i].transform.parent = GameObject.Find("TrainingArea").transform;

            // 禁用Mesh Renderer组件
            MeshRenderer meshRenderer = persons[i].GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            // 禁用Capsule Collider组件
            CapsuleCollider capsuleCollider = persons[i].GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
            }

            // 禁用Rigidbody组件
            Rigidbody rigidbody = persons[i].GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
            }
            crBody = copiedPersons[i].GetComponent<Rigidbody>();
            crBody.freezeRotation = true; // 禁用刚体的旋转
        }

        // 在场景中显示复制的人物对象
        for (int i = 0; i < copiedPersons.Length; i++)
        {
            copiedPersons[i].SetActive(true);
        }
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(14.75361f, 5.0f, 8.407773f);
        // 重置火源碰撞冷却计时器
        for (int i = 0; i < fires.Length; i++)
        {
            fireCollisionCooldown[i] = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int i = 0; i < copiedPersons.Length; i++)
        {
            // 检查人物对象是否存在
            if (copiedPersons[i] != null)
            {
                // Target and Agent positions
                sensor.AddObservation(copiedPersons[i].transform.localPosition);
            }
            else
            {
                // 如果人物对象已被销毁，添加一个空的观测
                sensor.AddObservation(Vector3.zero);
            }

            // Agent position
            //sensor.AddObservation(this.transform.localPosition);
        }
        for (int i = 0; i < fires.Length; i++)
        {

            // Target and Agent positions
            sensor.AddObservation(fires[i].transform.localPosition);
            

            // Agent position
            //sensor.AddObservation(this.transform.localPosition);
        }
        // Agent position
        sensor.AddObservation(this.transform.localPosition);
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        sensor.AddObservation(house.transform.localPosition);
        sensor.AddObservation(safe_area.transform.localPosition);
        sensor.AddObservation(door1.transform.localPosition);
        sensor.AddObservation(door2.transform.localPosition);
        sensor.AddObservation(door3.transform.localPosition);
        sensor.AddObservation(door4.transform.localPosition);
        sensor.AddObservation(door5.transform.localPosition);
        sensor.AddObservation(door6.transform.localPosition);
        // 计算Agent与房子门口的距离
        /*distanceToHouse = Vector3.Distance(transform.localPosition, door.transform.localPosition);

        // 判断Agent是否靠近房子的门口
        if (distanceToHouse <= thresholdDistance)
        {
            isNearHouse = true;
        }
        else
        {
            isNearHouse = false;
        }*/

        // 将isNearHouse添加到观测中
        //sensor.AddObservation(isNearHouse ? 1 : 0);

    }

    public float moveSpeed = 2;
    public float rotateSpeed = 20;
    public float forceMultiplier = 2;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 获取按下的方向键值
        float horizontalInput = actionBuffers.ContinuousActions[0];
        float verticalInput = actionBuffers.ContinuousActions[1];

        // 移动方向
        Vector3 moveDirection = Vector3.zero;

        // 根据方向键设置移动方向
        if (verticalInput > 0)
        {
            // 使用固定一个面前进
            moveDirection = transform.forward;
        }
        else if (verticalInput < 0)
        {
            // 使用相对位置的面前进
            moveDirection = -transform.forward;
        }

        // 移动Agent
        rBody.velocity = moveDirection * moveSpeed;

        // 根据方向键设置旋转方向
        if (horizontalInput > 0)
        {
            // 往右旋转
            transform.Rotate(Vector3.up, rotateSpeed);
        }
        else if (horizontalInput < 0)
        {
            // 往左旋转
            transform.Rotate(Vector3.up, -rotateSpeed);
        }


        // Rewards
        for (int i = 0; i < copiedPersons.Length; i++)
        {
            if (copiedPersons[i] != null && IsCollidingWithBObject(copiedPersons[i]))
            {
                // 销毁人物对象
                Destroy(copiedPersons[i]);
                copiedPersons[i] = null;
                reward += 10.0f;
                saved += 1;
                
            }
            /*
            // 檢查災民是否碰到安全區
            Collider personCollider = persons[i].GetComponent<Collider>();
            Collider safeAreaCollider = safe_area.GetComponent<Collider>();
            if (personCollider != null && safeAreaCollider != null && personCollider.bounds.Intersects(safeAreaCollider.bounds))
            {
                reward += 2.0f;
                // Destroy(persons[i]);
            }*/

        }
        if (IsCollidingWithBObject(safe_area))
        {
            if (copiedPersons.Length == 0) {
                reward += 10.0f;
            }
            SetReward(reward);
            for (int i = 0; i < persons.Length; i++)
            {
                // 启用Mesh Renderer组件
                MeshRenderer meshRenderer = persons[i].GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true;
                }

                // 启用Capsule Collider组件
                CapsuleCollider capsuleCollider = persons[i].GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                {
                    capsuleCollider.enabled = true;
                }

                // 启用Rigidbody组件
                Rigidbody rigidbody = persons[i].GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = false;
                }

            }
            EndEpisode();
        }

        for (int i = 0; i < fires.Length; i++)
        {
            if (IsCollidingWithBObject(fires[i]) && !fireCollisionCooldown[i])
            {
                // 设置碰撞冷却
                fireCollisionCooldown[i] = true;
                StartCoroutine(ResetFireCollisionCooldown(i));

                reward -= 0.0001f;
            }
        }
        /*
        if (persons.Length == 0)
        {
            EndEpisode();
        }*/
        // 鼓励Agent靠近房子的门口并进入
        /*if (isNearHouse)
        {
            // 给予正向奖励
            reward += 0.01f;
        }
        else
        {
            // 给予负向奖励
            reward -= 0.01f;
        }*/

        // 在与墙壁发生碰撞时给予负向奖励
        /*if (IsCollidingWithBObject(wall))
        {
            reward -= 0.01f;
        }*/
        SetReward(reward);
        //EndEpisode();
    }
    bool IsCollidingWithBObject(GameObject bObject)
    {
        // 获取A游戏物体和B游戏物体的碰撞器组件
        Collider aCollider = GetComponent<Collider>();
        Collider bCollider = bObject.GetComponent<Collider>();

        // 检测碰撞
        if (aCollider != null && bCollider != null)
        {
            return aCollider.bounds.Intersects(bCollider.bounds);
        }

        return false;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    private IEnumerator ResetFireCollisionCooldown(int index)
    {
        yield return new WaitForSeconds(fireCollisionCooldownDuration);
        fireCollisionCooldown[index] = false;
    }
}
