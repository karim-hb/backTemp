using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRuleSubModule
    {
        public HrRuleSubModule()
        {
            HrPermissions = new HashSet<HrPermission>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int? ParentId { get; set; }

        public virtual ICollection<HrPermission> HrPermissions { get; set; }
    }
}
