using DataProvider;
using DataProvider.Entities;
using Microsoft.EntityFrameworkCore;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class ProductService : BaseService,IProductService
    {
        public ProductService(MyDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// 购买商品
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> Buy(int saleId)
        {
            Sale saleInfo = await _dbContext.Sales.FirstOrDefaultAsync(x=> x.Id == saleId && x.IsFinished == false);
            if (saleInfo == null) 
            {
                return false;
            }
            if (saleInfo.SaleCount > 0)
            {
                saleInfo.SaleCount--;
                if (saleInfo.SaleCount == 0) 
                {
                    saleInfo.IsFinished = true;
                }
                _dbContext.Sales.Update(saleInfo);

                return await _dbContext.SaveChangesAsync() > 0;
            }
            else 
            {
                return false;
            }
        }
    }

    public interface IProductService 
    {
        ValueTask<bool> Buy(int saleId);
    }
}
