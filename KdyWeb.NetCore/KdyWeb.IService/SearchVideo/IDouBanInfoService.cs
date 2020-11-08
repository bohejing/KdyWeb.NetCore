﻿using System.Collections.Generic;
using System.Threading.Tasks;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.BaseInterface.Service;
using KdyWeb.Dto.SearchVideo;

namespace KdyWeb.IService.SearchVideo
{
    /// <summary>
    /// 豆瓣信息 服务接口
    /// </summary>
    public interface IDouBanInfoService : IKdyService
    {
        /// <summary>
        /// 创建豆瓣信息
        /// </summary>
        /// <param name="subjectId">豆瓣Id</param>
        /// <returns></returns>
        Task<KdyResult<CreateForSubjectIdDto>> CreateForSubjectIdAsync(string subjectId);

        /// <summary>
        /// 获取最新豆瓣信息
        /// </summary>
        /// <returns></returns>
        Task<KdyResult<List<GetTop50DouBanInfoDto>>> GetTopDouBanInfoAsync(int topNumber = 50);

        /// <summary>
        /// 查询豆瓣信息
        /// </summary>
        /// <returns></returns>
        Task<KdyResult<PageList<QueryDouBanInfoDto>>> QueryDouBanInfoAsync(QueryDouBanInfoInput input);

        /// <summary>
        /// 获取豆瓣信息
        /// </summary>
        /// <param name="douBanInfoId">豆瓣信息Id</param>
        /// <returns></returns>
        Task<KdyResult<GetDouBanInfoForIdDto>> GetDouBanInfoForIdAsync(int douBanInfoId);

        /// <summary>
        /// 变更豆瓣信息状态
        /// </summary>
        /// <returns></returns>
        Task<KdyResult> ChangeDouBanInfoStatusAsync(ChangeDouBanInfoStatusInput input);
    }
}
