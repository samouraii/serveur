using System;
using System.IO;
using System.Collections;

namespace DifferenceEngine
{
	public class DiffList_BinaryFile : IDiffList
	{
		private byte[] _byteList;



		public DiffList_BinaryFile(string fileName)
		{
			FileStream fs = null;
			BinaryReader br = null;
			try
			{
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				int len = (int)fs.Length;
				br = new BinaryReader(fs);
                _byteList = br.ReadBytes(len);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (br != null) br.Close();
				if (fs != null) fs.Close();
			}

		}
        public DiffList_BinaryFile(byte[] table)
        {
            this._byteList = table;

        }

        public byte[] ByteList
        {
            get
            {
                return _byteList;
            }

        }
        #region IDiffList Members

        public int Count()
		{
			return ByteList.Length;
		}

		public IComparable GetByIndex(int index)
		{
			return ByteList[index];
		}

		#endregion
	}
}