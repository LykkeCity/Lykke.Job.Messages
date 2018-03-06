using Lykke.Cqrs;
using Lykke.Job.Messages.Contract.Commands;
using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Job.Messages.Contract.Utils;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Contract
{
    public interface IEmailMessageSender
    {
        void SendEmail<T>(string emailAddress, string partnerId, T messageData) where T : IEmailMessageData;
    }

    public class EmailMessageSender : IEmailMessageSender
    {
        private readonly ICqrsEngine _cqrsEnginge;
        private readonly string _boundedContext;
        private readonly Dictionary<Type, string> _registeredTypes;

        public EmailMessageSender(ICqrsEngine cqrsEnginge, string boundedContext)
        {
            _cqrsEnginge = cqrsEnginge;
            _boundedContext = boundedContext;
            _registeredTypes = new Dictionary<Type, string>();

            RegisterTemplates();
        }

        public void SendEmail<T>(string emailAddress, string partnerId, T messageData) where T : IEmailMessageData
        {
            string serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(messageData);

            if (!_registeredTypes.TryGetValue(typeof(T), out var templateIdValue))
            {
                throw new Exception($"{typeof(T)} is not registered as email for sending");
            }

            var command = new SendEmailCommand()
            {
                EmailAddress = emailAddress,
                EmailTemplateId = templateIdValue,
                PartnerId = partnerId,
                SerializedMessageData = serializedData,
            };

            _cqrsEnginge.SendCommand(command, _boundedContext, EmailMessagesBoundedContext.Name);
        }

        private void RegisterTemplates()
        {
            var emailMessageDataType = typeof(IEmailMessageData);
            var emailTypes = ReflectionUtil.GetImplTypesAssignableToMarkerTypeFromAsssembly(
                                            emailMessageDataType.Assembly,
                                            emailMessageDataType);

            emailTypes.ForEach(type =>
            {
                var templateIdValue = (string)ReflectionUtil.ExtractConstValueFromType(type, "EmailTemplateId");
                _registeredTypes[type] = templateIdValue;
            });
        }
    }
}
