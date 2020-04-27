using DataProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class BaseService
    {
        protected readonly MyDbContext _dbContext;
        public BaseService(MyDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
    }
}
