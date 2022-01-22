using System;
using VEDriversLite;
using Xunit;
using Moq;
using System.IO;
using ASL.Parser;

namespace VEFrameworkUnitTest.VEASL
{
    public class ParsingTests
    {      
        public ParsingTests()
        {
            if (!load_asl_test_code())
                throw new Exception("Cannot reead the file in the parse ASL code test.");
        }

        private static string asl_code = string.Empty;
        private static ASLParser Parser = new ASLParser();

        private static bool load_asl_test_code()
        {
            try
            {
                var code = FileHelpers.ReadTextFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VEASL", "Resources", "aslprogram.txt"));
                if (!string.IsNullOrEmpty(code))
                {
                    asl_code = code;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reead the file in the parse ASL code test.");
            }
            return false;
        }

        [Fact]
        public void ParseWholeCodeTest()
        {
            var program = Parser.ParseCode(asl_code);

            var expectedNumOfVars = 5;
            var expectedNumOfFunctions = 3;
            var expectedThirdNameOfObserved = "nft.description.data.sensor.thermometer.temperature";
            if (program.IsLoaded)
            {
                Assert.Equal(expectedNumOfVars, program.RequestedRawVariables.Count);
                Assert.Equal(expectedThirdNameOfObserved, program.RequestedRawVariables[2].Represents);
                Assert.Equal(expectedNumOfFunctions, program.FunctionBlocks.Count);
            }
        }

        private static string[] code_settings_constants = {
             "network neblio"
           , "address NWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY"
           , "just_from_receiver NQFkjSdYWWwPGFE8kQD4toCYkmddXiNh8W"
           , "just_tokens La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8"
           , "nft_types NFTIoTMessages"
           , "start_provide 1"
           , "duration 100h"};

        [Fact]
        public void ParseSettingsConstantsTest()
        {
            var result = Parser.ParseConstants(code_settings_constants);

            Assert.Equal(result.network, "neblio");
            Assert.Equal(result.address, "NWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY");
            Assert.Equal(result.just_from_receiver, "NQFkjSdYWWwPGFE8kQD4toCYkmddXiNh8W");
            Assert.Equal(result.just_tokens, "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8");
            Assert.Equal(result.start_provide, "1");
            Assert.Equal(result.duration, "100h");
            Assert.Equal(result.nft_type, "NFTIoTMessages");
        }

        [Fact]
        public void ParseSettingsConstantsFailsTest()
        {
            var code = code_settings_constants;
            code[0] = "network eblio";
            code[1] = "addressNWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY";
            code[6] = "duration 100 h";
            var result = Parser.ParseConstants(code);

            Assert.NotEqual(result.network, "neblio");
            Assert.NotEqual(result.address, "NWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY");
            Assert.Equal(result.just_from_receiver, "NQFkjSdYWWwPGFE8kQD4toCYkmddXiNh8W");
            Assert.Equal(result.just_tokens, "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8");
            Assert.Equal(result.start_provide, "1");
            Assert.NotEqual(result.duration, "100h");
            //Assert.Equal(result.nft_type, "NFTIoTMessages");
        }

        private static string[] code_result_object = {
             "result dict {"
           , "descriptor:time"
           , "myvariable1:nft.name"
           , "temperature:nft.description.data.sensor.thermometer.temperature"
           , "tempdiff:new.temperaturediff"
        };

        [Fact]
        public void ParseResultObjectTest()
        {
            var result = Parser.ParseResultObject(code_result_object);
            Assert.Equal(result.Count, 4);
            Assert.Equal(result[0].Name, "descriptor");
            Assert.Equal(result[0].Represents, "time");
            Assert.Equal(result[0].VarType, typeof(DateTime));

            Assert.Equal(result[2].Name, "temperature");
            Assert.Equal(result[2].Represents, "nft.description.data.sensor.thermometer.temperature");
            Assert.Equal(result[2].VarType, typeof(double));

            //for this 6d20f7f4d14e1082526769263f6f5c0c6878e8de2cfc1647ef819518a48818e9 NFT now, TODO: connect to NFT ASL parameter: 
            Assert.Equal(result[2].Values[0], 22.5);
        }


        [Fact]
        public void GetFunctionOPCodeTest()
        {
            var result = Parser.GetFunctionOpCode("sortlist_1to9(myvariable1)");
            Assert.Equal(result.Item1, ASL.Functions.Opcodes.sortlist_1to9);
            result = Parser.GetFunctionOpCode("sortlist_19(myvariable1)");
            Assert.Equal(result.Item1, ASL.Functions.Opcodes.none);
        }

        [Fact]
        public void ParseCodeFunctionsTest()
        {
            var result = Parser.ParseProgram(asl_code.Split(new[] { '\n' }));
            Assert.Equal(result.Count, 3);
            Assert.Equal(result[0].OneLine, true);
            Assert.Equal(result[0].Functions[0].OpCode, ASL.Functions.Opcodes.sortlist_1to9);
        }


        [Fact]
        public void GetTypeOfParameterTest()
        {
            var value1 = "20.22";
            var value2 = "veframework";
            var value3 = "2022-01-21T08:54:03Z";

            var rvalue1 = Parser.GetTypeOfParameter(value1);
            if (rvalue1.Item1 != null)
            {
                Assert.Equal(rvalue1.Item1, typeof(double));
                Assert.Equal(rvalue1.Item2, 20.22);
            }
            var rvalue2 = Parser.GetTypeOfParameter(value2);
            if (rvalue2.Item1 != null)
            {
                Assert.Equal(rvalue2.Item1, typeof(string));
                Assert.Equal(rvalue2.Item2, value2);
            }
            var rvalue3 = Parser.GetTypeOfParameter(value3);
            if (rvalue3.Item1 != null)
            {
                Assert.Equal(rvalue3.Item1, typeof(DateTime));
                Assert.Equal(rvalue3.Item2, DateTime.Parse(value3));
            }
        }

        [Fact]
        public void GetTypeOfParameterDoubleWrongTest()
        {
            var value1 = "20..22";

            var rvalue1 = Parser.GetTypeOfParameter(value1);
            if (rvalue1.Item1 != null)
            {
                Assert.Equal(rvalue1.Item1, typeof(string));
                Assert.Equal(rvalue1.Item2, "20..22");
            }
        }
    }
}
