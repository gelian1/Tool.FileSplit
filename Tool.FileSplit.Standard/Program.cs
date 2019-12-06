using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool.FileSplit.Standard
{
    class Program
    {
        static void Main(string[] args)
        {
            Sectcut.GetInstance.FileSplitStart();

            Console.WriteLine("请按任意键结束本程序");
            Console.ReadKey();
        }
    }
}
