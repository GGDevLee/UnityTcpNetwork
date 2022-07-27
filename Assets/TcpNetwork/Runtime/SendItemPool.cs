using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeeFramework.Tcp
{
    public class SendItemPool
    {
        private Stack<SendItem> _Pool;
        //最大对象个数，<=0表示不限个数
        protected int _MaxCount = 0;
        //没有回收的对象个数
        protected int _NoRecycleCount = 0;

        public SendItemPool(int count)
        { 
            _Pool = new Stack<SendItem>(count);
        }


        public SendItem Spawn(bool isCreate = true)
        {
            if (_Pool.Count > 0)
            {
                SendItem rtn = _Pool.Pop();
                if (rtn == null)
                {
                    if (isCreate)
                    {
                        rtn = new SendItem();
                    }
                }

                _NoRecycleCount++;
                return rtn;
            }
            else
            {
                if (isCreate)
                {
                    SendItem rtn = new SendItem();
                    _NoRecycleCount++;
                    return rtn;
                }
            }
            return null;
        }

        public bool Recycle(SendItem item)
        {
            if (item == null)
            {
                return false;
            }

            _NoRecycleCount--;

            if (_Pool.Count >= _MaxCount && _MaxCount > 0)
            {
                item = null;
                return false;
            }

            item.Dispose();
            _Pool.Push(item);
            return true;
        }

        public void Dispose()
        {
            _Pool.Clear();
        }

    }

}