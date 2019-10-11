using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public class Status
    {
        public StatusEnum CurrentStatus { get; set; }
        public int TotalActionsNumber { get; set; }
        public int CurrentActionNumber { get; set; }
        public string Info { get; set; }

        public Status(StatusEnum initialStatus)
        {
            this.CurrentStatus = initialStatus;
        }
    }
}
