using System;

namespace Neo.Unity.SDK.Script
{
    /// <summary>
    /// Represents Neo VM operation codes (opcodes) for script building and execution.
    /// Each opcode corresponds to a specific instruction that the Neo VM can execute.
    /// </summary>
    public enum OpCode : byte
    {
        #region Constants
        
        /// <summary>Push 1-byte integer</summary>
        PUSHINT8 = 0x00,
        /// <summary>Push 2-byte integer</summary>
        PUSHINT16 = 0x01,
        /// <summary>Push 4-byte integer</summary>
        PUSHINT32 = 0x02,
        /// <summary>Push 8-byte integer</summary>
        PUSHINT64 = 0x03,
        /// <summary>Push 16-byte integer</summary>
        PUSHINT128 = 0x04,
        /// <summary>Push 32-byte integer</summary>
        PUSHINT256 = 0x05,
        
        /// <summary>Push address</summary>
        PUSHA = 0x0A,
        /// <summary>Push null</summary>
        PUSHNULL = 0x0B,
        /// <summary>Push data with 1-byte length prefix</summary>
        PUSHDATA1 = 0x0C,
        /// <summary>Push data with 2-byte length prefix</summary>
        PUSHDATA2 = 0x0D,
        /// <summary>Push data with 4-byte length prefix</summary>
        PUSHDATA4 = 0x0E,
        /// <summary>Push -1</summary>
        PUSHM1 = 0x0F,
        
        /// <summary>Push integer 0</summary>
        PUSH0 = 0x10,
        /// <summary>Push integer 1</summary>
        PUSH1 = 0x11,
        /// <summary>Push integer 2</summary>
        PUSH2 = 0x12,
        /// <summary>Push integer 3</summary>
        PUSH3 = 0x13,
        /// <summary>Push integer 4</summary>
        PUSH4 = 0x14,
        /// <summary>Push integer 5</summary>
        PUSH5 = 0x15,
        /// <summary>Push integer 6</summary>
        PUSH6 = 0x16,
        /// <summary>Push integer 7</summary>
        PUSH7 = 0x17,
        /// <summary>Push integer 8</summary>
        PUSH8 = 0x18,
        /// <summary>Push integer 9</summary>
        PUSH9 = 0x19,
        /// <summary>Push integer 10</summary>
        PUSH10 = 0x1A,
        /// <summary>Push integer 11</summary>
        PUSH11 = 0x1B,
        /// <summary>Push integer 12</summary>
        PUSH12 = 0x1C,
        /// <summary>Push integer 13</summary>
        PUSH13 = 0x1D,
        /// <summary>Push integer 14</summary>
        PUSH14 = 0x1E,
        /// <summary>Push integer 15</summary>
        PUSH15 = 0x1F,
        /// <summary>Push integer 16</summary>
        PUSH16 = 0x20,
        
        #endregion
        
        #region Flow Control
        
        /// <summary>No operation</summary>
        NOP = 0x21,
        /// <summary>Jump to target</summary>
        JMP = 0x22,
        /// <summary>Jump to target (long form)</summary>
        JMP_L = 0x23,
        /// <summary>Jump if true</summary>
        JMPIF = 0x24,
        /// <summary>Jump if true (long form)</summary>
        JMPIF_L = 0x25,
        /// <summary>Jump if false</summary>
        JMPIFNOT = 0x26,
        /// <summary>Jump if false (long form)</summary>
        JMPIFNOT_L = 0x27,
        /// <summary>Jump if equal</summary>
        JMPEQ = 0x28,
        /// <summary>Jump if equal (long form)</summary>
        JMPEQ_L = 0x29,
        /// <summary>Jump if not equal</summary>
        JMPNE = 0x2A,
        /// <summary>Jump if not equal (long form)</summary>
        JMPNE_L = 0x2B,
        /// <summary>Jump if greater than</summary>
        JMPGT = 0x2C,
        /// <summary>Jump if greater than (long form)</summary>
        JMPGT_L = 0x2D,
        /// <summary>Jump if greater than or equal</summary>
        JMPGE = 0x2E,
        /// <summary>Jump if greater than or equal (long form)</summary>
        JMPGE_L = 0x2F,
        /// <summary>Jump if less than</summary>
        JMPLT = 0x30,
        /// <summary>Jump if less than (long form)</summary>
        JMPLT_L = 0x31,
        /// <summary>Jump if less than or equal</summary>
        JMPLE = 0x32,
        /// <summary>Jump if less than or equal (long form)</summary>
        JMPLE_L = 0x33,
        
