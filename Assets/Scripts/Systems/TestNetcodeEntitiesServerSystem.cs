using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct TestNetcodeEntitiesServerSystem : ISystem {
    // [BurstCompile]
    // public void OnCreate(ref SystemState state) {

    // }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((
            RefRO<SimpleRpc> simpleRpc,
            RefRO<ReceiveRpcCommandRequest> ReceiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<SimpleRpc>,
                RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess()) {
            Debug.Log("Received Rpc: " + simpleRpc.ValueRO.value + " :: " + ReceiveRpcCommandRequest.ValueRO.SourceConnection);
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }

    // [BurstCompile]
    // public void OnDestroy(ref SystemState state) {

    // }
}
