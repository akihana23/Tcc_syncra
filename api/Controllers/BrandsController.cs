using Microsoft.AspNetCore.Mvc;
using SocialListening.API.DTOs.Brands;
using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandsController : ControllerBase
    {
        private readonly BrandMonitoringService _brandMonitoringService;

        public BrandsController(
            BrandMonitoringService brandMonitoringService
        )
        {
            _brandMonitoringService = brandMonitoringService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            var brands =
                await _brandMonitoringService.GetBrands();

            return Ok(brands);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBrand(int id)
        {
            var brand =
                await _brandMonitoringService.GetBrand(id);

            return brand is null
                ? NotFound(new { message = "Marca não encontrada." })
                : Ok(brand);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrand(
            [FromBody] BrandUpsertDto request
        )
        {
            if (
                request is null ||
                string.IsNullOrWhiteSpace(request.Name)
            )
            {
                return BadRequest(new
                {
                    message = "Informe o nome da marca."
                });
            }

            var brand =
                await _brandMonitoringService.CreateBrand(request);

            return CreatedAtAction(
                nameof(GetBrand),
                new { id = brand.Id },
                brand
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBrand(
            int id,
            [FromBody] BrandUpsertDto request
        )
        {
            if (
                request is null ||
                string.IsNullOrWhiteSpace(request.Name)
            )
            {
                return BadRequest(new
                {
                    message = "Informe o nome da marca."
                });
            }

            var brand =
                await _brandMonitoringService.UpdateBrand(id, request);

            return brand is null
                ? NotFound(new { message = "Marca não encontrada." })
                : Ok(brand);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var deleted =
                await _brandMonitoringService.DeleteBrand(id);

            return deleted
                ? NoContent()
                : NotFound(new { message = "Marca não encontrada." });
        }

        [HttpPost("{id:int}/snapshots")]
        public async Task<IActionResult> SaveSnapshot(
            int id,
            [FromBody] BrandSnapshotCreateDto request
        )
        {
            if (
                request is null ||
                string.IsNullOrWhiteSpace(request.Query)
            )
            {
                return BadRequest(new
                {
                    message = "Faça uma busca antes de salvar o snapshot."
                });
            }

            var snapshot =
                await _brandMonitoringService.SaveSnapshot(id, request);

            return snapshot is null
                ? NotFound(new { message = "Marca não encontrada." })
                : Ok(snapshot);
        }

        [HttpPost("compare")]
        public async Task<IActionResult> CompareSavedBrands(
            [FromBody] SavedBrandComparisonRequestDto request
        )
        {
            try
            {
                var comparison =
                    await _brandMonitoringService
                        .CompareSavedBrands(request);

                return Ok(comparison);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
