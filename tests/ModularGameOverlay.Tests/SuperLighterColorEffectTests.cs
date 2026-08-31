using SuperLighter.App.Services;

namespace ModularGameOverlay.Tests;

public sealed class SuperLighterColorEffectTests
{
    [Fact]
    public void GammaCompatibilityEffectMovesMidtonesInRequestedDirection()
    {
        foreach (var gamma in new[] { 0.5f, 1f, 4.17f, 6f })
        {
            var effect = DisplayColorEffectService.BuildGammaEffect(gamma);
            var transformedMidtone = ApplyGray(effect, 0.5f);

            if (gamma < 1f)
            {
                Assert.InRange(transformedMidtone, 0f, 0.49999f);
                Assert.Equal(0f, ApplyGray(effect, 0f), precision: 5);
            }
            else if (gamma > 1f)
            {
                Assert.InRange(transformedMidtone, 0.50001f, 1f);
                Assert.Equal(1f, ApplyGray(effect, 1f), precision: 5);
            }
            else
            {
                Assert.Equal(0.5f, transformedMidtone, precision: 5);
            }
        }
    }

    [Fact]
    public void ContrastEffectMatchesCenteredContrastFormula()
    {
        foreach (var contrast in new[] { 0.5f, 1f, 1.2f, 2f })
        {
            var effect = DisplayColorEffectService.BuildContrastEffect(contrast);
            foreach (var input in new[] { 0.25f, 0.5f, 0.75f })
            {
                var expected = ((input - 0.5f) * contrast) + 0.5f;
                Assert.Equal(expected, ApplyGray(effect, input), precision: 5);
            }
        }
    }

    [Fact]
    public void SoftwareBrightnessMatchesWhiteOverlayBlending()
    {
        foreach (var percentage in new[] { 0, 30, 60 })
        {
            var boost = percentage / 100f;
            var effect = DisplayColorEffectService.BuildBrightnessEffect(boost);
            var input = 0.25f;
            var transformed = (input * effect.Transform[0]) + effect.Transform[20];
            var expected = (input * (1f - boost)) + boost;

            Assert.Equal(1f - boost, effect.Transform[0], precision: 5);
            Assert.Equal(effect.Transform[0], effect.Transform[6]);
            Assert.Equal(effect.Transform[0], effect.Transform[12]);
            Assert.Equal(boost, effect.Transform[20], precision: 5);
            Assert.Equal(effect.Transform[20], effect.Transform[21]);
            Assert.Equal(effect.Transform[20], effect.Transform[22]);
            Assert.Equal(expected, transformed, precision: 5);
        }
    }

    private static float ApplyGray(
        SuperLighter.App.Native.NativeMethods.MagnificationColorEffect effect,
        float input) => (input * effect.Transform[0]) + effect.Transform[20];
}
