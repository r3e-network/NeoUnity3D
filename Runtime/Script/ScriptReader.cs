using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NeoUnity.Exceptions;
using NeoUnity.Serialization;
using NeoUnity.Utils;

namespace Neo.Unity.SDK.Script
{
    /// <summary>
    /// Reads and analyzes Neo VM scripts, converting bytecode to human-readable representations.
    /// Provides utilities for script inspection, debugging, and validation.
    /// </summary>
    public static class ScriptReader
    {
        #region Script Analysis
        
        /// <summary>
        /// Gets the InteropService that matches the provided hash.
        /// </summary>
        /// <param name="hash">The 4-byte hash of the InteropService</param>
        /// <returns>The matching InteropService, or null if not found</returns>
        public static InteropService? GetInteropServiceByHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return null;
            
            return InteropServiceExtensions.GetServiceByHash(hash);
        }
        
        /// <summary>
        /// Gets the InteropService that matches the provided hash bytes.
        /// </summary>
        /// <param name="hashBytes">The 4-byte hash of the InteropService</param>
        /// <returns>The matching InteropService, or null if not found</returns>
        public static InteropService? GetInteropServiceByHash(byte[] hashBytes)
        {
            return InteropServiceExtensions.GetServiceByHash(hashBytes);
        }
        
        #endregion
        
        #region Script Conversion
        
        /// <summary>
        /// Converts a Neo VM script from hex string to human-readable OpCode representation.
        /// </summary>
        /// <param name="hexScript">The script in hexadecimal format</param>
        /// <returns>The OpCode representation of the script</returns>
        public static string ConvertToOpCodeString(string hexScript)
        {
            if (string.IsNullOrEmpty(hexScript))
                return string.Empty;
            
            try
            {
                var scriptBytes = hexScript.HexStringToByteArray();
                return ConvertToOpCodeString(scriptBytes);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"ScriptReader: Error converting hex script: {ex.Message}", LogLevel.Error);
                return $"Error: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Converts a Neo VM script to human-readable OpCode representation.
        /// </summary>
        /// <param name="script">The script bytes to convert</param>
        /// <returns>The OpCode representation of the script</returns>
        public static string ConvertToOpCodeString(byte[] script)
        {
            if (script == null || script.Length == 0)
                return string.Empty;
            
            try
            {
                var reader = new BinaryReader(script);
                var result = new StringBuilder();
                var position = 0;
                
                while (position < script.Length)
                {
                    try
                    {
                        var opCodeByte = reader.ReadByte();
                        var opCode = (OpCode)opCodeByte;
                        position++;
                        
                        // Add position prefix for debugging
                        result.Append($"{position - 1:X4}: ");
                        
                        // Check if this is a valid OpCode
                        if (!Enum.IsDefined(typeof(OpCode), opCode))
                        {
                            result.AppendLine($"UNKNOWN_OPCODE_0x{opCodeByte:X2}");
                            continue;
                        }
                        
                        result.Append($"{opCode}");
                        
                        // Handle operands based on OpCode type
                        var operandInfo = opCode.GetOperandSize();
                        if (operandInfo.HasValue)
                        {
                            var operand = operandInfo.Value;
                            
                            if (operand.Size > 0)
                            {
                                // Fixed-size operand
                                if (position + operand.Size <= script.Length)
                                {
                                    var operandBytes = reader.ReadBytes(operand.Size);
                                    position += operand.Size;
                                    result.Append($" {operandBytes.ToHexString()}");
                                    
                                    // Add interpretation for common cases
                                    if (opCode == OpCode.SYSCALL && operand.Size == 4)
                                    {
                                        var service = GetInteropServiceByHash(operandBytes);
                                        if (service.HasValue)
                                        {
                                            result.Append($" ({service.Value.GetServiceName()})");
                                        }
                                    }
                                }
                                else
                                {
                                    result.Append(" <INCOMPLETE_OPERAND>");
                                    break;
                                }
                            }
                            else if (operand.PrefixSize > 0)
                            {
                                // Variable-size operand with length prefix
                                if (position + operand.PrefixSize <= script.Length)
                                {
                                    var dataLength = ReadOperandLength(reader, operand.PrefixSize);
                                    position += operand.PrefixSize;
                                    
                                    result.Append($" {dataLength}");
                                    
                                    if (position + dataLength <= script.Length)
                                    {
                                        var operandData = reader.ReadBytes(dataLength);
                                        position += dataLength;
                                        result.Append($" {operandData.ToHexString()}");
                                        
                                        // Add string interpretation for readable data
                                        if (dataLength <= 256 && IsReadableString(operandData))
                                        {
                                            var str = Encoding.UTF8.GetString(operandData);
                                            result.Append($" \"{str}\"");
                                        }
                                    }
                                    else
                                    {
                                        result.Append(" <INCOMPLETE_DATA>");
                                        break;
                                    }
                                }
                                else
                                {
                                    result.Append(" <INCOMPLETE_LENGTH>");
                                    break;
                                }
                            }
                        }
                        
                        // Add constant value interpretation for push opcodes
                        if (opCode.PushesConstantInteger())
                        {
                            result.Append($" ({opCode.GetConstantInteger()})");
                        }
                        
                        result.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        result.AppendLine($"<ERROR_AT_POSITION_{position:X4}: {ex.Message}>");
                        break;
                    }
                }
                
                Extensions.SafeLog($"ScriptReader: Converted script of {script.Length} bytes to OpCode representation");
                return result.ToString();
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"ScriptReader: Error converting script: {ex.Message}", LogLevel.Error);
                return $"Error: {ex.Message}";
            }
        }
        
