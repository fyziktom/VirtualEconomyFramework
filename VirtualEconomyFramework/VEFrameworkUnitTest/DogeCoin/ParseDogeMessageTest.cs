using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using VEDriversLite;
using VEDriversLite.DogeAPI;

namespace VEFrameworkUnitTest.DogeCoin
{
    public class ParseDogeMessageTest
    {
        /// <summary>
        /// Unit test method to verify if system is throwing an error when ParseDogeMessage is null.
        /// </summary>
        [Fact]
        public void ParseDogeMessageTest_Null_Test()
        {
            var data = DogeTransactionHelpers.ParseDogeMessage(null);
            
            //Assert  
            Assert.Equal("No input data provided.", data.Value);
        }

        /// <summary>
        /// Unit test method to verify if system is throwing an error when ParseDogeMessage success property doesnot return success.
        /// </summary>
        [Fact]
        public void ParseDogeMessageTest_NotSuccess_Test()
        {
            var txinfo = new GetTransactionInfoResponse();
            txinfo.Success = "fail";
            var data = DogeTransactionHelpers.ParseDogeMessage(txinfo);

            //Assert  
            Assert.Equal("No input data provided.", data.Value);
        }

        /// <summary>
        /// Unit test method to verify if system is throwing an error when ParseDogeMessage doesnot have outputs.
        /// </summary>
        [Fact]
        public void ParseDogeMessageTest_NoOutputs_Test()
        {
            var txinfo = new GetTransactionInfoResponse();
            txinfo.Success = "success";
            txinfo.Transaction = new TransactionResponseObject();
            var data = DogeTransactionHelpers.ParseDogeMessage(txinfo);

            //Assert  
            Assert.Equal("No outputs in transaction.", data.Value);

            txinfo.Transaction.Vout = new List<Vout>();
            var data1 = DogeTransactionHelpers.ParseDogeMessage(txinfo);

            //Assert  
            Assert.Equal("No outputs in transaction.", data1.Value);
        }

        /// <summary>
        /// Unit test method to verify if system is throwing an error when ParseDogeMessage has empty output.
        /// </summary>
        [Fact]
        public void ParseDogeMessageTest_EmptyOutput_Test()
        {
            var txinfo = new GetTransactionInfoResponse();
            txinfo.Transaction = new TransactionResponseObject();
            txinfo.Success = "success";
            var VoutObject = new Vout()
            {
                Address = "test"
            };
            txinfo.Transaction.Vout = new List<Vout>() { VoutObject };
            
            var data1 = DogeTransactionHelpers.ParseDogeMessage(txinfo);

            //Assert  
            Assert.Null(data1.Value);
            Assert.False(data1.Success);
        }

        /// <summary>
        /// Unit test method to verify if Fee is getting calculated correctly with correct input params.
        /// </summary>
        //[Fact]
        //public void ParseDogeMessageTest_Valid_Test()
        //{
        //    var txinfo = new GetTransactionInfoResponse();
        //    txinfo.Transaction = new TransactionResponseObject();

        //    var VoutObject = new Vout()
        //    {
        //        Address = "test"
        //    };
        //    txinfo.Transaction.Vout = new List<Vout>() { VoutObject };

        //    var data1 = DogeTransactionHelpers.ParseDogeMessage(txinfo);

        //    //Assert  
        //    Assert.NotEqual("", data1.Value);
        //    Assert.True(data1.Success);
        //}
    }
}
