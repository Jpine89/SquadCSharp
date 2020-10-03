using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using CoreRCON.Parsers;

namespace SquadCSharp
{
    public class PlayerList : IParseable
    {
        public string test { get; set; }
    }

    public class PlayerListParser : IParser<PlayerList>
    {
        public string Pattern => throw new NotImplementedException();

        public bool IsMatch(string input)
        {
            return true;
        }

        public PlayerList Load(GroupCollection groups)
        {
            throw new NotImplementedException();
        }

        public PlayerList Parse(string input)
        {
            //throw new NotImplementedException();

            return new PlayerList()
            {
                test = input
            };
        }

        public PlayerList Parse(Group group)
        {
            throw new NotImplementedException();
        }
    }

}
