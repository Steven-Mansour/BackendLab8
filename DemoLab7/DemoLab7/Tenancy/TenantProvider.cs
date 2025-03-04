namespace DemoLab7.Tenancy;

public interface ITenantProvider
{
    string GetTenantSchema();
}

public class TenantProvider : ITenantProvider
{
    public TenantProvider()
    {
        
    }

    public string GetTenantSchema()
    {
       return "Course-Branch";
    }
}