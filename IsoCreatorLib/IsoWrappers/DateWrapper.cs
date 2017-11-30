using ISO9660.PrimitiveTypes;
using System;
using System.IO;

namespace IsoCreatorLib.IsoWrappers
{
    internal class DateWrapper
    {
        #region Fields

        private BinaryDateRecord m_binaryDateRecord;
        private AsciiDateRecord m_asciiDateRecord;
        private DateTime m_date;

        #endregion Fields

        #region Properties

        public BinaryDateRecord BinaryDateRecord
        {
            get => m_binaryDateRecord;
            set => m_binaryDateRecord = value;
        }

        public AsciiDateRecord AsciiDateRecord
        {
            get => m_asciiDateRecord;
            set => m_asciiDateRecord = value;
        }

        public DateTime Date
        {
            get => m_date;
            set
            {
                m_date = value;
                SetAsciiDateRecord(value);
                SetBinaryDateRecord(value);
            }
        }

        public sbyte TimeZone
        {
            get => m_asciiDateRecord.TimeZone;
            set => m_asciiDateRecord.TimeZone = value;
        }

        #endregion Properties

        #region Constructor(s)

        public DateWrapper(DateTime date) => Date = date;

        public DateWrapper(DateTime date, sbyte timeZone)
        {
            m_date = date;
            SetAsciiDateRecord(date, timeZone);
            SetBinaryDateRecord(date);
        }

        public DateWrapper(BinaryDateRecord dateRecord)
        {
            m_binaryDateRecord = dateRecord;
            SetAsciiDateRecord(1900 + dateRecord.Year, dateRecord.Month, dateRecord.DayOfMonth, dateRecord.Hour, dateRecord.Minute, dateRecord.Second, 0, 8);
            m_date = new DateTime(1900 + dateRecord.Year, dateRecord.Month, dateRecord.DayOfMonth, dateRecord.Hour, dateRecord.Minute, dateRecord.Second);
        }

        public DateWrapper(AsciiDateRecord dateRecord)
        {
            m_asciiDateRecord = dateRecord;

            byte year = (byte)(Convert.ToInt32(IsoAlgorithm.ByteArrayToString(dateRecord.Year)) - 1900);
            byte month = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord.Month));
            byte dayOfMonth = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord.DayOfMonth));
            byte hour = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord.Hour));
            byte minute = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord.Minute));
            byte second = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord.Second));
            int millisecond = Convert.ToInt32(IsoAlgorithm.ByteArrayToString(dateRecord.HundredthsOfSecond)) * 10;

            SetBinaryDateRecord(year, month, dayOfMonth, hour, minute, second);
            m_date = new DateTime(1900 + year, month, dayOfMonth, hour, minute, second, millisecond);
        }

        #endregion Constructor(s)

        #region Set Methods

        private void SetBinaryDateRecord(byte year, byte month, byte dayOfMonth, byte hour, byte minute, byte second)
        {
            if (m_binaryDateRecord == null) m_binaryDateRecord = new BinaryDateRecord();

            m_binaryDateRecord.Year = year;
            m_binaryDateRecord.Month = month;
            m_binaryDateRecord.DayOfMonth = dayOfMonth;
            m_binaryDateRecord.Hour = hour;
            m_binaryDateRecord.Minute = minute;
            m_binaryDateRecord.Second = second;
        }

        private void SetBinaryDateRecord(DateTime date) => SetBinaryDateRecord(
                (byte)(date.Year - 1900),
                (byte)date.Month,
                (byte)date.Day,
                (byte)date.Hour,
                (byte)date.Minute,
                (byte)date.Second
            );

        private void SetAsciiDateRecord(int year, int month, int dayOfMonth, int hour, int minute, int second, int hundredthsOfSecond, sbyte timeZone)
        {
            if (m_asciiDateRecord == null) m_asciiDateRecord = new AsciiDateRecord();

            string sYear = String.Format("{0:D4}", year % 10000);
            string sMonth = String.Format("{0:D2}", month);
            string sDay = String.Format("{0:D2}", dayOfMonth);
            string sHour = String.Format("{0:D2}", hour);
            string sMinute = String.Format("{0:D2}", minute);
            string sSecond = String.Format("{0:D2}", second);
            string sHundredths = String.Format("{0:D2}", hundredthsOfSecond);

            m_asciiDateRecord.Year = IsoAlgorithm.StringToByteArray(sYear);
            m_asciiDateRecord.Month = IsoAlgorithm.StringToByteArray(sMonth);
            m_asciiDateRecord.DayOfMonth = IsoAlgorithm.StringToByteArray(sDay);
            m_asciiDateRecord.Hour = IsoAlgorithm.StringToByteArray(sHour);
            m_asciiDateRecord.Minute = IsoAlgorithm.StringToByteArray(sMinute);
            m_asciiDateRecord.Second = IsoAlgorithm.StringToByteArray(sSecond);
            m_asciiDateRecord.HundredthsOfSecond = IsoAlgorithm.StringToByteArray(sHundredths);
            m_asciiDateRecord.TimeZone = timeZone;
        }

        private void SetAsciiDateRecord(DateTime date, sbyte timeZone)
            => SetAsciiDateRecord(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond / 10, timeZone);

        private void SetAsciiDateRecord(DateTime date) => SetAsciiDateRecord(date, 8);

        public void ResetAsciiDateRecord()
        {
            m_date = new DateTime(0, 0, 0, 0, 0, 0, 0);
            SetAsciiDateRecord(m_date);
            SetBinaryDateRecord(m_date);
        }

        #endregion Set Methods

        #region I/O Methods

        public int WriteBinaryDateRecord(BinaryWriter writer)
        {
            if (m_binaryDateRecord == null) return 0;

            writer.Write(new byte[6] {
                    m_binaryDateRecord.Year,
                    m_binaryDateRecord.Month,
                    m_binaryDateRecord.DayOfMonth,
                    m_binaryDateRecord.Hour,
                    m_binaryDateRecord.Minute,
                    m_binaryDateRecord.Second
                });

            return 6;
        }

        public int WriteAsciiDateRecord(BinaryWriter writer)
        {
            if (m_asciiDateRecord == null) return 0;

            writer.Write(m_asciiDateRecord.Year);
            writer.Write(m_asciiDateRecord.Month);
            writer.Write(m_asciiDateRecord.DayOfMonth);
            writer.Write(m_asciiDateRecord.Hour);
            writer.Write(m_asciiDateRecord.Minute);
            writer.Write(m_asciiDateRecord.Second);
            writer.Write(m_asciiDateRecord.HundredthsOfSecond);
            writer.Write(m_asciiDateRecord.TimeZone);

            return 17;
        }

        #endregion I/O Methods
    }
}