using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Abstract
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression);

        Task CreateAsync(TEntity entity);
        TEntity Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
