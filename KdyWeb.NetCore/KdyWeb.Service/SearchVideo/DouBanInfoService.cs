﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.BaseInterface.Repository;
using KdyWeb.BaseInterface.Service;
using KdyWeb.Dto.SearchVideo;
using KdyWeb.Entity.SearchVideo;
using KdyWeb.IService.HttpCapture;
using KdyWeb.IService.ImageSave;
using KdyWeb.IService.SearchVideo;
using Microsoft.EntityFrameworkCore;

namespace KdyWeb.Service.SearchVideo
{
    /// <summary>
    /// 豆瓣信息 服务实现
    /// </summary>
    public class DouBanInfoService : BaseKdyService, IDouBanInfoService
    {
        private readonly IDouBanWebInfoService _douBanWebInfoService;
        private readonly IKdyRepository<DouBanInfo, int> _douBanInfoRepository;
        private readonly IKdyImgSaveService _kdyImgSaveService;

        public DouBanInfoService(IDouBanWebInfoService douBanWebInfoService, IKdyRepository<DouBanInfo, int> douBanInfoRepository, IKdyImgSaveService kdyImgSaveService)
        {
            _douBanWebInfoService = douBanWebInfoService;
            _douBanInfoRepository = douBanInfoRepository;
            _kdyImgSaveService = kdyImgSaveService;
        }

        /// <summary>
        /// 创建豆瓣信息
        /// </summary>
        /// <param name="subjectId">豆瓣Id</param>
        /// <returns></returns>
        public async Task<KdyResult<CreateForSubjectIdDto>> CreateForSubjectIdAsync(string subjectId)
        {
            CreateForSubjectIdDto result;
            var dbDouBanInfo = await _douBanInfoRepository.FirstOrDefaultAsync(a => a.VideoDetailId == subjectId);
            if (dbDouBanInfo != null)
            {
                //存在
                result = dbDouBanInfo.MapToExt<CreateForSubjectIdDto>();
                return KdyResult.Success(result);
            }

            //请求豆瓣
            var douBanWebResult = await _douBanWebInfoService.GetInfoBySubjectIdForPcWeb(subjectId);
            if (douBanWebResult.IsSuccess == false)
            {
                return KdyResult.Error<CreateForSubjectIdDto>(KdyResultCode.Error, douBanWebResult.Msg);
            }

            //上传图片
            var imgResult = await _kdyImgSaveService.PostFileByUrl(douBanWebResult.Data.Pic);
            if (imgResult.IsSuccess)
            {
                //上传成功时
                douBanWebResult.Data.Pic = $"{imgResult.Data}";
            }

            //保存数据库
            dbDouBanInfo = douBanWebResult.Data.MapToExt<DouBanInfo>();
            await _douBanInfoRepository.CreateAsync(dbDouBanInfo);

            result = dbDouBanInfo.MapToExt<CreateForSubjectIdDto>();
            return KdyResult.Success(result);
        }

        /// <summary>
        /// 获取最新豆瓣信息
        /// </summary>
        /// <returns></returns>
        public async Task<KdyResult<List<GetTop50DouBanInfoDto>>> GetTopDouBanInfoAsync(int topNumber = 50)
        {
            var dbList = await _douBanInfoRepository.GetQuery()
                .OrderByDescending(a => a.CreatedTime)
                .Take(topNumber)
                .ToListAsync();
            var list = dbList.MapToExt<List<GetTop50DouBanInfoDto>>();
            return KdyResult.Success(list);
        }
    }
}
