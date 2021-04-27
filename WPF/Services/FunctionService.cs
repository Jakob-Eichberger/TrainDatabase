using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Application.Infrastructure;

namespace WPF_Application.Services
{
   public class FunctionService : BaseService
    {

        public FunctionService(Database db) : base(db)
        {
        }

        public void Update(Function Func)
        {
            base.Update<Function>(Func);
        }

    }
}
