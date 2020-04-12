﻿using System.Net.Http;
using System.Text;

namespace KdyWeb.BaseInterface.BaseModel
{
    /// <summary>
    /// Http请求参数 接口
    /// </summary>
    public interface IHttpRequestInput<TExt>
    {
        /// <summary>
        /// 请求Url
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Cookie
        /// </summary>
        string Cookie { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        HttpMethod Method { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        Encoding EnCoding { get; set; }

        /// <summary>
        /// 扩展参数
        /// </summary>
        TExt ExtData { set; get; }
    }
}
