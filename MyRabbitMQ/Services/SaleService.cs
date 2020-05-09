using DataProvider;
using DataProvider.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SaleService : BaseService, ISaleService
    {
        public SaleService(MyDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Sale> GetSale(int sId)
        {
            return await _dbContext.Sales.FirstOrDefaultAsync(x=>x.Id == sId);
        }

        public async Task<IEnumerable<Sale>> GetSales()
        {
            return await Task.FromResult( _dbContext.Sales.Where(w=>w.IsFinished == false).ToList());
        }

        public async ValueTask<bool> ReduceSaleAsync(int sId)
        {
            var sale = await _dbContext.Sales.FirstOrDefaultAsync(x=>x.Id == sId);
            if (sale == null)
                return false;
                    
            if (sale.SaleCount > 0)
            {
                sale.SaleCount--;
            }
            if(sale.SaleCount == 0) 
            {
                sale.IsFinished = true;
            }
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
    public interface ISaleService 
    {
        Task<IEnumerable<Sale>> GetSales();
        Task<Sale> GetSale(int sId);
        ValueTask<bool> ReduceSaleAsync(int sId);
    }
}
