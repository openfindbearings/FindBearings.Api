using MediatR;
using Microsoft.Extensions.Logging;
using OpenFindBearings.Application.DTOs;
using OpenFindBearings.Application.Extensions;
using OpenFindBearings.Domain.Repositories;
using OpenFindBearings.Domain.Specifications;

namespace OpenFindBearings.Application.Queries.Bearings.SearchBearings
{
    public class SearchBearingsQueryHandler : IRequestHandler<SearchBearingsQuery, PagedResult<BearingDto>>
    {
        private readonly IBearingRepository _bearingRepository;
        private readonly ILogger<SearchBearingsQueryHandler> _logger;

        public SearchBearingsQueryHandler(
            IBearingRepository bearingRepository,
            ILogger<SearchBearingsQueryHandler> logger)
        {
            _bearingRepository = bearingRepository;
            _logger = logger;
        }

        public async Task<PagedResult<BearingDto>> Handle(SearchBearingsQuery request, CancellationToken cancellationToken)
        {
            // 改动说明：移除"至少一个搜索条件"的强校验（原抛 InvalidOperationException → 被映射为 400）。
            // 该接口是 public 搜索端点，无条件时应返回空/全量分页结果（与商家搜索移除同类校验保持一致），
            // 而非 400，避免前端正常浏览被误判为请求非法。
            var searchParams = new BearingSearchParams
            {
                PartNumber = request.PartNumber,
                OldNumber = request.OldNumber,
                Keyword = request.Keyword,
                MinInnerDiameter = request.MinInnerDiameter,
                MaxInnerDiameter = request.MaxInnerDiameter,
                MinOuterDiameter = request.MinOuterDiameter,
                MaxOuterDiameter = request.MaxOuterDiameter,
                MinWidth = request.MinWidth,
                MaxWidth = request.MaxWidth,
                OriginCountry = request.OriginCountry,
                Category = request.Category,
                BrandId = request.BrandId,
                BearingTypeId = request.BearingTypeId,
                IsStandard = request.IsStandard,
                IsActive = request.IncludeDeleted == true ? null : true,
                SortBy = request.SortBy,
                SortOrder = request.SortOrder,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var result = await _bearingRepository.SearchAsync(searchParams, cancellationToken);

            var items = result.Items.Select(b => b.ToDto()).ToList();

            return new PagedResult<BearingDto>
            {
                Items = items,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
    }
}
