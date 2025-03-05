using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct GoInGameClientSystem : ISystem {
    // [BurstCompile]
    // public void OnCreate(ref SystemState state) {

    // }

    // [BurstCompile]
    // 빠른 컴파일을 위해서 BurstCompile을 비활성화 최종단계에서 활성화하면됨
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((
            RefRO<NetworkId> NetworkId,
            Entity entity)
            in SystemAPI.Query<
                RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess()) { // 네트워크 아이디가 있지만 네트워크스트림인게임이 없는 엔티티조회

            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

            Entity rpcEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(rpcEntity, new GoInGameRequestRpc());
            entityCommandBuffer.AddComponent(rpcEntity, new SendRpcCommandRequest());
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }

    // [BurstCompile]
    // public void OnDestroy(ref SystemState state) {

    // }
}

public struct GoInGameRequestRpc : IRpcCommand {

}
