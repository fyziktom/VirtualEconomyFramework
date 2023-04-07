using IndexedDB.Blazor;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEFramework.VEBlazor.Models
{
    public class VEFDb : IndexedDb
    {
        public VEFDb(IJSRuntime jSRuntime, string name, int version) : base(jSRuntime, name, version) { }
        public IndexedSet<AccountInfo> Accounts { get; set; }
        public IndexedSet<SubAccountInfo> SubAccounts { get; set; }
    }
}
