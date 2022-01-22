using System;
using System.Collections.Generic;
using System.Text;

namespace ASL.Functions.Controllers
{
    public static class FunctionControllerFactory
    {
        public static IFunctionController GetController(FunctionControllerTypes type)
        {
            switch (type)
            {
                case FunctionControllerTypes.Main:
                    return new MainController();
            }
            return null;
        }
    }
}
