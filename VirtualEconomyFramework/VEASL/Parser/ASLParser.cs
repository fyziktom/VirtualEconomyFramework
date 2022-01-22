using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ASL.Variables;
using VEDriversLite.NFT;
using System.Linq;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.NFT.DevicesNFTs;
using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json.Linq;
using VEDriversLite;
using ASL.Common;
using ASL.Functions;

namespace ASL.Parser
{
    public class ASLParser
    {
        public ASLProgram ParseCode(string code)
        {
            var codelines = code.Split(new[] { '\r', '\n' });

            var program = new ASLProgram();

            try
            {
                Console.WriteLine("Parsin the program...");
                var programSettings = ParseConstants(codelines);
                if (programSettings != null)
                    program.ProgramSettingsConsts = programSettings;

                var resultObject = ParseResultObject(codelines);
                if (resultObject != null)
                    program.RequestedRawVariables = resultObject;

                var functionBlocks = ParseProgram(codelines);
                if (functionBlocks != null)
                    program.FunctionBlocks = functionBlocks;

                Console.WriteLine("Program Loaded.");

                Console.WriteLine("------Program Constants------");
                Console.WriteLine($"\tAddress: {programSettings.address}");
                Console.WriteLine($"\tNetwork: {programSettings.network}");
                Console.WriteLine($"\tJust From Receiver: {programSettings.just_from_receiver}");
                Console.WriteLine($"\tJust Tokens: {programSettings.just_tokens}");
                Console.WriteLine($"\tJust NFT Type: {programSettings.nft_type}");
                Console.WriteLine($"\tStart Provide: {programSettings.start_provide}");
                Console.WriteLine($"\tDuration: {programSettings.duration}");
                Console.WriteLine("------Result Variables------");
                foreach (var result in resultObject)
                    Console.WriteLine($"\t{result.Name} - {result.VarType.Name}");
                Console.WriteLine("------Functions------");
                foreach (var fnc in functionBlocks)
                {
                    Console.WriteLine($"\tLocation: {fnc.Location}");
                    if (fnc.OneLine)
                        Console.WriteLine("\tThis is one Line function");
                    foreach (var f in fnc.Functions)
                    {
                        Console.WriteLine($"\tFuntion: {Enum.GetName(typeof(Opcodes), f.OpCode)}");
                        foreach (var p in f.Paramseters)
                            Console.WriteLine($"\t\tParameter: {p}");
                    }
                }

                program.IsLoaded = true;
                return program;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot parse the program. " + ex.Message);
                return null;
            }
        }
        public List<IFunctionBlock> ParseProgram(string[] code)
        {
            var resultblocks = new List<IFunctionBlock>();
            var startReading = false;
            var resultObjectFound = false;
            var loadingBlock = false;
            IFunctionBlock fncblock = null;

            for (int i = 0; i < code.Length; i++)
            {
                var line = code[i].ToLower();
                if (line.Contains(Keywords.return_result)) break;
                if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
                {
                    if (!startReading)
                    {
                        if (line.Contains(Keywords.result + " ") && line[0] == 'r' && line[1] == 'e' && (line.Contains(Keywords.one) || line.Contains(Keywords.dict)))
                            resultObjectFound = true;
                        if (resultObjectFound && line.Contains("}"))
                            startReading = true;
                    }
                    else
                    {
                        var split = line.Split(new[] { ' ', '(' });
                        if (split.Length >= 2)
                        {
                            if (split.Length >= 4 && split[0].Contains(Keywords.from_loop))
                            {
                                fncblock = new LoopFunctionBlock()
                                {
                                    Location = i,
                                    OneLine = false,
                                };

                                if (split[1].Contains("0"))
                                    (fncblock as LoopFunctionBlock).SetLoopStart(split[1], "");
                                else  
                                    (fncblock as LoopFunctionBlock).SetLoopStart(split[1], split[2]);

                                loadingBlock = true;
                                if (split[1].Contains("0"))
                                {     
                                    if (split[2].Contains(Keywords.to_loop) || split[3].Contains(Keywords.to_loop))
                                    {
                                        if (split[3].Contains("0"))
                                            (fncblock as LoopFunctionBlock).SetLoopEnd(split[3], "");
                                        else if (!split[3].Contains("0") && split.Length >= 5)
                                            (fncblock as LoopFunctionBlock).SetLoopEnd(split[3], split[4]);
                                        fncblock.EndLocation = i;
                                        resultblocks.Add(fncblock);
                                        loadingBlock = false;
                                    }
                                }
                                else
                                {
                                    if (split[3].Contains(Keywords.to_loop) || split[4].Contains(Keywords.to_loop))
                                    {
                                        if (split[4].Contains("0"))
                                            (fncblock as LoopFunctionBlock).SetLoopEnd(split[4], "");
                                        else if (!split[4].Contains("0") && split.Length >= 5)
                                            (fncblock as LoopFunctionBlock).SetLoopEnd(split[4], split[5]);
                                        fncblock.EndLocation = i;
                                        resultblocks.Add(fncblock);
                                        loadingBlock = false;
                                    }
                                }

                            }
                            else if (loadingBlock)
                            {
                                if (split[0].Contains(Keywords.end_loop))
                                {
                                    fncblock.EndLocation = i;
                                    resultblocks.Add(fncblock);
                                    loadingBlock = false;
                                }
                                else
                                {
                                    var fn = GetFunctionOpCode(line);
                                    if (fn.Item2 != null)
                                    {
                                        fncblock.Functions.Add(new Function()
                                        {
                                            OpCode = fn.Item1,
                                            Paramseters = fn.Item2,
                                            Location = i
                                        });
                                    }
                                }
                            }
                            else if (!loadingBlock)
                            {
                                fncblock = new FunctionBlock()
                                {
                                    Location = i,
                                    OneLine = true,
                                };
                                var fn = GetFunctionOpCode(line);
                                if (fn.Item2 != null)
                                {
                                    fncblock.Functions.Add(new Function()
                                    {
                                        OpCode = fn.Item1,
                                        Paramseters = fn.Item2,
                                        Location = i
                                    });
                                    resultblocks.Add(fncblock);
                                }
                            }
                        }
                    }
                }
            }

            return resultblocks;
        }

