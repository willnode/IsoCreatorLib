namespace ISO9660.Enums
{
    internal enum Endian
    {
        LittleEndian,
        BigEndian
    }

    /// <summary>
	/// Describes the volume descriptor type; the numbers are set according to the ISO 9660 standard.
	/// </summary>
	internal enum VolumeType
    {
        /// <summary>
        /// Number 0: shall mean that the Volume Descriptor is a Boot Record
        /// (never used in this program... maybe in future distributions).
        /// </summary>
        BootRecord = 0,

        /// <summary>
        /// Number 1: shall mean that the Volume Descriptor is a Primary Volume Descriptor
        /// </summary>
        Primary = 1,

        /// <summary>
        /// Number 2: shall mean that the Volume Descriptor is a Supplementary Volume Descriptor
        /// </summary>
        Suplementary = 2,

        /// <summary>
        /// Number 3: shall mean that the Volume Descriptor is a Volume Partition Descriptor
        /// </summary>
        Partition = 3,

        /// <summary>
        ///  Number 255: shall mean that the Volume Descriptor is a Volume Descriptor Set Terminator.
        /// </summary>
        SetTerminator = 255
    };
}