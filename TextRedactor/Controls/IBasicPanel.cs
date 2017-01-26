using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public interface IBasicPanel<out T> where T : Item
    {
        void Save(string Name);
    }
}
