namespace SVPM.Views.SubWindowPages
{
    //TODO: Change list view to collection view
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
