using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_Application.Model
{
    public class AxisStatusModel
    {
      
            
        public AxisStatusModel() { }
        public int BitNumber { get; set; }
        public string StatusString { get; set; }
        public bool Status { get; set; }

        public void InitAxisStatus(ObservableCollection<AxisStatusModel> Axis, string[] status) {
            for (int bitNumber = 1; bitNumber <= 32; bitNumber++) { 
                Axis.Add(new AxisStatusModel { BitNumber = bitNumber ,StatusString = status[bitNumber-1],Status=false });
            
            }
        
        }
    }
}
