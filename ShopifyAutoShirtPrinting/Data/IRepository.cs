using System;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Data
{
    public interface IRepository<T>
    {
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        T Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        T GetById(int id);
        IEnumerable<T> All();
        IEnumerable<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        T FindOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    }
}
