using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Webstie
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
