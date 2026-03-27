

using System;

namespace TheProject
{
    public class GameEvents
    {
        public static class DiaLog
        {
            public const string StartDialogueWithIdSpeaker = "StartDialogueWithIdSpeaker";
            public const string StartDialogueWithIdNode = "StartDialogueWithIdNode";
            public const string NextDialogue = "NextDialogue";
            public const string OnChoiceSelected = "OnChoiceSelected";
            public const string ReadDialogue = "ReadDialogue";
            public const string EndDialogue = "EndDialogue";
        }
        public static class Quest
        {
            public const string GetAllActiveObjective = "GetAllActiveObjective";
            public const string SetQuestObjectiveComplete = "SetQuestObjectiveComplete";
            public const string OnQuestStatusChanged = "OnQuestStatusChanged";
            public const string OnQuestProgress = "OnQuestProgress";
        }
        public static class Objective
        {
            public const string OnObjectiveStatusChanged = "OnObjectiveStatusChanged";
        }
        public static class Choice
        {
            public const string ShowChoices = "ShowChoices";
            public const string EndChoice = "EndChoice";
        }
        public static class Camera
        {
            public const string EnableCamRotate = "EnableCamRotate";
        }
        public static class Player
        {
            public const string OnPlayerLoadData = "OnPlayerLoadData";
            public const string OnPlayerStateChanged = "OnPlayerStateChanged";
            public const string OnHealthChanged = "OnHealthChanged";
            public const string OnSanityChanged = "OnSanityChanged";
            public const string OnPlayerDied = "OnPlayerDied";
        }
        public static class ObjectPool
        {
            public const string HideAll = "HideAll";
        }
        public static class NpcWeaponAttack
        {
            public const string StartAttack = "StartAttack";
        }
        public static class SceneTransition
        {
            public const string OnPlayerSpawned = "OnPlayerSpawned";
            public const string OnSceneLoaded = "OnSceneLoaded";
            public const string OnSceneChangeRequested = "OnSceneChangeRequested";
            // Fire khi màn hình đen hoàn toàn (sau fade-out, trước fade-in)
            // Dùng để kích hoạt logic game (StartDay...) trong lúc player không thấy gì
            public const string OnBlackScreen = "SceneTransition.OnBlackScreen";
        }
        public static class ChapterManager
        {
            public const string OnChapterLoaded = "OnChapterLoaded";
            public const string OnChapterDataLoaded = "OnChapterDataLoaded";
        }
        public static class Npc
        {
            public const string OnLoadNpcData = "OnLoadNpcData";
        }
        public static class Flag
        {
            public const string OnFlagChanged = "OnFlagChanged";
        }
        public static class Object
        {
            public const string OnItemCollected = "OnItemCollected";
        }

        public class Puzzle
        {
            public const string OnPuzzleSolved = "OnPuzzleSolved";
        }

        public static class Story
        {
            public const string OnStoryEvent = "OnStoryEvent";
        }

        public static class Localization
        {
            public const string OnLanguageChanged = "OnLanguageChanged";
        }

        public static class Day
        {
            public const string OnDayChanged = "OnDayChanged";
        }

        public static class Anomaly
        {
            public const string OnDayStart = "Anomaly.OnDayStart";
            public const string OnDayCorrect = "Anomaly.OnDayCorrect";
            public const string OnWrong = "Anomaly.OnWrong";
            public const string OnPhaseChanged = "Anomaly.OnPhaseChanged";
            public const string OnGameReset = "Anomaly.OnGameReset";
            public const string OnGameComplete = "Anomaly.OnGameComplete";

            // --- GAME OVER SEQUENCE ---
            public const string OnGameOver = "Anomaly.OnGameOver";
            public const string OnGameOverDone = "Anomaly.OnGameOverDone";

            // --- ENEMY CHASE ---
            public const string OnEnemyChaseStart = "Anomaly.OnEnemyChaseStart";
            public const string OnPlayerCaught = "Anomaly.OnPlayerCaught";
            public const string OnPlayerCaughtDone = "Anomaly.OnPlayerCaughtDone";

            public const string OnJumpscare = "Anomaly.OnJumpscare";
        }

    }
    public struct SceneChangeData
    {
        public string SceneName;
        public string TargetSpawnID;
    }

    /// <summary>Dữ liệu trước khi chuyển ngày (phase và day đã +1 để hiển thị)</summary>
    public struct AnomalyResultData
    {
        public int Phase;
        public int Day;
        public bool WasAnomaly;
    }

    public struct JumpscareData
    {
        public UnityEngine.Sprite Sprite;
        public UnityEngine.AudioClip Sound; // null = dùng default sound trong JumpscareEffect
    }


    // public struct QuestEventData
    // {
    //     public EQuestObjectiveType Type; // Loại: Collect, Kill, Talk...
    //     public string TargetID;          // ID: "Milk_Strawberry", "Boss_1"...
    //     public int Amount;               // Số lượng
    // }
}