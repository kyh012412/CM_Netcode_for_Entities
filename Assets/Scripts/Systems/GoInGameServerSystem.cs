using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem {
    // [BurstCompile]
    // public void OnCreate(ref SystemState state) {

    // }

    // [BurstCompile]
    // 빠른 iteration을 위해서 게임 제작중에는 BurstCompile을 비활성화 해주는것이 좋다.
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess()) {

            // receiveRpcCommandRequest.ValueRO.SourceConnection 자체가 Entity이다.
            // 해당 rpc를 보낸 NetworkId를 포함하고 있다.
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            Debug.Log("Client Connected to Server");
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }

    // [BurstCompile]
    // public void OnDestroy(ref SystemState state) {

    // }
}
