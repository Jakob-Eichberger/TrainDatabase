using Model;
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
