﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.CloudParse.CloudParseEnum;
using KdyWeb.CloudParse.Extensions;
using KdyWeb.CloudParse.Input;
using KdyWeb.CloudParse.Out;
using KdyWeb.Dto.HttpCapture.KdyCloudParse.Cache;
using KdyWeb.Dto.KdyHttp;
using KdyWeb.IService;
using KdyWeb.IService.HttpCapture;
using KdyWeb.IService.HttpCapture.KdyCloudParse;
using KdyWeb.Utility;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace KdyWeb.Service.HttpCapture.KdyCloudParse
{
    /// <summary>
    /// 天翼企业云网盘解析 实现
    /// </summary>
    public class TyCropCloudParseService : BaseKdyCloudParseService<BaseConfigInput, string, BaseResultOut, string>, ITyCropCloudParseService
    {
        public TyCropCloudParseService(BaseConfigInput cloudConfig) : base(cloudConfig)
        {
            KdyRequestCommonInput = new KdyRequestCommonInput("https://b.cloud.189.cn")
            {
                TimeOut = 5000,
                ExtData = new KdyRequestCommonExtInput()
                {
                    IsAjax = true
                },
                Cookie = cloudConfig.ParseCookie,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36 Edg/94.0.992.38",
                Referer = "https://b.cloud.189.cn/main.action"
            };

        }

        protected override List<BaseResultOut> JArrayHandler(JObject jObject)
        {
            var resultList = new List<BaseResultOut>();
            if (jObject?["corpFolder"] is JObject corpFiles)
            {
                //公司列表 通用
                var model = new BaseResultOut
                {
                    ResultId = corpFiles["fileId"] + "",
                    ResultName = corpFiles["fileName"] + "",
                    FileType = CloudFileType.Dir
                };
                resultList.Add(model);
            }

            var dirArray = jObject?["data"] as JArray;
            if (dirArray == null || dirArray.Count <= 0)
            {
                return resultList;
            }

            foreach (JToken jToken in dirArray)
            {
                var model = new BaseResultOut
                {
                    ResultId = jToken["fileId"] + "",
                    ResultName = jToken["fileName"] + "",
                    FileSize = Convert.ToInt64(jToken["fileSize"])
                };

                var dirType = jToken["isFolder"] + "";
                if (dirType.ToLower() == "true" || dirType.ToLower() == "1")
                {
                    model.FileType = CloudFileType.Dir;
                }
                else
                {
                    model.FileType = model.ResultName.FileNameToFileType();
                }

                resultList.Add(model);
            }

            return resultList;
        }

        public override async Task<KdyResult<List<BaseResultOut>>> QueryFileAsync(BaseQueryInput<string> input)
        {
            var resultList = new List<BaseResultOut>();
            if (input.Page <= 0)
            {
                input.Page = 1;
            }

            if (input.PageSize <= 0)
            {
                input.PageSize = 80;
            }

            var reqUrl = $"/user/listDepartmentFolders.action?corpId={input.ExtData}&pageNum=1&pageSize=80&getUserName=true&getTotalNum=true&noCache=0.{DateTime.Now.ToMillisecondTimestamp()}";
            if (string.IsNullOrEmpty(input.InputId) == false)
            {
                reqUrl = $"/user/listCompanyFiles.action?corpId={input.ExtData}&fileId={input.InputId}&fileNameLike=&mediaType=&orderBy=1&order=ASC&pageNum={input.Page}&pageSize={input.PageSize}&recursive=false&noCache=0.{DateTime.Now.ToMillisecondTimestamp()}";
            }

            if (string.IsNullOrEmpty(input.KeyWord) == false)
            {
                //关键字搜索
                reqUrl = $"/user/getFullSearchList.action?corpId={input.ExtData}&corpFileSort=0&keyWord={input.KeyWord}&mediaType=&pageSize=20&searchDate=&searchId=&searchScore=&isSearchContent=1&noCache=0.{DateTime.Now.ToMillisecondTimestamp()}";
            }

            KdyRequestCommonInput.SetGetRequest(reqUrl);
            var reqResult = await KdyRequestClientCommon.SendAsync(KdyRequestCommonInput);
            if (reqResult.IsSuccess == false)
            {
                KdyLog.LogWarning("{userNick}企业搜索文件异常,Req:{input},ErrInfo:{msg}", CloudConfig.ReqUserInfo, input, reqResult.ErrMsg);
                return KdyResult.Success(resultList);
            }

            var jObject = JObject.Parse(reqResult.Data);
            var result = JArrayHandler(jObject);
            return KdyResult.Success(result);
        }

        public async Task<Dictionary<string, string>> GetCropListAsync()
        {
            var result = new Dictionary<string, string>();
            var cacheKey = $"{CacheKeyConst.TyCacheKey.UserCropInfoCache}:{CloudConfig.ReqUserInfo}";
            var cacheV = await KdyRedisCache.GetCache().GetValueAsync<Dictionary<string, string>>(cacheKey);
            if (cacheV != null)
            {
                return cacheV;
            }

            KdyRequestCommonInput.SetGetRequest($"/user/listCorp.action?noCache=0.{DateTime.Now.ToMillisecondTimestamp()}");
            var reqResult = await KdyRequestClientCommon.SendAsync(KdyRequestCommonInput);
            if (reqResult.IsSuccess == false)
            {
                throw new KdyCustomException($"{CloudConfig.ReqUserInfo},天翼获取家庭云列表异常.{reqResult.ErrMsg}");
                //return KdyResult.Error<AilYunCloudTokenCache>(KdyResultCode.HttpError, reqResult.ErrMsg);
            }

            var jObject = JObject.Parse(reqResult.Data);
            var jArray = jObject["data"] as JArray;
            if (jArray == null || jArray.Count <= 0)
            {
                return result;
            }

            foreach (JToken item in jArray)
            {
                result.Add($"{item["corpName"]}", $"{item["corpId"]}");
            }

            await KdyRedisCache.GetCache().SetValueAsync(cacheKey, result, TimeSpan.FromDays(1));
            return result;
        }

        public override async Task<KdyResult<string>> GetDownUrlForNoCacheAsync(BaseDownInput<string> input)
        {
            var fileInfo = await GetFileInfoAsync(input.FileId, input.ExtData);

            //1、获取下载
            var reqInput = new KdyRequestCommonInput(fileInfo.TempDownUrl, HttpMethod.Get)
            {
                Cookie = KdyRequestCommonInput.Cookie,
                Referer = KdyRequestCommonInput.Referer,
                ExtData = new KdyRequestCommonExtInput()
            };
            var reqResult = await KdyRequestClientCommon.SendAsync(reqInput);
            if (reqResult.IsSuccess == false &&
                reqResult.LocationUrl.IsEmptyExt())
            {
                KdyLog.LogWarning("{userNick}天翼获取企业云下载第一步异常,Req:{input},ErrInfo:{msg}", CloudConfig.ReqUserInfo, input, reqResult.ErrMsg);
                return KdyResult.Error<string>(KdyResultCode.Error, "获取地址异常01,请稍等1-2分钟后重试");
            }

            //2、获取最终地址
            reqInput.SetGetRequest(reqResult.LocationUrl);
            reqResult = await KdyRequestClientCommon.SendAsync(reqInput);
            if (reqResult.IsSuccess == false &&
                reqResult.LocationUrl.IsEmptyExt())
            {
                KdyLog.LogWarning("{userNick}天翼获取企业云下载第二步异常,Req:{input},ErrInfo:{msg}", CloudConfig.ReqUserInfo, input, reqResult.ErrMsg);
                return KdyResult.Error<string>(KdyResultCode.Error, "获取地址异常02,请稍等1-2分钟后重试");
            }

            var ts = GetExpiresByUrl(reqResult.LocationUrl, "Expires");
            //最后下载地址转为https的 这样移动网络可以不卡顿
            //2021-10 改为2小时
            var resultUrl = reqResult.LocationUrl.Replace("http:", "https:");
            await KdyRedisCache.GetCache().SetStringAsync(input.CacheKey, resultUrl, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ts
            });

            return KdyResult.Success<string>(resultUrl);
        }

        /// <summary>
        /// 根据文件Id和企业云Id获取文件信息
        /// </summary>
        /// <param name="fileId">文件Id</param>
        /// <param name="corpId">企业云Id</param>
        /// <returns></returns>
        internal async Task<TyCropFileInfoCache> GetFileInfoAsync(string fileId, string corpId)
        {
            var cacheKey = $"{CacheKeyConst.TyCacheKey.UserCropFileInfoCache}_{corpId}:{fileId}";
            var cacheV = await KdyRedisCache.GetCache().GetValueAsync<TyCropFileInfoCache>(cacheKey);
            if (cacheV != null)
            {
                return cacheV;
            }

            KdyRequestCommonInput.SetGetRequest($"/user/listHisVersion.action?corpId={corpId}&curFileId={fileId}&noCache=0.{DateTime.Now.ToMillisecondTimestamp()}");
            var reqResult = await KdyRequestClientCommon.SendAsync(KdyRequestCommonInput);
            if (reqResult.IsSuccess == false)
            {
                throw new KdyCustomException($"{CloudConfig.ReqUserInfo},天翼企业云获取文件异常.{reqResult.ErrMsg}");
            }

            var jObject = JObject.Parse(reqResult.Data);
            var fileModel = new TyCropFileInfoCache
            {
                FileId = fileId,
                ResultName = jObject.GetValueExt("currentFile.fileName"),
                TempDownUrl = jObject.GetValueExt("currentFile.downloadURL"),
            };

            if (string.IsNullOrEmpty(fileModel.TempDownUrl))
            {
                throw new KdyCustomException($"{CloudConfig.ReqUserInfo},天翼企业云获取文件异常01");
            }

            if (fileModel.TempDownUrl.StartsWith("//"))
            {
                fileModel.TempDownUrl = $"https:{fileModel.TempDownUrl}";
            }

            //大概有效期是1天左右 2019-03-26新增  这个地址获取的下载有效期长
            await KdyRedisCache.GetCache().SetValueAsync(cacheKey, fileModel, TimeSpan.FromDays(1));
            return fileModel;
        }
    }
}
