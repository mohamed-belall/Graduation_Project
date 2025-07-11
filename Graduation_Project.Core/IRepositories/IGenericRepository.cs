using Graduation_Project.Core.Models;
using Graduation_Project.Core.Specifications;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Graduation_Project.Core.IRepositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        public  Task AddAsync(T entity);
        public  Task<T> AddWithSaveAsync(T entity);
        public void Update(T entity);
        public void Delete(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        public Task<IReadOnlyList<T>?> GetAllWithSpecAsync(ISpecifications<T> specs); /* Task<IReadOnlyList<T> */
        public Task<IReadOnlyList<T>?> GetFirstWithSpecAsync(ISpecifications<T> specs , int number);
        public Task<T?> GetWithSpecsAsync(ISpecifications<T> specs);
        public Task<IReadOnlyList<TResult>> GetAllWithSpecAsync<TResult>(ISpecifications<T> spec, Expression<Func<T, TResult>> selector);
        Task<T?> GetAsync(int id);
        Task<T?> GetWithNameAsync(string name);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<int> GetCountAsync(ISpecifications<T> spec);

        Task<decimal> GetSumAsync(ISpecifications<T> spec, Expression<Func<T, decimal>> selector);
        public void Detach(T entity);
        public Task SaveAsync();
        public void DeleteRange(IEnumerable<T> entities);
        public Task AddRangeAsync(IEnumerable<T> entities);

        public void attaching(T entity);
        public Task<T?> GetByConditionAsync(Expression<Func<T, bool>> expression);
        public Task<IReadOnlyList<T>?> GetManyByConditionAsync(Expression<Func<T, bool>> expression);
    }
}
