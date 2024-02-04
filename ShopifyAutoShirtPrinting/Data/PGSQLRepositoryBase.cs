using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace ShopifyEasyShirtPrinting.Data
{
    public abstract class PGSQLRepositoryBase<T> : IRepository<T> where T : EntityBase
    {
        protected readonly IMapper _mapper;
        private readonly string _connectionString;

        public PGSQLRepositoryBase(string connectionString, IMapper mapper)
        {
            _mapper = mapper;
            _connectionString = connectionString;
        }

        public void Add(T entity)
        {
            using var context = new LonelyKidsContext(_connectionString);
            context.Set<T>().Add(entity);
            context.SaveChanges();
        }

        public void Delete(T entity)
        {
            using var context = new LonelyKidsContext(_connectionString);

            var existing = context.Set<T>().FirstOrDefault(e => e.Id == entity.Id);
            if (existing != null)
            {
                context.Set<T>().Remove(existing);
            }

            context.SaveChanges();
        }

    
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            using var context = new LonelyKidsContext(_connectionString);

            return context.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .ToArray();
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            using var context = new LonelyKidsContext(_connectionString);

            return context.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .SingleOrDefault();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            using var context = new LonelyKidsContext(_connectionString);

            return context.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .SingleOrDefault();
        }

        public IEnumerable<T> All()
        {
            using var context = new LonelyKidsContext(_connectionString);

            return context.Set<T>()
                .AsNoTracking()
                .ToArray();
        }

        public T GetById(int id)
        {
            using var context = new LonelyKidsContext(_connectionString);

            return context.Set<T>()
                .Where(x => x.Id == id)
                .AsNoTracking()
                .SingleOrDefault();
        }

        public void Update(T entity)
        {
            using var context = new LonelyKidsContext(_connectionString);

            var existing = context.Set<T>().Find(entity.Id);
            if (existing != null)
            {
                _mapper.Map(entity, existing);
                context.Entry(existing).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void AddRange(IEnumerable<T> entities)
        {
            using var context = new LonelyKidsContext(_connectionString);
            context.Set<T>().AddRange(entities.ToArray());
            context.SaveChanges();
        }
    }
}
