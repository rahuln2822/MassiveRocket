using FluentAssertions;
using MassiveRocketAssignment;
using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Tests;
using MassiveRocketAssignment.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;

namespace MassiveRocketAssignent.Tests
{
    [TestClass]
    public class BatchProcessorUnitTests
    {
        private readonly int BatchSize = 100;

        [TestMethod]
        public void CreateBatches_WithValidEntities_ReturnsCorrectBatches()
        {
            // Arrange
            var dependencies = new BatchProcessorUnitTestsDependencies();           

            var entities = dependencies.PrepareSampleData();  
            var batchProcessor = dependencies.CreateInstance();
            var resultantBatches = 1000/BatchSize;

            // Act
            var result = batchProcessor.CreateBatches(entities);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(resultantBatches);
        }

        private class BatchProcessorUnitTestsDependencies
        {
            public IConfiguration Configuration { get; set; }
            public IHost HostedService { get; set; } = MassiveRocketAssignment.Tests.DependencyRoot.BuildAndRunHost();

            public IBatchProcessor<string> CreateInstance()
            {
                return HostedService.Services.GetService<IBatchProcessor<string>>();
            }

            public IEnumerable<string> PrepareSampleData()
            {
                for (int i = 0; i < 1000; i++)
                {
                    yield return $"Entity{i}";
                }
            }
        }
    }
}
