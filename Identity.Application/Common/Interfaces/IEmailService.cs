using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationCodeAsync(
            string toEmail,
            string userName,
            string confirmationCode,
            CancellationToken cancellationToken = default);
    }
}
