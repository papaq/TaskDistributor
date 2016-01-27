using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDistr
{
    internal class Processor
    {
        public int Index;
        public int LeftTime;
        public List<Unit> QueueList = new List<Unit>();
        //public int 

        public Processor(int id)
        {
            Index = id;
        }
    }
}
