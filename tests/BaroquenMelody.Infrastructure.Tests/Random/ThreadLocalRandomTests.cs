﻿using BaroquenMelody.Infrastructure.Random;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Random;

[TestFixture]
internal sealed class ThreadLocalRandomTests
{
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public void Next_ShouldReturnNumberInRange(int maxValue)
    {
        // act
        var result = ThreadLocalRandom.Next(maxValue);

        // assert
        result.Should().BeGreaterThanOrEqualTo(0).And.BeLessThan(maxValue);
    }

    [TestCase(10, 20)]
    [TestCase(100, 200)]
    [TestCase(1000, 2000)]
    public void Next_ShouldReturnNumberInRange(int minValue, int maxValue)
    {
        // act
        var result = ThreadLocalRandom.Next(minValue, maxValue);

        // assert
        result.Should().BeGreaterThanOrEqualTo(minValue).And.BeLessThan(maxValue);
    }

    [Test]
    public void MultipleThreads_ShouldProduceDifferentNumbers()
    {
        var results = new List<int>();
        var tasks = new List<Task>();

        for (var i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var randomValue = ThreadLocalRandom.Next(int.MaxValue);

                lock (results)
                {
                    results.Add(randomValue);
                }
            }));
        }

        Task.WhenAll(tasks).Wait();

        results.Should().OnlyHaveUniqueItems();
    }
}
