namespace Shared;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PacketDecodeError : Exception
{
    public PacketDecodeError() { }

    public PacketDecodeError(string message) : base(message) { }

    public PacketDecodeError(string message, Exception inner) : base(message, inner) { }
}

public struct Packet
{
    public int UID;
    public int From;
    public List<byte[]> contents;

    public Packet(int _UID, List<byte[]> _contents, int from = -1)
    {
        UID = _UID;
        contents = _contents;
        From = from;
    }
}

public class PacketMissingAttributeException : Exception
{
    public PacketMissingAttributeException() { }

    public PacketMissingAttributeException(string message) : base(message) { }

    public PacketMissingAttributeException(string message, Exception inner) : base(message, inner)
    { }
}

public static class PacketManager
{
    private static Encoding encoder = new UTF8Encoding();

    public const int PacketLenLen = 4;
    public const int UIDLen = 4;
    public const int DataLenLen = 4;

    /*
    public static string RemoveEOF(string data){
        return data.Substring(0, data.Length - 5);
    }
    */

    public static int GetPacketLength(byte[] bytes)
    {
        return BitConverter.ToInt32(ArrayExtentions.Slice(bytes, 0, PacketLenLen)) + PacketLenLen;
    }

    public static byte[] Build(int UID, List<byte[]> contents, int RID = 0)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int cursor = PacketLenLen;
            ArrayExtentions.Merge(buffer, BitConverter.GetBytes(UID), cursor);
            cursor += UIDLen;

            foreach (byte[] c in contents)
            {
                ArrayExtentions.Merge(buffer, BitConverter.GetBytes(c.Length), cursor);
                cursor += 4;
                ArrayExtentions.Merge(buffer, c, cursor);
                cursor += c.Length;
            }

            // Add packet length
            ArrayExtentions.Merge(buffer, BitConverter.GetBytes(cursor - 4), 0);

            return ArrayExtentions.Slice(buffer, 0, cursor);
        }
        catch (Exception e)
        {
            throw new PacketDecodeError("Error decoding packet: " + e);
        }
    }

    public static byte[] ByteEncode(string input)
    {
        return encoder.GetBytes(input);
    }

    public static Packet Decode(byte[] data, int from = -1)
    {
        int cursor = 4;
        int UID = BitConverter.ToInt32(ArrayExtentions.Slice(data, cursor, cursor + UIDLen));
        cursor += UIDLen;

        List<byte[]> contents = new List<byte[]>();
        while (cursor < data.Length)
        {
            int data_len = BitConverter.ToInt32(
                ArrayExtentions.Slice(data, cursor, cursor + DataLenLen)
            );
            data_len = BitConverter.ToInt32(
                ArrayExtentions.Slice(data, cursor, cursor + DataLenLen)
            );
            cursor += DataLenLen;
            byte[] content = ArrayExtentions.Slice(data, cursor, cursor + data_len);
            cursor += data_len;
            contents.Add(content);
        }

        return new Packet(UID, contents, from);
    }
}