        #endregion
        
        #region Async Operations
        
        /// <summary>
        /// Asynchronously converts a large script to OpCode representation to avoid blocking Unity's main thread.
        /// </summary>
        /// <param name="script">The script bytes to convert</param>
        /// <returns>Task that returns the OpCode representation</returns>
        public static async Task<string> ConvertToOpCodeStringAsync(byte[] script)
        {
            if (script == null || script.Length == 0)
                return string.Empty;
            
            // For large scripts, process on a background thread
            if (script.Length > 10000)
            {
                return await Task.Run(() => ConvertToOpCodeString(script));
            }
            
            // For small scripts, process synchronously
            return ConvertToOpCodeString(script);
        }
        
        /// <summary>
        /// Asynchronously analyzes a script and provides detailed information about its structure.
        /// </summary>
        /// <param name="script">The script bytes to analyze</param>
        /// <returns>Task that returns script analysis information</returns>
        public static async Task<ScriptAnalysis> AnalyzeScriptAsync(byte[] script)
        {
            if (script == null || script.Length == 0)
                return new ScriptAnalysis { IsEmpty = true };
            
            return await Task.Run(() => AnalyzeScript(script));
        }
        
        #endregion
        
        #region Script Analysis
        
        /// <summary>
        /// Analyzes a script and provides detailed information about its structure and operations.
        /// </summary>
        /// <param name="script">The script bytes to analyze</param>
        /// <returns>Detailed script analysis</returns>
        public static ScriptAnalysis AnalyzeScript(byte[] script)
        {
            var analysis = new ScriptAnalysis
            {
                ScriptLength = script?.Length ?? 0,
                IsEmpty = script == null || script.Length == 0
            };
            
            if (analysis.IsEmpty)
                return analysis;
            
            try
            {
                var reader = new BinaryReader(script);
                var position = 0;
                var instructions = new List<ScriptInstruction>();
                var systemCalls = new List<InteropService>();
                var dataSegments = new List<DataSegment>();
                
                while (position < script.Length)
                {
                    var instructionStart = position;
                    var opCodeByte = reader.ReadByte();
                    position++;
                    
                    if (!Enum.IsDefined(typeof(OpCode), opCodeByte))
                    {
                        analysis.HasInvalidOpCodes = true;
                        continue;
                    }
                    
                    var opCode = (OpCode)opCodeByte;
                    var instruction = new ScriptInstruction
                    {
                        Position = instructionStart,
                        OpCode = opCode,
                        OpCodeByte = opCodeByte
                    };
                    
                    // Handle operands
                    var operandInfo = opCode.GetOperandSize();
                    if (operandInfo.HasValue)
                    {
                        var operand = operandInfo.Value;
                        
                        if (operand.Size > 0)
                        {
                            if (position + operand.Size <= script.Length)
                            {
                                instruction.Operand = reader.ReadBytes(operand.Size);
                                position += operand.Size;
                                
                                // Track system calls
                                if (opCode == OpCode.SYSCALL && operand.Size == 4)
                                {
                                    var service = GetInteropServiceByHash(instruction.Operand);
                                    if (service.HasValue)
                                    {
                                        systemCalls.Add(service.Value);
                                        instruction.SystemCall = service.Value;
                                    }
                                }
                            }
                            else
                            {
                                analysis.HasIncompleteInstructions = true;
                                break;
                            }
                        }
                        else if (operand.PrefixSize > 0)
                        {
                            if (position + operand.PrefixSize <= script.Length)
                            {
                                var dataLength = ReadOperandLength(reader, operand.PrefixSize);
                                position += operand.PrefixSize;
                                
                                if (position + dataLength <= script.Length)
                                {
                                    var data = reader.ReadBytes(dataLength);
                                    position += dataLength;
                                    
                                    instruction.Operand = data;
                                    
                                    // Track data segments for analysis
                                    dataSegments.Add(new DataSegment
                                    {
                                        Position = instructionStart,
                                        Length = dataLength,
                                        Data = data,
                                        OpCode = opCode
                                    });
                                }
                                else
                                {
                                    analysis.HasIncompleteInstructions = true;
                                    break;
                                }
                            }
                            else
                            {
                                analysis.HasIncompleteInstructions = true;
                                break;
                            }
                        }
                    }
                    
                    instructions.Add(instruction);
                    
                    // Update analysis statistics
                    analysis.InstructionCount++;
                    analysis.EstimatedGasCost += opCode.GetPrice();
                    
                    // Track OpCode usage
                    if (!analysis.OpCodeUsage.ContainsKey(opCode))
                        analysis.OpCodeUsage[opCode] = 0;
                    analysis.OpCodeUsage[opCode]++;
                }
                
                analysis.Instructions = instructions.ToArray();
                analysis.SystemCalls = systemCalls.ToArray();
                analysis.DataSegments = dataSegments.ToArray();
                analysis.HasSystemCalls = systemCalls.Count > 0;
                analysis.DataSegmentCount = dataSegments.Count;
                
                // Calculate complexity score
                analysis.ComplexityScore = CalculateComplexityScore(analysis);
                
                Extensions.SafeLog($"ScriptReader: Analyzed script - {analysis.InstructionCount} instructions, " +
                                 $"{analysis.SystemCalls.Length} syscalls, estimated {analysis.EstimatedGasCost} GAS");
                
                return analysis;
            }
            catch (Exception ex)
            {
                analysis.AnalysisError = ex.Message;
                Extensions.SafeLog($"ScriptReader: Error analyzing script: {ex.Message}", LogLevel.Error);
                return analysis;
            }
        }
        
