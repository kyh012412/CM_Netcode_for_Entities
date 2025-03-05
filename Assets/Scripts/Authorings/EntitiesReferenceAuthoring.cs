using Unity.Entities;
using UnityEngine;

public class EntitiesReferenceAuthoring : MonoBehaviour {

    public GameObject playerPrefabGameObject;
    public class Baker : Baker<EntitiesReferenceAuthoring> {
        public override void Bake(EntitiesReferenceAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReference {
                PlayerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct EntitiesReference : IComponentData {
    public Entity PlayerPrefabEntity;
}