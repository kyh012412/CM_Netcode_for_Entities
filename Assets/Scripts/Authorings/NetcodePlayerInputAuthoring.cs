using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NetcodePlayerInputAuthoring : MonoBehaviour {
    public class Baker : Baker<NetcodePlayerInputAuthoring> {
        public override void Bake(NetcodePlayerInputAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NetcodePlayerInput());
        }
    }
}

public struct NetcodePlayerInput : IInputComponentData {
    public float2 inputVector;
    public InputEvent shoot; // 기본적으로는 bool이지만 tick이 있음..
}