using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFW.Service
{
    public interface IService<T>
    {
         ICollection<T> All();
    }
}
