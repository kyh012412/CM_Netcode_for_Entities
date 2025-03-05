[Getting Started with Netcode for Entities! (DOTS Multiplayer Tutorial Unity 6)](https://www.youtube.com/watch?v=HpUmpw_N8BA&t=1s)

1. 패키지 매니저에서 패키지 설치
   1. Netcode for Entities
   2. Entities Graphics
      1. 유니티 에디터 > 프로젝트 세팅 > 에디터 >
      2. Enter Play Mode Settings > Do not reload Domain or Scene로 변경
2. 설치 후 Play를 눌러서 빈장면이 에러 오류없이 작동되는지 확인
3. 에디터 > 윈도우 > Entities > Hierarchy
   1. 여기에서 Play를 하면 ServerWorld와 Client World를 각각 볼 수 있다.
4. 서브씬에 오브젝트를 둬도 자동으로 연결되지 않는다 수동으로 연결해야 NetworkId를 가질수있다.

### RPCs

1. Scripts/GameBootstrap 파일을 만들어준다.
   1. MonoBehaviour대신에 ClientServerBootStrap으로 바꿔준다.
2. ```cs
   public class GameBootstrap : ClientServerBootstrap {
   	public override bool Initialize(string defaultWorldName) {
   		AutoConnectPort = 7979;
   		return base.Initialize(defaultWorldName);
   	}
   }
   ```
3. 이 파일이 존재하기만하면 실제로 부트스트래핑후 (자동으로) 연결이 된다.
4. 여기까지 작성후 테스트실행해보면 에러가나오는데
   1. run in background가 불가능한 상태라서 그렇다.
   2. 프로젝트세팅 > Player > Resolution and Presentation에서 Run In Background를 체크해준다.
5. 다시 테스트 Entities Hierarchy에서 NetworkId를 가진 객체를 볼 수 있다.
6. 모든 ClientRpc는 서버에 Rpc를 보낼수있으며, 서버는 모든 또는 일부분의 클라이언트에게 Rpc를 보낼 수 있다.
7. SimpleRpc 를 만들어준다.
   1. MonoBeHaviour를 지워주고 IRpcCommand로 바꿔준다.(Unity.Netcode 에 있는)
   2. ```cs
      public struct SimpleRpc : IRpcCommand {
      	// class가 아닌 struct에 주의할 것
      	public int value;
      }
      ```

### Client and Server Systems

1. TestNetcodeEntitiesClientSystem과 TestNetcodeEntitiesServerSystem을 만들어준다.
   1. 서버에서만 실행되던가 클라이언트에서만 실행이되도록 속성을 사용해준다.
   2. `[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]` 또는 `[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]`를 사용해준다.
2. Window > Entities > Systems 사용
   1. ServerWorld에서
   2. Update > Simulation System Group 내부에 Test Netcode Entities Server system이 있는 것을 확인
   3. ClientWorld로 가면
   4. Test Netcode Entities Client system이 보인다.

### Sending and Receiving RPC

1. `TestNetcodeEntitiesClientSystem`에서 특정 조건이 되면
   1. 엔티티를 만들고 SimpleRpc를 붙여주는데
   2. `SendRpcCommandRequest`도 붙여준다.
   3. `SendRpcCommandRequest`는 rpc 시스템에서 rpc를 보내기 위해서 소모되며
   4. `SendRpcCommandRequest`에는 TargetConnection에 해당하는 옵션의 값도 부여 할 수 있다.
   5. 현재는 클라이언트 rpc 구성중이고 대상은 무조건 서버가 되고
   6. 서버 rpc 구성중이였다면 target이 없으면 모든 클라이언트에게 발송이 된다.
2. 로그도 추가해주고 `[BurstCompile]`도 주석 처리 해준다.
3. `TestNetcodeEntitiesServerSystem`에서는 rpc를 사용한후 파괴해야만 한다.
4. ```cs
   [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
   partial struct TestNetcodeEntitiesClientSystem : ISystem {
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
   ```

   ```cs
   [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
   partial struct TestNetcodeEntitiesServerSystem : ISystem {
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
   }
   ```

5. Entities Hierarchy에서 필터에 Entity Index로 특정 엔티티를 빠르게 검색할 수 있다.

### Client Build and PlayMode Tools

1. 유니티 에디터 > Edit > Project Settings > Multiplayer > Build
   1. 여기에서 빌드에 대해 정의할 수있다.
   2. 빌드한 것은 클라이언트만 되도록 설정하고
   3. 에디터에서 client and server로 할 예정이다.
   4. Netcode Client Target을 Client로 변경
2. 유니티 에디터 > Edit > Project Settings > Player > Resolution Fullscreen Mode > Windowed
3. 끝났다면 File > Build and Run을 눌러본다.
4. 에디터에서 서버를 먼저 작동시켜주고 빌드물을 실행시켜서 T를눌러 상호작용 테스트를 해본다.

### Setup Connection as InGame

1. Windows > Multiplayer > PlayMode Tools
   1. 여기서 많은 설정을 할 수 있다.
2. Edit > Project Settings > Multiplayer > Build 에서 값을
   1. client and server로 바꾸면 PlayMode Tools내에서 Server Emulation이라는 옵션이 생긴다.
   2. 기존대로 build은 client로 쓰고
   3. 에디터를 client and server로 사용
3. 에디터를 키고 빌드물을 켜보면
4. 엔티티 하이라키에서 > 서버월드에서 any=NetworkId를 검색해보면
   1. 2개의 NetworkConnection이 생긴것을 볼 수 있다.
5. 빌드를 통한 연결이 잘 되는 것을 확인했지만 빌드를 하는것은 시간이 걸리는 일이다.
6. 유니티에있는 멀티플레이 모드라는것을 사용
   1. 이것을 사용하여 빠른 테스트 가능
7. 패키지매니저로가서 Multiplayer Play Mode 설치
   1. 설치했다면 Windows > Multiplayer > Multiplayer Playe Mode가 있을것
   2. Player 2를 체크하고 조금 기다릴것
   3. Play를해서 Player2를 켜준다음에 우상단에 Layout에서 Playmode tools를 열어주고
   4. PlayMode type을 client로 변경해준뒤 에디터에서 다시 PlayMode에 진입
   5. 정상작동이되고 각자 T를 눌러서 상호작용이 되는지 확인한다.
8. 지금은 데이터가 동기화가 된것은 아니고 단순히 rpc를 보낼 뿐이다.
9. any=NetworkStreamConnection를 검색하여서 인스펙터 runtime을보면
   1. NetworkSnapshot Ack를 보면 모든게 정적인값이고 바뀌지않는다.
10. Client Connects Marks Connection to Server as InGame
11. Client sends InGameRequestRpc to Server
12. Server receives Rpc Marks Connection to Client as InGame
13. GoInGameClientSystem 을 만들어준다.

    1. 다음이 잘 되는 지 확인하고 Server단을 만들어준다.

    ```cs
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]

    partial struct GoInGameClientSystem : ISystem {
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
    }

    public struct GoInGameRequestRpc : IRpcCommand {

    }
    ```

```cs
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem {
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
}
```

1. any=NetworkStreamConnection를 검색해서
   1. NetworkConnection의 태그에 Network Stream In Game이 있는지 확인 있다면 ok
2. 빠른 iteration을 위해서 게임 제작중에는 BurstCompile을 비활성화 해주는것이 좋다.
3. Jobs > Burst > Enable Compilation을 비활성화까지 해준다.
   1. 최종 빌드전에 다시 활성화 해준다.
4. 다시 테스트를해보면 NetworkConnection을 inspector runtime으로보면
   1. Network Snapshot Ack가 계속 바뀌는것을 확인 할 수 있다.

### Netcode Ghosts, Player Object

1. 넷코드 내에서 고스트라는 컨셉을 알아야한다.
2. Netcode - Ghost
   1. Synchornizes snapshot data across Server and Clients
   2. Auto Syncs Entity Spawning, Destroying, and LocalTransform
   3. Custom data must be marked as \[GhostField]
3. 고스트 만들어보기
4. 메인 씬이하에 Capsule을 만들어 준다.(Player)
   1. scale 5, material 재량
5. PlayerAuthoring을 만들어준다.

   ```cs
   public class PlayerAuthoring : MonoBehaviour {
       public class Baker : Baker<PlayerAuthoring> {
           public override void Bake(PlayerAuthoring authoring) {
               Entity entity = GetEntity(TransformUsageFlags.Dynamic);
               AddComponent(entity, new Player());
           }
       }
   }

   public struct Player : IComponentData {

   }
   ```

6. Player에 Ghost Authoring Component를 붙여준다.
7. Player를 Prefab화 해준다.
   1. 씬에서는 제거
8. Ghost Authoring Component 내에서 Has Owner를 체크해준다.
   1. 각각의 오브젝트가 각각의 플레이어에게 귀속되기를 원하므로 체크
   2. 만약에 체크하지 않으면 서버에 귀속되게 되며
   3. 이 방식은 NPC를 만들때 유용하다.
   4. Default Ghost Mode는 Owner Predicted로 해준다.
9. 이제 테스트할때 Player를 동적으로 생성해주면된다.
10. 이제 엔티티로 만들어주기위하여 참조할수있어야한다.
11. EntitiesReferenceAuthoring를 만들어준다.
12. GoInGameServerSystem수정
    1. Player생성하는 코드 추가
13. GoInGameClientSystem 수정
    1. OnCreate단에 RequireForUpdate 추가
14. 에디터에서 EntitiesReference를 만들어주고 cs붙여주고 prefab을 연결해준다.
15. 테스트 - 정상
16. Spawning Object하고나서 disconnect시 cleanup도 같이 해야한다.
17. 각각의 NetworkConnection마다 Linked Entity Group이 있다.
    1. Spawning할때 해당 connection이하에 있는 Linked Entity Group에 Player를 추가하여서 종료시 같이 제거될수있게 해준다.
18. Player2 에서 tools를 열어주고 Client DC버튼이 (Client disconnect)이다.
19. 테스트 - 정상

### Player Movement, IInputComponentData

1. 플레이어 이동제어를 위해서 새로운 스크립트 생성
2. NetcodePlayerInputAuthoring

```cs
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
}
```

1. `IInputComponentData`사용 Unity.NetCode 이하에 있음
2. Player Prefab에 부착해준다.
3. NetcodePlayerInputSystem .cs 를만들어준다.

   ```cs
   [UpdateInGroup(typeof(GhostInputSystemGroup))]
   partial struct NetcodePlayerInputSystem : ISystem {
       // [BurstCompile]
       public void OnCreate(ref SystemState state) {
           state.RequireForUpdate<NetworkStreamInGame>();
           state.RequireForUpdate<NetcodePlayerInput>();
       }

       // [BurstCompile]
       public void OnUpdate(ref SystemState state) {
           foreach (RefRW<NetcodePlayerInput> netcodePlayerInput in SystemAPI.Query<RefRW<NetcodePlayerInput>>().WithAll<GhostOwnerIsLocal>()) { // GhostOwnerIsLocal 컴포넌트가 있는 엔티티만 필터링됩니다.
               float2 inputVector = new float2();
               if (Input.GetKey(KeyCode.W)) {
                   inputVector.y += 1f;
               }
               if (Input.GetKey(KeyCode.S)) {
                   inputVector.y -= 1f;
               }
               if (Input.GetKey(KeyCode.A)) {
                   inputVector.x -= 1f;
               }
               if (Input.GetKey(KeyCode.D)) {
                   inputVector.x += 1f;
               }
               netcodePlayerInput.ValueRW.inputVector = inputVector;
           }
       }
   }
   ```

4. 1. `[UpdateInGroup(typeof(GhostInputSystemGroup))]`사용까지
      1. 내부를 확인해보면 ClientSimulation인것을 알 수있다.
5. 제어를 잘못하게되면 한플레이어의 입력이 모든 플레이어를 이동제어해버릴수있다.
   1. Player내부에 Ghost Owner is Local이 True인 것만 제어하도록 해야한다.
6. NetcodePlayerMovementSystem 도 만들어준다.
7. `[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]`를 같이 사용한다.

   ```cs
   [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
   partial struct NetcodePlayerMovementSystem : ISystem {
       [BurstCompile]
       public void OnUpdate(ref SystemState state) {
           foreach ((RefRO<NetcodePlayerInput> netcodePlayerInput,
           RefRW<LocalTransform> localTransform)
           in SystemAPI.Query<
               RefRO<NetcodePlayerInput>,
               RefRW<LocalTransform>>().WithAll<Simulate>()) {

               float moveSpeed = 10f;
               float3 moveVector = new float3(netcodePlayerInput.ValueRO.inputVector.x, 0, netcodePlayerInput.ValueRO.inputVector.y);
               localTransform.ValueRW.Position += moveVector * moveSpeed * SystemAPI.Time.DeltaTime;
           }
       }
   }
   ```

8. `.WithAll<Simulate>()`를 같이 사용해야 한다.
