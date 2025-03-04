using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct TestNetcodeEntitiesClientSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {

    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        if (Input.GetKeyDown(KeyCode.T)) {
            // Send Rpc
            Entity rpcEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(rpcEntity, new SimpleRpc {
                value = 45,
            });
            state.EntityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest());
            Debug.Log("Sending Rpc");
        }
    }
}
