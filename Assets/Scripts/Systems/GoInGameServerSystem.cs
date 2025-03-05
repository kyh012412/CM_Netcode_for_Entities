using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem {
    // [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EntitiesReference>(); // update 호출을 위한 필수 의존성 설정
        state.RequireForUpdate<NetworkId>();
    }

    // [BurstCompile]
    // 빠른 iteration을 위해서 게임 제작중에는 BurstCompile을 비활성화 해주는것이 좋다.
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        EntitiesReference entitiesReference = SystemAPI.GetSingleton<EntitiesReference>();

        foreach ((
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess()) {

            // receiveRpcCommandRequest.ValueRO.SourceConnection 자체가 Entity이다.
            // 해당 rpc를 보낸 NetworkId를 포함하고 있다.
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            UnityEngine.Debug.Log("Client Connected to Server");

            Entity playerEntity = entityCommandBuffer.Instantiate(entitiesReference.PlayerPrefabEntity);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(
                UnityEngine.Random.Range(-10, +10), 0, 0
            )));

            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection);

            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner {
                NetworkId = networkId.Value,
            });
            entityCommandBuffer.AppendToBuffer(receiveRpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup { // LinkedEntityGroup은 컴포넌트가 아닌 동적 버퍼이기때문에 Append to Buffer를 사용한다.
                Value = playerEntity,
            });

            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }

    // [BurstCompile]
    // public void OnDestroy(ref SystemState state) {

    // }
}
