﻿using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Volo.Abp.AuditLogging.EntityFrameworkCore
{
    public class EfCoreAuditLogRepository : EfCoreRepository<IAuditLoggingDbContext, AuditLog, Guid>, IAuditLogRepository
    {
        public EfCoreAuditLogRepository(IDbContextProvider<IAuditLoggingDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }

        public async Task<Paging.PagedResult<AuditLog>> GetListAsync(
            string sorting = null,
            int maxResultCount = 50,
            int skipCount = 0,
            string filter = null,
            string httpMethod = null,
            string url = null,
            HttpStatusCode? httpStatusCode = null,
            bool includeDetails = false)
        {
            var query = DbSet.AsNoTracking()
                .IncludeDetails(includeDetails)
                .WhereIf(httpMethod != null, q => q.HttpMethod != null && q.HttpMethod.ToLowerInvariant() == httpMethod.ToLowerInvariant())
                .WhereIf(url != null, q => q.Url != null && q.Url.ToLowerInvariant().Contains(url.ToLowerInvariant()))
                .WhereIf(httpStatusCode != null && httpStatusCode > 0, q => q.HttpStatusCode == (int?)httpStatusCode);

            var totalCount = await query.LongCountAsync();

            var auditLogs = await query.OrderBy(sorting)
                .PageBy(skipCount, maxResultCount)
                .ToListAsync();

            return new Paging.PagedResult<AuditLog>(totalCount, auditLogs);
        }

        public override IQueryable<AuditLog> WithDetails()
        {
            return GetQueryable().IncludeDetails();
        }
    }
}
