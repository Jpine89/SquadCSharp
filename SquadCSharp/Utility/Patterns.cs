using System;
using System.Collections.Generic;
using System.Text;

namespace SquadCSharp.Utility
{
    /*
    * Problem: We need to move from a single file do all feature to a modulated system.
    *          Because of this change, we will be running into issues with passing data
    *          from one class to another. 
    *          
    * 
    * Solution: This class will be used like a struct. Used as a template to store
    *           all variables into one object to make it easier to pass data on to
    *           all the modules.
    */
    class Patterns
    {
        public Dictionary<string, string> cID_to_steam;


        public Patterns() 
        {
            cID_to_steam = new Dictionary<string, string>();
        }
    
        private void privateVoid() { }

        public void publicVoid() { }
    
    
    }
}
