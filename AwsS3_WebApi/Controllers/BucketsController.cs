using Amazon.S3;
using AwsS3_WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AwsS3_WebApi.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class BucketsController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;

        public BucketsController(IAmazonS3 amazonS3)
        {
           
            _amazonS3 = amazonS3;
        }

        [HttpPost("{bucketName}")]
        public async Task<IActionResult> CreateBucket(string bucketName)
        {
            try
            {
                await _amazonS3.PutBucketAsync(bucketName);
                return Ok($"Bucket {bucketName} created");
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse();
                response.Success = false;
                response.Message= ex.Message;
                return BadRequest(response);
                    
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBuckets()
        {
            try
            {
                var data = await _amazonS3.ListBucketsAsync();
                var buckets= data.Buckets.Select(b => { return b.BucketName; });
                return Ok(buckets);

            }
            catch (Exception ex)
            {
                var response = new ServiceResponse();
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }
        [HttpDelete("{bucketName}")]
        public async Task<IActionResult> DeleteBucket(string bucketName)
        {
            try
            {
                await _amazonS3.DeleteBucketAsync(bucketName);
                return Ok($"{bucketName} deleted!");
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse();
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }







    }
}
