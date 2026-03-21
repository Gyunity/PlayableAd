namespace PlayableAd.Interface
{
    public enum ResourceType
    {
        Ore, //광물
        Handcuff, //수갑
        Money, // 돈
        Weapon // 무기
    }

    public enum UpgradeType
    {
        Weapon1, //무기 1단계
        Weapon2, //무기 2단계
        Miner, //광부
        Worker, //동료
        PrisonExpand //수용소 증가
    }

    public enum NPCRole
    {
        Miner,
        FactoryWorker,
        SalesWorker,
        Prisoner
    }
    public enum GameStage
    {
        Stage1_Mining = 1,
        Stage2_Factory,
        Stage3_Sales,
        Stage4_WeaponUpgrade,
        Stage5_BuyMiner,
        Stage6_BuyWorker,
        Stage7_WeaponUpgrade2,
        Stage8_PrisonExpand,
        Stage9_End
    }
}