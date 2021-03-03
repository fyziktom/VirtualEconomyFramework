using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Security
{
    [Flags]
    public enum Rights
    {
        Administration = 1,
        LimitedAdministration = 2,
        EconomySetup = 4,
        Dashboards = 8,
        Common = 16,
        Analytics = 32,
        Simulation = 64,
        Debugging = 128,
        IssueTracker = 256,

        RoleAdministrator = Administration,
        RoleDevelopper = Administration | Debugging | Analytics | Simulation | IssueTracker,
        RoleTrader = Dashboards | Common | Analytics | LimitedAdministration | Simulation | IssueTracker,
        RoleVisitor = Dashboards
    }

}
