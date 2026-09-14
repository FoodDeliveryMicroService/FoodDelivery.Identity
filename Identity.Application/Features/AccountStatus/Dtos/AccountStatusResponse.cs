using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.AccountStatus.Dtos;

public sealed record AccountStatusResponse(Guid UserId, string Status);