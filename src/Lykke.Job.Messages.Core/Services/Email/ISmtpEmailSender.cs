﻿using System.Threading.Tasks;
using Lykke.Job.Messages.Contract.Emails;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface ISmtpEmailSender
    {
        Task SendEmailAsync(string partnerId, string emailAddress, EmailMessage message, string sender = null);
        Task SendBroadcastAsync(string partnerId, BroadcastGroup broadcastGroup, EmailMessage message);
    }
}