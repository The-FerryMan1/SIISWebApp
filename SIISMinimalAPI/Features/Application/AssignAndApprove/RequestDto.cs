using System;
using SIISMinimalAPI.Features.Application.GetById;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Application.AssignAndApprove;

public class RequestDto
{
    public OfficeNameEnum Office { get; set; }
}
