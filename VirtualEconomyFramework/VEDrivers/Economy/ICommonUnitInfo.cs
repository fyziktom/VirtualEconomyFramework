using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Economy
{
    public interface ICommonUnitInfo
    {
        string Creator { get; set; }
        string RepositoryURL { get; set; }
        string ProjectWebsite { get; set; }
    }
}
