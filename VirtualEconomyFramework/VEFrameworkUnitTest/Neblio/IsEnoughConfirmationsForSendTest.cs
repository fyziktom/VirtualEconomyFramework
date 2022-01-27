﻿using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class IsEnoughConfirmationsForSendTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public void IsEnoughConfirmationsForSend_Valid_Test()
        {
            int confirmations = 10;

            var result = NeblioTransactionHelpers.IsEnoughConfirmationsForSend(confirmations);

            Assert.True(result.Item1);
            Assert.Contains(">", result.Item2);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public void IsEnoughConfirmationsForSend_Error_Test()
        {
            int confirmations = 1;

            var result = NeblioTransactionHelpers.IsEnoughConfirmationsForSend(confirmations);

            Assert.False(result.Item1);
            Assert.Equal(confirmations.ToString(), result.Item2);
        }
    }
}
