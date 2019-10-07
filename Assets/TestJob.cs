using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

public class TestJob : JobComponentSystem
{
    [RequireComponentTag(typeof(Grounded))]
    struct MoveJob : IJobForEach<PhysicsVelocity, Translation>
    {
        public float horizontal;
        public float vertical;
        public bool space;
        [ReadOnly]
        public PhysicsWorld physicsWorld;
        public CollisionFilter filter;

        bool hit;

        [BurstCompile]
        public void Execute(ref PhysicsVelocity velocity, ref Translation translation)
        {
            var rayInput = new RaycastInput();

            rayInput.Start = translation.Value;
            rayInput.End = translation.Value - 1.4f * new float3(0,1,0);
            rayInput.Filter = filter;
            hit = physicsWorld.CollisionWorld.CastRay(rayInput);

            velocity.Linear = new float3(3 * horizontal, velocity.Linear.y, 3 * vertical);

            if(space && hit)
            {
                velocity.Linear = new float3(velocity.Linear.x, 6, velocity.Linear.z);
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var physicsWorld = World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var job = new MoveJob{
            horizontal = Input.GetAxis("Horizontal"),
            vertical = Input.GetAxis("Vertical"),
            space = Input.GetKeyDown(KeyCode.Space),
            physicsWorld = physicsWorld.PhysicsWorld,
            filter = new CollisionFilter {
                BelongsTo = (uint)(1 << 1),
                CollidesWith = (uint)(1 << 0),
            },
        };

        var newDeps = JobHandle.CombineDependencies(inputDeps, physicsWorld.FinalJobHandle);
        var handle = job.Schedule(this, newDeps);
        handle.Complete();

        return handle;
    }
}
