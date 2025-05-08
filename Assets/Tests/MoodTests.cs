//using NUnit.Framework;
//using UnityEngine;

//public class MoodTests
//{
//    private GameObject testVillagerGO;
//    private VillagerMood villagerMood;

//    [SetUp] // Runs before each test
//    public void Setup()
//    {

//        testVillagerGO = new GameObject("TestVillagerForMood");

//        villagerMood = testVillagerGO.AddComponent<VillagerMood>();
//    }

//    [TearDown] // Runs after each test
//    public void Teardown()
//    {
//        // Clean up the temporary GameObject
//        GameObject.DestroyImmediate(testVillagerGO);
//    }

//    [Test]
//    public void GetWorkEfficiencyMultiplier_LowHappiness_ReturnsLowMultiplier()
//    {
//        // Arrange

//        float expectedMultiplier = Mathf.Lerp(0.5f, 1.5f, 10f / 100f); // 0.5 + (1.5-0.5)*(0.1) = 0.5 + 0.1 = 0.6

//        // Act
//        float multiplier = villagerMood.GetWorkEfficiencyMultiplier();

//        // Assert
//        Assert.AreEqual(expectedMultiplier, multiplier, 0.01f);
//    }

//    [Test]
//    public void GetWorkEfficiencyMultiplier_MidHappiness_ReturnsMidMultiplier()
//    {
//        // Arrange
//        SetHappiness(villagerMood, 50f);
//        float expectedMultiplier = Mathf.Lerp(0.5f, 1.5f, 50f / 100f); // 0.5 + 1.0*0.5 = 1.0

//        // Act
//        float multiplier = villagerMood.GetWorkEfficiencyMultiplier();

//        // Assert
//        Assert.AreEqual(expectedMultiplier, multiplier, 0.01f);
//    }

//    [Test]
//    public void GetWorkEfficiencyMultiplier_HighHappiness_ReturnsHighMultiplier()
//    {
//        // Arrange
//        SetHappiness(villagerMood, 90f);
//        float expectedMultiplier = Mathf.Lerp(0.5f, 1.5f, 90f / 100f); // 0.5 + 1.0*0.9 = 1.4

//        // Act
//        float multiplier = villagerMood.GetWorkEfficiencyMultiplier();

//        // Assert
//        Assert.AreEqual(expectedMultiplier, multiplier, 0.01f);
//    }

//    private void SetHappiness(VillagerMood mood, float value)
//    {

//        var fieldInfo = typeof(VillagerMood).GetField("happiness",
//            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        if (fieldInfo != null)
//        {
//            fieldInfo.SetValue(mood, Mathf.Clamp(value, 0f, 100f));
//        }
//        else
//        {
//            Assert.Fail("Could not find private field 'happiness' via reflection.");
//        }
//    }
//}