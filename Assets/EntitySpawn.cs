using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Collider = Unity.Physics.Collider;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;

public class EntitySpawn : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    Entity entity;
    EntityManager entityManager;
    void Start()
    {
        entityManager = World.Active.EntityManager;

        Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);

        BlobAssetReference<Collider> sourceCollider = entityManager.GetComponentData<PhysicsCollider>(sourceEntity).Value;

        for (int i = 0; i < 1; i++)
        {
            entity = entityManager.Instantiate(sourceEntity);
            entityManager.SetComponentData(entity, new Translation { Value = new float3(0, 5, 0)});
            entityManager.SetComponentData(entity, new PhysicsCollider { Value = sourceCollider});

            var data = entityManager.GetComponentData<PhysicsMass>(entity);

            data.InverseInertia = new float3(0);
            entityManager.SetComponentData(entity, data);
            entityManager.AddComponent(entity, typeof(Grounded));
        }
    }
}
