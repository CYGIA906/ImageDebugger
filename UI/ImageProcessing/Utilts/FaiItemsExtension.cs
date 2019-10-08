using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI.ViewModels;

namespace UI.ImageProcessing.Utilts
{
    public static class FaiItemsExtension
    {
        public static FaiItem ByName(this IEnumerable<FaiItem> items, string name)
        {
            return items.First(ele => ele.Name == name);
        }
    }
}