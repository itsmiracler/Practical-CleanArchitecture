﻿using ClassifiedAds.Application;
using ClassifiedAds.CrossCuttingConcerns.ExtensionMethods;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Infrastructure.Identity;
using ClassifiedAds.Services.Storage.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifiedAds.Services.Storage.Commands
{
    public class AddAuditLogEntryCommand : ICommand
    {
        public AuditLogEntry AuditLogEntry { get; set; }
    }

    public class AddAuditLogEntryCommandHandler : ICommandHandler<AddAuditLogEntryCommand>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<AuditLogEntry, Guid> _auditLogRepository;
        private readonly IRepository<OutboxEvent, long> _outboxEventRepository;

        public AddAuditLogEntryCommandHandler(ICurrentUser currentUser,
            IRepository<AuditLogEntry, Guid> auditLogRepository,
            IRepository<OutboxEvent, long> outboxEventRepository)
        {
            _currentUser = currentUser;
            _auditLogRepository = auditLogRepository;
            _outboxEventRepository = outboxEventRepository;
        }

        public async Task HandleAsync(AddAuditLogEntryCommand command, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLogEntry
            {
                UserId = command.AuditLogEntry.UserId,
                CreatedDateTime = command.AuditLogEntry.CreatedDateTime,
                Action = command.AuditLogEntry.Action,
                ObjectId = command.AuditLogEntry.ObjectId,
                Log = command.AuditLogEntry.Log,
            };

            await _auditLogRepository.AddOrUpdateAsync(auditLog);
            await _auditLogRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            await _outboxEventRepository.AddOrUpdateAsync(new OutboxEvent
            {
                EventType = "AUDIT_LOG_ENTRY_CREATED",
                TriggeredById = _currentUser.UserId,
                CreatedDateTime = auditLog.CreatedDateTime,
                ObjectId = auditLog.Id.ToString(),
                Message = auditLog.AsJsonString(),
                Published = false,
            }, cancellationToken);

            await _outboxEventRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
