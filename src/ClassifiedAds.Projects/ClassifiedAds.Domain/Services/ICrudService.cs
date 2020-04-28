﻿using ClassifiedAds.Domain.Entities;
using System;
using System.Collections.Generic;

namespace ClassifiedAds.Domain.Services
{
    public interface ICrudService<T>
        where T : AggregateRoot<Guid>
    {
        IList<T> Get();

        T GetById(Guid guid);

        void Add(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}
