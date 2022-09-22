﻿using Amazon.S3;
using Amazon.S3.Model;
using AwsS3_WebApi.Dto;
using AwsS3_WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AwsS3_WebApi.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;

        public FilesController(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(IFormFile file ,string bucketName,string? prefix)
        {
            try
            {
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = String.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                    InputStream = file.OpenReadStream()
                };
                request.Metadata.Add("Content-Type",file.ContentType);  
                await _amazonS3.PutObjectAsync(request);
                return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse();
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFiles(string bucketName,string? prefix)
        {
            try
            {
                var request = new ListObjectsV2Request()
                {
                    BucketName = bucketName,
                    Prefix = prefix
                };
                var result = await _amazonS3.ListObjectsV2Async(request);
                var s3Objects = result.S3Objects.Select(s =>
                {
                    var urlRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = bucketName,
                        Key = s.Key,
                        Expires = DateTime.UtcNow.AddMinutes(1),
                    };
                    return new S3ObjectDto()
                    {
                        Name = s.Key.ToString(),
                        PresignedUrl = _amazonS3.GetPreSignedURL(urlRequest),
                    };
                });
                return Ok(s3Objects);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse();
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFileByKey(string bucketName,string key)
        {
            try
            {
                var s3Object= await _amazonS3.GetObjectAsync(bucketName, key);  

                return File(s3Object.ResponseStream,s3Object.Headers.ContentType);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse();
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFile(string bucketName,string key)
        {
            try
            {
                await _amazonS3.DeleteObjectAsync(bucketName, key);
                return Ok($"{key} deleted!");
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
