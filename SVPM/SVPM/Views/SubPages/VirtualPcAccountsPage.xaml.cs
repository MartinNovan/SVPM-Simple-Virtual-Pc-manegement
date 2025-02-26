using SVPM.Models;

namespace SVPM.Views.SubPages
{
    //TODO: Change list view to collection view
    public partial class VirtualPcAccountsPage
    {
        private readonly VirtualPc _virtualPc;

        public VirtualPcAccountsPage(VirtualPc virtualPcId)
        {
            InitializeComponent();
            _virtualPc = virtualPcId;
        }
    }
}
