namespace EnterpriseBase.EntityFrameworkCore.Seed.Host;

public class InitialHostDbBuilder
{
    private readonly EnterpriseBaseDbContext _context;

    public InitialHostDbBuilder(EnterpriseBaseDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        new DefaultEditionCreator(_context).Create();
        new DefaultLanguagesCreator(_context).Create();
        new HostRoleAndUserCreator(_context).Create();
        new DefaultSettingsCreator(_context).Create();
        new DefaultCatalogCreator(_context).Create();

        _context.SaveChanges();
    }
}
