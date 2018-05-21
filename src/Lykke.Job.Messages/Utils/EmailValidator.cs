using System;
using Common;

namespace Lykke.Job.Messages.Utils
{
    internal static class EmailValidator
    {
        internal static void ValidateEmail(string email, Guid clientId)
        {
            ValidateEmail(email, clientId.ToString());
        }

        internal static void ValidateEmail(string email, string clientId)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException($"Email is empty for client {clientId}");

            if (!email.IsValidEmail())
                throw new InvalidOperationException($"Client {clientId} has invalid email - {email}");
        }
    }
}
