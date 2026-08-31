using SuperLighter.App.Services;

namespace ModularGameOverlay.Tests;

public sealed class SuperLighterDisplayRoutingTests
{
    [Fact]
    public void NvidiaDetectionAcceptsVendorIdOrAdapterNameOnly()
    {
        Assert.True(DisplayAdapterDetector.IsNvidiaAdapter(
            @"PCI\VEN_10DE&DEV_2206&SUBSYS_15373842",
            "Microsoft Basic Display Adapter"));
        Assert.True(DisplayAdapterDetector.IsNvidiaAdapter(
            string.Empty,
            "NVIDIA GeForce RTX 3080"));

        Assert.False(DisplayAdapterDetector.IsNvidiaAdapter(
            @"PCI\VEN_1002&DEV_73BF",
            "AMD Radeon RX 6800 XT"));
        Assert.False(DisplayAdapterDetector.IsNvidiaAdapter(
            @"PCI\VEN_8086&DEV_9BC5",
            "Intel UHD Graphics"));
        Assert.False(DisplayAdapterDetector.IsNvidiaAdapter(
            @"USB\VID_17E9&PID_6006",
            "DisplayLink USB Device"));
    }

    [Fact]
    public void NvidiaAndNonNvidiaUseMutuallyExclusiveEffectPaths()
    {
        var nvidia = DisplayEffectRouting.FromNvidiaPresence(true);
        Assert.False(nvidia.UseLegacyGammaAndBrightness);
        Assert.True(nvidia.UseNvidiaCompatibilityMatrix);

        var nonNvidia = DisplayEffectRouting.FromNvidiaPresence(false);
        Assert.True(nonNvidia.UseLegacyGammaAndBrightness);
        Assert.False(nonNvidia.UseNvidiaCompatibilityMatrix);
    }

    [Fact]
    public void NonNvidiaLegacyGammaRampRetainsOriginalNonlinearBehavior()
    {
        var identity = GammaRampService.CreateIdentityRamp();
        var neutral = GammaRampService.BuildRamp(identity, contrast: 1d, gamma: 1d);
        var boosted = GammaRampService.BuildRamp(identity, contrast: 1.2d, gamma: 4.17d);

        Assert.Equal(identity.Red, neutral.Red);
        Assert.Equal(identity.Green, neutral.Green);
        Assert.Equal(identity.Blue, neutral.Blue);
        Assert.True(boosted.Red[128] > neutral.Red[128]);
        Assert.True(boosted.Red.Zip(boosted.Red.Skip(1), (left, right) => left <= right).All(value => value));
    }
}