        /// <summary>Call subroutine</summary>
        CALL = 0x34,
        /// <summary>Call subroutine (long form)</summary>
        CALL_L = 0x35,
        /// <summary>Call with address</summary>
        CALLA = 0x36,
        /// <summary>Call with token</summary>
        CALLT = 0x37,
        
        /// <summary>Abort execution</summary>
        ABORT = 0x38,
        /// <summary>Assert condition</summary>
        ASSERT = 0x39,
        /// <summary>Throw exception</summary>
        THROW = 0x3A,
        /// <summary>Try block</summary>
        TRY = 0x3B,
        /// <summary>Try block (long form)</summary>
        TRY_L = 0x3C,
        /// <summary>End try block</summary>
        ENDTRY = 0x3D,
        /// <summary>End try block (long form)</summary>
        ENDTRY_L = 0x3E,
        /// <summary>End finally block</summary>
        ENDFINALLY = 0x3F,
        /// <summary>Return from call</summary>
        RET = 0x40,
        /// <summary>System call</summary>
        SYSCALL = 0x41,
        
        #endregion
        
        #region Stack Operations
        
        /// <summary>Get stack depth</summary>
        DEPTH = 0x43,
        /// <summary>Drop top item</summary>
        DROP = 0x45,
        /// <summary>Remove second item</summary>
        NIP = 0x46,
        /// <summary>Remove item at depth</summary>
        XDROP = 0x48,
        /// <summary>Clear stack</summary>
        CLEAR = 0x49,
        /// <summary>Duplicate top item</summary>
        DUP = 0x4A,
        /// <summary>Copy second item to top</summary>
        OVER = 0x4B,
        /// <summary>Copy item at depth to top</summary>
        PICK = 0x4D,
        /// <summary>Copy top item under second</summary>
        TUCK = 0x4E,
        /// <summary>Swap top two items</summary>
        SWAP = 0x50,
        /// <summary>Rotate top three items</summary>
        ROT = 0x51,
        /// <summary>Move item at depth to top</summary>
        ROLL = 0x52,
        /// <summary>Reverse top three items</summary>
        REVERSE3 = 0x53,
        /// <summary>Reverse top four items</summary>
        REVERSE4 = 0x54,
        /// <summary>Reverse top N items</summary>
        REVERSEN = 0x55,
        
        #endregion
        
        #region Slot Operations
        
        /// <summary>Initialize static slots</summary>
        INITSSLOT = 0x56,
        /// <summary>Initialize slots</summary>
        INITSLOT = 0x57,
        
        /// <summary>Load static field 0</summary>
        LDSFLD0 = 0x58,
        /// <summary>Load static field 1</summary>
        LDSFLD1 = 0x59,
        /// <summary>Load static field 2</summary>
        LDSFLD2 = 0x5A,
        /// <summary>Load static field 3</summary>
        LDSFLD3 = 0x5B,
        /// <summary>Load static field 4</summary>
        LDSFLD4 = 0x5C,
        /// <summary>Load static field 5</summary>
        LDSFLD5 = 0x5D,
        /// <summary>Load static field 6</summary>
        LDSFLD6 = 0x5E,
        /// <summary>Load static field</summary>
        LDSFLD = 0x5F,
        
        /// <summary>Store static field 0</summary>
        STSFLD0 = 0x60,
        /// <summary>Store static field 1</summary>
        STSFLD1 = 0x61,
        /// <summary>Store static field 2</summary>
        STSFLD2 = 0x62,
        /// <summary>Store static field 3</summary>
        STSFLD3 = 0x63,
        /// <summary>Store static field 4</summary>
        STSFLD4 = 0x64,
        /// <summary>Store static field 5</summary>
        STSFLD5 = 0x65,
        /// <summary>Store static field 6</summary>
        STSFLD6 = 0x66,
        /// <summary>Store static field</summary>
        STSFLD = 0x67,
        
