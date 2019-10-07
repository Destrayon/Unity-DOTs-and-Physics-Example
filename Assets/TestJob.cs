using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Burst;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
public class TestJob : JobComponentSystem
{
    [RequireComponentTag(typeof(Grounded))]
    [BurstCompile]
    struct MoveJob : IJobForEach<PhysicsVelocity, Translation>
    {
        public float horizontal;
        public float vertical;
        public bool space;
       
        bool hit;
 
        [ReadOnly]
        public PhysicsWorld physicsWorld;
        public CollisionFilter filter;
        public void Execute(ref PhysicsVelocity velocity, ref Translation translation)
        {
            var rayInput = new RaycastInput();
            rayInput.Filter = filter;
 
            for(int i = -1; i < 2; i+=2)
            {
                rayInput.Start = new float3((float)i / 2,0,0) + translation.Value;
                rayInput.End = new float3((float)i / 2,0,0) + translation.Value + 1.1f * new float3(0,-1,0);
                hit =  physicsWorld.CollisionWorld.CastRay(rayInput);
                if (hit)
                {
                    break;
                }
                rayInput.Start = new float3(0,0,(float)i/2) + translation.Value;
                rayInput.End = new float3(0,0,(float)i/2) + translation.Value + 1.1f * new float3(0,-1,0);
                hit =  physicsWorld.CollisionWorld.CastRay(rayInput);
                if (hit)
                {
                    break;
                }
            }
           
            if (space && hit)
            {
                velocity.Linear = new float3(velocity.Linear.x, 6, velocity.Linear.z);
            }
            velocity.Linear = new float3(3 * horizontal, velocity.Linear.y, 3 * vertical);
            velocity.Angular = new float3(0,0,0);
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var physicsWorld = World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld;
        var job = new MoveJob{
            horizontal = Input.GetAxis("Horizontal"),
            vertical = Input.GetAxis("Vertical"),
            space = Input.GetKeyDown(KeyCode.Space),
            physicsWorld = physicsWorld,
            filter = new CollisionFilter {
                BelongsTo = ~0u,
                CollidesWith = (uint)(1 << 0),
            },
        };
        var newDeps = JobHandle.CombineDependencies(inputDeps, World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().FinalJobHandle);
        var handle = job.Schedule(this, newDeps);
        handle.Complete();
        return handle;
    }
}