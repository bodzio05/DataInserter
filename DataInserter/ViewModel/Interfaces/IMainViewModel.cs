using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.ViewModel.Interfaces
{
    public interface IMainViewModel: IViewModel
    {
        bool? IsRunnable { get; set; }
        bool AllowDeleting { get; set; }
    }
}