        /// <summary>Load local variable 0</summary>
        LDLOC0 = 0x68,
        /// <summary>Load local variable 1</summary>
        LDLOC1 = 0x69,
        /// <summary>Load local variable 2</summary>
        LDLOC2 = 0x6A,
        /// <summary>Load local variable 3</summary>
        LDLOC3 = 0x6B,
        /// <summary>Load local variable 4</summary>
        LDLOC4 = 0x6C,
        /// <summary>Load local variable 5</summary>
        LDLOC5 = 0x6D,
        /// <summary>Load local variable 6</summary>
        LDLOC6 = 0x6E,
        /// <summary>Load local variable</summary>
        LDLOC = 0x6F,
        
        /// <summary>Store local variable 0</summary>
        STLOC0 = 0x70,
        /// <summary>Store local variable 1</summary>
        STLOC1 = 0x71,
        /// <summary>Store local variable 2</summary>
        STLOC2 = 0x72,
        /// <summary>Store local variable 3</summary>
        STLOC3 = 0x73,
        /// <summary>Store local variable 4</summary>
        STLOC4 = 0x74,
        /// <summary>Store local variable 5</summary>
        STLOC5 = 0x75,
        /// <summary>Store local variable 6</summary>
        STLOC6 = 0x76,
        /// <summary>Store local variable</summary>
        STLOC = 0x77,
        
        /// <summary>Load argument 0</summary>
        LDARG0 = 0x78,
        /// <summary>Load argument 1</summary>
        LDARG1 = 0x79,
        /// <summary>Load argument 2</summary>
        LDARG2 = 0x7A,
        /// <summary>Load argument 3</summary>
        LDARG3 = 0x7B,
        /// <summary>Load argument 4</summary>
        LDARG4 = 0x7C,
        /// <summary>Load argument 5</summary>
        LDARG5 = 0x7D,
        /// <summary>Load argument 6</summary>
        LDARG6 = 0x7E,
        /// <summary>Load argument</summary>
        LDARG = 0x7F,
        
        /// <summary>Store argument 0</summary>
        STARG0 = 0x80,
        /// <summary>Store argument 1</summary>
        STARG1 = 0x81,
        /// <summary>Store argument 2</summary>
        STARG2 = 0x82,
        /// <summary>Store argument 3</summary>
        STARG3 = 0x83,
        /// <summary>Store argument 4</summary>
        STARG4 = 0x84,
        /// <summary>Store argument 5</summary>
        STARG5 = 0x85,
        /// <summary>Store argument 6</summary>
        STARG6 = 0x86,
        /// <summary>Store argument</summary>
        STARG = 0x87,
        
        #endregion
        
        #region Splice Operations
        
        /// <summary>Create new buffer</summary>
        NEWBUFFER = 0x88,
        /// <summary>Memory copy</summary>
        MEMCPY = 0x89,
        /// <summary>Concatenate</summary>
        CAT = 0x8B,
        /// <summary>Substring</summary>
        SUBSTR = 0x8C,
        /// <summary>Left substring</summary>
        LEFT = 0x8D,
        /// <summary>Right substring</summary>
        RIGHT = 0x8E,
        
        #endregion
        
        #region Bitwise Logic
        
        /// <summary>Bitwise invert</summary>
        INVERT = 0x90,
        /// <summary>Bitwise AND</summary>
        AND = 0x91,
        /// <summary>Bitwise OR</summary>
        OR = 0x92,
        /// <summary>Bitwise XOR</summary>
        XOR = 0x93,
        /// <summary>Equal comparison</summary>
        EQUAL = 0x97,
        /// <summary>Not equal comparison</summary>
        NOTEQUAL = 0x98,
        
        #endregion
        
        #region Arithmetic
        
