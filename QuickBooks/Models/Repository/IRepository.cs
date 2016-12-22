﻿using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IRepository<T> where T : class
    {
        void Create(T entity);
    }
}
