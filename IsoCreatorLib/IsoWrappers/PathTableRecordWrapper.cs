using ISO9660.Enums;
using ISO9660.PrimitiveTypes;
using System;
using System.IO;

namespace IsoCreatorLib.IsoWrappers
{
    internal class PathTableRecordWrapper
    {
        #region Fields

        private PathTableRecord m_record = new PathTableRecord();

        private Endian m_endian = Endian.LittleEndian;

        private VolumeType m_volumeDescriptorType = VolumeType.Primary;

        #endregion Fields

        #region Constructors

        public PathTableRecordWrapper()
        {
        }

        public PathTableRecordWrapper(UInt32 extentLocation, UInt16 parentNumber, string name)
            => SetPathTableRecord(extentLocation, parentNumber, name);

        #endregion Constructors

        #region Properties

        public PathTableRecord Record
        {
            get => m_record;
            set => m_record = value;
        }

        public Endian Endian
        {
            get => m_endian;
            set
            {
                if (value != m_endian)
                {
                    m_record.ExtentLocation = IsoAlgorithm.ChangeEndian(m_record.ExtentLocation);
                    m_record.ParentNumber = IsoAlgorithm.ChangeEndian(m_record.ParentNumber);
                }
                m_endian = value;
            }
        }

        public VolumeType VolumeDescriptorType
        {
            get => m_volumeDescriptorType;
            set
            {
                if (m_record == null || m_record.Identifier.Length == 1 && m_record.Identifier[0] == 0)
                {
                    m_volumeDescriptorType = value;
                    return;
                }
                if (m_volumeDescriptorType != value &&
                    (m_volumeDescriptorType == VolumeType.Suplementary ||
                    value == VolumeType.Suplementary))
                {
                    switch (value)
                    {
                        case VolumeType.Suplementary:

                            m_record.Identifier = IsoAlgorithm.AsciiToUnicode(m_record.Identifier);
                            m_record.Length = (byte)(m_record.Identifier.Length);

                            if (m_record.Identifier.Length > 255)
                            {
                                throw new Exception("Depasire!");
                            }

                            break;

                        default:

                            m_record.Identifier = IsoAlgorithm.UnicodeToAscii(m_record.Identifier);
                            m_record.Length = (byte)(m_record.Identifier.Length);

                            if (m_record.Identifier.Length > 255)
                            {
                                throw new Exception("Depasire!");
                            }

                            break;
                    }
                }
                m_volumeDescriptorType = value;
            }
        }

        public UInt32 ExtentLocation
        {
            get
            {
                if (m_endian == Endian.BigEndian)
                    return IsoAlgorithm.ChangeEndian(m_record.ExtentLocation);
                else
                    return m_record.ExtentLocation;
            }
            set
            {
                if (m_endian == Endian.BigEndian)
                    m_record.ExtentLocation = IsoAlgorithm.ChangeEndian(value);
                else
                    m_record.ExtentLocation = value;
            }
        }

        public UInt16 ParentNumber
        {
            get
            {
                if (m_endian == Endian.BigEndian)
                    return IsoAlgorithm.ChangeEndian(m_record.ParentNumber);
                else
                    return m_record.ParentNumber;
            }
            set
            {
                if (m_endian == Endian.BigEndian)
                    m_record.ParentNumber = IsoAlgorithm.ChangeEndian(value);
                else
                    m_record.ParentNumber = value;
            }
        }

        public string Name
        {
            get => (m_record.Identifier.Length == 1 && m_record.Identifier[0] == 0) ? "." : IsoAlgorithm.ByteArrayToString(m_record.Identifier);
            set
            {
                if (value == ".")
                {
                    m_record.Identifier = new byte[1] { 0 };
                    m_record.Length = 1;
                }
                else
                {
                    m_record.Identifier = IsoAlgorithm.StringToByteArray(value);
                    m_record.Length = (byte)m_record.Identifier.Length;

                    if (m_record.Identifier.Length > 255) throw new Exception("Depasire!");

                    if (m_volumeDescriptorType == VolumeType.Suplementary)
                    {
                        m_volumeDescriptorType = VolumeType.Primary;
                        VolumeDescriptorType = VolumeType.Suplementary;
                    }
                }
            }
        }

        public byte Length => m_record.Length;

        #endregion Properties

        #region Set Methods

        private void SetPathTableRecord(UInt32 extentLocation, UInt16 parentNumber, byte[] identifier)
        {
            if (m_record == null)
                m_record = new PathTableRecord();

            m_record.Length = (byte)identifier.Length;

            if (identifier.Length > 255) throw new Exception("Depasire!");

            m_record.Identifier = identifier;

            m_record.ExtentLocation = extentLocation;
            m_record.ParentNumber = parentNumber;

            if (m_volumeDescriptorType == VolumeType.Suplementary && (identifier.Length > 1 || identifier[0] != 0))
            {
                m_volumeDescriptorType = VolumeType.Primary;
                VolumeDescriptorType = VolumeType.Suplementary;
            }
        }

        public void SetPathTableRecord(UInt32 extentLocation, UInt16 parentNumber, string name)
        {
            if (m_endian == Endian.BigEndian)
            {
                extentLocation = IsoAlgorithm.ChangeEndian(extentLocation);
                parentNumber = IsoAlgorithm.ChangeEndian(parentNumber);
            }

            byte[] identifier = name == "." ? new byte[1] { 0 } : IsoAlgorithm.StringToByteArray(name);

            SetPathTableRecord(extentLocation, parentNumber, identifier);
        }

        #endregion Set Methods

        #region I/O Methods

        public int Write(BinaryWriter writer)
        {
            if (m_record == null)
                m_record = new PathTableRecord
                {
                    Length = 1,
                    Identifier = new byte[1] { 65 }
                };

            writer.Write(m_record.Length);
            writer.Write(m_record.ExtendedLength);
            writer.Write(m_record.ExtentLocation);
            writer.Write(m_record.ParentNumber);
            writer.Write(m_record.Identifier);

            if (m_record.Length % 2 == 1)
                writer.Write((byte)0);

            return 8 + m_record.Length + (m_record.Length % 2);
        }

        #endregion I/O Methods
    }
}