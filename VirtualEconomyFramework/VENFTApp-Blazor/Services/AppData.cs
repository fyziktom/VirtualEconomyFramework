using System;
using System.Collections.Generic;
using System.Linq;
using VEDriversLite;
using VEDriversLite.Bookmarks;

public class AppData
{
    public NeblioAccount Account { get; set; } = new NeblioAccount();
    public List<TokenOwnerDto> VENFTOwners = new List<TokenOwnerDto>();
}

