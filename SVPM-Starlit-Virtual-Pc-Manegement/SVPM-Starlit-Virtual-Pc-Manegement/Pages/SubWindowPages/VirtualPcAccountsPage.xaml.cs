using Microsoft.Data.SqlClient;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages
{
    public partial class VirtualPcAccountsPage
    {
        private readonly Guid _virtualPcId;

        public VirtualPcAccountsPage(Guid virtualPcId)
        {
            InitializeComponent();
            _virtualPcId = virtualPcId;
            //LoadVirtualPc();
            //LoadAccounts();
        }
    }
}
