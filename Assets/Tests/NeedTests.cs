//using NUnit.Framework;
//using UnityEngine;
//using System.Collections.Generic; // Required for List if Need used it internally, maybe not needed here

//// Test double class to allow setting CurrentValue directly
//public class MockNeed : Need
//{
//    // Make Villager optional in constructor for testing
//    public MockNeed(string name, Villager villager = null, float importanceWeight = 1.0f, ResourceType requiredResource = ResourceType.None, float resourceAmountNeeded = 1.0f)
//        : base(name, villager, importanceWeight, requiredResource, resourceAmountNeeded) { }

//    // Public setter for testing
//    public void SetCurrentValue_Test(float value)
//    {
//        CurrentValue = Mathf.Clamp(value, 0f, 100f);
//    }

//    // Override Decay slightly if TimeManager is problematic, assume TimeScaleFactor = 1 for test
//    public override void Decay(float deltaTime)
//    {
//        // Simplified decay for testing, assuming TimeScaleFactor = 1 and no personality for now
//        float timeScaledDecay = baseDecayRate * deltaTime; // Assumes baseDecayRate is accessible or known
//        float previousValue = CurrentValue;
//        CurrentValue = Mathf.Max(0f, CurrentValue - timeScaledDecay);

//        // Note: Doesn't trigger events or use personality for simplicity in this mock test scenario
//    }

//    // Override Fulfill slightly for testing value changes without full dependencies
//    public override bool Fulfill(float fulfillmentAmount)
//    {
//        // Basic fulfillment for testing clamping, ignoring resources/wealth/diminishing returns here
//        float previousValue = CurrentValue;
//        CurrentValue = Mathf.Clamp(CurrentValue + fulfillmentAmount, 0f, 100f);
//        return CurrentValue > previousValue; // Simple success metric
//    }
//}


//public class NeedTests
//{
//    [Test]
//    public void GetUrgency_HighNeed_ReturnsLowUrgency()
//    {
//        // Arrange
//        var need = new MockNeed("TestNeed", null, 1.0f); // Importance = 1.0
//        need.SetCurrentValue_Test(90f); // High value
//        float expectedMaxUrgency = 0.1f * 1.0f; // (1 - 0.9) * 1.0

//        // Act
//        float urgency = need.GetUrgency();

//        // Assert
//        Assert.AreEqual(expectedMaxUrgency, urgency, 0.01f); // Use tolerance for float comparison
//    }

//    [Test]
//    public void GetUrgency_LowNeed_ReturnsHighUrgency()
//    {
//        // Arrange
//        var need = new MockNeed("TestNeed", null, 1.5f); // Higher importance
//        need.SetCurrentValue_Test(30f); // Low value
//        float expectedUrgency = (1f - 0.3f) * 1.5f; // (1 - 0.3) * 1.5 = 0.7 * 1.5 = 1.05

//        // Act
//        float urgency = need.GetUrgency();

//        // Assert
//        Assert.AreEqual(expectedUrgency, urgency, 0.01f);
//    }

//    [Test]
//    public void GetUrgency_CriticalNeed_ReturnsVeryHighUrgency()
//    {
//        // Arrange
//        var need = new MockNeed("TestNeed", null, 1.0f);
//        // Assume CriticalThreshold is 20f
//        need.SetCurrentValue_Test(15f); // Below critical
//        float expectedBaseUrgency = (1f - 0.15f) * 1.0f; // 0.85
//        float expectedCriticalUrgency = expectedBaseUrgency * 2f; // * 2 multiplier

//        // Act
//        float urgency = need.GetUrgency();

//        // Assert
//        Assert.AreEqual(expectedCriticalUrgency, urgency, 0.01f);
//    }

//    [Test]
//    public void Fulfill_ClampsAt100()
//    {
//        // Arrange
//        var need = new MockNeed("TestNeed");
//        need.SetCurrentValue_Test(90f);

//        // Act
//        need.Fulfill(30f); // Try to fulfill beyond 100

//        // Assert
//        Assert.AreEqual(100f, need.CurrentValue, 0.01f);
//    }

//    [Test]
//    public void Decay_ReducesValue()
//    {
//        // Arrange
//        var need = new MockNeed("TestNeed", null, 1.0f); // baseDecayRate
//        need.SetCurrentValue_Test(50f);
//        float deltaTime = 1f; // Simulate 1 second (assuming TimeScaleFactor=1 in mock)
//        float expectedDecay = need.DecayRate * deltaTime; // Expect decay of 1.0

//        // Act
//        need.Decay(deltaTime);

//        // Assert
//        Assert.AreEqual(50f - expectedDecay, need.CurrentValue, 0.01f);
//    }

//    [Test]
//    public void Decay_ClampsAt0()
//    {
//        // Arrange
//        var need = new MockNeed("TestNeed");
//        need.SetCurrentValue_Test(5f);
//        float deltaTime = 10f; // Simulate large time delta

//        // Act
//        need.Decay(deltaTime); // Try to decay below 0

//        // Assert
//        Assert.AreEqual(0f, need.CurrentValue, 0.01f);
//    }
//}