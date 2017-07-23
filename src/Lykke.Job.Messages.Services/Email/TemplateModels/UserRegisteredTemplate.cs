﻿using System;

namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class UserRegisteredTemplate
    {
        public DateTime DateTime { get; set; }
        public string Email { get; set; }
        public string ContactPhone { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string UserId { get; set; }
    }
}
