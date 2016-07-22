using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var test = new Test();
            test.test();
        }
    }
    class Test
    {
        public event EventHandler TestEnvent;
        public void test()
        {
            TestEnvent += Test_TestEnvent;
            TestEnvent -= Test_TestEnvent;
        }

        private void Test_TestEnvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
