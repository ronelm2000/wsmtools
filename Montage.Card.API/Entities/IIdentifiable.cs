using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Entities;
public interface IIdentifiable<K> where K : struct
{
    K Id { get; }
    void AssignNewID();
    static abstract K Empty { get; }
}
