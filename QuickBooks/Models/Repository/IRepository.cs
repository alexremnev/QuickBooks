﻿using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Create(T entity);
    }
}