using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.Models.ViewModel
{
    public class ProductFilterVM
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories{ get; set; }
        public List<int> SelectedCategoryIds { get; set; }
        public List<int> RangerPrice { get; set; }
        public string SearchInput {  get; set; }
    }
}
