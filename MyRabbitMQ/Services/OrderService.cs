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
    public class OrderService : BaseService,IOrderService
    {


        public OrderService(MyDbContext dbContext):base(dbContext)
        {
            
        }

        public async ValueTask<bool> CreateOrderAsync(Order order)
        {
            await _dbContext.Orders.AddAsync(order);

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<Order> FindUserOrderAsync(string userId, int pId = -1)
        {
            var order = _dbContext.Orders.Where(f=>f.UserId == userId);
            if (pId != -1)
            {
                return await order.FirstOrDefaultAsync(x => x.PId == pId);
            }
            else 
            {
                return await order.FirstOrDefaultAsync();
            }
        }

        public async Task<Order> FindUserSaleOrderAsync(string userId, int pId,int sId)
        {
            var order = _dbContext.Orders.Where(f => f.UserId == userId);
            return await order.FirstOrDefaultAsync(f=>f.PId == pId && f.SId == sId);
        }
    }

    public interface IOrderService 
    {
        Task<Order> FindUserSaleOrderAsync(string userId, int pId,int sId);
        Task<Order> FindUserOrderAsync(string userId,int pId = -1);
        ValueTask<bool> CreateOrderAsync(Order order);

    }
}