        /// <summary>Get sign of number</summary>
        SIGN = 0x99,
        /// <summary>Absolute value</summary>
        ABS = 0x9A,
        /// <summary>Negate number</summary>
        NEGATE = 0x9B,
        /// <summary>Increment</summary>
        INC = 0x9C,
        /// <summary>Decrement</summary>
        DEC = 0x9D,
        /// <summary>Addition</summary>
        ADD = 0x9E,
        /// <summary>Subtraction</summary>
        SUB = 0x9F,
        /// <summary>Multiplication</summary>
        MUL = 0xA0,
        /// <summary>Division</summary>
        DIV = 0xA1,
        /// <summary>Modulo</summary>
        MOD = 0xA2,
        /// <summary>Power</summary>
        POW = 0xA3,
        /// <summary>Square root</summary>
        SQRT = 0xA4,
        /// <summary>Modular multiplication</summary>
        MODMUL = 0xA5,
        /// <summary>Modular power</summary>
        MODPOW = 0xA6,
        /// <summary>Shift left</summary>
        SHL = 0xA8,
        /// <summary>Shift right</summary>
        SHR = 0xA9,
        /// <summary>Logical NOT</summary>
        NOT = 0xAA,
        /// <summary>Boolean AND</summary>
        BOOLAND = 0xAB,
        /// <summary>Boolean OR</summary>
        BOOLOR = 0xAC,
        /// <summary>Not zero</summary>
        NZ = 0xB1,
        /// <summary>Numeric equal</summary>
        NUMEQUAL = 0xB3,
        /// <summary>Numeric not equal</summary>
        NUMNOTEQUAL = 0xB4,
        /// <summary>Less than</summary>
        LT = 0xB5,
        /// <summary>Less than or equal</summary>
        LE = 0xB6,
        /// <summary>Greater than</summary>
        GT = 0xB7,
        /// <summary>Greater than or equal</summary>
        GE = 0xB8,
        /// <summary>Minimum</summary>
        MIN = 0xB9,
        /// <summary>Maximum</summary>
        MAX = 0xBA,
        /// <summary>Within range</summary>
        WITHIN = 0xBB,
        
        #endregion
        
        #region Compound Types
        
        /// <summary>Pack map</summary>
        PACKMAP = 0xBE,
        /// <summary>Pack struct</summary>
        PACKSTRUCT = 0xBF,
        /// <summary>Pack array</summary>
        PACK = 0xC0,
        /// <summary>Unpack array</summary>
        UNPACK = 0xC1,
        /// <summary>Create new array (size 0)</summary>
        NEWARRAY0 = 0xC2,
        /// <summary>Create new array</summary>
        NEWARRAY = 0xC3,
        /// <summary>Create new array with type</summary>
        NEWARRAY_T = 0xC4,
        /// <summary>Create new struct (size 0)</summary>
        NEWSTRUCT0 = 0xC5,
        /// <summary>Create new struct</summary>
        NEWSTRUCT = 0xC6,
        /// <summary>Create new map</summary>
        NEWMAP = 0xC8,
        /// <summary>Get size</summary>
        SIZE = 0xCA,
        /// <summary>Check if key exists</summary>
        HASKEY = 0xCB,
        /// <summary>Get keys</summary>
        KEYS = 0xCC,
        /// <summary>Get values</summary>
        VALUES = 0xCD,
        /// <summary>Pick item at index</summary>
        PICKITEM = 0xCE,
        /// <summary>Append item</summary>
        APPEND = 0xCF,
        /// <summary>Set item at index</summary>
        SETITEM = 0xD0,
        /// <summary>Reverse items</summary>
        REVERSEITEMS = 0xD1,
        /// <summary>Remove item</summary>
        REMOVE = 0xD2,
        /// <summary>Clear items</summary>
        CLEARITEMS = 0xD3,
        
        #endregion
        
        #region Type Operations
        
        /// <summary>Check if null</summary>
        ISNULL = 0xD8,
        /// <summary>Check type</summary>
        ISTYPE = 0xD9,
        /// <summary>Convert type</summary>
        CONVERT = 0xDB,
        
        #endregion
    }
    
    /// <summary>
    /// Extension methods for OpCode operations.
    /// </summary>
    public static class OpCodeExtensions
    {
        /// <summary>
        /// Gets the byte value of the opcode.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The byte value</returns>
        public static byte ToByte(this OpCode opCode)
        {
            return (byte)opCode;
        }
        
        /// <summary>
        /// Gets the hex string representation of the opcode.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The hex string</returns>
        public static string ToHexString(this OpCode opCode)
        {
            return ((byte)opCode).ToString("X2");
        }
        
