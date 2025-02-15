﻿using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Abstract
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
        void Commit();
        Task<IDbContextTransaction>BeginTransactionAsync();
    }
}
