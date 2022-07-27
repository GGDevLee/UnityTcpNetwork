using System;

namespace LeeFramework.Tcp
{
    public class SendItem: IDisposable
    {
        public byte[] buff = null;

        public SendItem()
        { 
            
        }

        public SendItem(byte[] data)
        {
            buff = data;
        }

        public void Dispose()
        {
            buff = null;
        }
    }

}