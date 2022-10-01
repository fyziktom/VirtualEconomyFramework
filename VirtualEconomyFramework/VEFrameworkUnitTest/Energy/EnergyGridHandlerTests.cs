using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Energy.Handlers;
using Xunit;

namespace VEFrameworkUnitTest.Energy
{
    public class EnergyGridHandlerTests
    {
        private EnergyBlocksTestHelpers ebth = new EnergyBlocksTestHelpers();

        [Fact]
        public void ExportImportSettings()
        {
            var eGrid = ebth.GetTestEnergyGridHandler();
            var exp = eGrid.ExportToConfig();
            var eGridClone = new EnergyGridHandler();
            if (exp.Item1)
            {
                eGridClone.LoadFromConfig(exp.Item2);
                Assert.Equal(eGrid.Entities.Count, eGridClone.Entities.Count);
                var expclone = eGrid.ExportToConfig();
                Assert.Equal(exp, expclone);
            }
        }
    }
}
