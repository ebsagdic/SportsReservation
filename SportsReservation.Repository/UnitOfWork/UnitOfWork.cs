using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using SportsReservation.Core.Abstract;
using SportsReservation.Repository.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        //IDbContextTransaction nesnesi transaction başlatılmadan oluşturulamaz.
        //Sen her işlemde yeni bir transaction oluşturup yönetmelisin.
        //Transaction’ı constructor içinde DI(Dependency Injection) ile almadık, çünkü her işlemde yeni bir transaction başlatıyoruz.

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
            //Eğer transaction başlatırsan, o süreçte yapılan tüm işlemler ya hep ya hiç mantığıyla çalışır.
            //Eğer işlem başarılı olursa CommitTransactionAsync() ile kaydedilir, başarısız olursa RollBackTransactionAsync() ile geri alınır.
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
