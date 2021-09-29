using System.IO;

namespace ErogeHelper.ShellMenuHandler
{
    public enum PeType
    {
        X32,
        X64,
        Arm,
        Arm64,
        Unknown
    }

    public static class PeFileReader
    {
        public static PeType GetPeType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PeType.Unknown;

            try
            {
                using (var br = new BinaryReader(new FileStream(path,
                                                    FileMode.Open,
                                                    FileAccess.Read,
                                                    FileShare.ReadWrite)))
                {
                    if (br.BaseStream.Length < 0x3C + 4 || br.ReadUInt16() != 0x5A4D)
                        return PeType.Unknown;

                    br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                    var pos = br.ReadUInt32() + 4;

                    if (pos + 2 > br.BaseStream.Length)
                        return PeType.Unknown;

                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                    var machine = br.ReadUInt16();

                    switch (machine)
                    {
                        case 0x014C:
                            return PeType.X32;
                        case 0x8664:
                            return PeType.X64;
                        case 0x01C4:
                            return PeType.Arm;
                        case 0xAA64:
                            return PeType.Arm64;
                        default:
                            return PeType.Unknown;
                    }
                }
            }
            catch
            {
                return PeType.Unknown;
            }
        }
    }
}
