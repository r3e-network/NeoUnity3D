using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Represents different types of conditions that can be used in witness rules.
    /// These conditions determine when a witness rule should be applied.
    /// </summary>
    [Serializable]
    public abstract class WitnessCondition : INeoSerializable, IEquatable<WitnessCondition>
    {
        public const int MAX_SUBITEMS = 16;
        public const int MAX_NESTING_DEPTH = 2;

        // Type identifiers for JSON serialization
        public const string BOOLEAN_VALUE = "Boolean";
        public const string NOT_VALUE = "Not";
        public const string AND_VALUE = "And";
        public const string OR_VALUE = "Or";
        public const string SCRIPT_HASH_VALUE = "ScriptHash";
        public const string GROUP_VALUE = "Group";
        public const string CALLED_BY_ENTRY_VALUE = "CalledByEntry";
        public const string CALLED_BY_CONTRACT_VALUE = "CalledByContract";
        public const string CALLED_BY_GROUP_VALUE = "CalledByGroup";

        // Type bytes for binary serialization
        public const byte BOOLEAN_BYTE = 0x00;
        public const byte NOT_BYTE = 0x01;
        public const byte AND_BYTE = 0x02;
        public const byte OR_BYTE = 0x03;
        public const byte SCRIPT_HASH_BYTE = 0x18;
        public const byte GROUP_BYTE = 0x19;
        public const byte CALLED_BY_ENTRY_BYTE = 0x20;
        public const byte CALLED_BY_CONTRACT_BYTE = 0x28;
        public const byte CALLED_BY_GROUP_BYTE = 0x29;

        /// <summary>
        /// Gets the JSON type identifier for this condition.
        /// </summary>
        public abstract string JsonValue { get; }

        /// <summary>
        /// Gets the byte identifier for this condition.
        /// </summary>
        public abstract byte TypeByte { get; }

        /// <summary>
        /// Gets the size of this condition when serialized.
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// Serializes this condition to the specified writer.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        public abstract void Serialize(BinaryWriter writer);

        /// <summary>
        /// Deserializes a witness condition from the specified reader.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <returns>The deserialized witness condition.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the type byte is invalid.</exception>
        public static WitnessCondition Deserialize(BinaryReader reader)
        {
            var typeByte = reader.ReadByte();
            return typeByte switch
            {
                BOOLEAN_BYTE => new BooleanCondition(reader.ReadBoolean()),
                NOT_BYTE => new NotCondition(Deserialize(reader)),
                AND_BYTE => new AndCondition(ReadConditionArray(reader)),
                OR_BYTE => new OrCondition(ReadConditionArray(reader)),
                SCRIPT_HASH_BYTE => new ScriptHashCondition(Hash160.Deserialize(reader)),
                GROUP_BYTE => new GroupCondition(ECPublicKey.Deserialize(reader)),
                CALLED_BY_ENTRY_BYTE => new CalledByEntryCondition(),
                CALLED_BY_CONTRACT_BYTE => new CalledByContractCondition(Hash160.Deserialize(reader)),
                CALLED_BY_GROUP_BYTE => new CalledByGroupCondition(ECPublicKey.Deserialize(reader)),
                _ => throw new InvalidOperationException($"Unknown witness condition type byte: {typeByte}")
            };
        }

        private static WitnessCondition[] ReadConditionArray(BinaryReader reader)
        {
            var count = (int)reader.ReadVarInt();
            var conditions = new WitnessCondition[count];
            for (int i = 0; i < count; i++)
            {
                conditions[i] = Deserialize(reader);
            }
            return conditions;
        }

        public abstract bool Equals(WitnessCondition other);
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }

    /// <summary>
    /// A condition that evaluates to a constant boolean value.
    /// </summary>
    [Serializable]
    public class BooleanCondition : WitnessCondition
    {
        [SerializeField]
        private bool _expression;

        public bool Expression => _expression;
        public override string JsonValue => BOOLEAN_VALUE;
        public override byte TypeByte => BOOLEAN_BYTE;
        public override int Size => 2; // 1 byte type + 1 byte boolean

        public BooleanCondition(bool expression)
        {
            _expression = expression;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            writer.WriteBoolean(_expression);
        }

        public override bool Equals(WitnessCondition other) =>
            other is BooleanCondition bc && bc._expression == _expression;

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _expression);
    }

    /// <summary>
    /// A condition that negates another condition.
    /// </summary>
    [Serializable]
    public class NotCondition : WitnessCondition
    {
        [SerializeField]
        private WitnessCondition _expression;

        public WitnessCondition Expression => _expression;
        public override string JsonValue => NOT_VALUE;
        public override byte TypeByte => NOT_BYTE;
        public override int Size => 1 + _expression.Size;

        public NotCondition(WitnessCondition expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            _expression.Serialize(writer);
        }

        public override bool Equals(WitnessCondition other) =>
            other is NotCondition nc && nc._expression.Equals(_expression);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _expression);
    }

    /// <summary>
    /// A condition that requires all sub-conditions to be true.
    /// </summary>
    [Serializable]
    public class AndCondition : WitnessCondition
    {
        [SerializeField]
        private WitnessCondition[] _expressions;

        public WitnessCondition[] Expressions => _expressions;
        public override string JsonValue => AND_VALUE;
        public override byte TypeByte => AND_BYTE;
        public override int Size => 1 + GetVarSize(_expressions.Length) + _expressions.Sum(e => e.Size);

        public AndCondition(WitnessCondition[] expressions)
        {
            ValidateExpressions(expressions);
            _expressions = expressions;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            writer.WriteVarInt((uint)_expressions.Length);
            foreach (var expr in _expressions)
            {
                expr.Serialize(writer);
            }
        }

        public override bool Equals(WitnessCondition other) =>
            other is AndCondition ac && ac._expressions.SequenceEqual(_expressions);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _expressions);
    }

    /// <summary>
    /// A condition that requires at least one sub-condition to be true.
    /// </summary>
    [Serializable]
    public class OrCondition : WitnessCondition
    {
        [SerializeField]
        private WitnessCondition[] _expressions;

        public WitnessCondition[] Expressions => _expressions;
        public override string JsonValue => OR_VALUE;
        public override byte TypeByte => OR_BYTE;
        public override int Size => 1 + GetVarSize(_expressions.Length) + _expressions.Sum(e => e.Size);

        public OrCondition(WitnessCondition[] expressions)
        {
            ValidateExpressions(expressions);
            _expressions = expressions;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            writer.WriteVarInt((uint)_expressions.Length);
            foreach (var expr in _expressions)
            {
                expr.Serialize(writer);
            }
        }

        public override bool Equals(WitnessCondition other) =>
            other is OrCondition oc && oc._expressions.SequenceEqual(_expressions);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _expressions);
    }

    /// <summary>
    /// A condition that checks if the current contract matches a specific script hash.
    /// </summary>
    [Serializable]
    public class ScriptHashCondition : WitnessCondition
    {
        [SerializeField]
        private Hash160 _hash;

        public Hash160 Hash => _hash;
        public override string JsonValue => SCRIPT_HASH_VALUE;
        public override byte TypeByte => SCRIPT_HASH_BYTE;
        public override int Size => 1 + _hash.Size;

        public ScriptHashCondition(Hash160 hash)
        {
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            _hash.Serialize(writer);
        }

        public override bool Equals(WitnessCondition other) =>
            other is ScriptHashCondition shc && shc._hash.Equals(_hash);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _hash);
    }

    /// <summary>
    /// A condition that checks if the current contract belongs to a specific group.
    /// </summary>
    [Serializable]
    public class GroupCondition : WitnessCondition
    {
        [SerializeField]
        private ECPublicKey _group;

        public ECPublicKey Group => _group;
        public override string JsonValue => GROUP_VALUE;
        public override byte TypeByte => GROUP_BYTE;
        public override int Size => 1 + _group.Size;

        public GroupCondition(ECPublicKey group)
        {
            _group = group ?? throw new ArgumentNullException(nameof(group));
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            _group.Serialize(writer);
        }

        public override bool Equals(WitnessCondition other) =>
            other is GroupCondition gc && gc._group.Equals(_group);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _group);
    }

    /// <summary>
    /// A condition that checks if the current context is called by entry.
    /// </summary>
    [Serializable]
    public class CalledByEntryCondition : WitnessCondition
    {
        public override string JsonValue => CALLED_BY_ENTRY_VALUE;
        public override byte TypeByte => CALLED_BY_ENTRY_BYTE;
        public override int Size => 1;

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
        }

        public override bool Equals(WitnessCondition other) => other is CalledByEntryCondition;

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => TypeByte.GetHashCode();
    }

    /// <summary>
    /// A condition that checks if the current context is called by a specific contract.
    /// </summary>
    [Serializable]
    public class CalledByContractCondition : WitnessCondition
    {
        [SerializeField]
        private Hash160 _hash;

        public Hash160 Hash => _hash;
        public override string JsonValue => CALLED_BY_CONTRACT_VALUE;
        public override byte TypeByte => CALLED_BY_CONTRACT_BYTE;
        public override int Size => 1 + _hash.Size;

        public CalledByContractCondition(Hash160 hash)
        {
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            _hash.Serialize(writer);
        }

        public override bool Equals(WitnessCondition other) =>
            other is CalledByContractCondition cbcc && cbcc._hash.Equals(_hash);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _hash);
    }

    /// <summary>
    /// A condition that checks if the current context is called by a contract from a specific group.
    /// </summary>
    [Serializable]
    public class CalledByGroupCondition : WitnessCondition
    {
        [SerializeField]
        private ECPublicKey _group;

        public ECPublicKey Group => _group;
        public override string JsonValue => CALLED_BY_GROUP_VALUE;
        public override byte TypeByte => CALLED_BY_GROUP_BYTE;
        public override int Size => 1 + _group.Size;

        public CalledByGroupCondition(ECPublicKey group)
        {
            _group = group ?? throw new ArgumentNullException(nameof(group));
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(TypeByte);
            _group.Serialize(writer);
        }

        public override bool Equals(WitnessCondition other) =>
            other is CalledByGroupCondition cbgc && cbgc._group.Equals(_group);

        public override bool Equals(object obj) => Equals(obj as WitnessCondition);

        public override int GetHashCode() => HashCode.Combine(TypeByte, _group);
    }

    /// <summary>
    /// Utility methods for witness conditions.
    /// </summary>
    public static class WitnessConditionExtensions
    {
        private static void ValidateExpressions(WitnessCondition[] expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));
            if (expressions.Length == 0)
                throw new ArgumentException("Expressions array cannot be empty", nameof(expressions));
            if (expressions.Length > WitnessCondition.MAX_SUBITEMS)
                throw new ArgumentException($"Too many sub-expressions. Maximum allowed: {WitnessCondition.MAX_SUBITEMS}", nameof(expressions));
        }

        private static int GetVarSize(int value)
        {
            if (value < 0xFD) return 1;
            else if (value <= 0xFFFF) return 3;
            else if (value <= 0xFFFFFFFF) return 5;
            else return 9;
        }
    }
}