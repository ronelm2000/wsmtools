using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.Extensions;
public static class CollectionExtensions
{

    public static void SortByDescending<T,K>(this ObservableCollection<T> collection, Func<T,K> sortKeyMapper) where K : IComparable
    {
        List<T> sorted = collection.OrderByDescending(sortKeyMapper).ToList();
        for (int i = 0; i < sorted.Count(); i++)
            collection.Move(collection.IndexOf(sorted[i]), i);
    }
}

