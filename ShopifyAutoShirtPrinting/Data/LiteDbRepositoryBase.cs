using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ShopifyEasyShirtPrinting.Data
{

    // This file is ignored from compiling
    public abstract class LiteDbRepositoryBase<T> : IRepository<T> where T : EntityBase
    {
        protected readonly LiteDatabase liteDatabase;
        protected readonly ILiteCollection<T> _collection;

        public LiteDbRepositoryBase(LiteDatabase liteDatabase)
        {
            this.liteDatabase = liteDatabase;
            _collection = liteDatabase.GetCollection<T>();
        }
        public void Add(T item)
        {
            _collection.Insert(item);
        }

        public void DeleteAll()
        {
            _collection.DeleteAll();
        }

        public void Delete(T item)
        {
            _collection.Delete(item.Id);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _collection.Find(predicate);
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            return _collection.Find(predicate).FirstOrDefault();
        }

        public T Get(int id)
        {
            return _collection.FindById(id);
        }

        public IEnumerable<T> GetAll()
        {
            return _collection.FindAll();
        }

        public T GetById(int id)
        {
            return _collection.FindOne(i => i.Id == id);
        }

        public void Update(T item)
        {
            _collection.Update(item);
        }
    }
}
