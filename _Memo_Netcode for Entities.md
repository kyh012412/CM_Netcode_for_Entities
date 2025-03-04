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
   1. MonobeHaviour를 지워주고 IRpcCommand로 바꿔준다.(Unity.Netcode 에 있는)
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
   				RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess()) {            Debug.Log("Received Rpc: " + simpleRpc.ValueRO.value + " :: " + ReceiveRpcCommandRequest.ValueRO.SourceConnection);
   			entityCommandBuffer.DestroyEntity(entity);
   		}
   		entityCommandBuffer.Playback(state.EntityManager);
   	}
   }
   ```

5. Entities Hierarchy에서 필터에 Entity Index로 특정 엔티티를 빠르게 검색할 수 있다.
