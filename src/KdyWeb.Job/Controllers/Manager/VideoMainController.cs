﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.Dto;
using KdyWeb.Dto.SearchVideo;
using KdyWeb.IService.SearchVideo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KdyWeb.Job.Controllers.Manager
{
    /// <summary>
    /// 影片相关
    /// </summary>
    public class VideoMainController : BaseManagerController
    {
        private readonly IVideoMainService _videoMainService;

        public VideoMainController(IVideoMainService videoMainService)
        {
            _videoMainService = videoMainService;
        }

        /// <summary>
        /// 通过豆瓣信息创建影片信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(KdyResult<CreateForDouBanInfoDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateForDouBanInfoAsync(CreateForDouBanInfoInput input)
        {
            var result = await _videoMainService.CreateForDouBanInfoAsync(input);
            return Ok(result);
        }

        /// <summary>
        /// 更新字段值
        /// </summary>
        /// <returns></returns>
        [HttpPost("updateField")]
        [ProducesResponseType(typeof(KdyResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateValueByFieldAsync(UpdateValueByFieldInput input)
        {
            var result = await _videoMainService.UpdateValueByFieldAsync(input);
            return Ok(result);
        }

        /// <summary>
        /// 删除影片
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(KdyResult), (int)HttpStatusCode.OK)]
        [Authorize(Policy = AuthorizationConst.NormalPolicyName.SuperAdminPolicy)]
        public async Task<IActionResult> DeleteAsync(BatchDeleteForLongKeyInput input)
        {
            var result = await _videoMainService.DeleteAsync(input);
            return Ok(result);
        }

        /// <summary>
        /// 匹配豆瓣信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("matchDouBanInfo")]
        [ProducesResponseType(typeof(KdyResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MatchDouBanInfoAsync(MatchDouBanInfoInput input)
        {
            var result = await _videoMainService.MatchDouBanInfoAsync(input);
            return Ok(result);
        }

        /// <summary>
        /// 修改影片详情
        /// </summary>
        /// <returns></returns>
        [HttpPost("modify")]
        [ProducesResponseType(typeof(KdyResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ModifyVideoMainAsync(ModifyVideoMainInput input)
        {
            var result = await _videoMainService.ModifyVideoMainAsync(input);
            return Ok(result);
        }

        /// <summary>
        /// 强制同步影片主表
        /// </summary>
        /// <returns></returns>
        [HttpGet("forceSync/{mainId}")]
        [ProducesResponseType(typeof(KdyResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ForceSyncVideoMainAsync(long mainId)
        {
            var result = await _videoMainService.ForceSyncVideoMainAsync(mainId);
            return Ok(result);
        }

        /// <summary>
        /// 下架影片
        /// </summary>
        /// <returns></returns>
        [HttpDelete("down/{mainId}")]
        [ProducesResponseType(typeof(KdyResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DownVodAsync(long mainId)
        {
            var result = await _videoMainService.DownVodAsync(mainId);
            return Ok(result);
        }
    }
}