        public (Opcodes,string[]) GetFunctionOpCode(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var split = line.Split(new[] { ' ', '(', ')' });
                if (split.Length >= 2)
                {
                    var parameters = split[1].Split(',');

                    if (split[0].Contains(Keywords.sortlist_1to9))
                        return (Opcodes.sortlist_1to9, parameters);
                    if (split[0].Contains(Keywords.sortlist_9to1))
                        return (Opcodes.sortlist_9to1, parameters);
                    if (split[0].Contains(Keywords.multiply_all))
                        return (Opcodes.multiply_all, parameters);
                    if (split[0].Contains(Keywords.add_all))
                        return (Opcodes.add_all, parameters);
                    if (split[0].Contains(Keywords.sub_all))
                        return (Opcodes.sub_all, parameters);
                    if (split[0].Contains(Keywords.replace_all))
                        return (Opcodes.replace_all, parameters);
                    if (split[0].Contains(Keywords.trim_all))
                        return (Opcodes.trim_all, parameters);
                }
            }
            return (Opcodes.none, null);
        }
        
        /// <summary>
        /// Parse constants - the settings of the program
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ProgramSettingsConstants ParseConstants(string[] code)
        {
            var ps = new ProgramSettingsConstants();

            for (int i = 0; i < code.Length; i++)
            {
                var line = code[i];
                if (line.Contains(Keywords.result + " ") && line.Contains(Keywords.return_result)) break;
                if (line.Contains(" "))
                {
                    var split = line.Split(' ');
                    if (split.Length == 2)
                    {
                        if (split[0].Contains(Keywords.address))
                            ps.address = split[1].Trim(' ');
                        if (split[0].Contains(Keywords.network))
                            ps.network = split[1].Trim(' ');
                        if (split[0].Contains(Keywords.just_from_receiver))
                            ps.just_from_receiver = split[1].Trim(' ');
                        if (split[0].Contains(Keywords.just_tokens))
                            ps.just_tokens = split[1].Trim(' ');
                        if (split[0].Contains(Keywords.nft_types))
                            ps.nft_type = split[1].Trim(' ');
                        if (split[0].Contains(Keywords.start_provide))
                            ps.start_provide = split[1].Trim(' ');
                        if (split[0].Contains(Keywords.duration))
                            ps.duration = split[1].Trim(' ');
                    }
                }
            }
            return ps;
        }

