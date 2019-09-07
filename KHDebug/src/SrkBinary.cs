using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public class SrkBinary
    {
        public static void MakePrincipal(ref float val)
        {
            val = (float)Math.Atan2(Math.Sin(val), Math.Cos(val));
            /*if (val<0)
            {
                val = -val;
                val = (float)(val % (Math.PI * 2));
                val = -val;
            }
            else
            {
                val = (float)(val % (Math.PI * 2));
            }*/
        }

        public static void ExtractFile(string sourceArchive, string destination)
        {
            string zPath = "7za.exe";
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = zPath,
                    Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination)
                };
                Console.WriteLine("Extracting file " + sourceArchive + "...");
                Process x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (System.Exception Ex)
            {
                //handle error
            }
        }
        public static byte[] lastBuffer = new byte[0];
        public static string lastBufferFilename = "";
        public bool DataEdited;

        public byte[] Buffer
        {
            get;
            set;
        }

        public static void Align16(ref int valeur)
        {
            while (valeur % 16 > 0)
                valeur++;
        }

        public static void AlignBy(ref int valeur, int by)
        {
            while (valeur % by > 0)
                valeur++;
        }

        public static bool GetBytes(string filename)
        {
            FileStream fileStream;
            lastBufferFilename = "";
            try
            {
                fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                lastBufferFilename = filename;
            }
            catch (IOException)
            {
                //Console.WriteLine("Unable to open " + Path.GetFileName(filename) + ".");
                //Console.WriteLine("This file in being used by another process.");
                return false;
            }
            lastBuffer = new byte[fileStream.Length];
            fileStream.Read(lastBuffer, 0, lastBuffer.Length);
            fileStream.Close();
            return lastBuffer.Length > 0;
        }

        public SrkBinary()
        {
            this.Buffer = lastBuffer;
        }

        public SrkBinary(ref byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public Int32 Pointer
        {
            get; set;
        }

        /*public unsafe System.Int16 ReadShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Int16*)(ptr);
        }*/

        public System.Int16 ReadShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            return BitConverter.ToInt16(this.Buffer, (int)address);
        }

        /*public unsafe System.UInt16 ReadUShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.UInt16*)(ptr);
        }*/

        public System.UInt16 ReadUShort(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 2 > this.Buffer.Length)
                return 0;
            return BitConverter.ToUInt16(this.Buffer, (int)address);
        }

        /*public unsafe System.Int32 ReadInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Int32*)(ptr);
        }*/

        public System.Int32 ReadInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            return BitConverter.ToInt32(this.Buffer, (int)address);
        }

        /*public unsafe System.UInt32 ReadUInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.UInt32*)(ptr);
        }*/

        public System.UInt32 ReadUInt(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            return BitConverter.ToUInt32(this.Buffer, (int)address);
        }

        /*public unsafe System.Int64 ReadInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Int64*)(ptr);
        }*/

        public System.Int64 ReadInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            return BitConverter.ToInt64(this.Buffer, (int)address);
        }

        /*public unsafe System.UInt64 ReadUInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.UInt64*)(ptr);
        }*/

        public System.UInt64 ReadUInt64(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 8 > this.Buffer.Length)
                return 0;
            return BitConverter.ToUInt64(this.Buffer, (int)address);
        }

        /*public unsafe System.Single ReadFloat(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            fixed (System.Byte* ptr = &this.Buffer[address])
                return *(System.Single*)(ptr);
        }*/

        public System.Single ReadFloat(System.Int64 address, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + 4 > this.Buffer.Length)
                return 0;
            return BitConverter.ToSingle(this.Buffer, (int)address);
        }

        public System.String ReadASCII(System.Int64 address, System.Int64 length, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            if (address + length > this.Buffer.Length)
                return "";

            for (System.Int64 i = 0; i < length; i++)
                if (this.Buffer[address + i] < 32 || this.Buffer[address + i] > 126)
                {
                    length = i;
                    break;
                }
            System.Byte[] result = new System.Byte[length];
            System.Array.Copy(this.Buffer, address, result, 0, length);
            return Encoding.ASCII.GetString(result);
        }


        public void WriteInt(System.Int64 address, System.Int16 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteInt(System.Int64 address, System.Int32 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteInt(System.Int64 address, System.UInt32 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteInt(System.Int64 address, System.Int64 valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteFloat(System.Int64 address, System.Single valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = BitConverter.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }

        public void WriteString(System.Int64 address, System.String valeur, Boolean pointer)
        {
            if (pointer) address += this.Pointer;
            this.DataEdited = true;
            byte[] donnees = Encoding.ASCII.GetBytes(valeur);
            System.Array.Copy(donnees, 0, this.Buffer, address, donnees.Length);
        }
    }
}

