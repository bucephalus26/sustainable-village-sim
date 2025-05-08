//using NUnit.Framework;
//using UnityEngine;

//public class PersonalityTests
//{
//    private GameObject testVillagerGO;
//    private VillagerPersonality personality;
//    private MockNeed hungerNeed;
//    private MockNeed restNeed;
//    private MockNeed socialNeed;

//    [SetUp]
//    public void Setup()
//    {
//        testVillagerGO = new GameObject("TestVillagerForPersonality");
//        personality = testVillagerGO.AddComponent<VillagerPersonality>();
//        // Add Villager component as Personality.Awake needs it
//        testVillagerGO.AddComponent<Villager>();

//        // Create mock needs to pass into the method
//        hungerNeed = new MockNeed("Hunger");
//        restNeed = new MockNeed("Rest");
//        socialNeed = new MockNeed("Social");
//    }

//    [TearDown]
//    public void Teardown()
//    {
//        GameObject.DestroyImmediate(testVillagerGO);
//    }

//    [Test]
//    public void GetNeedDecayMultiplier_HighSociability_DecreasesSocialDecay()
//    {
//        // Arrange
//        personality.sociability = 0.8f; // High sociability
//        float expectedMultiplier = 1.0f + (0.5f - 0.8f) * 0.5f; // 1.0 + (-0.3)*0.5 = 1.0 - 0.15 = 0.85

//        // Act
//        float multiplier = personality.GetNeedDecayMultiplier(socialNeed);

//        // Assert
//        Assert.AreEqual(expectedMultiplier, multiplier, 0.01f); // Decay should be slower
//        Assert.Less(multiplier, 1.0f);
//    }

//    [Test]
//    public void GetNeedDecayMultiplier_LowSociability_IncreasesSocialDecay()
//    {
//        // Arrange
//        personality.sociability = 0.2f; // Low sociability
//        float expectedMultiplier = 1.0f + (0.5f - 0.2f) * 0.5f; // 1.0 + (0.3)*0.5 = 1.0 + 0.15 = 1.15

//        // Act
//        float multiplier = personality.GetNeedDecayMultiplier(socialNeed);

//        // Assert
//        Assert.AreEqual(expectedMultiplier, multiplier, 0.01f); // Decay should be faster
//        Assert.Greater(multiplier, 1.0f);
//    }

//    // Add similar tests for Rest/Resilience and Hunger/Impulsivity
//    [Test]
//    public void GetNeedDecayMultiplier_HighResilience_DecreasesRestDecay()
//    {
//        // Arrange
//        personality.resilience = 0.9f;
//        float expectedMultiplier = 1.0f + (0.5f - 0.9f) * 0.5f; // 1.0 + (-0.4)*0.5 = 0.8

//        // Act
//        float multiplier = personality.GetNeedDecayMultiplier(restNeed);

//        // Assert
//        Assert.AreEqual(expectedMultiplier, multiplier, 0.01f);
//        Assert.Less(multiplier, 1.0f);
//    }
//}