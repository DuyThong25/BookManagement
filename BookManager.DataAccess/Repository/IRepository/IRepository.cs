using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class // Kiểu Genegic phải là class
    {
        // VD: T - Product
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? fillter = null, string? includeProperties = null, Expression<Func<T, object>>? orderByDescending = null);
        T Get(Expression<Func<T, bool>> fillter, string? includeProperties = null);  // VD: listCate.where(x => x.id > 5 );
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        

        // Không cần phương thức update bởi vì khi update thì sẽ có thể có nhiều logic 
        // khác nhau xảy ra bên trong 
    }
}
