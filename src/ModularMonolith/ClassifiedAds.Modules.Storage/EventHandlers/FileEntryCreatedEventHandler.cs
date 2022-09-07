﻿using ClassifiedAds.CrossCuttingConcerns.ExtensionMethods;
using ClassifiedAds.Domain.Events;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Modules.Identity.Contracts.Services;
using ClassifiedAds.Modules.Storage.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifiedAds.Modules.Storage.EventHandlers
{
    public class FileEntryCreatedEventHandler : IDomainEventHandler<EntityCreatedEvent<FileEntry>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<AuditLogEntry, Guid> _auditLogRepository;
        private readonly IRepository<OutboxEvent, long> _outboxEventRepository;

        public FileEntryCreatedEventHandler(ICurrentUser currentUser,
            IRepository<AuditLogEntry, Guid> auditLogRepository,
            IRepository<OutboxEvent, long> outboxEventRepository)
        {
            _currentUser = currentUser;
            _auditLogRepository = auditLogRepository;
            _outboxEventRepository = outboxEventRepository;
        }

        public async Task HandleAsync(EntityCreatedEvent<FileEntry> domainEvent, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLogEntry
            {
                UserId = _currentUser.IsAuthenticated ? _currentUser.UserId : Guid.Empty,
                CreatedDateTime = domainEvent.EventDateTime,
                Action = "CREATED_FILEENTRY",
                ObjectId = domainEvent.Entity.Id.ToString(),
                Log = domainEvent.Entity.AsJsonString(),
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

            await _outboxEventRepository.AddOrUpdateAsync(new OutboxEvent
            {
                EventType = "FILEENTRY_CREATED",
                TriggeredById = _currentUser.UserId,
                CreatedDateTime = domainEvent.EventDateTime,
                ObjectId = domainEvent.Entity.Id.ToString(),
                Message = domainEvent.Entity.AsJsonString(),
                Published = false,
            }, cancellationToken);

            await _outboxEventRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
