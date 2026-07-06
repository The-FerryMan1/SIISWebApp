using System;

namespace SIISMinimalAPI.Features.Auth.Logout;

public interface ILogoutService
{
  Task Logout();
}
