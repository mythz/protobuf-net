﻿using System;
using System.Collections.Generic;
using ProtoBuf.Internal.Serializers;
using ProtoBuf.Serializers;

namespace ProtoBuf.Meta
{
    /// <summary>
    /// Represents an inherited type in a type hierarchy.
    /// </summary>
    public sealed class SubType
    {
        internal sealed class Comparer : System.Collections.IComparer, IComparer<SubType>
        {
            public static readonly Comparer Default = new Comparer();

            public int Compare(object x, object y)
            {
                return Compare(x as SubType, y as SubType);
            }

            public int Compare(SubType x, SubType y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return x.FieldNumber.CompareTo(y.FieldNumber);
            }
        }

        private readonly int fieldNumber;

        /// <summary>
        /// The field-number that is used to encapsulate the data (as a nested
        /// message) for the derived dype.
        /// </summary>
        public int FieldNumber => fieldNumber;

        /// <summary>
        /// The sub-type to be considered.
        /// </summary>
        public MetaType DerivedType => derivedType;
        private readonly MetaType derivedType;

        /// <summary>
        /// Creates a new SubType instance.
        /// </summary>
        /// <param name="fieldNumber">The field-number that is used to encapsulate the data (as a nested
        /// message) for the derived dype.</param>
        /// <param name="derivedType">The sub-type to be considered.</param>
        /// <param name="format">Specific encoding style to use; in particular, Grouped can be used to avoid buffering, but is not the default.</param>
        public SubType(int fieldNumber, MetaType derivedType, DataFormat format)
        {
            if (fieldNumber <= 0) throw new ArgumentOutOfRangeException(nameof(fieldNumber));
            this.fieldNumber = fieldNumber;
            this.derivedType = derivedType ?? throw new ArgumentNullException(nameof(derivedType));
            this.dataFormat = format;
        }

        private readonly DataFormat dataFormat;

        private IRuntimeProtoSerializerNode serializer;

        internal IRuntimeProtoSerializerNode GetSerializer(Type parentType) => serializer ?? (serializer = BuildSerializer(parentType));

        private IRuntimeProtoSerializerNode BuildSerializer(Type parentType)
        {
            // note the caller here is MetaType.BuildSerializer, which already has the sync-lock
            WireType wireType = WireType.String;
            if(dataFormat == DataFormat.Group) wireType = WireType.StartGroup; // only one exception
            
            IRuntimeProtoSerializerNode ser = SubItemSerializer.Create(derivedType.Type, derivedType, parentType);
            return new TagDecorator(fieldNumber, wireType, false, ser);
        }
    }
}