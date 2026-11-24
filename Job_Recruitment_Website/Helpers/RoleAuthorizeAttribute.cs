using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Helpers
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var session = ctx.HttpContext.Session;
            var role = session != null ? session["Role"] as string : null;

            if (session == null || session["UserId"] == null || string.IsNullOrEmpty(role))
            {
                string rawUrl = ctx.HttpContext.Request != null ? ctx.HttpContext.Request.RawUrl : "";
                string loginUrl = "~/Account/Login";
                if (!string.IsNullOrEmpty(rawUrl))
                {
                    loginUrl += "?returnUrl=" + HttpUtility.UrlEncode(rawUrl);
                }
                ctx.Result = new RedirectResult(loginUrl);
                return;
            }

            if (_roles != null && _roles.Length > 0 && !_roles.Contains(role))
            {
                string roleDisplay = role == "Recruiter" ? "Employer" : role;
                string requiredDisplay = string.Join(", ", _roles.Select(r => r == "Recruiter" ? "Employer" : r));

                ctx.Result = ForbiddenResult(
                    ctx.HttpContext.Response,
                    "Tài khoản hiện tại của bạn có vai trò <strong>" + HttpUtility.HtmlEncode(roleDisplay) + "</strong>, không được phép truy cập chức năng dành riêng cho vai trò <strong>" + HttpUtility.HtmlEncode(requiredDisplay) + "</strong>."
                );
            }
        }

        // Hàm dùng chung trả về trang cảnh báo 403 Forbidden rõ nét cho TC17, TC23, TC24, TC30
        public static ActionResult ForbiddenResult(HttpResponseBase response, string message, string backUrl = "/Jobs/Index")
        {
            if (response != null)
            {
                response.StatusCode = 403;
                response.TrySkipIisCustomErrors = true;
            }

            string html =
                "<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'/><title>403 Forbidden - Từ chối truy cập</title></head>" +
                "<body style='font-family:Segoe UI,Arial,sans-serif;background:#f8f9fc;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;'>" +
                "<div style='background:#fff;border:1px solid #f5c2c7;border-radius:16px;padding:34px 42px;max-width:560px;text-align:center;box-shadow:0 8px 28px rgba(15,23,42,0.06);'>" +
                "<div style='font-size:48px;color:#dc3545;margin-bottom:8px;'>&#9888;</div>" +
                "<h2 style='color:#dc3545;margin:0 0 12px;'>403 Forbidden - Truy cập bị từ chối!</h2>" +
                "<p style='color:#475569;line-height:1.65;margin:0 0 24px;'>" + message + "</p>" +
                "<a href='" + backUrl + "' style='display:inline-block;background:#f7901e;color:#fff;text-decoration:none;font-weight:700;padding:10px 26px;border-radius:10px;'>Quay về trang an toàn</a>" +
                "</div></body></html>";

            return new ContentResult
            {
                Content = html,
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}