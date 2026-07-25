using ProjectLEGO.Core;
using Xunit;

namespace ProjectLEGO.Core.Tests;

public sealed class ProjectIdentityTests
{
    [Fact]
    public void Name_IsProjectLego()
    {
        Assert.Equal("Project LEGO", ProjectIdentity.Name);
    }
}