        /// <summary>
        /// Gets the GAS price for executing this opcode.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The GAS price in fractions</returns>
        public static long GetPrice(this OpCode opCode)
        {
            return opCode switch
            {
                // 1 GAS fraction
                OpCode.PUSHINT8 or OpCode.PUSHINT16 or OpCode.PUSHINT32 or OpCode.PUSHINT64 or
                OpCode.PUSHNULL or OpCode.PUSHM1 or OpCode.PUSH0 or OpCode.PUSH1 or OpCode.PUSH2 or
                OpCode.PUSH3 or OpCode.PUSH4 or OpCode.PUSH5 or OpCode.PUSH6 or OpCode.PUSH7 or
                OpCode.PUSH8 or OpCode.PUSH9 or OpCode.PUSH10 or OpCode.PUSH11 or OpCode.PUSH12 or
                OpCode.PUSH13 or OpCode.PUSH14 or OpCode.PUSH15 or OpCode.PUSH16 or OpCode.NOP or
                OpCode.ASSERT => 1,
                
                // 4 GAS fractions (1 << 2)
                OpCode.PUSHINT128 or OpCode.PUSHINT256 or OpCode.PUSHA or OpCode.TRY or
                OpCode.ENDTRY or OpCode.ENDTRY_L or OpCode.ENDFINALLY or OpCode.INVERT or
                OpCode.SIGN or OpCode.ABS or OpCode.NEGATE or OpCode.INC or OpCode.DEC or
                OpCode.NOT or OpCode.NZ or OpCode.SIZE => 1 << 2,
                
                // 8 GAS fractions (1 << 3)
                OpCode.PUSHDATA1 or OpCode.AND or OpCode.OR or OpCode.XOR or OpCode.ADD or
                OpCode.SUB or OpCode.MUL or OpCode.DIV or OpCode.MOD or OpCode.SHL or
                OpCode.SHR or OpCode.BOOLAND or OpCode.BOOLOR or OpCode.NUMEQUAL or
                OpCode.NUMNOTEQUAL or OpCode.LT or OpCode.LE or OpCode.GT or OpCode.GE or
                OpCode.MIN or OpCode.MAX or OpCode.WITHIN or OpCode.NEWMAP => 1 << 3,
                
                // 16 GAS fractions (1 << 4)
                OpCode.XDROP or OpCode.CLEAR or OpCode.ROLL or OpCode.REVERSEN or OpCode.INITSSLOT or
                OpCode.NEWARRAY0 or OpCode.NEWSTRUCT0 or OpCode.KEYS or OpCode.REMOVE or
                OpCode.CLEARITEMS => 1 << 4,
                
                // 32 GAS fractions (1 << 5)
                OpCode.EQUAL or OpCode.NOTEQUAL or OpCode.MODMUL => 1 << 5,
                
                // 64 GAS fractions (1 << 6)
                OpCode.INITSLOT or OpCode.POW or OpCode.HASKEY or OpCode.PICKITEM => 1 << 6,
                
                // 256 GAS fractions (1 << 8)
                OpCode.NEWBUFFER => 1 << 8,
                
                // 512 GAS fractions (1 << 9)
                OpCode.PUSHDATA2 or OpCode.CALL or OpCode.CALL_L or OpCode.CALLA or OpCode.THROW or
                OpCode.NEWARRAY or OpCode.NEWARRAY_T or OpCode.NEWSTRUCT => 1 << 9,
                
                // 2048 GAS fractions (1 << 11)
                OpCode.MEMCPY or OpCode.CAT or OpCode.SUBSTR or OpCode.LEFT or OpCode.RIGHT or
                OpCode.SQRT or OpCode.MODPOW or OpCode.PACKMAP or OpCode.PACKSTRUCT or
                OpCode.PACK or OpCode.UNPACK => 1 << 11,
                
                // 4096 GAS fractions (1 << 12)
                OpCode.PUSHDATA4 => 1 << 12,
                
                // 8192 GAS fractions (1 << 13)
                OpCode.VALUES or OpCode.APPEND or OpCode.SETITEM or OpCode.REVERSEITEMS or
                OpCode.CONVERT => 1 << 13,
                
                // 32768 GAS fractions (1 << 15)
                OpCode.CALLT => 1 << 15,
                
                // Free operations
                OpCode.ABORT or OpCode.RET or OpCode.SYSCALL => 0,
                
                // Default: 2 GAS fractions (1 << 1)
                _ => 1 << 1
            };
        }
        
