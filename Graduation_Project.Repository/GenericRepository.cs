using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.Models;
using Graduation_Project.Core.Specifications;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Graduation_Project.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task AddAsync(T entity)
        {
             await dbContext.Set<T>().AddAsync(entity);
        }

        public async Task<T> AddWithSaveAsync(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync(); // Save changes immediately to get the ID

            return entity;
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await dbContext.Set<T>().AddRangeAsync(entities);
        }
        public void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
        }
        public void DeleteRange(IEnumerable<T> entities)
        {
            dbContext.Set<T>().RemoveRange(entities);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbContext.Set<T>().AnyAsync(predicate);
        }
        public async Task<IReadOnlyList<T>?> GetAllWithSpecAsync(ISpecifications<T> specs)
        {
            return (IReadOnlyList<T>?)await ApplyQuery(specs).ToListAsync();
        }


        public async Task<IReadOnlyList<T>?> GetFirstWithSpecAsync(ISpecifications<T> specs, int take)
        {
            return await ApplyQuery(specs).Take(take).ToListAsync();
        }


        public async Task<IReadOnlyList<TResult>> GetAllWithSpecAsync<TResult>(ISpecifications<T> spec, Expression<Func<T, TResult>> selector)
        {
            return await ApplyQuery(spec).Select(selector).ToListAsync();
        }

        public async Task<T?> GetWithSpecsAsync(ISpecifications<T> specs)
        {
            return await ApplyQuery(specs).FirstOrDefaultAsync();
        }

     

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {

            return await dbContext.Set<T>().ToListAsync();
        }

        public async Task<T?> GetAsync(int id)
        {
            return await dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetWithNameAsync(string name)
        {
            return await dbContext.Set<T>().FindAsync(name);

        }

        public void Detach(T entity)
        {
            var entry = dbContext.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }

        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            dbContext.Set<T>().Update(entity);
        }
        public async Task<int> GetCountAsync(ISpecifications<T> spec)
        {
            return await ApplyQuery(spec).CountAsync();
        }

        public async Task<decimal> GetSumAsync(ISpecifications<T> spec, Expression<Func<T, decimal>> selector)
        {
            return await ApplyQuery(spec).Select(selector).SumAsync();
        }

        public IQueryable<T> ApplyQuery(ISpecifications<T> specs ) //Helper Method
        {
            return SpecificationsEvaluator<T>.GetQuery(dbContext.Set<T>(), specs);
        }

        public void attaching(T entity)
        {
            dbContext.Attach(entity);
        }

        public async Task<T?> GetByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await dbContext.Set<T>().FirstOrDefaultAsync(expression);
        }

        public async Task<IReadOnlyList<T>?> GetManyByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await dbContext.Set<T>().Where(expression).ToListAsync();
        }

    }
}
