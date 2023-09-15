﻿using System.Linq;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace KdyWeb.BaseInterface.Service
{
    /// <summary>
    /// 登录信息 实现
    /// </summary>
    public class LoginUserInfo : ILoginUserInfo
    {
        public LoginUserInfo(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return;
            }

            InitUserInfo(httpContextAccessor.HttpContext);
        }

        public bool IsLogin { get; set; }

        public string? UserAgent { get; set; }

        public string? UserNick { get; set; }

        public string? UserName { get; set; }

        public string? UserEmail { get; set; }

        public long? UserId { get; set; }

        public bool IsSuperAdmin { get; set; }

        public long GetUserId()
        {
            if (UserId.HasValue == false)
            {
                throw new KdyCustomException("用户信息丢失");
            }

            return UserId.Value;
        }

        public string? LoginToken { get; set; }
        public string? RoleName { get; set; }

        /// <summary>
        /// 是否资源管理
        /// </summary>
        public bool IsVodAdmin { get; set; }

        /// <summary>
        /// 是否普通用户（非资源管理和超管）
        /// </summary>
        public bool IsNormal { get; set; }

        /// <summary>
        /// 从当前请求初始化登录信息
        /// </summary>
        internal void InitUserInfo(HttpContext httpContext)
        {
            var user = httpContext.User;
            if (user.Identity?.IsAuthenticated == false)
            {
                return;
            }

            var subStr = user.Claims.FirstOrDefault(a => a.Type == JwtClaimTypes.Subject)?.Value;
            if (string.IsNullOrEmpty(subStr))
            {
                return;
            }

            var roleName = string.Empty;
            var roleList = user.Claims
                .Where(a => a.Type == JwtClaimTypes.Role)
                .ToList();
            if (roleList.Any())
            {
                roleName = string.Join(",", roleList.Select(a => a.Value));
            }

            IsLogin = true;
            UserId = long.Parse(subStr);
            UserName = user.Claims.FirstOrDefault(a => a.Type == JwtClaimTypes.Name)?.Value;
            UserEmail = user.Claims.FirstOrDefault(a => a.Type == JwtClaimTypes.Email)?.Value;
            UserNick = user.Claims.FirstOrDefault(a => a.Type == JwtClaimTypes.NickName)?.Value;
            IsSuperAdmin = user.HasClaim(a => a.Type == JwtClaimTypes.Role &&
                                              a.Value == AuthorizationConst.NormalRoleName.SuperAdmin);
            httpContext.Request.Headers.TryGetValue("Authorization", out var loginToken);
            LoginToken = loginToken + "";
            RoleName = roleName;
            IsVodAdmin = user.HasClaim(a => a.Type == JwtClaimTypes.Role &&
                                            a.Value == AuthorizationConst.NormalRoleName.VodAdmin);

            IsNormal = IsSuperAdmin == false &&
                        IsVodAdmin == false;
        }
    }
}
