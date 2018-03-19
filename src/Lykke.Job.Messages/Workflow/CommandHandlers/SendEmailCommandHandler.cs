//using Common;
//using Common.Log;
//using JetBrains.Annotations;
//using Lykke.Cqrs;
//using Lykke.Job.Messages.Contract.Commands;
//using Lykke.Job.Messages.Contract.Emails.MessageData;
//using Lykke.Job.Messages.Core.Services.Email;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace Lykke.Job.Messages.Workflow.CommandHandlers
//{
//    public class SendEmailCommandHandler
//    {
//        private readonly ILog _log;
//        private readonly IEmailMessageProcessor _emailMessageProcessor;

//        public SendEmailCommandHandler(ILog log,
//           IEmailMessageProcessor emailMessageProcessor
//            )
//        {
//            _log = log;
//            _emailMessageProcessor = emailMessageProcessor;
//        }

//        [UsedImplicitly]
//        public async Task<CommandHandlingResult> Handle(SendEmailCommand command, IEventPublisher publisher)
//        {
//            Type emailType = null;
//            dynamic email = null;

//            _log.WriteInfo(nameof(SendEmailCommandHandler), command, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" + 
//                $"{Environment.NewLine}Email to: {command.EmailAddress.SanitizeEmail()}");

//            try
//            {
//                emailType = _emailMessageProcessor.GetTypeForTemplateId(command.EmailTemplateId);

//                if (emailType == null)
//                {
//                    throw new Exception($"No registered email for command.EmailTemplateId == {command.EmailTemplateId}");
//                }

//                email = Newtonsoft.Json.JsonConvert.DeserializeObject(command.SerializedMessageData, emailType);
//            }
//            catch (Exception e)
//            {
//                _log.WriteError(command.EmailAddress, command, e);

//                CommandHandlingResult.Ok();
//            }

//            var d1 = typeof(Core.Domain.Email.Models.SendEmailRequest<>);
//            Type[] typeArgs = { emailType };
//            var makeme = d1.MakeGenericType(typeArgs);
//            object o = Activator.CreateInstance(makeme, command.EmailAddress, command.PartnerId, email);

//            await _emailMessageProcessor.SendAsync((dynamic)o);

//            return CommandHandlingResult.Ok();
//        }

//        public async Task WrapTryCathAsync(Func<Task> func, string process, object context)
//        {
//            try
//            {
//                await func();
//            }
//            catch (Exception e)
//            {
//                _log.WriteError(process, context, e);
//            }
//        }
//    }
//}
