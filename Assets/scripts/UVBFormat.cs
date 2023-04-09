using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// Custom class to represent the data structure
public class UVBMAPLine
{
    public int x, y, z, index;
}

class UVBFormat
{
    public static List<UVBMAPLine> GetUVBMAPLineData(string fileName)
    {
        List<UVBMAPLine> dataList = new List<UVBMAPLine>();

        using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                UVBMAPLine data = new UVBMAPLine();

                // Read x value
                data.x = ReadVariableLengthInt(reader);

                // Read y value
                data.y = ReadVariableLengthInt(reader);

                // Read z value
                data.z = ReadVariableLengthInt(reader);

                // Read index value
                data.index = ReadVariableLengthInt(reader);

                dataList.Add(data);
            }
        }
        return dataList;
    }

    private static void WriteVariableLengthInt(BinaryWriter writer, int value)
    {
        uint unsignedValue = (uint)value;

        while (unsignedValue >= 0x80)
        {
            writer.Write((byte)(unsignedValue | 0x80));
            unsignedValue >>= 7;
        }

        writer.Write((byte)unsignedValue);
    }

    private static int ReadVariableLengthInt(BinaryReader reader)
    {
        int result = 0;
        int shift = 0;
        byte b;

        do
        {
            b = reader.ReadByte();
            result |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return result;
    }

    public static void SaveUVB(List<UVBMAPLine> dataList, string fileName)
    {    
        using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        {
            foreach (UVBMAPLine data in dataList)
            {
                // Write x value
                WriteVariableLengthInt(writer, data.x);

                // Write y value
                WriteVariableLengthInt(writer, data.y);

                // Write z value
                WriteVariableLengthInt(writer, data.z);

                // Write index value
                WriteVariableLengthInt(writer, data.index);
            }
    }
    }
}

  