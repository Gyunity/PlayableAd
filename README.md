# 🎮 Prison Life — Playable AD

## 📌 프로젝트 개요

> 광물 채굴 → 수갑 제작 → 판매 → 수용소 운영으로 이어지는 타이쿤 루프를 가진 플레이어블 광고 게임입니다.
> 짧은 플레이 시간 안에 핵심 게임 루프를 직관적으로 경험시키는 것을 목표로 하며,
> 확장 가능한 아키텍처 설계에 집중했습니다.

| 항목 | 내용 |
|---|---|
| 장르 | 타이쿤 / 플레이어블 광고 |
| 플랫폼 | Android / iOS |
| 엔진 | Unity 2022.3.62f2 (C#) |
| 개발 인원 | 1인 (기획 · 개발) |

---

## ⛏️ 핵심 게임 루프

```
광물 채굴 → 광물 반납 → 수갑 제작 → 수갑 판매
→ 범죄자 수용 → 수용소 운영 → 업그레이드
```

---

## 🔧 구현 내용

### 🪨 채굴 시스템
- `Physics.OverlapSphere` 기반 범위 내 광물 자동 채굴
- 무기 업그레이드 단계별 채굴 속도 / 범위 / 최대 보유량 변경
- 광물 MAX 시에도 채굴 동작 유지 — 파티클 연출 후 리소스 미적용
- 채굴 시 파티클 이펙트 + 등 뒤 스택 스케일 팝 애니메이션

### 🔗 수갑 제작 시스템
- 광물 반납존(OreDepositZone) → 수갑 공장(HandcuffFactory) 2단계 구조
- 광물 1개당 일정 시간 후 수갑 1개 자동 생산
- MAX 100개 도달 시 생산 중단 + MAX 이미지 팝업 연출

### 💰 판매 및 수익 시스템
- 범죄자 NPC가 줄을 서서 수갑 4개씩 구매
- 수갑 부족 시 범죄자 머리 위에 잔여 수갑 수 실시간 표시
- CoinZone을 통한 돈 수령 방식 — 직접 지급이 아닌 존 진입 시 수령
- 10원 단위 코인 오브젝트 포물선 이동 연출

### 🏢 수용소 시스템
- 수감자 최대 20명 제한 — 가득 차면 판매 자동 중단
- 수용소 만석 시 카메라 연출 + 확장 구매존 활성화
- 50원 지불 시 수용소 확장 → 게임 종료 화면 전환

### 🤖 NPC 자동화
- **광부 NPC** (3마리) : 광석 자동 채굴 → OreDepositZone으로 직접 이동
- **동료 NPC** : HandcuffFactory ↔ SalesZone 자동 왕복 판매
- **범죄자 NPC** : 대기 줄 → 수갑 구매 → 수용소 이동 → 입장
- NavMesh 기반 경로 탐색 + 도착 콜백 구조

### ⬆️ 업그레이드 시스템
- 무기 1단계 (20원) : 채굴 속도 증가 / 범위 확대 / 광물 MAX 20
- 무기 2단계 (50원) : 범위 대폭 확대 / 광물 MAX 50
- 광부 구매 (50원) : MinerNPC 3마리 자동 생성
- 동료 구매 (20원) : WorkerNPC 1명 자동 생성
- 상점 진입 시 보유 금액만큼 코인이 날아가며 가격 채워지는 연출

### 🎥 카메라 연출 (Cinemachine)
- 돈 최초 획득 시 → 무기 업그레이드존 카메라 전환
- 수용소 만석 시 → 수용소 카메라 전환
- 수용소 확장 시 → 확장 수용소 카메라 전환
- 컷씬 재생 중 플레이어 입력 자동 차단

### 🎨 연출 시스템
- 아이템 포물선 이동 (easeOutCubic)
- 아이템 도착 시 스케일 팝 애니메이션 (1.5 → 1.0)
- 플레이어 이동 시 등 뒤 스택 스프링 관성 연출
- 멈출 때 스프링 진동으로 복귀 (또잉~ 효과)
- 화살표 가이드 시스템 (단계별 목적지 안내)
- MAX 이미지 팝업 후 위로 올라가며 사라지는 연출

### 🔊 사운드 시스템
- 무기 단계별 채굴 사운드 (3종)
- 광석 / 수갑 내려놓기 / 돈 획득 / 지불 / 구매 완료 사운드
- 마스터 볼륨 + 효과음별 개별 볼륨 조절

### 🖥️ UI 시스템
- 잔액 / 수용소 인원 실시간 HUD
- 상점 가격표 — 지불 비율에 따라 아래서부터 초록색으로 채워지는 Fill 연출
- 버튼 평상시 탄성 반복 애니메이션 + 클릭 시 스쿼시 & 스트레치

---

## 🏗️ 개발 철학

- **SOLID 원칙** 기반 설계
- **낮은 결합도** — EventBus로 시스템 간 직접 참조 완전 제거
- **높은 응집도** — 단일 책임 원칙에 따른 스크립트 분리
- **확장성** — 새 NPC / 업그레이드 추가 시 기존 코드 수정 없음

---

## 🎯 사용한 디자인 패턴

| 패턴 | 적용 위치 |
|---|---|
| Singleton | GameManager, ResourceManager, CurrencyManager, SoundManager 등 |
| Observer (EventBus) | GameEventBus — 시스템 간 완전한 의존성 분리 |
| Object Pool | OreItem, HandcuffItem, CoinItem 반복 생성 최적화 |
| State Machine | MinerNPC, WorkerNPC, CriminalNPC AI 행동 관리 |
| Service Locator | IResourceContainer 런타임 의존성 주입 |
| Strategy | ICollectable, ISellable, IPurchasable 인터페이스 분리 |
| Data Object | MiningConfig ScriptableObject — 수치 데이터 코드 분리 |
| Dependency Injection | ShopZone → NPC Init()으로 씬 참조 주입 |

---

## 📂 폴더 트리

```
Assets/
└─ _Project/
   ├─ Scripts/
   │  ├─ Core/
   │  │  ├─ GameManager.cs           # 게임 생명주기 및 스테이지 관리
   │  │  ├─ ResourceManager.cs       # 광물/수갑 논리 수량 관리
   │  │  ├─ CurrencyManager.cs       # 재화 관리
   │  │  ├─ PrisonManager.cs         # 수용소 인원 관리
   │  │  ├─ GameEventBus.cs          # 옵저버 패턴 이벤트 버스
   │  │  └─ SoundManager.cs          # 사운드 재생 제어
   │  │
   │  ├─ Player/
   │  │  ├─ PlayerController.cs      # 터치/드래그 이동 입력 처리
   │  │  ├─ PlayerMiner.cs           # 채굴 행동 담당
   │  │  ├─ PlayerInventory.cs       # 시각적 아이템 스택 관리
   │  │  ├─ StackSlotManager.cs      # 등 뒤 스택 위치 계산
   │  │  ├─ StackInertia.cs          # 이동 시 스택 관성 연출
   │  │  ├─ WeaponVisual.cs          # 무기 오브젝트 표시/교체
   │  │  └─ PlayerAnimator.cs        # 애니메이터 파라미터 제어
   │  │
   │  ├─ NPC/
   │  │  ├─ NPCStateMachine.cs       # NPC 상태 머신 추상 베이스
   │  │  ├─ NPCNavigator.cs          # NavMeshAgent 래퍼
   │  │  ├─ MinerNPC.cs              # 광부 NPC AI
   │  │  ├─ WorkerNPC.cs             # 동료 NPC AI
   │  │  └─ CriminalNPC.cs          # 범죄자 NPC AI
   │  │
   │  ├─ Systems/
   │  │  ├─ OreZone.cs               # 광물 스폰 / 리스폰 관리
   │  │  ├─ OreDepositZone.cs        # 광물 반납 존
   │  │  ├─ HandcuffFactory.cs       # 수갑 생산 공장
   │  │  ├─ SalesZone.cs             # 수갑 판매 존
   │  │  ├─ CoinZone.cs              # 코인 수령 존
   │  │  ├─ ShopZone.cs              # NPC 구매 상점 존
   │  │  ├─ UpgradeSystem.cs         # 무기 업그레이드 존
   │  │  ├─ CriminalQueue.cs         # 범죄자 대기 줄 관리
   │  │  ├─ OreReservationSystem.cs  # NPC 광석 중복 채굴 방지
   │  │  ├─ ItemSpawnSystem.cs       # 스테이지별 존 활성화
   │  │  └─ ArrowGuideSystem.cs      # 단계별 화살표 가이드
   │  │
   │  ├─ Items/
   │  │  ├─ OreItem.cs               # 광물 아이템
   │  │  ├─ HandcuffItem.cs          # 수갑 아이템
   │  │  ├─ CoinItem.cs              # 코인 아이템
   │  │  └─ ItemMover.cs             # 포물선 이동 컴포넌트
   │  │
   │  ├─ UI/
   │  │  ├─ HUDController.cs         # 잔액 / 수용소 인원 표시
   │  │  ├─ CameraDirector.cs        # Cinemachine 컷씬 연출
   │  │  ├─ MaxIndicator.cs          # MAX 팝업 연출
   │  │  ├─ ShopPriceDisplay.cs      # 상점 가격표 Fill 연출
   │  │  ├─ IndicatorArrow.cs        # 존 위 화살표 둥실 연출
   │  │  ├─ ButtonBounce.cs          # 버튼 탄성 애니메이션
   │  │  └─ EndScreenUI.cs           # 게임 종료 화면
   │  │
   │  ├─ Interfaces/
   │  │  ├─ ICollectable.cs
   │  │  ├─ IProcessable.cs
   │  │  ├─ ISellable.cs
   │  │  ├─ IPurchasable.cs
   │  │  ├─ IUpgradeable.cs
   │  │  ├─ IWorker.cs
   │  │  └─ IResourceContainer.cs
   │  │
   │  └─ Utils/
   │     ├─ Singleton.cs              # 싱글톤 베이스 클래스
   │     ├─ ServiceLocator.cs         # 런타임 의존성 주입
   │     ├─ ObjectPool.cs             # 키 기반 오브젝트 풀
   │     ├─ OreParticlePool.cs        # 채굴 파티클 풀
   │     ├─ ScalePopUtility.cs        # 스케일 팝 애니메이션 유틸
   │     └─ CoroutineRunner.cs        # 정적 클래스용 코루틴 실행
   │
   ├─ Data/
   │  ├─ Scripts/
   │  │  └─ MiningConfig.cs           # 채굴 수치 ScriptableObject
   │  └─ Instances/
   │     └─ MiningConfig_Default.asset
   │
   ├─ Art/
   │  ├─ Models/
   │  ├─ Animations/
   │  ├─ Textures/
   │  ├─ Materials/
   │  └─ VFX/
   │
   └─ Audio/
      ├─ SFX/
      └─ BGM/
```
