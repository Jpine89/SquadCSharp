using System;
using System.Collections.Generic;
using System.Text;

namespace SquadCSharp
{
    class InternalClass
    {
        public string _InternalTest;
        public List<string> _InternalChatMessage;
        public InternalClass()
        {
            _InternalTest = "Test";
            _InternalChatMessage = new List<string>();
        }

        public void Add(string[] substring)
        {
            //foreach (var sub in subStrings)
            //    Console.WriteLine(sub);
            foreach(var sub in substring)
            {
                _InternalChatMessage.Add(sub);
                Console.WriteLine(sub);
            }
        }
    }
}
