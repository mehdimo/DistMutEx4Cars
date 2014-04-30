using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServer
{
    public class CarMessage
    {
        
        public int Id{set; get;}
        public char Type { set; get; }
        public long Timespan{set;get;}

        public CarMessage(char type, int id, long span)
        {
            this.Type = type;
            this.Id = id;
            this.Timespan = span;
        }

        public override string ToString()
        {
            return Type + "-" + Id + "-" + Timespan;
        }

        public static CarMessage Create(string text)
        {
            string[] items = text.Split('-');
            char ty = items[0][0];
            int id = int.Parse(items[1]);
            long span = long.Parse(items[2]);
            CarMessage msg = new CarMessage(ty, id, span);
            return msg;
        }
    }
}
