﻿using ClassifiedAds.CrossCuttingConcerns.ExtensionMethods;
using ClassifiedAds.Domain.Events;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Modules.Identity.Contracts.Services;
using ClassifiedAds.Modules.Product.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifiedAds.Modules.Product.EventHandlers
{
    public class ProductUpdatedEventHandler : IDomainEventHandler<EntityUpdatedEvent<Entities.Product>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<AuditLogEntry, Guid> _auditLogRepository;
        private readonly IRepository<EventLog, long> _eventLogRepository;

        public ProductUpdatedEventHandler(ICurrentUser currentUser,
            IRepository<AuditLogEntry, Guid> auditLogRepository,
            IRepository<EventLog, long> eventLogRepository)
        {
            _currentUser = currentUser;
            _auditLogRepository = auditLogRepository;
            _eventLogRepository = eventLogRepository;
        }

        public async Task HandleAsync(EntityUpdatedEvent<Entities.Product> domainEvent, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLogEntry
            {
                UserId = _currentUser.UserId,
                CreatedDateTime = domainEvent.EventDateTime,
                Action = "UPDATED_PRODUCT",
                ObjectId = domainEvent.Entity.Id.ToString(),
                Log = domainEvent.Entity.AsJsonString(),
            };

            await _auditLogRepository.AddOrUpdateAsync(auditLog);
            await _auditLogRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            await _eventLogRepository.AddOrUpdateAsync(new EventLog
            {
                EventType = "AUDIT_LOG_ENTRY_CREATED",
                TriggeredById = _currentUser.UserId,
                CreatedDateTime = auditLog.CreatedDateTime,
                ObjectId = auditLog.Id.ToString(),
                Message = auditLog.AsJsonString(),
                Published = false,
            }, cancellationToken);

            await _eventLogRepository.AddOrUpdateAsync(new EventLog
            {
                EventType = "PRODUCT_UPDATED",
                TriggeredById = _currentUser.UserId,
                CreatedDateTime = domainEvent.EventDateTime,
                ObjectId = domainEvent.Entity.Id.ToString(),
                Message = domainEvent.Entity.AsJsonString(),
                Published = false,
            }, cancellationToken);

            await _eventLogRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
