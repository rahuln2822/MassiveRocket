using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace MassiveRocketAssignent.Tests
{
    [TestClass]
    public class BatchProcessorUnitTests
    {
        private static int BatchSize = 100;

        [TestMethod]
        public void CreateBatches_WithValidEntities_ReturnsCorrectBatches()
        {
            // Arrange
            var entities = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                entities.Add($"Entity{i}");
            }
            // Act
            // Assert
        }

        private class BatchProcessorUnitTestsDependencies
        { 
            public IBatchProcessor<string> CreateInstance()
            { 
                
            }
        }
    }
}
