using System;

namespace SIISMinimalAPI.Features.OnBoarding;

public interface IOnBoadringService
{
    Task CreateOnBoarding(OnBoardingDto onBoardingDto, CancellationToken ct);
}