        /// <summary>
        /// Gets the operand size information for this opcode.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The operand size information or null if no operand</returns>
        public static OperandSize? GetOperandSize(this OpCode opCode)
        {
            return opCode switch
            {
                // 1-byte operand
                OpCode.PUSHINT8 or OpCode.JMP or OpCode.JMPIF or OpCode.JMPIFNOT or OpCode.JMPEQ or
                OpCode.JMPNE or OpCode.JMPGT or OpCode.JMPGE or OpCode.JMPLT or OpCode.JMPLE or
                OpCode.CALL or OpCode.ENDTRY or OpCode.INITSSLOT or OpCode.LDSFLD or OpCode.STSFLD or
                OpCode.LDLOC or OpCode.STLOC or OpCode.LDARG or OpCode.STARG or OpCode.NEWARRAY_T or
                OpCode.ISTYPE or OpCode.CONVERT => new OperandSize(1),
                
                // 2-byte operand
                OpCode.PUSHINT16 or OpCode.CALLT or OpCode.TRY or OpCode.INITSLOT => new OperandSize(2),
                
                // 4-byte operand
                OpCode.PUSHINT32 or OpCode.PUSHA or OpCode.JMP_L or OpCode.JMPIF_L or OpCode.JMPIFNOT_L or
                OpCode.JMPEQ_L or OpCode.JMPNE_L or OpCode.JMPGT_L or OpCode.JMPGE_L or OpCode.JMPLT_L or
                OpCode.JMPLE_L or OpCode.CALL_L or OpCode.ENDTRY_L or OpCode.SYSCALL => new OperandSize(4),
                
                // 8-byte operand
                OpCode.PUSHINT64 or OpCode.TRY_L => new OperandSize(8),
                
                // 16-byte operand
                OpCode.PUSHINT128 => new OperandSize(16),
                
                // 32-byte operand
                OpCode.PUSHINT256 => new OperandSize(32),
                
                // Variable-length operands with prefix
                OpCode.PUSHDATA1 => new OperandSize(0, 1), // 1-byte length prefix
                OpCode.PUSHDATA2 => new OperandSize(0, 2), // 2-byte length prefix
                OpCode.PUSHDATA4 => new OperandSize(0, 4), // 4-byte length prefix
                
                // No operand
                _ => null
            };
        }
        
        /// <summary>
        /// Checks if this opcode pushes a constant integer value.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>True if it pushes a constant integer</returns>
        public static bool PushesConstantInteger(this OpCode opCode)
        {
            return opCode >= OpCode.PUSHM1 && opCode <= OpCode.PUSH16;
        }
        
        /// <summary>
        /// Gets the constant integer value pushed by this opcode.
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The constant integer value</returns>
        /// <exception cref="ArgumentException">If the opcode doesn't push a constant integer</exception>
        public static int GetConstantInteger(this OpCode opCode)
        {
            return opCode switch
            {
                OpCode.PUSHM1 => -1,
                OpCode.PUSH0 => 0,
                OpCode.PUSH1 => 1,
                OpCode.PUSH2 => 2,
                OpCode.PUSH3 => 3,
                OpCode.PUSH4 => 4,
                OpCode.PUSH5 => 5,
                OpCode.PUSH6 => 6,
                OpCode.PUSH7 => 7,
                OpCode.PUSH8 => 8,
                OpCode.PUSH9 => 9,
                OpCode.PUSH10 => 10,
                OpCode.PUSH11 => 11,
                OpCode.PUSH12 => 12,
                OpCode.PUSH13 => 13,
                OpCode.PUSH14 => 14,
                OpCode.PUSH15 => 15,
                OpCode.PUSH16 => 16,
                _ => throw new ArgumentException($"OpCode {opCode} does not push a constant integer")
            };
        }
    }
    
    /// <summary>
    /// Represents the operand size information for an opcode.
    /// </summary>
    public struct OperandSize
    {
        /// <summary>The fixed size of the operand in bytes</summary>
        public int Size { get; }
        
        /// <summary>The size of the length prefix for variable-length operands</summary>
        public int PrefixSize { get; }
        
        /// <summary>Whether this is a variable-length operand</summary>
        public bool IsVariableLength => PrefixSize > 0;
        
        /// <summary>
        /// Creates an operand size with a fixed size.
        /// </summary>
        /// <param name="size">The operand size in bytes</param>
        public OperandSize(int size)
        {
            Size = size;
            PrefixSize = 0;
        }
        
        /// <summary>
        /// Creates an operand size with variable length and prefix.
        /// </summary>
        /// <param name="size">The operand size (0 for variable)</param>
        /// <param name="prefixSize">The length prefix size</param>
        public OperandSize(int size, int prefixSize)
        {
            Size = size;
            PrefixSize = prefixSize;
        }
    }
}