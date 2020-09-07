﻿using System.Threading.Tasks;
using KdyWeb.BaseInterface.HttpBase;
using KdyWeb.BaseInterface.Service;
using KdyWeb.Dto.KdyHttp;

namespace KdyWeb.IService.KdyHttp
{
    /// <summary>
    /// 通用Http请求 接口
    /// </summary>
    /// <remarks>
    /// 基于WebRequest
    /// </remarks>
    public interface IKdyRequestCommon : IKdyHttp<KdyRequestCommonResult, string, KdyRequestCommonInput, KdyRequestCommonExtInput>, IKdyService
    {
    }
}
