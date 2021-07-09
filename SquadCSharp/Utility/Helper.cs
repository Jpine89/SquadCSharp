using System;
using System.Collections.Generic;
using System.Text;

namespace SquadCSharp.Utility
{
    /*
     * Helper Class is to be used in conjuction with the Pattern Class. 
     * The concept of this is to help spread the Methods out to make categorizing easier
     * and to help reduce large pages. 
     * This goes against the original design but will help
     * with documentation between hiatus. 
     */

    class Helper
    {

        public void publicVoid<T>(T dic)
        {
            Console.WriteLine(dic);
        }
    }
}
