using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

[TestFixture]
internal sealed class FooTests
{
    [Test]
    public void Bar_ReturnsBar() => new Foo().Bar().Should().Be("Bar");
}