        /// <summary>
        /// Parse result Object
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<OVariable> ParseResultObject(string[] code)
        {
            var resultlines = new Dictionary<string,string>();
            var resultType = ResultType.Dict;
            var startReading = false;
            for (int i = 0; i < code.Length; i++)
            {
                var line = code[i].ToLower();
                if (startReading)
                {
                    if (!line.Contains("}"))
                    {
                        var varline = line.Replace("\t", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        var varlinesplit = varline.Split(':');
                        if (varlinesplit.Length >= 2)
                            resultlines.Add(varlinesplit[0], varlinesplit[1]);
                    }
                    else
                    {
                        startReading = false;
                        break;
                    }
                }
                if (line.Contains(Keywords.result + " ") && line[0] == 'r' && line[1] == 'e' && (line.Contains(Keywords.one) || line.Contains(Keywords.dict)))
                {
                    if (line.Contains(Keywords.one))
                        resultType = ResultType.One;

                    //TODO create "one" result

                    startReading = true;
                }
            }

            var result = new List<OVariable>();
            // TODO: take from topper layer
            var nft = NFTFactory.GetNFT("", "6d20f7f4d14e1082526769263f6f5c0c6878e8de2cfc1647ef819518a48818e9", 0, 0, true).GetAwaiter().GetResult();
            
            foreach (var line in resultlines)
            {
                var split = line.Value.Split('.');
                if (split != null)
                {
                    if (split.Length >= 2 && split[0].ToLower().Trim(' ') == Keywords.nft)
                    {
                        var res = GetVariableTypeValue(line.Value, nft);
                        if (res.Item1 != null)
                        {
                            var ov = new OVariable(line.Key)
                            {
                                Represents = line.Value,
                                VarType = res.Item1
                            };
                            ov.AddNewValues(new List<object>() { res.Item2 });
                            result.Add(ov);
                        }
                    }
                    else if (split.Length >= 2 && split[0].ToLower().Trim(' ') == Keywords.new_var)
                    {
                        var ov = new OVariable(line.Key)
                        {
                            Represents = line.Value,
                            VarType = typeof(object)
                        };
                        result.Add(ov);
                    }
                    else if (split.Length == 1)
                    {
                        if (line.Key.ToLower() == Keywords.descriptor)
                        {
                            var vartype = typeof(string);
                            var sp = split[0].ToLower().Trim(' ');
                            if (sp == Keywords.time)
                                vartype = typeof(DateTime);
                            else if (sp == Keywords.txid)
                                vartype = typeof(string);
                            else if (sp == Keywords.num)
                                vartype = typeof(int);

                            var ov = new OVariable(line.Key)
                            {
                                Represents = line.Value,
                                VarType = vartype
                            };
                            result.Add(ov);
                        }
                    }
                }
            }

            return result;
        }

        public (Type,object) GetVariableTypeValue(string parsedobservedvarname, INFT nft = null)
        {
            var split = parsedobservedvarname?.Split('.');
            if (split != null && split.Length >= 2)
            {
                if (split[0].Contains(Keywords.nft) && split.Length == 2)
                {
                    var props = typeof(ImageNFT).GetProperties();
                    foreach (var pro in props)
                        if (pro.Name.ToLower() == split[1].Trim(' '))
                            return (pro.PropertyType, pro.GetValue(nft));
                }
                else if (split[0].Contains(Keywords.nft) && split.Length > 2)
                {
                    if (nft != null)
                    {
                        var propertyType = NFTFactory.GetTypeOfNFT(nft);
                        if (propertyType != null)
                        {
                            var props = propertyType.GetProperties();
                            foreach(var prop in props)
                            {
                                if (prop.Name.ToLower() == split[1])
                                {
                                    try
                                    {
                                        var value = prop.GetValue(nft);
                                        if (value != null && value.GetType() == typeof(string))
                                        {
                                            if ((value as string).Contains(split[2]))
                                            {
                                                var obj = JObject.Parse(value as string);
                                                if (obj != null)
                                                {
                                                    switch (split.Length - 2)
                                                    {
                                                        case 1:
                                                            return GetTypeOfParameter(obj[split[2]].ToString());
                                                        case 2:
                                                            return GetTypeOfParameter(obj[split[2]][split[3]].ToString());
                                                        case 3:
                                                            return GetTypeOfParameter(obj[split[2]][split[3]][split[4]].ToString());
                                                        case 4:
                                                            return GetTypeOfParameter(obj[split[2]][split[3]][split[4]][split[5]].ToString());
                                                       case 5:
                                                            return GetTypeOfParameter(obj[split[2]][split[3]][split[4]][split[5]][split[6]].ToString());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine("Property does not contains the json."); 
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (null,null);
        }

        public (Type,object) GetTypeOfParameter(string parameter)
        {
            try
            {
                var dbl = double.Parse(parameter.Replace(',', '.'), CultureInfo.InvariantCulture);
                return (typeof(double),dbl);
            }
            catch(Exception ex)
            {
                Console.WriteLine("it is not double.");
            }
            try
            {
                var intv = int.Parse(parameter);
                return (typeof(int),intv);
            }
            catch (Exception ex)
            {
                Console.WriteLine("it is not int.");
            }
            try
            {
                var date = DateTime.Parse(parameter);
                return (typeof(DateTime),date);
            }
            catch (Exception ex)
            {
                Console.WriteLine("it is not datetime.");
            }

            return (typeof(string),parameter);
        }
    }
}
