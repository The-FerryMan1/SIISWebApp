using System;

namespace SIISMinimalAPI.Features.OnBoarding;

public interface IOnBoadringService
{
    Task CreateOnBoarding(OnBoardingDto onBoardingDto, CancellationToken ct);

    Task UpdatedOnBoarding(Guid uuid, OnBoardUpdateDto dto, CancellationToken ct);
}
