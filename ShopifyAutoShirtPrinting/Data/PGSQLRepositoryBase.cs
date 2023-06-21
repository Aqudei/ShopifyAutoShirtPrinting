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
        protected readonly LonelyKidsContext _context;
        protected readonly IMapper _mapper;

        public PGSQLRepositoryBase(LonelyKidsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            var existing = _context.Set<T>().FirstOrDefault(e => e.Id == entity.Id);
            if (existing != null)
            {
                _context.Set<T>().Remove(existing);
            }

            _context.SaveChanges();
        }

        public void DeleteAll()
        {
            _context.Set<T>().RemoveRange(_context.Set<T>().ToArray());
            _context.SaveChanges();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .ToArray();
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .SingleOrDefault();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .SingleOrDefault();
        }

        public IEnumerable<T> All()
        {
            return _context.Set<T>()
                .AsNoTracking()
                .ToArray();
        }

        public T GetById(int id)
        {
            return _context.Set<T>()
                .Where(x => x.Id == id)
                .AsNoTracking()
                .SingleOrDefault();
        }

        public void Update(T entity)
        {
            var existing = _context.Set<T>().Find(entity.Id);
            if (existing != null)
            {
                _mapper.Map(entity, existing);
                _context.Entry(existing).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities.ToArray());
            _context.SaveChanges();
        }
    }
}
