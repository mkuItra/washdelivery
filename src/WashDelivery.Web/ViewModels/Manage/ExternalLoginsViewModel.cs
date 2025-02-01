using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace WashDelivery.Web.ViewModels.Manage
{
    public class ExternalLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; } = null!;

        public IList<AuthenticationScheme> OtherLogins { get; set; } = null!;

        public bool ShowRemoveButton { get; set; }

        public string StatusMessage { get; set; } = null!;
    }
}
