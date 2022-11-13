using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Entities;

public interface IFetchInfo
{
    public IEnumerable<string> RIDsOrSerials { get; }
    public IEnumerable<string> Flags { get; }

}