        #endregion
        
        #region Private Helpers
        
        /// <summary>
        /// Reads operand length from the binary reader based on prefix size.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <param name="prefixSize">Size of the length prefix</param>
        /// <returns>The operand length</returns>
        private static int ReadOperandLength(BinaryReader reader, int prefixSize)
        {
            return prefixSize switch
            {
                1 => reader.ReadByte(),
                2 => reader.ReadUInt16(),
                4 => (int)reader.ReadUInt32(),
                _ => throw new IllegalArgumentException($"Unsupported operand prefix size: {prefixSize}")
            };
        }
        
        /// <summary>
        /// Checks if a byte array contains readable UTF-8 string data.
        /// </summary>
        /// <param name="data">The byte data to check</param>
        /// <returns>True if the data appears to be a readable string</returns>
        private static bool IsReadableString(byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;
            
            try
            {
                var str = Encoding.UTF8.GetString(data);
                
                // Check if string contains only printable characters
                foreach (char c in str)
                {
                    if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                        return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Calculates a complexity score for the script based on various factors.
        /// </summary>
        /// <param name="analysis">The script analysis data</param>
        /// <returns>A complexity score from 0.0 to 1.0</returns>
        private static float CalculateComplexityScore(ScriptAnalysis analysis)
        {
            if (analysis.IsEmpty)
                return 0.0f;
            
            float score = 0.0f;
            
            // Instruction count factor (normalized)
            score += Math.Min(analysis.InstructionCount / 1000.0f, 0.3f);
            
            // System call variety factor
            score += Math.Min(analysis.SystemCalls.Length / 10.0f, 0.2f);
            
            // OpCode variety factor
            score += Math.Min(analysis.OpCodeUsage.Count / 50.0f, 0.2f);
            
            // GAS cost factor (normalized)
            score += Math.Min(analysis.EstimatedGasCost / 1000000.0f, 0.2f);
            
            // Error penalty
            if (analysis.HasInvalidOpCodes || analysis.HasIncompleteInstructions)
                score += 0.1f;
            
            return Math.Min(score, 1.0f);
        }
        
        #endregion
    }
    
    #region Analysis Data Structures
    
    /// <summary>
    /// Represents the results of script analysis.
    /// </summary>
    public class ScriptAnalysis
    {
        /// <summary>Gets or sets whether the script is empty.</summary>
        public bool IsEmpty { get; set; }
        
        /// <summary>Gets or sets the total script length in bytes.</summary>
        public int ScriptLength { get; set; }
        
        /// <summary>Gets or sets the number of instructions in the script.</summary>
        public int InstructionCount { get; set; }
        
        /// <summary>Gets or sets the estimated GAS cost for execution.</summary>
        public long EstimatedGasCost { get; set; }
        
        /// <summary>Gets or sets whether the script contains system calls.</summary>
        public bool HasSystemCalls { get; set; }
        
        /// <summary>Gets or sets whether the script contains invalid opcodes.</summary>
        public bool HasInvalidOpCodes { get; set; }
        
        /// <summary>Gets or sets whether the script has incomplete instructions.</summary>
        public bool HasIncompleteInstructions { get; set; }
        
        /// <summary>Gets or sets the number of data segments in the script.</summary>
        public int DataSegmentCount { get; set; }
        
        /// <summary>Gets or sets the complexity score (0.0 to 1.0).</summary>
        public float ComplexityScore { get; set; }
        
        /// <summary>Gets or sets any analysis error message.</summary>
        public string AnalysisError { get; set; }
        
        /// <summary>Gets or sets the parsed instructions.</summary>
        public ScriptInstruction[] Instructions { get; set; } = Array.Empty<ScriptInstruction>();
        
        /// <summary>Gets or sets the system calls found in the script.</summary>
        public InteropService[] SystemCalls { get; set; } = Array.Empty<InteropService>();
        
        /// <summary>Gets or sets the data segments found in the script.</summary>
        public DataSegment[] DataSegments { get; set; } = Array.Empty<DataSegment>();
        
        /// <summary>Gets or sets the OpCode usage statistics.</summary>
        public Dictionary<OpCode, int> OpCodeUsage { get; set; } = new Dictionary<OpCode, int>();
        
        /// <summary>
        /// Returns a summary string of the analysis.
        /// </summary>
        public override string ToString()
        {
            if (IsEmpty)
                return "Empty Script";
            
            var summary = new StringBuilder();
            summary.AppendLine($"Script Analysis Summary:");
            summary.AppendLine($"  Length: {ScriptLength} bytes");
            summary.AppendLine($"  Instructions: {InstructionCount}");
            summary.AppendLine($"  Estimated GAS: {EstimatedGasCost}");
            summary.AppendLine($"  System Calls: {SystemCalls.Length}");
            summary.AppendLine($"  Data Segments: {DataSegmentCount}");
            summary.AppendLine($"  Complexity: {ComplexityScore:F2}");
            
            if (!string.IsNullOrEmpty(AnalysisError))
                summary.AppendLine($"  Error: {AnalysisError}");
            
            return summary.ToString();
        }
    }
    
    /// <summary>
    /// Represents a single instruction in a Neo VM script.
    /// </summary>
    public class ScriptInstruction
    {
        /// <summary>Gets or sets the position in the script.</summary>
        public int Position { get; set; }
        
        /// <summary>Gets or sets the OpCode.</summary>
        public OpCode OpCode { get; set; }
        
        /// <summary>Gets or sets the raw opcode byte.</summary>
        public byte OpCodeByte { get; set; }
        
        /// <summary>Gets or sets the operand data (if any).</summary>
        public byte[] Operand { get; set; }
        
        /// <summary>Gets or sets the system call (if this is a SYSCALL instruction).</summary>
        public InteropService? SystemCall { get; set; }
        
        /// <summary>
        /// Returns a string representation of the instruction.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder($"{Position:X4}: {OpCode}");
            
            if (Operand != null && Operand.Length > 0)
            {
                result.Append($" {Operand.ToHexString()}");
            }
            
            if (SystemCall.HasValue)
            {
                result.Append($" ({SystemCall.Value.GetServiceName()})");
            }
            
            return result.ToString();
        }
    }
    
    /// <summary>
    /// Represents a data segment within a script.
    /// </summary>
    public class DataSegment
    {
        /// <summary>Gets or sets the position in the script.</summary>
        public int Position { get; set; }
        
        /// <summary>Gets or sets the length of the data.</summary>
        public int Length { get; set; }
        
        /// <summary>Gets or sets the data bytes.</summary>
        public byte[] Data { get; set; }
        
        /// <summary>Gets or sets the OpCode that introduced this data.</summary>
        public OpCode OpCode { get; set; }
        
        /// <summary>
        /// Gets the data as a hex string.
        /// </summary>
        public string AsHex => Data?.ToHexString() ?? string.Empty;
        
        /// <summary>
        /// Gets the data as a UTF-8 string if possible.
        /// </summary>
        public string AsString
        {
            get
            {
                if (Data == null) return string.Empty;
                try
                {
                    return Encoding.UTF8.GetString(Data);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Returns a string representation of the data segment.
        /// </summary>
        public override string ToString()
        {
            return $"DataSegment at {Position:X4}: {Length} bytes ({OpCode})";
        }
    }
    
    #endregion
}