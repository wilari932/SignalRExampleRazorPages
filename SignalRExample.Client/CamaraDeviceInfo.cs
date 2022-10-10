using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.DirectShow;

namespace SignalRExample.Client
{
    public class CamaraDeviceInfo
    {
       public readonly FilterInfo FilterInfo;
       public  Guid Id { get; init; }= Guid.NewGuid();
       public CamaraDeviceInfo(FilterInfo filterInfo)
       {
           FilterInfo = filterInfo;
       }

       public override string ToString()
       {
           return FilterInfo.Name;
       }
    }
}
