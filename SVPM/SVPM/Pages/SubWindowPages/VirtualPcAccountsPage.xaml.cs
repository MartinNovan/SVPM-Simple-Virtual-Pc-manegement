namespace SVPM.Pages.SubWindowPages
{
    public partial class VirtualPcAccountsPage
    {
        private readonly Guid _virtualPcId;

        public VirtualPcAccountsPage(Guid virtualPcId)
        {
            InitializeComponent();
            _virtualPcId = virtualPcId;
        }
    }
}
